// 月蝕タイムアタック・ミッションの純粋ロジック。DOM・保存・通信には触れない。
// 呼び出し側（拠点／ダンジョン進行）が状態を保持し、ここへ都度渡す。

export const MISSION_STATUS = Object.freeze({
  LOCKED: 'locked',
  IDLE: 'idle',
  ACTIVE: 'active',
  RESULT: 'result',
});

export function createInitialMissionState() {
  return {
    unlocked: false,
    relicAcquired: false,
    relicLevel: 0,
    status: MISSION_STATUS.LOCKED,
    activeRun: null,
    lastResult: null,
  };
}

export function isUnlockReached(maxFloorReached, config) {
  return maxFloorReached >= config.unlockFloor;
}

// B20F初到達時に一度だけ呼ぶ。恒久遺物「月蝕の砂時計」Lv.1を獲得し解放する。
export function unlockMission(state, config) {
  if (state.unlocked) return state;
  return {
    ...state,
    unlocked: true,
    relicAcquired: true,
    relicLevel: 1,
    status: MISSION_STATUS.IDLE,
    activeRun: null,
  };
}

// Lv.1で25%、強化1段階ごとに+1%、上限40%。
export function getOccurrenceRate(relicLevel, config) {
  if (relicLevel <= 0) return 0;
  const stage = relicLevel - 1;
  const rate = config.baseOccurrenceRate + stage * config.occurrenceRateStep;
  return Math.min(rate, config.maxOccurrenceRate);
}

// 発生中（ACTIVE）は再抽選しない。
export function canRollOccurrence(state) {
  return state.unlocked && state.status === MISSION_STATUS.IDLE;
}

// 帰還後、次の潜行へ進む際に呼ぶ抽選。
export function rollForNextDescent(state, config, rng = Math.random, nowMs = Date.now()) {
  if (!canRollOccurrence(state)) {
    return { state, occurred: false, rate: 0, skipped: true };
  }
  const rate = getOccurrenceRate(state.relicLevel, config);
  const occurred = rng() < rate;
  if (!occurred) {
    return { state, occurred: false, rate, skipped: false };
  }
  const nextState = {
    ...state,
    status: MISSION_STATUS.ACTIVE,
    activeRun: {
      startedAt: nowMs,
      timeLimitSec: config.timeLimitSec,
      floorReached: 0,
    },
  };
  return { state: nextState, occurred: true, rate, skipped: false };
}

export function getRemainingSec(activeRun, nowMs = Date.now()) {
  if (!activeRun) return 0;
  const elapsedSec = (nowMs - activeRun.startedAt) / 1000;
  return Math.max(0, activeRun.timeLimitSec - elapsedSec);
}

export function isTimeExpired(activeRun, nowMs = Date.now()) {
  return getRemainingSec(activeRun, nowMs) <= 0;
}

// 到達階を更新する。下がることはない。ACTIVE以外では何もしない。
export function recordFloorReached(state, floor) {
  if (state.status !== MISSION_STATUS.ACTIVE || !state.activeRun) return state;
  const floorReached = Math.max(state.activeRun.floorReached, floor);
  return {
    ...state,
    activeRun: { ...state.activeRun, floorReached },
  };
}

// 到達階以下の途中報酬をすべて返す。
export function getRewardsUpToFloor(floor, config) {
  return config.rewardTiers.filter(tier => tier.floor <= floor);
}

export function getItemRankForFloor(floor, config) {
  let best = config.rankThresholds[0];
  for (const threshold of config.rankThresholds) {
    if (floor >= threshold.floor) best = threshold;
  }
  return best;
}

export function getNextRewardTier(floor, config) {
  return config.rewardTiers.find(tier => tier.floor > floor) ?? null;
}

// 制限時間終了、または帰還によりミッションを終える。到達階に応じた報酬とランクを確定する。
export function finalizeMissionRun(state, config) {
  if (state.status !== MISSION_STATUS.ACTIVE || !state.activeRun) return state;
  const floorReached = state.activeRun.floorReached;
  const rewards = getRewardsUpToFloor(floorReached, config);
  const rank = getItemRankForFloor(floorReached, config);
  return {
    ...state,
    status: MISSION_STATUS.RESULT,
    lastResult: { floorReached, rewards, rank },
  };
}

// 結果確認後、次の抽選ができるようIDLEへ戻す。lastResultは表示用に残す。
export function acknowledgeResult(state) {
  if (state.status !== MISSION_STATUS.RESULT) return state;
  return {
    ...state,
    status: MISSION_STATUS.IDLE,
    activeRun: null,
  };
}

// UI表示用に状態と設定をまとめて計算する。mission.htmlの単独プレビューと
// 本体UIの双方から同じ計算結果を使えるようにするための集約関数。
export function buildMissionViewModel(state, config, nowMs = Date.now()) {
  const occurrenceRate = getOccurrenceRate(state.relicLevel, config);

  if (!state.unlocked) {
    return {
      screen: 'locked',
      unlockFloor: config.unlockFloor,
    };
  }

  if (state.status === MISSION_STATUS.ACTIVE && state.activeRun) {
    const floorReached = state.activeRun.floorReached;
    return {
      screen: 'active',
      relicLevel: state.relicLevel,
      remainingSec: getRemainingSec(state.activeRun, nowMs),
      timeLimitSec: state.activeRun.timeLimitSec,
      floorReached,
      rewardsSoFar: getRewardsUpToFloor(floorReached, config),
      currentRank: getItemRankForFloor(floorReached, config),
      nextRewardTier: getNextRewardTier(floorReached, config),
    };
  }

  if (state.status === MISSION_STATUS.RESULT && state.lastResult) {
    return {
      screen: 'result',
      relicLevel: state.relicLevel,
      floorReached: state.lastResult.floorReached,
      rewards: state.lastResult.rewards,
      rank: state.lastResult.rank,
    };
  }

  return {
    screen: 'idle',
    relicLevel: state.relicLevel,
    relicName: config.relicName,
    occurrenceRatePercent: Math.round(occurrenceRate * 100),
    nextRewardTier: getNextRewardTier(0, config),
  };
}
