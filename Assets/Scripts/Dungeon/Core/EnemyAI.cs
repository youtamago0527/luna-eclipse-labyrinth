using System.Collections.Generic;
using UnityEngine;

namespace LunaEclipse.Dungeon
{
    public static class EnemyAI
    {
        public static bool Recognizes(DungeonMap map, Vector2Int enemy, Vector2Int player)
        {
            int room = map.RoomIndex(enemy);
            return DungeonRules.Distance(enemy, player) <= DungeonRules.AwarenessRadius
                || (room >= 0 && room == map.RoomIndex(player));
        }
        public static Vector2Int NextStep(DungeonMap map, EnemyData enemy,
            Vector2Int player, IList<EnemyData> enemies)
        {
            var occupied = new HashSet<Vector2Int>();
            foreach (var other in enemies) if (other.Id != enemy.Id && other.Hp > 0) occupied.Add(other.Cell);
            var firstStep = new Dictionary<Vector2Int, Vector2Int>();
            var queue = new Queue<Vector2Int>();
            firstStep[enemy.Cell] = enemy.Cell; queue.Enqueue(enemy.Cell);
            while (queue.Count > 0)
            {
                var cell = queue.Dequeue();
                foreach (var direction in DungeonRules.MovementDirections)
                {
                    var next = cell + direction;
                    if (!DungeonRules.CanStep(map,cell,next) || occupied.Contains(next) || firstStep.ContainsKey(next)) continue;
                    var first = cell == enemy.Cell ? next : firstStep[cell];
                    if (next == player) return first == player ? enemy.Cell : first;
                    firstStep[next] = first; queue.Enqueue(next);
                }
            }
            return enemy.Cell;
        }
    }
}
