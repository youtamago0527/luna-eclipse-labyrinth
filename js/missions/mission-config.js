// 月蝕タイムアタック・ミッションの調整値。
// 制限時間・階層別報酬・ランク区分は仮値。バランス確定後にここだけ差し替える。
export const MISSION_CONFIG = Object.freeze({
  unlockFloor: 20,
  relicId: 'lunarEclipseHourglass',
  relicName: '月蝕の砂時計',

  baseOccurrenceRate: 0.25,
  occurrenceRateStep: 0.01,
  maxOccurrenceRate: 0.40,
  maxRelicLevel: 16,

  timeLimitSec: 180,

  // 到達階ごとの途中報酬。floor以下に到達した時点ですべて獲得する。仮値。
  rewardTiers: Object.freeze([
    Object.freeze({ floor: 5, itemId: 'moonShardFragment', label: '月の欠片', quantity: 1 }),
    Object.freeze({ floor: 10, itemId: 'eclipseDust', label: '月蝕の砂', quantity: 2 }),
    Object.freeze({ floor: 15, itemId: 'eclipseDust', label: '月蝕の砂', quantity: 3 }),
    Object.freeze({ floor: 20, itemId: 'eclipseCrystal', label: '月蝕結晶', quantity: 1 }),
  ]),

  // 制限時間内の最高到達階から決まる恒久アイテムランク。floor以上で該当ランクになる。仮値。
  rankThresholds: Object.freeze([
    Object.freeze({ floor: 0, rankId: 'none', label: '未到達' }),
    Object.freeze({ floor: 5, rankId: 'bronze', label: '銅' }),
    Object.freeze({ floor: 10, rankId: 'silver', label: '銀' }),
    Object.freeze({ floor: 15, rankId: 'gold', label: '金' }),
    Object.freeze({ floor: 20, rankId: 'platinum', label: '白金' }),
  ]),
});
