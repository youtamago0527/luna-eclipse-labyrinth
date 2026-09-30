using System.Collections.Generic;

namespace LunaEclipse.Dungeon
{
    /// <summary>Stable IDs shared with the art task. Unavailable artwork must use an explicit fallback.</summary>
    public static class ContentCatalog
    {
        public sealed class Monster
        {
            public readonly string Id, Name;
            public readonly int Hp, Attack;
            public Monster(string id,string name,int hp,int attack){Id=id;Name=name;Hp=hp;Attack=attack;}
            public string SpriteResource => "Art/Monsters/"+Id;
        }
        public sealed class Item
        {
            public readonly string Id, Name, Description;
            public readonly int Healing;
            public readonly bool Equipment;
            public Item(string id,string name,string description,int healing=0,bool equipment=false)
            {Id=id;Name=name;Description=description;Healing=healing;Equipment=equipment;}
            public string SpriteResource => "Art/Items/"+Id;
        }
        // Provisional encounter values, separate from permanent growth. No new AI here.
        public static readonly Monster[] Monsters={
            new Monster("moon_slime","月色スライム",5,1),
            new Monster("moon_bat","月羽コウモリ",4,1),
            new Monster("little_ghost","こゆきゴースト",6,1),
            new Monster("mushroom","星茸ぷにこ",7,1),
            new Monster("mimic","いたずら宝箱",8,2),
            new Monster("stone_golem","こいしゴーレム",10,2)
        };
        public static readonly Dictionary<string,Item> Items=new Dictionary<string,Item>{
            {"herb",new Item("herb","薬草","HP8回復／満腹回復なし",8)},
            {"potion",new Item("potion","月露の回復薬","HP16回復／満腹回復なし",16)},
            {"moon_sword",new Item("moon_sword","月光の剣","装備して攻撃を補助する。",equipment:true)},
            {"moon_shield",new Item("moon_shield","月光の盾","装備して被ダメージを軽減する。",equipment:true)},
            {"return_scroll",new Item("return_scroll","帰還の巻物","帰還に使う試作候補。接続完了までは出現しない。")}
        };
        public static Monster FindMonster(string id)
        {foreach(var monster in Monsters)if(monster.Id==id)return monster;return Monsters[0];}
        public static Item FindItem(string id) => !string.IsNullOrEmpty(id)&&Items.TryGetValue(id,out var item)?item:null;
    }
}
