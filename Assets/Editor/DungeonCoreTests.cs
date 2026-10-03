using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LunaEclipse.Dungeon;
using UnityEditor;
using UnityEngine;

namespace LunaEclipse.EditorTools
{
    public static class DungeonCoreTests
    {
        [Serializable] private sealed class Report
        { public bool passed; public string[] checks; public string failure; }
        [MenuItem("Luna/Validate Dungeon Core")]
        public static void Run()
        {
            var checks = new List<string>();
            try
            {
                for (int seed = 0; seed < 120; seed++)
                {
                    var run = new DungeonRun(seed);
                    var distances = DungeonGenerator.Distances(run.Map, run.PlayerCell);
                    Check(run.Map.Width == 39 && run.Map.Height == 45, "Map dimensions");
                    Check(distances.Count == run.Map.FloorCells().Count, "Every floor reachable at seed " + seed);
                    Check(distances.ContainsKey(run.Map.Stairs), "Reachable stair");
                    Check(run.Map.Stairs != run.PlayerCell, "Stair separate from entry");
                    Check(run.Items.Any(item => DungeonRules.Distance(item.Cell, run.PlayerCell) == 1), "Entry herb");
                    foreach (var enemy in run.Enemies) Check(DungeonRules.Distance(enemy.Cell, run.PlayerCell) >= 5, "Safe enemy spawn");
                    ValidatePositions(run);
                    Check(!run.Map.Explored[run.Map.Stairs.x, run.Map.Stairs.y], "Remote stair initially hidden");
                    Check(run.Map.Rooms.All(room=>room.width>=5&&room.width<=7&&room.height>=5&&room.height<=8),"Compact distinct rooms");
                    Check(run.Map.FloorCells().Any(cell=>run.Map.RoomIndex(cell)<0&&DungeonRules.Directions.Count(d=>run.Map.Walkable(cell+d))==1),"Reachable dead-end passage");
                    int exploredBefore=run.Map.FloorCells().Count(cell=>run.Map.Explored[cell.x,cell.y]);
                    Check(exploredBefore<run.Map.FloorCells().Count/2,"Only current room/local passage revealed");
                    var start=run.PlayerCell;run.Map.Reveal(run.Map.Stairs);
                    Check(run.Map.Explored[start.x,start.y]&&!run.Map.Visible[start.x,start.y],"Fog retains explored room without current visibility");
                }
                checks.Add("120 seeds: compact rooms, connected narrow passages/dead ends, safe spawns, hidden distant rooms, persistent explored fog");

                var walking = new DungeonRun(123){AmbientEncounters=false}; walking.Enemies.Clear();
                for (int i = 0; i < 100; i++)
                {
                    var direction = DungeonRules.Directions.First(dir => walking.Map.Walkable(walking.PlayerCell + dir));
                    var before = walking.PlayerCell;
                    Check(walking.Move(direction), "Accepted move");
                    Check(walking.PlayerCell == before + direction, "Exactly one grid move");
                    ValidatePositions(walking);
                }
                Check(walking.Turns == 100 && walking.Satiety == 80, "100 turns and satiety interval");
                var edge = walking.Map.FloorCells().First(cell => DungeonRules.Directions.Any(dir => !walking.Map.Walkable(cell + dir)));
                WalkTo(walking, edge);
                var wall = DungeonRules.Directions.First(dir => !walking.Map.Walkable(edge + dir));
                int turn = walking.Turns; var originalCell = walking.PlayerCell;
                Check(!walking.Move(wall), "Wall rejects movement");
                Check(walking.Turns == turn && walking.PlayerCell == originalCell && walking.Facing == wall, "Wall changes facing only");
                Check(!walking.Move(new Vector2Int(2, 1)), "Non-unit movement rejected");
                checks.Add("100 accepted movements, exact cell increments, wall facing/no turn, 5-turn satiety");

                var battle = new DungeonRun(14); battle.Enemies.Clear();
                var adjacent = battle.PlayerCell + Vector2Int.right;
                battle.Enemies.Add(new EnemyData { Id = 1, Cell = adjacent });
                var origin = battle.PlayerCell;
                Check(battle.Move(Vector2Int.right), "Bump attack");
                Check(battle.Enemies[0].Hp == 3 && battle.Hp == 19, "Player attack 2 then enemy attack 1");
                Check(battle.PlayerCell == origin && battle.LastAttackCell == adjacent, "Attack remains on origin");
                battle.Attack(); battle.Attack();
                Check(battle.Enemies.Count == 0 && battle.Hp == 18, "Slain enemy cannot retaliate");
                battle.Enemies.Add(new EnemyData { Id = 2, Cell = adjacent });
                while (!battle.Dead) battle.Wait();
                turn = battle.Turns;
                Check(!battle.Attack() && !battle.Move(Vector2Int.up) && !battle.Wait() && !battle.PickUp() && !battle.Descend(), "Death disables actions");
                Check(battle.Turns == turn && battle.Hp == 0, "Death cannot advance simulation");
                checks.Add("Fixed damage, bump attack, single enemy response, lethal removal, dead-input guard");

                var crowd = new DungeonRun(71); crowd.Enemies.Clear();
                crowd.Enemies.Add(new EnemyData { Id = 1, Cell = crowd.PlayerCell + Vector2Int.right * 2 });
                crowd.Enemies.Add(new EnemyData { Id = 2, Cell = crowd.PlayerCell + Vector2Int.up * 2 });
                for (int i = 0; i < 100; i++) { crowd.Wait(); ValidatePositions(crowd); }
                checks.Add("Pursuit never stacks enemies or overlaps player through 100 action requests");

                var exploration = new DungeonRun(92){AmbientEncounters=false}; exploration.Enemies.Clear();
                var herb = exploration.Items.First(item => DungeonRules.Distance(item.Cell, exploration.PlayerCell) == 1);
                exploration.Move(herb.Cell - exploration.PlayerCell);
                Check(exploration.Bag.Count == 1 && !exploration.CanPickUp, "Walking automatically collects item");
                Check(exploration.Bag[0].Id == herb.Item.Id, "Auto pickup preserves identity");
                string carriedId = exploration.Bag[0].Id;
                for (int floor = 1; floor <= 5; floor++)
                {
                    exploration.Enemies.Clear(); WalkTo(exploration, exploration.Map.Stairs);
                    int hp = exploration.Hp, satiety = exploration.Satiety, turns = exploration.Turns;
                    Check(exploration.CanDescend && exploration.Descend(), "Descend accepted");
                    Check(exploration.Floor == floor + 1 && exploration.Hp == hp && exploration.Bag[0].Id == carriedId, "Floor preserves HP/inventory");
                    Check(exploration.Turns == turns + 1 && exploration.Satiety <= satiety, "Floor preserves satiety clock");
                    ValidatePositions(exploration);
                    exploration.Enemies.Clear();
                    Check(exploration.Move(Vector2Int.up), "Can move immediately next floor");
                }
                while (exploration.Bag.Count < DungeonRules.BagCapacity) exploration.Bag.Add(new ItemData { Id = "test-capacity-" + exploration.Bag.Count });
                exploration.Items.Add(new FloorItem { Cell = exploration.PlayerCell, Item = new ItemData { Id = "overflow" } });
                turn = exploration.Turns;
                Check(!exploration.PickUp() && exploration.Bag.Count == 20 && exploration.Turns == turn, "Bag cap20 no lost item/action");
                while (exploration.Turns < 1100) exploration.Wait();
                Check(exploration.Satiety == 0 && exploration.Hp == 20, "Satiety zero has no unrequested starvation penalty");
                checks.Add("Auto pickup, bag20 cap, five consecutive floors preserve HP/bag/satiety and stay playable");
                checks.Add("Satiety clamps at zero without adding starvation rules");
                SaveReport(true, checks, null);
                Debug.Log("Dungeon core checks passed: " + string.Join("; ", checks));
            }
            catch (Exception error) { SaveReport(false, checks, error.ToString()); throw; }
        }
        private static void ValidatePositions(DungeonRun run)
        {
            Check(run.Map.Walkable(run.PlayerCell), "Player on floor");
            var occupied = new HashSet<Vector2Int> { run.PlayerCell };
            foreach (var enemy in run.Enemies)
                Check(run.Map.Walkable(enemy.Cell) && occupied.Add(enemy.Cell), "Enemy floor/unique occupancy");
            foreach (var item in run.Items) Check(run.Map.Walkable(item.Cell), "Item on floor");
        }
        private static void WalkTo(DungeonRun run, Vector2Int target)
        {
            var parent = new Dictionary<Vector2Int, Vector2Int>();
            var queue = new Queue<Vector2Int>(); queue.Enqueue(run.PlayerCell); parent[run.PlayerCell] = run.PlayerCell;
            while (queue.Count > 0 && !parent.ContainsKey(target))
            {
                var cell = queue.Dequeue();
                foreach (var dir in DungeonRules.Directions)
                { var next = cell + dir; if (run.Map.Walkable(next) && !parent.ContainsKey(next)) { parent[next] = cell; queue.Enqueue(next); } }
            }
            Check(parent.ContainsKey(target), "Route exists");
            var path = new Stack<Vector2Int>();
            for (var cell = target; cell != run.PlayerCell; cell = parent[cell]) path.Push(cell);
            while (path.Count > 0) Check(run.Move(path.Pop() - run.PlayerCell), "Route step accepted");
        }
        private static void Check(bool condition, string message)
        { if (!condition) throw new InvalidOperationException("Dungeon test failed: " + message); }
        private static void SaveReport(bool passed, List<string> checks, string failure)
        {
            Directory.CreateDirectory("Library");
            File.WriteAllText("Library/DungeonCoreTests.json", JsonUtility.ToJson(new Report { passed = passed, checks = checks.ToArray(), failure = failure }, true));
        }
    }
}
