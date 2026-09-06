import test from 'node:test';
import assert from 'node:assert/strict';
import { MISSION_CONFIG } from '../js/missions/mission-config.js';
import {
  MISSION_STATUS,
  createInitialMissionState,
  isUnlockReached,
  unlockMission,
  getOccurrenceRate,
  canRollOccurrence,
  rollForNextDescent,
  getRemainingSec,
  isTimeExpired,
  recordFloorReached,
  getRewardsUpToFloor,
  getItemRankForFloor,
  getNextRewardTier,
  finalizeMissionRun,
  acknowledgeResult,
  buildMissionViewModel,
} from '../js/missions/mission-system.js';

test('B20F未到達では解放判定がfalse', () => {
  assert.equal(isUnlockReached(19, MISSION_CONFIG), false);
});

test('B20F到達で解放判定がtrue', () => {
  assert.equal(isUnlockReached(20, MISSION_CONFIG), true);
});

test('初期状態はLOCKEDで未解放', () => {
  const state = createInitialMissionState();
  assert.equal(state.status, MISSION_STATUS.LOCKED);
  assert.equal(state.unlocked, false);
  assert.equal(state.relicLevel, 0);
});

test('unlockMissionで解放・遺物Lv.1獲得・IDLEへ遷移する', () => {
  const state = unlockMission(createInitialMissionState(), MISSION_CONFIG);
  assert.equal(state.unlocked, true);
  assert.equal(state.relicAcquired, true);
  assert.equal(state.relicLevel, 1);
  assert.equal(state.status, MISSION_STATUS.IDLE);
});

test('unlockMissionは二重解放しても状態を変えない', () => {
  const once = unlockMission(createInitialMissionState(), MISSION_CONFIG);
  const twice = unlockMission(once, MISSION_CONFIG);
  assert.deepEqual(twice, once);
});

test('発生率はLv.1で25%', () => {
  assert.equal(getOccurrenceRate(1, MISSION_CONFIG), 0.25);
});

test('発生率は強化1段階につき+1%される', () => {
  assert.equal(Math.round(getOccurrenceRate(2, MISSION_CONFIG) * 100), 26);
  assert.equal(Math.round(getOccurrenceRate(6, MISSION_CONFIG) * 100), 30);
});

test('発生率は上限40%を超えない', () => {
  assert.equal(getOccurrenceRate(100, MISSION_CONFIG), 0.40);
  assert.equal(getOccurrenceRate(MISSION_CONFIG.maxRelicLevel, MISSION_CONFIG), 0.40);
});

test('未解放では再抽選可否がfalse', () => {
  assert.equal(canRollOccurrence(createInitialMissionState()), false);
});

test('解放済みIDLEでは抽選可能', () => {
  const state = unlockMission(createInitialMissionState(), MISSION_CONFIG);
  assert.equal(canRollOccurrence(state), true);
});

test('ACTIVE中は再抽選しない（canRollOccurrenceがfalse）', () => {
  const unlocked = unlockMission(createInitialMissionState(), MISSION_CONFIG);
  const { state: active } = rollForNextDescent(unlocked, MISSION_CONFIG, () => 0);
  assert.equal(active.status, MISSION_STATUS.ACTIVE);
  assert.equal(canRollOccurrence(active), false);
});

test('rollForNextDescentは未解放だとskippedを返し状態を変えない', () => {
  const locked = createInitialMissionState();
  const result = rollForNextDescent(locked, MISSION_CONFIG, () => 0);
  assert.equal(result.skipped, true);
  assert.equal(result.occurred, false);
  assert.equal(result.state, locked);
});

test('rollForNextDescentはACTIVE中に呼んでもskippedになり再抽選しない', () => {
  const unlocked = unlockMission(createInitialMissionState(), MISSION_CONFIG);
  const { state: active } = rollForNextDescent(unlocked, MISSION_CONFIG, () => 0);
  const second = rollForNextDescent(active, MISSION_CONFIG, () => 0);
  assert.equal(second.skipped, true);
  assert.equal(second.state, active);
});

test('rngが発生率未満なら発生しACTIVEへ遷移する', () => {
  const unlocked = unlockMission(createInitialMissionState(), MISSION_CONFIG);
  const now = 1_000_000;
  const { state, occurred, rate } = rollForNextDescent(unlocked, MISSION_CONFIG, () => 0.24, now);
  assert.equal(occurred, true);
  assert.equal(rate, 0.25);
  assert.equal(state.status, MISSION_STATUS.ACTIVE);
  assert.equal(state.activeRun.floorReached, 0);
  assert.equal(state.activeRun.startedAt, now);
  assert.equal(state.activeRun.timeLimitSec, MISSION_CONFIG.timeLimitSec);
});

test('rngが発生率以上なら発生せずIDLEのまま', () => {
  const unlocked = unlockMission(createInitialMissionState(), MISSION_CONFIG);
  const { state, occurred } = rollForNextDescent(unlocked, MISSION_CONFIG, () => 0.25);
  assert.equal(occurred, false);
  assert.equal(state.status, MISSION_STATUS.IDLE);
});

test('getRemainingSecは経過時間ぶん減り、0未満にはならない', () => {
  const activeRun = { startedAt: 0, timeLimitSec: 180, floorReached: 0 };
  assert.equal(getRemainingSec(activeRun, 60_000), 120);
  assert.equal(getRemainingSec(activeRun, 999_000), 0);
});

test('isTimeExpiredは制限時間到達でtrue', () => {
  const activeRun = { startedAt: 0, timeLimitSec: 180, floorReached: 0 };
  assert.equal(isTimeExpired(activeRun, 179_000), false);
  assert.equal(isTimeExpired(activeRun, 180_000), true);
});

test('recordFloorReachedはACTIVE中のみ最高到達階を更新する', () => {
  const unlocked = unlockMission(createInitialMissionState(), MISSION_CONFIG);
  const { state: active } = rollForNextDescent(unlocked, MISSION_CONFIG, () => 0);
  const afterFloor5 = recordFloorReached(active, 5);
  assert.equal(afterFloor5.activeRun.floorReached, 5);
  const afterLowerFloor = recordFloorReached(afterFloor5, 3);
  assert.equal(afterLowerFloor.activeRun.floorReached, 5, '到達階は下がらない');
  const afterFloor12 = recordFloorReached(afterFloor5, 12);
  assert.equal(afterFloor12.activeRun.floorReached, 12);
});

test('recordFloorReachedはIDLE状態では何もしない', () => {
  const unlocked = unlockMission(createInitialMissionState(), MISSION_CONFIG);
  const unchanged = recordFloorReached(unlocked, 10);
  assert.equal(unchanged, unlocked);
});

test('getRewardsUpToFloorは到達階以下の報酬をすべて含む', () => {
  assert.deepEqual(getRewardsUpToFloor(4, MISSION_CONFIG), []);
  assert.equal(getRewardsUpToFloor(5, MISSION_CONFIG).length, 1);
  assert.equal(getRewardsUpToFloor(12, MISSION_CONFIG).length, 2);
  assert.equal(getRewardsUpToFloor(20, MISSION_CONFIG).length, 4);
  assert.equal(getRewardsUpToFloor(99, MISSION_CONFIG).length, 4);
});

test('getItemRankForFloorは到達階に応じたランクを返す', () => {
  assert.equal(getItemRankForFloor(0, MISSION_CONFIG).rankId, 'none');
  assert.equal(getItemRankForFloor(4, MISSION_CONFIG).rankId, 'none');
  assert.equal(getItemRankForFloor(5, MISSION_CONFIG).rankId, 'bronze');
  assert.equal(getItemRankForFloor(14, MISSION_CONFIG).rankId, 'silver');
  assert.equal(getItemRankForFloor(20, MISSION_CONFIG).rankId, 'platinum');
  assert.equal(getItemRankForFloor(999, MISSION_CONFIG).rankId, 'platinum');
});

test('getNextRewardTierは到達階より先の直近報酬を返す', () => {
  assert.equal(getNextRewardTier(0, MISSION_CONFIG).floor, 5);
  assert.equal(getNextRewardTier(7, MISSION_CONFIG).floor, 10);
  assert.equal(getNextRewardTier(20, MISSION_CONFIG), null);
});

test('finalizeMissionRunは到達階から報酬とランクを確定しRESULTへ遷移する', () => {
  const unlocked = unlockMission(createInitialMissionState(), MISSION_CONFIG);
  const { state: active } = rollForNextDescent(unlocked, MISSION_CONFIG, () => 0);
  const progressed = recordFloorReached(active, 11);
  const finished = finalizeMissionRun(progressed, MISSION_CONFIG);
  assert.equal(finished.status, MISSION_STATUS.RESULT);
  assert.equal(finished.lastResult.floorReached, 11);
  assert.equal(finished.lastResult.rank.rankId, 'silver');
  assert.equal(finished.lastResult.rewards.length, 2);
});

test('finalizeMissionRunはACTIVE以外では何もしない', () => {
  const unlocked = unlockMission(createInitialMissionState(), MISSION_CONFIG);
  assert.equal(finalizeMissionRun(unlocked, MISSION_CONFIG), unlocked);
});

test('acknowledgeResultはRESULTからIDLEへ戻し次の抽選を可能にする', () => {
  const unlocked = unlockMission(createInitialMissionState(), MISSION_CONFIG);
  const { state: active } = rollForNextDescent(unlocked, MISSION_CONFIG, () => 0);
  const progressed = recordFloorReached(active, 8);
  const finished = finalizeMissionRun(progressed, MISSION_CONFIG);
  const acked = acknowledgeResult(finished);
  assert.equal(acked.status, MISSION_STATUS.IDLE);
  assert.equal(acked.activeRun, null);
  assert.equal(canRollOccurrence(acked), true);
});

test('buildMissionViewModelはLOCKED状態でlocked画面を返す', () => {
  const vm = buildMissionViewModel(createInitialMissionState(), MISSION_CONFIG);
  assert.equal(vm.screen, 'locked');
  assert.equal(vm.unlockFloor, 20);
});

test('buildMissionViewModelはIDLE状態でidle画面と発生率%を返す', () => {
  const unlocked = unlockMission(createInitialMissionState(), MISSION_CONFIG);
  const vm = buildMissionViewModel(unlocked, MISSION_CONFIG);
  assert.equal(vm.screen, 'idle');
  assert.equal(vm.occurrenceRatePercent, 25);
  assert.equal(vm.relicName, MISSION_CONFIG.relicName);
});

test('buildMissionViewModelはACTIVE状態で残り時間と現在到達階を返す', () => {
  const unlocked = unlockMission(createInitialMissionState(), MISSION_CONFIG);
  const now = 0;
  const { state: active } = rollForNextDescent(unlocked, MISSION_CONFIG, () => 0, now);
  const progressed = recordFloorReached(active, 6);
  const vm = buildMissionViewModel(progressed, MISSION_CONFIG, 30_000);
  assert.equal(vm.screen, 'active');
  assert.equal(vm.floorReached, 6);
  assert.equal(vm.remainingSec, 150);
  assert.equal(vm.currentRank.rankId, 'bronze');
  assert.equal(vm.nextRewardTier.floor, 10);
});

test('buildMissionViewModelはRESULT状態で確定結果を返す', () => {
  const unlocked = unlockMission(createInitialMissionState(), MISSION_CONFIG);
  const { state: active } = rollForNextDescent(unlocked, MISSION_CONFIG, () => 0);
  const progressed = recordFloorReached(active, 16);
  const finished = finalizeMissionRun(progressed, MISSION_CONFIG);
  const vm = buildMissionViewModel(finished, MISSION_CONFIG);
  assert.equal(vm.screen, 'result');
  assert.equal(vm.floorReached, 16);
  assert.equal(vm.rank.rankId, 'gold');
  assert.equal(vm.rewards.length, 3);
});
