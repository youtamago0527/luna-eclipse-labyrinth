const SOUND_EFFECTS = Object.freeze({
  uiSelect: 'assets/audio/se/otologic/snes-rpg03/SNES-RPG03-7(Select).mp3',
  uiConfirm: 'assets/audio/se/otologic/nes-rpg02/NES-RPG02-01(Enter).mp3',
  uiCancel: 'assets/audio/se/otologic/nes-rpg02/NES-RPG02-02(Wall).mp3',
  message: 'assets/audio/se/otologic/nes-rpg02/NES-RPG02-09(Message).mp3',
  treasure: 'assets/audio/se/otologic/nes-rpg02/NES-RPG02-03(Treasure).mp3',
  door: 'assets/audio/se/otologic/snes-rpg03/SNES-RPG03-3(Door).mp3',
  purchase: 'assets/audio/se/otologic/snes-rpg03/SNES-RPG03-6(Purchase).mp3',
  attack: 'assets/audio/se/otologic/snes-rpg05/SNES-RPG05-01(Attack-Sword).mp3',
  damage: 'assets/audio/se/otologic/snes-rpg05/SNES-RPG05-10(Damage).mp3',
  miss: 'assets/audio/se/otologic/snes-rpg05/SNES-RPG05-09(Miss).mp3',
  defeat: 'assets/audio/se/otologic/snes-rpg05/SNES-RPG05-15(Defeat).mp3',
  encounter: 'assets/audio/se/otologic/snes-rpg05/SNES-RPG05-17(Encounter).mp3',
});

const pool = new Map();

export function playSoundEffect(name, { volume = 0.35 } = {}) {
  const source = SOUND_EFFECTS[name];
  if (!source) return false;
  const audio = pool.get(name) ?? new Audio(source);
  pool.set(name, audio);
  audio.pause();
  audio.currentTime = 0;
  audio.volume = Math.max(0, Math.min(1, volume));
  audio.play().catch(() => {});
  return true;
}

export { SOUND_EFFECTS };
