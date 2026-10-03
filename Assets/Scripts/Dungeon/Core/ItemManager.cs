using System.Collections.Generic;
using UnityEngine;

namespace LunaEclipse.Dungeon
{
    public static class ItemManager
    {
        public static List<FloorItem> CreateItems(DungeonMap map, int seed, int floor, int equipmentBonus=0)
        {
            var result = new List<FloorItem>();
            var cells = map.FloorCells();
            DungeonGenerator.Shuffle(cells, new System.Random(unchecked(seed + floor * 131)));
            // A visible herb beside the entry makes the pickup loop immediately discoverable.
            foreach (var direction in DungeonRules.Directions)
            {
                var cell = map.Start + direction;
                if (!map.Walkable(cell) || cell == map.Stairs) continue;
                result.Add(Create(cell, seed, floor, 0, equipmentBonus)); break;
            }
            foreach (var cell in cells)
            {
                if (result.Count >= 8) break;
                if (cell == map.Start || cell == map.Stairs || result.Exists(item => item.Cell == cell)) continue;
                result.Add(Create(cell, seed, floor, result.Count, equipmentBonus));
            }
            return result;
        }
        private static FloorItem Create(Vector2Int cell, int seed, int floor, int index, int bonus)
        {
            string[] kinds={"herb","moon_sword","moon_shield","potion","herb","potion","moon_sword","herb"};
            string kind=kinds[index%kinds.Length];var definition=ContentCatalog.FindItem(kind);
            var random=new System.Random(unchecked(seed^floor*733^index*227));
            int rolled=random.Next(100)<DungeonRules.NegativeEquipmentPercent?-random.Next(1,4):random.Next(0,4);
            return new FloorItem{Cell=cell,Item=new ItemData{Id=floor+":"+kind+":"+index,Kind=kind,Name=definition.Name,
                Enhancement=definition.Equipment?rolled+bonus:0}};
        }
    }
}
