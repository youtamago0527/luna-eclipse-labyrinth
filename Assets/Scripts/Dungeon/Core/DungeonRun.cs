using System.Collections.Generic;
using UnityEngine;

namespace LunaEclipse.Dungeon
{
    /// <summary>Pure grid simulation. No GameObjects, timing, or input-device dependencies.</summary>
    public sealed class DungeonRun
    {
        public DungeonMap Map { get; private set; }
        public Vector2Int PlayerCell { get; private set; }
        public Vector2Int Facing { get; private set; } = Vector2Int.down;
        public int Hp { get; private set; } = DungeonRules.PlayerHp;
        public int MaxHp => DungeonRules.PlayerHp;
        public string RunId {get;}
        public int StartFloor {get;}
        public RunModifiers Modifiers {get;}
        public int BagCapacity => Mathf.Clamp(Modifiers.BagCapacity,20,40);
        public int SatietyInterval => BoundedStat((long)DungeonRules.SatietyTurnInterval+Modifiers.SatietyIntervalBonus,1);
        public string WeaponId {get;private set;}
        public string ShieldId {get;private set;}
        public int AttackPower => BoundedStat(DungeonRules.PlayerAttack+EquippedBonus(WeaponId),1);
        public int DefensePower => BoundedStat(EquippedBonus(ShieldId),0);
        long EquippedBonus(string id){var item=Bag.Find(i=>i.Id==id);return item==null?0L:(long)item.Enhancement+1;}
        static int BoundedStat(long value,int minimum)=>(int)System.Math.Max(minimum,System.Math.Min(int.MaxValue,value));
        public int Satiety { get; private set; } = DungeonRules.MaximumSatiety;
        public int Turns { get; private set; }
        public int Floor { get; private set; } = 1;
        public List<ItemData> Bag { get; } = new List<ItemData>();
        public List<EnemyData> Enemies { get; } = new List<EnemyData>();
        public List<FloorItem> Items { get; } = new List<FloorItem>();
        public List<string> Logs { get; } = new List<string>();
        public Vector2Int? LastAttackCell { get; private set; }
        public string LastAction { get; private set; } = "idle";
        public bool Dead => Hp <= 0;
        public bool CanPickUp => !Dead && Bag.Count < BagCapacity && Items.Exists(item => item.Cell == PlayerCell);
        public bool CanDescend => !Dead && PlayerCell == Map.Stairs;
        private readonly int seed;

        public DungeonRun(int seed,RunModifiers modifiers=null,int startFloor=1,string runId=null,IEnumerable<ItemData> initialItems=null)
        {
            this.seed = seed;
            RunId=string.IsNullOrWhiteSpace(runId)?System.Guid.NewGuid().ToString("N"):runId;
            Modifiers=modifiers??new RunModifiers();StartFloor=Mathf.Max(1,startFloor);Floor=StartFloor;
            if(initialItems!=null)foreach(var item in initialItems)
            {
                if(item==null||string.IsNullOrWhiteSpace(item.Id)||Bag.Exists(i=>i.Id==item.Id)||Bag.Count>=BagCapacity)
                    throw new System.ArgumentException("Invalid expedition loadout");
                Bag.Add(new ItemData{Id=item.Id,Kind=item.Kind,Name=item.Name,Enhancement=item.Enhancement});
            }
            EnterFloor();
            Log("月影の迷宮へ。近くの薬草を拾ってみよう。");
        }
        public bool Move(Vector2Int direction)
        {
            if (Dead || !DungeonRules.IsCardinal(direction)) return false;
            Facing = direction; LastAttackCell = null;
            var next = PlayerCell + direction;
            if (Enemies.Exists(enemy => enemy.Cell == next && enemy.Hp > 0)) return Attack();
            if (!Map.Walkable(next)) { LastAction = "blocked"; Log("壁には進めない。"); return false; }
            PlayerCell = next; LastAction = "move";
            Map.Reveal(PlayerCell);
            if (Items.Exists(item => item.Cell == next)) Log(Items.Find(item=>item.Cell==next).Item.DisplayName+"を見つけた。「拾う」でバッグへ。");
            else if (next == Map.Stairs) Log("階段を見つけた。");
            else Log("ルナは一歩進んだ。");
            CompleteTurn(); return true;
        }
        public bool Attack()
        {
            if (Dead) return false;
            LastAction = "attack"; LastAttackCell = PlayerCell + Facing;
            var enemy = Enemies.Find(candidate => candidate.Cell == LastAttackCell.Value && candidate.Hp > 0);
            if (enemy == null) Log("ルナは剣を振った。");
            else
            {
                enemy.Hp = Mathf.Max(0, enemy.Hp - AttackPower);
                Log("ルナの攻撃！ "+enemy.Name+"に"+AttackPower+"ダメージ。");
                if (enemy.Hp == 0) { Enemies.Remove(enemy); Log(enemy.Name+"を倒した！"); }
            }
            CompleteTurn(); return true;
        }
        public bool Wait()
        {
            if (Dead) return false;
            LastAction = "wait"; LastAttackCell = null; Log("ルナはその場で待機した。");
            CompleteTurn(); return true;
        }
        public bool PickUp()
        {
            if (!CanPickUp) { if (!Dead) Log(Bag.Count >= BagCapacity ? "バッグがいっぱい。" : "足元にアイテムがない。"); return false; }
            var item = Items.Find(candidate => candidate.Cell == PlayerCell);
            Items.Remove(item); Bag.Add(item.Item);
            LastAction = "pickup"; LastAttackCell = null; Log(item.Item.DisplayName + "を拾った。");
            CompleteTurn(); return true;
        }
        public bool UseItem(string id)
        {
            if(Dead)return false;var item=Bag.Find(i=>i.Id==id);if(item==null)return false;
            var def=ContentCatalog.FindItem(item.Kind);if(def==null)return false;
            if(def.Equipment)
            {
                if(item.Kind=="moon_sword"){if(WeaponId==id)return false;WeaponId=id;}
                else {if(ShieldId==id)return false;ShieldId=id;}
                Log(item.DisplayName+"を装備した。");
            }
            else if(def.Healing>0)
            {
                if(Hp>=MaxHp){Log("HPは満タン。アイテムは使わなかった。");return false;}
                int healed=Mathf.Min(def.Healing,MaxHp-Hp);Hp+=healed;Bag.Remove(item);Log(item.Name+"でHPを"+healed+"回復。");
            }
            else return false;
            LastAction="use";LastAttackCell=null;CompleteTurn();return true;
        }
        public bool DropItem(string id)
        {
            if(Dead)return false;var item=Bag.Find(i=>i.Id==id);if(item==null)return false;
            Bag.Remove(item);if(WeaponId==id)WeaponId=null;if(ShieldId==id)ShieldId=null;
            Items.Add(new FloorItem{Cell=PlayerCell,Item=item});LastAction="drop";LastAttackCell=null;
            Log(item.DisplayName+"を足元に置いた。");CompleteTurn();return true;
        }
        public bool Descend()
        {
            if (!CanDescend) return false;
            SpendTurn(); Floor++; EnterFloor();
            LastAction = "stairs"; LastAttackCell = null;
            Log("B" + Floor + "Fへ降りた。探索を続けよう。");
            return true;
        }
        public void Log(string text)
        {
            Logs.Add(text);
            while (Logs.Count > 3) Logs.RemoveAt(0);
        }
        private void SpendTurn()
        {
            Turns++;
            if (Turns % SatietyInterval == 0) Satiety = Mathf.Max(0, Satiety - 1);
        }
        private void CompleteTurn()
        {
            SpendTurn();
            foreach (var enemy in Enemies)
            {
                if (Dead) break;
                if (DungeonRules.Distance(enemy.Cell, PlayerCell) == 1)
                {
                    int damage=Mathf.Max(1,enemy.AttackPower-DefensePower);
                    Hp = Mathf.Max(0, Hp - damage);
                    Log(enemy.Name+"の攻撃！ ルナに"+damage+"ダメージ。");
                    if (Dead) Log("ルナは倒れた……。「再挑戦」でやり直せます。");
                }
                else if (EnemyAI.Recognizes(Map, enemy.Cell, PlayerCell))
                    enemy.Cell = EnemyAI.NextStep(Map, enemy, PlayerCell, Enemies);
            }
        }
        private void EnterFloor()
        {
            Map = DungeonGenerator.Generate(seed, Floor); PlayerCell = Map.Start; Facing = Vector2Int.down;
            Items.Clear(); Items.AddRange(ItemManager.CreateItems(Map, seed, Floor,Modifiers.EquipmentEnhancement)); Enemies.Clear();
            foreach(var item in Items)item.Item.Id=RunId+":"+item.Item.Id;
            var candidates = Map.FloorCells();
            DungeonGenerator.Shuffle(candidates, new System.Random(unchecked(seed ^ Floor * 1709)));
            foreach (var cell in candidates)
            {
                if (Enemies.Count >= DungeonRules.EnemyCount) break;
                if (DungeonRules.Distance(cell, PlayerCell) < 5 || cell == Map.Stairs || Items.Exists(item => item.Cell == cell)) continue;
                var type=ContentCatalog.Monsters[(Enemies.Count+(Floor-1)/3)%ContentCatalog.Monsters.Length];
                int depth=(Floor-1)/5;
                Enemies.Add(new EnemyData { Id = Enemies.Count + 1, Cell = cell,Archetype=type.Id,Hp=type.Hp+depth,MaximumHp=type.Hp+depth,AttackPower=type.Attack+depth/2 });
            }
        }
    }
}
