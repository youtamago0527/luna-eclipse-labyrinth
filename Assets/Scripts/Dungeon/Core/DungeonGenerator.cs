using System;
using System.Collections.Generic;
using UnityEngine;

namespace LunaEclipse.Dungeon
{
    public static class DungeonGenerator
    {
        public static DungeonMap Generate(int seed, int floor)
        {
            var random = new System.Random(unchecked(seed * 397 ^ floor * 7919));
            var map = new DungeonMap(DungeonRules.Width, DungeonRules.Height);
            var sectors = new List<int>();
            for (int i = 0; i < 9; i++) sectors.Add(i);
            Shuffle(sectors, random);
            // Start in the middle sector: the camera can centre Luna without hitting map bounds.
            int middle = sectors.IndexOf(4); int first = sectors[0]; sectors[0] = 4; sectors[middle] = first;
            int count = random.Next(5, 8);
            for (int i = 0; i < count; i++)
            {
                int col = sectors[i] % 3, row = sectors[i] / 3;
                // Rooms fit inside the nine-tile-wide view, with visible perimeter walls.
                int width = random.Next(5, 8), height = random.Next(5, 9);
                int x = col * 13 + random.Next(1, 13 - width);
                int y = row * 15 + random.Next(1, 15 - height);
                var room = new RectInt(x, y, width, height);
                map.Rooms.Add(room);
                for (int cy = room.yMin; cy < room.yMax; cy++)
                    for (int cx = room.xMin; cx < room.xMax; cx++) map.Carve(new Vector2Int(cx, cy));
                if (i > 0)
                {
                    var source = Center(room);
                    var target = Center(map.Rooms[0]);
                    int nearest = int.MaxValue;
                    for (int j = 0; j < i; j++)
                    {
                        var candidate = Center(map.Rooms[j]);
                        int distance = DungeonRules.Distance(source, candidate);
                        if (distance < nearest) { nearest = distance; target = candidate; }
                    }
                    Connect(map, source, target, random.Next(2) == 0);
                }
            }
            AddDeadEnds(map, random);
            map.Start = Center(map.Rooms[0]);
            var distances = Distances(map, map.Start);
            int farthest = -1;
            foreach (var room in map.Rooms)
            {
                var center = Center(room);
                if (distances[center] > farthest) { farthest = distances[center]; map.Stairs = center; }
            }
            map.Reveal(map.Start);
            return map;
        }
        public static Dictionary<Vector2Int, int> Distances(DungeonMap map, Vector2Int origin)
        {
            var result = new Dictionary<Vector2Int, int>();
            if (!map.Walkable(origin)) return result;
            var queue = new Queue<Vector2Int>(); result[origin] = 0; queue.Enqueue(origin);
            while (queue.Count > 0)
            {
                var cell = queue.Dequeue();
                foreach (var direction in DungeonRules.Directions)
                {
                    var next = cell + direction;
                    if (!map.Walkable(next) || result.ContainsKey(next)) continue;
                    result[next] = result[cell] + 1; queue.Enqueue(next);
                }
            }
            return result;
        }
        internal static void Shuffle<T>(IList<T> items, System.Random random)
        {
            for (int i = items.Count - 1; i > 0; i--)
            { int j = random.Next(i + 1); T value = items[i]; items[i] = items[j]; items[j] = value; }
        }
        private static Vector2Int Center(RectInt room)
            => new Vector2Int(room.x + room.width / 2, room.y + room.height / 2);
        private static void AddDeadEnds(DungeonMap map, System.Random random)
        {
            var origins = map.FloorCells().FindAll(cell => map.RoomIndex(cell) < 0);
            Shuffle(origins, random); int made = 0;
            foreach (var origin in origins)
            {
                var directions = new List<Vector2Int>(DungeonRules.Directions); Shuffle(directions, random);
                foreach (var direction in directions)
                {
                    var branch = new List<Vector2Int>(); var previous = origin;
                    for (int step = 1, length = random.Next(2, 5); step <= length; step++)
                    {
                        var next = origin + direction * step;
                        if (next.x < 2 || next.y < 2 || next.x >= map.Width-2 || next.y >= map.Height-2 || map.Walkable(next)) break;
                        bool separated = true;
                        foreach (var neighbor in DungeonRules.Directions)
                            if (next + neighbor != previous && map.Walkable(next + neighbor)) separated = false;
                        if (!separated) break;
                        branch.Add(next); previous = next;
                    }
                    if (branch.Count < 2) continue;
                    foreach (var cell in branch) map.Carve(cell);
                    made++; break;
                }
                if (made >= 3) return;
            }
        }
        private static void Connect(DungeonMap map, Vector2Int from, Vector2Int to, bool horizontalFirst)
        {
            var cell = from;
            while (cell != to)
            {
                map.Carve(cell);
                if ((horizontalFirst && cell.x != to.x) || cell.y == to.y)
                    cell.x += Math.Sign(to.x - cell.x);
                else cell.y += Math.Sign(to.y - cell.y);
            }
            map.Carve(to);
        }
    }
}
