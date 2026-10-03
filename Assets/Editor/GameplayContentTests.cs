using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LunaEclipse.Dungeon;
using UnityEditor;
using UnityEngine;

namespace LunaEclipse.EditorTools
{
    /// <summary>Independent deterministic checks. No runtime state or scene modifications.</summary>
    public static class GameplayContentTests
    {
        [Serializable] private sealed class Report
        { public bool passed; public string utc; public string[] checks; public string failure; }

        [MenuItem("Luna/Validate Gameplay Content")]
        public static void Run()
        {
            var passed = new List<string>();
            try
            {
                CheckCase("catalog and 120-seed item generation", CatalogAndGeneration, passed);
                CheckCase("equipment, swapping, drop and re-pick", EquipmentAndTransfers, passed);
                CheckCase("fixed single enemy damage and one-turn actions", CombatAndTurns, passed);
                CheckCase("healing, full-HP rejection, no satiety recovery", Healing, passed);
                CheckCase("bag capacity clamps and full-bag rejection", Capacity, passed);
                CheckCase("satiety five-action base and endurance interval", Satiety, passed);
                CheckCase("starting floor and three floor transitions preserve state", FloorContinuation, passed);
                CheckCase("invalid items and unimplemented return scroll", InvalidItems, passed);
                SaveReport(true, passed, null);
                Debug.Log("Gameplay content tests passed: " + string.Join("; ", passed));
            }
            catch (Exception error)
            {
                SaveReport(false, passed, error.ToString());
                throw;
            }
        }

        private static void CheckCase(string name, Action action, List<string> passed)
        {
            try { action(); passed.Add(name); }
            catch (Exception error) { throw new InvalidOperationException(name + ": " + error.Message, error); }
        }

        private static DungeonRun QuietRun(int seed = 31, RunModifiers modifiers = null, int floor = 1)
        {
            var run = new DungeonRun(seed, modifiers, floor);
            run.Enemies.Clear(); return run;
        }

        private static ItemData Item(string id, string kind, int enhancement = 0)
        {
            var definition = ContentCatalog.FindItem(kind);
            return new ItemData { Id = id, Kind = kind, Name = definition?.Name ?? kind, Enhancement = enhancement };
        }

        private static void CatalogAndGeneration()
        {
            Expect(ContentCatalog.Monsters.Length == 6, "six monster archetypes");
            Expect(ContentCatalog.Monsters.Select(monster => monster.Id).Distinct().Count() == 6, "monster IDs unique");
            foreach (var monster in ContentCatalog.Monsters)
                Expect(monster.Hp > 0 && monster.Attack > 0 && !string.IsNullOrEmpty(monster.Name), "monster numeric/name data");
            var allowed = new HashSet<string> { "herb", "potion", "moon_sword", "moon_shield" };
            bool sawNegative = false;
            for (int seed = 0; seed < 120; seed++)
            {
                var map = DungeonGenerator.Generate(seed, 1);
                var basic = ItemManager.CreateItems(map, seed, 1, 0);
                var plusOne = ItemManager.CreateItems(map, seed, 1, 1);
                var plusTen = ItemManager.CreateItems(map, seed, 1, 10);
                Expect(basic.Select(item => item.Item.Id).Distinct().Count() == basic.Count, "IDs unique per generated floor");
                Expect(allowed.SetEquals(basic.Select(item => item.Item.Kind)), "all four playable item types generated");
                Expect(basic.Select(item => item.Cell).Distinct().Count() == basic.Count, "generated pickups do not overlap");
                for (int i = 0; i < basic.Count; i++)
                {
                    var original = basic[i].Item;
                    Expect(map.Walkable(basic[i].Cell), "generated item is on floor");
                    Expect(!string.IsNullOrEmpty(original.Id) && !string.IsNullOrEmpty(original.Name), "item identity and name present");
                    Expect(original.Kind != "return_scroll", "return scroll never generated");
                    if (ContentCatalog.FindItem(original.Kind).Equipment)
                    {
                        Expect(original.Enhancement >= -3 && original.Enhancement <= 3, "unmodified equipment roll within -3..3");
                        Equal(plusOne[i].Item.Enhancement, original.Enhancement + 1, "level one is added to final roll");
                        Equal(plusTen[i].Item.Enhancement, original.Enhancement + 10, "level ten added to final roll");
                        if (original.Enhancement == -3)
                        { sawNegative = true; Equal(plusOne[i].Item.Enhancement, -2, "negative enhancement must not clamp to positive"); }
                    }
                    else
                    { Equal(plusTen[i].Item.Enhancement, original.Enhancement, "blacksmith does not affect consumables"); }
                }
                var next = ItemManager.CreateItems(DungeonGenerator.Generate(seed, 2), seed, 2);
                Expect(!basic.Select(item => item.Item.Id).Intersect(next.Select(item => item.Item.Id)).Any(), "IDs differ between floors");
            }
            Expect(sawNegative, "test seeds actually exercised a -3 roll");
            var plain = new DungeonRun(6);
            var modified = new DungeonRun(6, new RunModifiers { EquipmentEnhancement = 7 });
            for (int i = 0; i < plain.Items.Count; i++)
                if (ContentCatalog.FindItem(plain.Items[i].Item.Kind).Equipment)
                    Equal(modified.Items[i].Item.Enhancement, plain.Items[i].Item.Enhancement + 7, "run modifier reaches floor generation");
            Expect(!string.IsNullOrEmpty(plain.RunId) && plain.RunId != modified.RunId, "each expedition has distinct RunId");
            Expect(!plain.Items.Select(item => item.Item.Id).Intersect(modified.Items.Select(item => item.Item.Id)).Any(), "item IDs do not collide between expeditions with identical seeds");
        }

        private static void EquipmentAndTransfers()
        {
            var run = QuietRun(); run.Items.Clear();
            var sword = Item("test-sword", "moon_sword", 2);
            var alternate = Item("test-negative-sword", "moon_sword", -3);
            var shield = Item("test-shield", "moon_shield", 2);
            run.Bag.AddRange(new[] { sword, alternate, shield });
            AcceptedOnce(run, () => run.UseItem(sword.Id), "equip sword");
            Equal(run.AttackPower, 5, "+2 sword adds three attack to base two");
            Expect(run.WeaponId == sword.Id && run.Bag.Count == 3, "equipping retains item in bag");
            RejectedWithoutTurn(run, () => run.UseItem(sword.Id), "already-equipped item");
            AcceptedOnce(run, () => run.UseItem(alternate.Id), "swap sword");
            Equal(run.AttackPower, 1, "negative sword reduces power while floor remains one");
            Equal(alternate.Enhancement, -3, "equipping preserves negative roll");
            Expect(run.WeaponId == alternate.Id && run.Bag.Contains(sword), "replacement retains old sword");
            AcceptedOnce(run, () => run.UseItem(shield.Id), "equip shield");
            Equal(run.DefensePower, 3, "+2 shield gives three defense");
            RejectedWithoutTurn(run, () => run.DropItem(alternate.Id), "cursed weapon cannot drop");
            RejectedWithoutTurn(run, () => run.UseItem(sword.Id), "cursed weapon cannot swap");
            Expect(run.WeaponId==alternate.Id&&alternate.Identified,"curse stays equipped and identifies");
            AcceptedOnce(run, () => run.DropItem(sword.Id), "drop spare sword");
            Expect(run.Items.Count(item => item.Item.Id == sword.Id && item.Cell == run.PlayerCell) == 1, "same item placed once");
            AcceptedOnce(run, run.PickUp, "re-pick dropped sword");
            Expect(run.Bag.Contains(sword) && run.WeaponId == alternate.Id, "re-pick preserves identity and curse");
            AcceptedOnce(run, () => run.DropItem(shield.Id), "drop equipped shield");
            Expect(run.ShieldId == null && run.DefensePower == 0, "dropping unequips shield");
        }

        private static void CombatAndTurns()
        {
            var run = QuietRun();
            var sword = Item("combat-sword", "moon_sword", 2);
            var shield = Item("combat-shield", "moon_shield", 2);
            run.Bag.Add(sword); run.Bag.Add(shield);
            run.UseItem(sword.Id); run.UseItem(shield.Id);
            var target = new EnemyData { Id = 900, Cell = run.PlayerCell + Vector2Int.right,
                Hp = 10, MaximumHp = 10, AttackPower = 5 };
            run.Enemies.Clear(); run.Enemies.Add(target);
            int hp = run.Hp; var cell = run.PlayerCell;
            AcceptedOnce(run, () => run.Move(Vector2Int.right), "bump attack");
            Equal(target.Hp, 5, "attack damage exactly five");
            Equal(run.Hp, hp - 2, "one retaliation: five attack minus three defense");
            Expect(run.PlayerCell == cell, "bump attack cannot move onto enemy");
            hp = run.Hp;
            AcceptedOnce(run, run.Attack, "lethal attack");
            Expect(run.Enemies.Count == 0 && run.Hp == hp, "defeated enemy never retaliates");
            AcceptedOnce(run, run.Wait, "wait");
            AcceptedOnce(run, () => run.Move(Vector2Int.up), "empty movement");
        }

        private static void Healing()
        {
            var run = QuietRun();
            var herb = Item("healing-herb", "herb"); var potion = Item("healing-potion", "potion");
            run.Bag.Add(herb); run.Bag.Add(potion);
            RejectedWithoutTurn(run, () => run.UseItem(herb.Id), "full-HP herb");
            Expect(run.Bag.Contains(herb), "full HP does not consume herb");
            for (int i = 0; i < 4; i++) run.Wait();
            run.Enemies.Add(new EnemyData { Id = 901, Cell = run.PlayerCell + Vector2Int.right,
                Hp = 20, MaximumHp = 20, AttackPower = 10 });
            run.Wait(); run.Enemies.Clear();
            Equal(run.Hp, 11, "ten damage then one natural recovery"); Equal(run.Satiety, 99, "fifth action consumes satiety");
            int satiety = run.Satiety;
            AcceptedOnce(run, () => run.UseItem(herb.Id), "herb use");
            Equal(run.Hp, 19, "herb restores exactly eight HP");
            Expect(!run.Bag.Contains(herb) && run.Satiety == satiety, "herb consumed without satiety recovery");
            AcceptedOnce(run, () => run.UseItem(potion.Id), "potion use");
            Equal(run.Hp, 20, "potion capped at max HP");
            Expect(run.Satiety <= satiety && !run.Bag.Contains(potion), "potion cannot recover satiety");
            var unused = Item("unused-potion", "potion"); run.Bag.Add(unused);
            RejectedWithoutTurn(run, () => run.UseItem(unused.Id), "full-HP potion");
            Expect(run.Bag.Contains(unused), "full-HP potion is retained");
        }

        private static void Capacity()
        {
            foreach (var requested in new[] { 0, 20, 30, 40, 99 })
            {
                var run = QuietRun(modifiers: new RunModifiers { BagCapacity = requested }); run.Items.Clear();
                int expected = requested < 20 ? 20 : requested > 40 ? 40 : requested;
                Equal(run.BagCapacity, expected, "capacity clamp");
                for (int i = 0; i < expected; i++)
                {
                    run.Items.Add(new FloorItem { Cell = run.PlayerCell, Item = Item("capacity-" + i, "herb") });
                    AcceptedOnce(run, run.PickUp, "fill bag");
                }
                var excess = new FloorItem { Cell = run.PlayerCell, Item = Item("overflow", "herb") };
                run.Items.Add(excess);
                Expect(!run.CanPickUp, "full bag disables pickup");
                RejectedWithoutTurn(run, run.PickUp, "overflow pickup");
                Expect(run.Bag.Count == expected && run.Items.Contains(excess), "overflow remains on floor");
            }
        }

        private static void Satiety()
        {
            var standard = QuietRun(); Equal(standard.SatietyInterval, 5, "base five actions");
            for (int i = 0; i < 4; i++) standard.Wait();
            Equal(standard.Satiety, 100, "first four actions free of depletion");
            standard.Wait(); Equal(standard.Satiety, 99, "fifth action decreases one");
            var endurance = QuietRun(modifiers: new RunModifiers { SatietyIntervalBonus = 2 });
            Equal(endurance.SatietyInterval, 7, "endurance adds interval");
            for (int i = 0; i < 6; i++) endurance.Wait();
            Equal(endurance.Satiety, 100, "endurance first six actions");
            endurance.Wait(); Equal(endurance.Satiety, 99, "seventh action decreases one");
            for (int i = 0; i < 7; i++) endurance.Wait();
            Equal(endurance.Satiety, 98, "next seven actions decreases one");
            for (int i = 0; i < 600; i++) standard.Wait();
            Equal(standard.Satiety, 0, "satiety stops at zero");
        }

        private static void FloorContinuation()
        {
            var run = QuietRun(44, new RunModifiers { BagCapacity = 30, SatietyIntervalBonus = 2, EquipmentEnhancement = 8 }, 50);
            Equal(run.StartFloor, 50, "chosen starting floor"); Equal(run.Floor, 50, "current starting floor");
            Equal(run.Satiety, 100, "skipped start has full satiety");
            var sword = Item("carried-sword", "moon_sword", 4); var shield = Item("carried-shield", "moon_shield", 3);
            run.Bag.AddRange(new[] { sword, shield, Item("carried-herb", "herb") });
            run.UseItem(sword.Id); run.UseItem(shield.Id);
            string runId = run.RunId;
            for (int floor = 50; floor < 53; floor++)
            {
                run.Enemies.Clear(); WalkTo(run, run.Map.Stairs);
                int hp = run.Hp, satiety = run.Satiety, turns = run.Turns;
                var ids = run.Bag.Select(item => item.Id).ToArray();
                AcceptedOnce(run, run.Descend, "descend");
                Equal(run.Floor, floor + 1, "next floor"); Equal(run.Hp, hp, "HP preserved on stairs");
                Equal(run.Satiety, satiety - ((turns + 1) % run.SatietyInterval == 0 ? 1 : 0), "satiety clock continues over stairs");
                Expect(run.Bag.Select(item => item.Id).SequenceEqual(ids), "bag unchanged across stairs");
                Expect(run.WeaponId == sword.Id && run.ShieldId == shield.Id, "equipped identities persist");
                Expect(run.AttackPower == 7 && run.DefensePower == 4, "equipment effects persist");
                Expect(run.RunId == runId && run.StartFloor == 50 && run.BagCapacity == 30, "expedition metadata persists");
                run.Enemies.Clear(); AcceptedOnce(run, () => run.Move(Vector2Int.up), "movement on next floor");
            }
        }

        private static void InvalidItems()
        {
            var run = QuietRun();
            Expect(ContentCatalog.FindItem(null)==null&&ContentCatalog.FindItem("")==null,"missing catalog kind is safe");
            var unknown=new ItemData{Id="unknown-kind",Name="種類不明",Kind=null};run.Bag.Add(unknown);
            RejectedWithoutTurn(run,()=>run.UseItem(unknown.Id),"missing kind preserves item and turn");
            Expect(run.Bag.Contains(unknown),"missing kind does not consume item");
            RejectedWithoutTurn(run, () => run.UseItem("missing-id"), "missing item use");
            RejectedWithoutTurn(run, () => run.DropItem("missing-id"), "missing item drop");
            var scroll = Item("not-generated-scroll", "return_scroll"); run.Bag.Add(scroll);
            RejectedWithoutTurn(run, () => run.UseItem(scroll.Id), "unimplemented return scroll");
            Expect(run.Bag.Contains(scroll), "unimplemented item is not consumed");
            var extremeSword=Item("extreme-sword","moon_sword",int.MaxValue);
            var extremeShield=Item("extreme-shield","moon_shield",int.MaxValue);
            run.Bag.Add(extremeSword);run.Bag.Add(extremeShield);
            run.UseItem(extremeSword.Id);run.UseItem(extremeShield.Id);
            Equal(run.AttackPower,int.MaxValue,"extreme equipment does not wrap attack");
            Equal(run.DefensePower,int.MaxValue,"extreme equipment does not wrap defense");
            extremeSword.Enhancement=int.MinValue;extremeShield.Enhancement=int.MinValue;
            Equal(run.AttackPower,1,"extreme negative weapon respects minimum");
            Equal(run.DefensePower,0,"extreme negative shield respects minimum");
            var extremeInterval=new DungeonRun(1,new RunModifiers{SatietyIntervalBonus=int.MaxValue});
            Equal(extremeInterval.SatietyInterval,int.MaxValue,"extreme interval does not wrap");
            for (int floor = 1; floor <= 100; floor++)
            {
                var items = ItemManager.CreateItems(DungeonGenerator.Generate(121, floor), 121, floor);
                Expect(items.All(item => item.Item.Kind != "return_scroll"), "no return scroll through 100 floors");
            }
        }

        private static void AcceptedOnce(DungeonRun run, Func<bool> action, string context)
        { int turn = run.Turns; Expect(action(), context + " accepted"); Equal(run.Turns, turn + 1, context + " advances exactly one turn"); }
        private static void RejectedWithoutTurn(DungeonRun run, Func<bool> action, string context)
        { int turn = run.Turns; Expect(!action(), context + " rejected"); Equal(run.Turns, turn, context + " has no turn cost"); }
        private static void Equal(int actual, int expected, string context)
        { Expect(actual == expected, context + " expected " + expected + ", got " + actual); }
        private static void Expect(bool value, string context)
        { if (!value) throw new InvalidOperationException(context); }
        private static void WalkTo(DungeonRun run, Vector2Int destination)
        {
            var parents = new Dictionary<Vector2Int, Vector2Int> { { run.PlayerCell, run.PlayerCell } };
            var queue = new Queue<Vector2Int>(); queue.Enqueue(run.PlayerCell);
            while (queue.Count > 0 && !parents.ContainsKey(destination))
            {
                var cell = queue.Dequeue();
                foreach (var direction in DungeonRules.Directions)
                {
                    var next = cell + direction;
                    if (!run.Map.Walkable(next) || parents.ContainsKey(next)) continue;
                    parents[next] = cell; queue.Enqueue(next);
                }
            }
            Expect(parents.ContainsKey(destination), "path to stair exists");
            var path = new Stack<Vector2Int>();
            for (var cell = destination; cell != run.PlayerCell; cell = parents[cell]) path.Push(cell);
            while (path.Count > 0)
            { var next = path.Pop(); AcceptedOnce(run, () => run.Move(next - run.PlayerCell), "path movement"); }
        }
        private static void SaveReport(bool passed, List<string> checks, string failure)
        {
            Directory.CreateDirectory("Library");
            File.WriteAllText("Library/GameplayContentTests.json", JsonUtility.ToJson(new Report
            { passed = passed, utc = DateTime.UtcNow.ToString("O"), checks = checks.ToArray(), failure = failure }, true));
        }
    }
}
