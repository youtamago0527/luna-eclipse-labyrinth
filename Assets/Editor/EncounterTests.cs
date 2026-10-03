using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using LunaEclipse.Dungeon;
namespace LunaEclipse.EditorTools
{
 public static class EncounterTests
 {
  static int checks;
  static void Check(bool value,string name){checks++;if(!value)throw new Exception(name);}
  public static void Run()
  {
   checks=0;
   foreach(int floor in new[]{1,3,5,10,20})for(int seed=0;seed<100;seed++)
   {
    var r=new DungeonRun(seed,startFloor:floor);var copy=new DungeonRun(seed,startFloor:floor);
    Check(r.Enemies.Count>=EncounterDirector.MinimumCount(floor)&&r.Enemies.Count<=EncounterDirector.MinimumCount(floor)+2,"initial density");
    Check(r.Enemies.Select(e=>e.Cell).Distinct().Count()==r.Enemies.Count,"no stacked spawns");
    Check(r.Enemies.Select(e=>r.Map.RoomIndex(e.Cell)).Distinct().Count()>=r.Map.Rooms.Count-1,"room distribution");
    Check(r.Enemies.Select(e=>e.Archetype+e.Cell).SequenceEqual(copy.Enemies.Select(e=>e.Archetype+e.Cell)),"deterministic layout");
    foreach(var e in r.Enemies)
    {
     Check(r.Map.Walkable(e.Cell)&&e.Cell!=r.Map.Stairs&&!r.Items.Any(i=>i.Cell==e.Cell)&&DungeonRules.Distance(e.Cell,r.PlayerCell)>=5,"valid safe initial spawn");
     if(floor==1)Check(e.Archetype!="mimic"&&e.Archetype!="stone_golem"&&e.Archetype!="mushroom","B1 tier restriction");
    }
    var director=new EncounterDirector(seed,floor);r.Enemies.Clear();int interval=EncounterDirector.SpawnInterval(floor);
    Check(!director.Reinforce(r,interval-1),"no early reinforcement");
    Check(director.Reinforce(r,interval),"reinforce empty floor");
    var born=r.Enemies.Last();Check(!r.Map.Visible[born.Cell.x,born.Cell.y]&&DungeonRules.Distance(born.Cell,r.PlayerCell)>=12,"spawn outside sight and far away");
    for(int n=2;n<35;n++)director.Reinforce(r,n*interval);
    Check(r.Enemies.Count==EncounterDirector.MaximumAlive(floor),"live cap");
    Check(r.Enemies.Select(e=>e.Id).Distinct().Count()==r.Enemies.Count,"unique ids");
   }
   var live=new DungeonRun(7);live.Enemies.Clear();
   for(int i=0;i<63;i++)live.Wait();Check(live.Enemies.Count==0,"63 quiet turns");
   live.Wait();Check(live.Enemies.Count==1&&live.Hp==live.MaxHp,"64th turn spawn does not attack");
   int floorTurns=live.FloorTurns;live.Move(new Vector2Int(2,2));Check(live.FloorTurns==floorTurns,"rejected input does not advance spawn clock");
   // Real game simulation, without clearing enemies or disabling ambient encounters.
   int dead=0,escaped=0,timeout=0;
   for(int seed=0;seed<50;seed++)
   {
    var r=new DungeonRun(seed);
    for(int turn=0;turn<350&&!r.Dead&&!r.CanDescend;turn++)
    {
     var heal=r.Bag.FirstOrDefault(i=>ContentCatalog.FindItem(i.Kind)?.Healing>0);
     if(heal!=null&&r.Hp<=r.MaxHp-8){r.UseItem(heal.Id);continue;}
     var equipment=r.Bag.FirstOrDefault(i=>(i.Kind=="moon_sword"&&r.WeaponId==null)||(i.Kind=="moon_shield"&&r.ShieldId==null));
     if(equipment!=null){r.UseItem(equipment.Id);continue;}
     var near=r.Enemies.FirstOrDefault(e=>DungeonRules.CanStep(r.Map,r.PlayerCell,e.Cell));
     if(near!=null){r.Move(near.Cell-r.PlayerCell);continue;}
     var q=new Queue<Vector2Int>();var previous=new Dictionary<Vector2Int,Vector2Int>();q.Enqueue(r.PlayerCell);previous[r.PlayerCell]=r.PlayerCell;
     while(q.Count>0){var cell=q.Dequeue();if(cell==r.Map.Stairs)break;foreach(var d in DungeonRules.Directions){var next=cell+d;if(r.Map.Walkable(next)&&!previous.ContainsKey(next)){previous[next]=cell;q.Enqueue(next);}}}
     var target=r.Map.Stairs;while(previous[target]!=r.PlayerCell)target=previous[target];r.Move(target-r.PlayerCell);
     Check(r.Enemies.Select(e=>e.Cell).Distinct().Count()==r.Enemies.Count&&!r.Enemies.Any(e=>e.Cell==r.PlayerCell),"live no actor overlap");
    }
    if(r.Dead)dead++;else if(r.CanDescend)escaped++;else timeout++;
   }
   EightWayProgressionTests.Run();
   File.WriteAllText("Library/EncounterTests.json","{\"passed\":true,\"checks\":"+checks+",\"seeds\":500,\"b1BotDeaths\":"+dead+",\"b1BotStairs\":"+escaped+",\"b1BotTimeouts\":"+timeout+"}");
   Debug.Log("Encounter tests passed. B1 bot deaths="+dead+", stairs="+escaped+", timeout="+timeout);
  }
 }
}
