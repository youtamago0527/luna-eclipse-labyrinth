// mission.html だけで挙動を確認するための接続処理。
// 本体（拠点／ダンジョン）とは接続しない。保存も通信も行わない。
import { MISSION_CONFIG } from './mission-config.js';
import {
  MISSION_STATUS,
  createInitialMissionState,
  unlockMission,
  rollForNextDescent,
  recordFloorReached,
  finalizeMissionRun,
  acknowledgeResult,
  isTimeExpired,
  buildMissionViewModel,
} from './mission-system.js';

const PREVIEW_ACTIVE_SAMPLE_FLOOR = 7;
const PREVIEW_RESULT_SAMPLE_FLOOR = 16;
const PREVIEW_MAX_FLOOR_FOR_ADVANCE = 30;

const panels = document.querySelectorAll('.mission-panel');
const devButtons = document.querySelectorAll('[data-preview]');

const lockedConditionText = document.querySelector('#locked-condition-text');

const idleRelicName = document.querySelector('#idle-relic-name');
const idleRate = document.querySelector('#idle-rate');
const idleRelicLevel = document.querySelector('#idle-relic-level');
const idleNextReward = document.querySelector('#idle-next-reward');
const idleRollNote = document.querySelector('#idle-roll-note');

const activeRemaining = document.querySelector('#active-remaining');
const activeFloor = document.querySelector('#active-floor');
const activeNextFloor = document.querySelector('#active-next-floor');
const activeRewardList = document.querySelector('#active-reward-list');

const resultFloor = document.querySelector('#result-floor');
const resultRank = document.querySelector('#result-rank');
const resultRewardList = document.querySelector('#result-reward-list');

let state = createInitialMissionState();

function formatTime(totalSec) {
  const sec = Math.max(0, Math.ceil(totalSec));
  const m = Math.floor(sec / 60);
  const s = sec % 60;
  return `${String(m).padStart(2, '0')}:${String(s).padStart(2, '0')}`;
}

function renderRewardList(listEl, tiers) {
  listEl.innerHTML = '';
  if (tiers.length === 0) {
    const li = document.createElement('li');
    li.textContent = 'まだ報酬はありません。';
    listEl.appendChild(li);
    return;
  }
  for (const tier of tiers) {
    const li = document.createElement('li');
    li.classList.add('got');
    li.innerHTML = `<span class="tag">B${tier.floor}F</span><span>${tier.label} ×${tier.quantity}</span>`;
    listEl.appendChild(li);
  }
}

function render() {
  const vm = buildMissionViewModel(state, MISSION_CONFIG, Date.now());

  panels.forEach(panel => panel.classList.toggle('active', panel.dataset.screen === vm.screen));
  devButtons.forEach(button => button.classList.toggle('on', button.dataset.preview === vm.screen));

  if (vm.screen === 'locked') {
    lockedConditionText.textContent = `迷宮 B${vm.unlockFloor}F へ到達すると解放されます。`;
    return;
  }

  if (vm.screen === 'idle') {
    idleRelicName.textContent = vm.relicName;
    idleRate.textContent = `${vm.occurrenceRatePercent}%`;
    idleRelicLevel.textContent = `Lv.${vm.relicLevel}`;
    idleNextReward.textContent = vm.nextRewardTier
      ? `B${vm.nextRewardTier.floor}F 到達で ${vm.nextRewardTier.label} ×${vm.nextRewardTier.quantity}`
      : 'すべての報酬階に到達済みです。';
    return;
  }

  if (vm.screen === 'active') {
    activeRemaining.textContent = formatTime(vm.remainingSec);
    activeFloor.textContent = `B${vm.floorReached}F`;
    activeNextFloor.textContent = vm.nextRewardTier ? `B${vm.nextRewardTier.floor}F` : '――';
    renderRewardList(activeRewardList, vm.rewardsSoFar);
    return;
  }

  if (vm.screen === 'result') {
    resultFloor.textContent = `B${vm.floorReached}F`;
    resultRank.textContent = `ランク：${vm.rank.label}`;
    renderRewardList(resultRewardList, vm.rewards);
  }
}

function buildPreviewSample(screen) {
  if (screen === 'locked') return createInitialMissionState();

  if (screen === 'idle') return unlockMission(createInitialMissionState(), MISSION_CONFIG);

  if (screen === 'active') {
    const unlocked = unlockMission(createInitialMissionState(), MISSION_CONFIG);
    const { state: active } = rollForNextDescent(unlocked, MISSION_CONFIG, () => 0, Date.now());
    return recordFloorReached(active, PREVIEW_ACTIVE_SAMPLE_FLOOR);
  }

  // result
  const unlocked = unlockMission(createInitialMissionState(), MISSION_CONFIG);
  const { state: active } = rollForNextDescent(unlocked, MISSION_CONFIG, () => 0, Date.now());
  const progressed = recordFloorReached(active, PREVIEW_RESULT_SAMPLE_FLOOR);
  return finalizeMissionRun(progressed, MISSION_CONFIG);
}

devButtons.forEach(button => {
  button.addEventListener('click', () => {
    state = buildPreviewSample(button.dataset.preview);
    idleRollNote.textContent = '';
    render();
  });
});

document.querySelector('[data-action="preview-unlock"]').addEventListener('click', () => {
  state = unlockMission(state, MISSION_CONFIG);
  render();
});

document.querySelector('[data-action="relic-level-up"]').addEventListener('click', () => {
  if (state.status !== MISSION_STATUS.IDLE) return;
  state = { ...state, relicLevel: Math.min(state.relicLevel + 1, MISSION_CONFIG.maxRelicLevel) };
  render();
});

document.querySelector('[data-action="relic-level-down"]').addEventListener('click', () => {
  if (state.status !== MISSION_STATUS.IDLE) return;
  state = { ...state, relicLevel: Math.max(state.relicLevel - 1, 1) };
  render();
});

document.querySelector('[data-action="roll-occurrence"]').addEventListener('click', () => {
  const result = rollForNextDescent(state, MISSION_CONFIG);
  state = result.state;
  idleRollNote.textContent = result.occurred
    ? `発生しました！（発生率 ${Math.round(result.rate * 100)}%）`
    : `発生しませんでした（発生率 ${Math.round(result.rate * 100)}%）。次の潜行でまた抽選します。`;
  render();
});

document.querySelector('[data-action="advance-floor"]').addEventListener('click', () => {
  if (state.status !== MISSION_STATUS.ACTIVE || !state.activeRun) return;
  const nextFloor = Math.min(state.activeRun.floorReached + 1, PREVIEW_MAX_FLOOR_FOR_ADVANCE);
  state = recordFloorReached(state, nextFloor);
  render();
});

document.querySelector('[data-action="finish-run"]').addEventListener('click', () => {
  state = finalizeMissionRun(state, MISSION_CONFIG);
  render();
});

document.querySelector('[data-action="ack-result"]').addEventListener('click', () => {
  state = acknowledgeResult(state);
  idleRollNote.textContent = '';
  render();
});

// 制限時間切れを見せるため、発生中のみ定期的に残り時間を更新し、0で自動確定する。
setInterval(() => {
  if (state.status !== MISSION_STATUS.ACTIVE || !state.activeRun) return;
  if (isTimeExpired(state.activeRun)) {
    state = finalizeMissionRun(state, MISSION_CONFIG);
  }
  render();
}, 500);

render();
