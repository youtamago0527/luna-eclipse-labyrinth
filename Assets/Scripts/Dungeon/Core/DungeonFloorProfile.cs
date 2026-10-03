using UnityEngine;
namespace LunaEclipse.Dungeon
{
    // Layout and visual identity only. Never changes collision rules, tile size or encounter balance.
    public sealed class DungeonFloorProfile
    {
        public readonly string Name;
        public readonly int MinRooms,MaxRooms,MinWidth,MaxWidth,MinHeight,MaxHeight,ExtraLinks,DeadEnds,CrackEvery;
        public readonly Color Tint;
        DungeonFloorProfile(string name,int minRooms,int maxRooms,int minWidth,int maxWidth,int minHeight,int maxHeight,
            int links,int deadEnds,int crackEvery,Color tint)
        {Name=name;MinRooms=minRooms;MaxRooms=maxRooms;MinWidth=minWidth;MaxWidth=maxWidth;MinHeight=minHeight;MaxHeight=maxHeight;
            ExtraLinks=links;DeadEnds=deadEnds;CrackEvery=crackEvery;Tint=tint;}
        static readonly DungeonFloorProfile Ruins=new DungeonFloorProfile("月影の遺跡",5,7,5,7,5,8,0,3,29,Color.white);
        static readonly DungeonFloorProfile Corridors=new DungeonFloorProfile("蒼月の回廊",7,9,4,6,4,7,2,4,23,new Color(.88f,1.04f,1.10f));
        static readonly DungeonFloorProfile Halls=new DungeonFloorProfile("紫晶の広間",5,7,7,10,7,10,1,3,13,new Color(1.06f,.91f,1.15f));
        static readonly DungeonFloorProfile Cells=new DungeonFloorProfile("月鎖の獄",8,9,4,6,4,6,3,5,17,new Color(.85f,1.02f,1.03f));
        static readonly DungeonFloorProfile Depths=new DungeonFloorProfile("月蝕の深淵",6,9,4,10,4,11,3,6,9,new Color(1.10f,.86f,1.04f));
        public static DungeonFloorProfile ForFloor(int floor)=>floor<5?Ruins:floor<10?Corridors:floor<15?Halls:floor<20?Cells:Depths;
    }
}
