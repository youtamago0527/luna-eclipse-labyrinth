using System;
using System.Collections.Generic;
using UnityEngine;
namespace LunaEclipse.Dungeon
{
    // Original Luna balance. Room distribution and reinforcements are deterministic per expedition.
    public sealed class EncounterDirector
    {
        readonly System.Random random;
        int nextId=1;
        public EncounterDirector(int seed,int floor){random=new System.Random(unchecked(seed*193 ^ floor*1709));}
        public static int MinimumCount(int floor)=>6+Mathf.Min(6,(Mathf.Max(1,floor)-1)/2);
        public static int MaximumAlive(int floor)=>MinimumCount(floor)+6;
        public static int SpawnInterval(int floor)=>Mathf.Max(32,64-((Mathf.Max(1,floor)-1)/5)*8);
        bool Free(DungeonRun run,Vector2Int cell)=>run.Map.Walkable(cell)&&cell!=run.PlayerCell&&cell!=run.Map.Stairs
            &&!run.Items.Exists(i=>i.Cell==cell)&&!run.Enemies.Exists(e=>e.Cell==cell&&e.Hp>0);
        EnemyData Create(int floor,Vector2Int cell)
        {
            // Floor bands avoid high-tier monsters on B1F and stop deep floors cycling back to weak packs.
            int[][] weights={new[]{60,30,10,0,0,0},new[]{25,30,25,20,0,0},new[]{10,20,25,25,15,5},new[]{5,10,20,25,20,20}};
            int band=floor<3?0:floor<6?1:floor<10?2:3,roll=random.Next(100),index=0;
            for(;index<5;index++){if(roll<weights[band][index])break;roll-=weights[band][index];}
            var type=ContentCatalog.Monsters[index];int depth=Mathf.Min(30,(floor-1)/4);
            return new EnemyData{Id=nextId++,Cell=cell,Archetype=type.Id,Hp=type.Hp+depth,MaximumHp=type.Hp+depth,AttackPower=type.Attack+1+depth/3};
        }
        public void Populate(DungeonRun run)
        {
            int target=MinimumCount(run.Floor)+random.Next(3);
            var rooms=new List<int>();for(int i=0;i<run.Map.Rooms.Count;i++)rooms.Add(i);
            DungeonGenerator.Shuffle(rooms,random);
            // One pass spreads encounters; further passes create unpredictable pairs/trios.
            for(int pass=0;pass<4&&run.Enemies.Count<target;pass++)foreach(int room in rooms)
            {
                if(run.Enemies.Count>=target)break;
                if(room==run.Map.RoomIndex(run.PlayerCell)&&pass>0)continue;
                var cells=run.Map.FloorCells();DungeonGenerator.Shuffle(cells,random);
                foreach(var cell in cells)if(run.Map.RoomIndex(cell)==room&&Free(run,cell)&&DungeonRules.Distance(cell,run.PlayerCell)>=5)
                {run.Enemies.Add(Create(run.Floor,cell));break;}
            }
        }
        public bool Reinforce(DungeonRun run,int floorTurns)
        {
            if(run.Dead||floorTurns==0||floorTurns%SpawnInterval(run.Floor)!=0||run.Enemies.Count>=MaximumAlive(run.Floor))return false;
            var cells=run.Map.FloorCells();DungeonGenerator.Shuffle(cells,random);
            foreach(var cell in cells)if(Free(run,cell)&&!run.Map.Visible[cell.x,cell.y]&&DungeonRules.Distance(cell,run.PlayerCell)>=12)
            {
                while(run.Enemies.Exists(e=>e.Id==nextId))nextId++;
                run.Enemies.Add(Create(run.Floor,cell));return true;
            }
            return false;
        }
        public Vector2Int Wander(DungeonRun run,EnemyData enemy)
        {
            if(random.Next(2)==0)return enemy.Cell;
            var choices=new List<Vector2Int>();
            foreach(var d in DungeonRules.MovementDirections){var next=enemy.Cell+d;
                if(next!=run.PlayerCell&&DungeonRules.CanStep(run.Map,enemy.Cell,next)&&!run.Enemies.Exists(e=>e!=enemy&&e.Hp>0&&e.Cell==next))choices.Add(next);}
            return choices.Count==0?enemy.Cell:choices[random.Next(choices.Count)];
        }
    }
}
