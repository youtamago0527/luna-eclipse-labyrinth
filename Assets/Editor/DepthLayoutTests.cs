using System;
using System.IO;
using System.Linq;
using UnityEngine;
using LunaEclipse.Dungeon;
namespace LunaEclipse.EditorTools
{
 public static class DepthLayoutTests
 {
  static int checks;
  static void Check(bool value,string name){checks++;if(!value)throw new Exception(name);}
  public static void Run()
  {
   checks=0;double shallowArea=0,hallArea=0,cellArea=0;
   foreach(int floor in new[]{1,4,5,9,10,14,15,19,20,40})for(int seed=0;seed<100;seed++)
   {
    var map=DungeonGenerator.Generate(seed,floor);var p=DungeonFloorProfile.ForFloor(floor);
    var cells=map.FloorCells();var distances=DungeonGenerator.Distances(map,map.Start);
    Check(map.Width==39&&map.Height==45,"map dimensions unchanged");
    Check(cells.Count==distances.Count&&distances.ContainsKey(map.Stairs)&&map.Stairs!=map.Start,"all cells and stairs connected");
    Check(cells.All(c=>c.x>0&&c.y>0&&c.x<map.Width-1&&c.y<map.Height-1),"solid outer border");
    Check(map.Rooms.Count>=p.MinRooms&&map.Rooms.Count<=p.MaxRooms,"profile room count");
    Check(map.Rooms.Skip(1).All(r=>r.width>=p.MinWidth&&r.width<=p.MaxWidth&&r.height>=p.MinHeight&&r.height<=p.MaxHeight),"profile room sizes");
    Check(!map.Explored[map.Stairs.x,map.Stairs.y],"stairs not exposed at landing");
    for(int a=0;a<map.Rooms.Count;a++)for(int b=a+1;b<map.Rooms.Count;b++)Check(!map.Rooms[a].Overlaps(map.Rooms[b]),"rooms separated");
    var repeat=DungeonGenerator.Generate(seed,floor);Check(cells.SequenceEqual(repeat.FloorCells())&&map.Stairs==repeat.Stairs,"reproducible floor");
    var next=DungeonGenerator.Generate(seed,floor+1);Check(!cells.SequenceEqual(next.FloorCells()),"next floor changes layout");
    double area=map.Rooms.Skip(1).Average(r=>r.width*r.height);
    if(floor==1)shallowArea+=area;if(floor==10)hallArea+=area;if(floor==15)cellArea+=area;
   }
   Check(hallArea>shallowArea*1.4&&shallowArea>cellArea*1.2,"measurable hall/cell contrast across 100 seeds");
   Check(Mathf.Approximately(DungeonPresentation.LunaHeight,.85f*1.2f)&&DungeonPresentation.TilesAcross==9&&DungeonPresentation.LunaFootOffset==-.40f,"20 percent sprite-only scale");
   EncounterTests.Run();
   File.WriteAllText("Library/DepthLayoutTests.json","{\"passed\":true,\"checks\":"+checks+",\"maps\":1000,\"shallowRoomArea\":"+(shallowArea/100).ToString(System.Globalization.CultureInfo.InvariantCulture)+",\"hallRoomArea\":"+(hallArea/100).ToString(System.Globalization.CultureInfo.InvariantCulture)+",\"cellRoomArea\":"+(cellArea/100).ToString(System.Globalization.CultureInfo.InvariantCulture)+"}");
   Debug.Log("Depth layout tests passed: "+checks);
  }
 }
}
