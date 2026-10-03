using UnityEngine;

namespace LunaEclipse.Dungeon
{
    // Central prototype values, ready to replace with a configuration asset later.
    public static class DungeonRules
    {
        public const int Width = 39, Height = 45;
        public const int PlayerHp = 20, PlayerAttack = 2;
        public const int EnemyHp = 5, EnemyAttack = 1, EnemyCount = 3;
        public const int BagCapacity = 20, MaximumSatiety = 100, SatietyTurnInterval = 5;
        public const int AwarenessRadius = 7, LanternRadius = 4;
        public const int NegativeEquipmentPercent = 5, ItemsPerPage = 10;
        public const float HoldDelay = .28f, HoldRepeat = .16f;
        public static readonly Vector2Int[] Directions =
        { Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left };
        public static int Distance(Vector2Int a, Vector2Int b)
            => Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
        public static bool IsCardinal(Vector2Int direction)
            => Mathf.Abs(direction.x) + Mathf.Abs(direction.y) == 1;
    }

    public sealed class EnemyData
    {
        public int Id;
        public Vector2Int Cell;
        public int Hp = DungeonRules.EnemyHp, MaximumHp = DungeonRules.EnemyHp, AttackPower = DungeonRules.EnemyAttack;
        public string Archetype = "moon_slime";
        public int MaxHp => MaximumHp;
        public string Name => ContentCatalog.FindMonster(Archetype).Name;
    }

    [System.Serializable]
    public sealed class ItemData
    {
        public string Id;
        public string Name = "薬草";
        public string Kind = "herb";
        public int Enhancement;
        public bool Identified;
        public bool Equipment => Kind=="moon_sword"||Kind=="moon_shield";
        public bool Cursed => Equipment && Enhancement<0;
        public string DisplayName => Name + (Equipment ? (!Identified ? " [?]" : (Enhancement>=0?" +":" ")+Enhancement+(Cursed?" 呪":"")) : "");
    }

    public sealed class RunModifiers
    {
        public int EquipmentEnhancement, SatietyIntervalBonus;
        public int BagCapacity = DungeonRules.BagCapacity;
    }

    public sealed class FloorItem
    {
        public Vector2Int Cell;
        public ItemData Item;
    }
}
