using System;
using System.Collections.Generic;
namespace LunaEclipse.Progression
{
    public enum RelicEffect { EquipmentQuality, SatietyInterval, StartFloor, Warehouse, Bag, WeaponQuality, ArmorQuality, MissionChance }
    [Serializable] public sealed class RelicDefinition
    {
        public string Id, Name, Description, Rank; public RelicEffect Effect; public int MaxLevel, PerLevel=1; public bool Enabled=true;
        public RelicDefinition(string id,string name,string description,string rank,RelicEffect effect,int max,bool enabled=true){Id=id;Name=name;Description=description;Rank=rank;Effect=effect;MaxLevel=max;Enabled=enabled;}
    }
    [Serializable] public sealed class RelicLevel { public string Id; public int Level; }
    [Serializable] public sealed class RelicReward { public string Id,RelicId,Rank; public int Floor; public bool Claimed; }
    [Serializable] public sealed class RelicRunRecord { public string RunId; public int StartFloor,MaxFloor; public List<RelicReward> Rewards=new List<RelicReward>(); }
    [Serializable] public sealed class RelicProfile
    { public int Version=1,HighestFloor=1; public List<RelicLevel> Levels=new List<RelicLevel>(); public List<RelicRunRecord> Runs=new List<RelicRunRecord>(); }
    public sealed class RelicBonus
    {
        public int EquipmentEnhancement,WeaponEnhancement,ArmorEnhancement,SatietyIntervalBonus,BagBonus,WarehouseBonus,MaxStartFloor=1;
        public float MissionChanceBonus;
        public int BagCapacity=>Math.Min(40,20+BagBonus); public int WarehouseCapacity=>20+WarehouseBonus;
    }
    public sealed class RelicBalance
    {
        // Trial values are centralized, not claims about the reference game.
        public int RewardInterval=20; public bool EnableTrialRewardTable=false;
        public int[] RankFloors={20,40,80,120,200}; public string[] Ranks={"D","C","B","A","S"};
        public int MissionUnlockFloor=20; public float MissionChance=.25f,MaximumMissionChance=.40f;
        public static readonly RelicDefinition[] Definitions={
            new RelicDefinition("blacksmith_memory","月炉の残響","武器・防具の生成強化値にLv分を加える。","D",RelicEffect.EquipmentQuality,999),
            new RelicDefinition("satiety_endurance","静夜の香炉","満腹度が減るまでの行動数を延ばす。","D",RelicEffect.SatietyInterval,0),
            new RelicDefinition("deep_compass","星底の羅針盤","到達済みの開始候補階を増やす。","C",RelicEffect.StartFloor,0),
            new RelicDefinition("warehouse_expansion","月蔵の鍵束","拠点倉庫の容量をLv分増やす。","D",RelicEffect.Warehouse,999),
            new RelicDefinition("bag_expansion","宵縫いの留め具","探索バッグを最大40枠まで広げる。","D",RelicEffect.Bag,20),
            new RelicDefinition("blade_prism","薄月の研晶","試作候補：生成武器の品質を磨く。","B",RelicEffect.WeaponQuality,30,false),
            new RelicDefinition("armor_thread","夜帳の銀糸","試作候補：生成防具の品質を整える。","A",RelicEffect.ArmorQuality,30,false),
            new RelicDefinition("eclipse_clock","月環の振り子","試作候補：帰還後のミッション発生率を補助。","S",RelicEffect.MissionChance,15,false),
        };
    }
}
