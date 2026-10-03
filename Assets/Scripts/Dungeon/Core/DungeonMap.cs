using System.Collections.Generic;
using UnityEngine;

namespace LunaEclipse.Dungeon
{
    public sealed class DungeonMap
    {
        public int Width { get; }
        public int Height { get; }
        public DungeonFloorProfile Profile {get;internal set;} = DungeonFloorProfile.ForFloor(1);
        public Vector2Int Start { get; internal set; }
        public Vector2Int Stairs { get; internal set; }
        public List<RectInt> Rooms { get; } = new List<RectInt>();
        public bool[,] Explored { get; }
        public bool[,] Visible { get; }
        private readonly bool[,] floor;

        public DungeonMap(int width, int height)
        {
            Width = width; Height = height;
            floor = new bool[width, height];
            Explored = new bool[width, height]; Visible = new bool[width, height];
        }
        public bool InBounds(Vector2Int cell)
            => cell.x >= 0 && cell.y >= 0 && cell.x < Width && cell.y < Height;
        public bool Walkable(Vector2Int cell) => InBounds(cell) && floor[cell.x, cell.y];
        public bool IsStairs(Vector2Int cell) => cell == Stairs;
        internal void Carve(Vector2Int cell) { if (InBounds(cell)) floor[cell.x, cell.y] = true; }
        public int RoomIndex(Vector2Int cell)
        {
            for (int i = 0; i < Rooms.Count; i++) if (Rooms[i].Contains(cell)) return i;
            return -1;
        }
        public List<Vector2Int> FloorCells()
        {
            var cells = new List<Vector2Int>();
            for (int y = 0; y < Height; y++) for (int x = 0; x < Width; x++)
                if (floor[x, y]) cells.Add(new Vector2Int(x, y));
            return cells;
        }
        public void Reveal(Vector2Int origin)
        {
            int room = RoomIndex(origin);
            var exits = new List<Vector2Int>();
            if(room>=0)
            {
                var area=Rooms[room];
                for(int x=area.xMin;x<area.xMax;x++)
                { AddExit(new Vector2Int(x,area.yMin-1)); AddExit(new Vector2Int(x,area.yMax)); }
                for(int y=area.yMin;y<area.yMax;y++)
                { AddExit(new Vector2Int(area.xMin-1,y)); AddExit(new Vector2Int(area.xMax,y)); }
            }
            void AddExit(Vector2Int cell) { if(Walkable(cell)&&RoomIndex(cell)<0)exits.Add(cell); }
            for (int y = 0; y < Height; y++) for (int x = 0; x < Width; x++)
            {
                var cell = new Vector2Int(x, y);
                bool roomVisible = room >= 0 && new RectInt(Rooms[room].x - 1, Rooms[room].y - 1,
                    Rooms[room].width + 2, Rooms[room].height + 2).Contains(cell);
                bool visible = roomVisible || (DungeonRules.Distance(cell, origin) <= DungeonRules.LanternRadius
                    && HasSight(origin, cell));
                // Show a short, wall-occluded glimpse at each room exit. Distant rooms stay hidden.
                if(!visible && RoomIndex(cell)<0)
                    foreach(var exit in exits)
                        if(DungeonRules.Distance(exit,cell)<=2&&HasSight(exit,cell)){visible=true;break;}
                Visible[x, y] = visible;
                Explored[x, y] |= visible;
            }
        }
        // Integer line-of-sight reveals the first blocking wall, not rooms behind it.
        private bool HasSight(Vector2Int from, Vector2Int to)
        {
            int x = from.x, y = from.y;
            int dx = Mathf.Abs(to.x - x), dy = Mathf.Abs(to.y - y);
            int sx = x < to.x ? 1 : -1, sy = y < to.y ? 1 : -1, error = dx - dy;
            while (x != to.x || y != to.y)
            {
                int twice = error * 2;
                if (twice > -dy) { error -= dy; x += sx; }
                if (twice < dx) { error += dx; y += sy; }
                if (x == to.x && y == to.y) return true;
                if (!Walkable(new Vector2Int(x, y))) return false;
            }
            return true;
        }
    }
}
