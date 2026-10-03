using System;
using System.Reflection;
using System.IO;
using UnityEngine;
using LunaEclipse.Dungeon;
namespace LunaEclipse.EditorTools
{
 public static class EightWayProgressionTests
 {
  static int checks;
  static void Check(bool b,string s){checks++;if(!b)throw new Exception(s);}
  static void Set(DungeonRun r,string name,object value)=>typeof(DungeonRun).GetProperty(name).SetValue(r,value);
  static bool[,] Cells(DungeonRun r)=>(bool[,])typeof(DungeonMap).GetField("floor",BindingFlags.NonPublic|BindingFlags.Instance).GetValue(r.Map);
  static DungeonRun Room()
  {
   var r=new DungeonRun(21);r.Enemies.Clear();r.Items.Clear();
   var f=Cells(r);Array.Clear(f,0,f.Length);
   for(int x=5;x<=11;x++)for(int y=5;y<=11;y++)f[x,y]=true;
   Set(r,"PlayerCell",new Vector2Int(8,8));r.Map.Reveal(r.PlayerCell);return r;
  }
  public static void Run()
  {
   foreach(var d in DungeonRules.MovementDirections)
   {
    var r=Room();Check(r.Move(d)&&r.PlayerCell==new Vector2Int(8,8)+d&&r.Turns==1,"8 directions one turn");
    r=Room();r.Enemies.Add(new EnemyData{Id=1,Cell=r.PlayerCell+d,Hp=5});
    Check(r.Move(d)&&r.Enemies[0].Hp==3&&r.Hp==19&&r.PlayerCell==new Vector2Int(8,8),"8 way mutual attack");
   }
   foreach(int x in new[]{-1,1})foreach(int y in new[]{-1,1})foreach(bool horizontal in new[]{true,false})
   {
    var r=Room();var d=new Vector2Int(x,y);Cells(r)[8+(horizontal?x:0),8+(horizontal?0:y)]=false;
    var e=new EnemyData{Id=1,Cell=r.PlayerCell+d};r.Enemies.Add(e);
    Check(!r.Move(d)&&r.Turns==0&&r.Facing==d,"corner blocked without turn");
    r.Attack();Check(e.Hp==5&&r.Hp==20,"neither attacks through corner");
    Check(e.Cell!=r.PlayerCell,"enemy never overlaps player");
   }
   var level=Room();
   for(int i=0;i<5;i++){level.Enemies.Add(new EnemyData{Id=i,Cell=level.PlayerCell+Vector2Int.right,Hp=1,MaximumHp=5});level.Move(Vector2Int.right);}
   Check(level.Level==2&&level.Experience==10&&level.MaxHp==22,"kill XP and run level");
   Check(new DungeonRun(21).Level==1,"new run resets level");
   var recovery=Room();Set(recovery,"Hp",10);
   for(int i=0;i<4;i++)recovery.Wait();Check(recovery.Hp==10,"no early healing");
   recovery.Wait();Check(recovery.Hp==11&&recovery.Satiety==99,"fifth action heals one and consumes hunger");
   Set(recovery,"Satiety",0);for(int i=0;i<5;i++)recovery.Wait();Check(recovery.Hp==11,"no empty belly regeneration");
   var rest=Room();Set(rest,"Hp",19);Check(rest.CanRest,"safe injured rest");
   for(int i=0;i<5;i++)Check(rest.Rest(),"rest consumes turn");
   Check(rest.Hp==20&&!rest.CanRest&&!rest.Rest()&&rest.Turns==5,"full HP stops rest");
   Set(rest,"Hp",10);Set(rest,"Satiety",10);Check(!rest.CanRest,"low hunger stops rest");
   Set(rest,"Satiety",100);rest.Enemies.Add(new EnemyData{Id=1,Cell=rest.PlayerCell+Vector2Int.right});Check(!rest.CanRest,"danger stops rest");
   var dead=Room();Set(dead,"Turns",4);dead.Enemies.Add(new EnemyData{Id=1,Cell=dead.PlayerCell+Vector2Int.right,AttackPower=30});dead.Wait();Check(dead.Dead,"regen cannot resurrect");
   var atlas=Resources.Load<Texture2D>("Luna/diagonal-atlas");Check(atlas!=null&&atlas.isReadable,"atlas available");
   var pixels=atlas.GetPixels32();int transparent=0,opaque=0;foreach(var p in pixels){if(p.a==0)transparent++;if(p.a>128)opaque++;}
   Check(transparent>pixels.Length/2&&opaque>1000,"genuine alpha sprite atlas");
   DungeonComfortTests.Run();
   File.WriteAllText("Library/EightWayProgressionTests.json","{\"passed\":true,\"checks\":"+checks+"}");
   Debug.Log("Eight-way progression tests passed: "+checks);
  }
 }
}
