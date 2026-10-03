using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
namespace LunaEclipse.Dungeon
{
 // Explicit opt-in isolated runtime QA. Never executes during ordinary Play.
 public sealed class DungeonPlaytest:MonoBehaviour
 {
  string output; int checks; bool done;
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
  static void StartTest(){if(Array.IndexOf(Environment.GetCommandLineArgs(),"--luna-qa")>=0)new GameObject("Dungeon Runtime QA").AddComponent<DungeonPlaytest>();}
  void Awake(){output=Path.GetFullPath(Path.Combine(Application.dataPath,"..","QA"));Directory.CreateDirectory(output);Application.logMessageReceived+=OnLog;StartCoroutine(Exercise());}
  void OnDestroy(){Application.logMessageReceived-=OnLog;}
  void OnLog(string message,string stack,LogType type){if(!done&&(type==LogType.Exception||type==LogType.Error)){done=true;File.WriteAllText(Path.Combine(output,"runtime-result.json"),JsonUtility.ToJson(new Report{passed=false,checks=checks,detail=message},true));Application.Quit(1);}}
  void Check(bool condition,string label){if(!condition)throw new Exception("Runtime QA: "+label);checks++;}
  IEnumerator Ready(GameManager game){while(game.Busy)yield return null;yield return null;}
  IEnumerator Exercise()
  {
   yield return null;yield return null;
   var game=FindFirstObjectByType<GameManager>();Check(game!=null,"Dungeon entry scene");
   yield return new WaitForSeconds(.4f);
   ScreenCapture.CaptureScreenshot(Path.Combine(output,"dungeon-b1.png"));yield return new WaitForEndOfFrame();
   Check(game.Run.Hp==20&&game.Run.Satiety==100&&game.Run.Bag.Count==0,"initial values");
   Check(FindObjectsByType<Camera>(FindObjectsSortMode.None).Length==1,"one world camera");
   var worldCamera=GameObject.Find("Dungeon Camera").GetComponent<Camera>();
   Check(Mathf.Abs(worldCamera.orthographicSize*2*worldCamera.aspect-9)<.01f,"nine large tiles across, no zoom out");
   Check(Mathf.Abs(game.UI.Viewport.rect.height-DungeonPresentation.ViewHeight)<.01f,"viewport reaches controls without gap");
   var luna=GameObject.Find("Luna").GetComponent<SpriteRenderer>();
   Check(Mathf.Abs(luna.bounds.size.y*DungeonPresentation.LunaBodyCanvasRatio-.85f)<.01f,"visible reference body occupies85percent of tile");
   Check(Vector2.Distance(worldCamera.transform.position,game.Run.PlayerCell)<.01f,"entry camera centred on Luna");
   Click(game,"持ち物");Check(game.Modal,"bag opens");int modalTurns=game.Run.Turns;game.Wait();Check(game.Run.Turns==modalTurns,"modal blocks commands");Click(game,"閉じる");yield return null;Check(!game.Modal,"bag closes");
   // Isolate render/input stress from combat; separately verify combat below.
   game.Run.AmbientEncounters=false;game.Run.Enemies.Clear();game.Renderer.Refresh(game.Run);
   int old=game.Run.Turns;game.Wait();game.Wait();game.Move(Vector2Int.up);Check(game.Run.Turns==old+1,"rapid input cannot overlap turns");yield return Ready(game);
   var herb=game.Run.Items[0];
   foreach(var cell in PathTo(game.Run,herb.Cell)){game.Move(cell-game.Run.PlayerCell);yield return Ready(game);}
   Check(!game.Run.CanPickUp,"item automatically removed from floor");Check(game.Run.Bag.Count==1,"bag increments");
   Check(HasLabel(game,"バッグ 1 / 20"),"bag HUD updates");
   var random=new System.Random(514);
   for(int i=0;i<100;i++){
    var options=new List<Vector2Int>();foreach(var d in Directions)if(game.Run.Map.Walkable(game.Run.PlayerCell+d))options.Add(d);
    var direction=options[random.Next(options.Count)];var target=game.Run.PlayerCell+direction;game.Move(direction);yield return Ready(game);
    Check(game.Run.PlayerCell==target&&game.Run.Map.Walkable(target),"movement "+i);
    var actor=GameObject.Find("Luna");Check(actor!=null&&Vector2.Distance(actor.transform.position,new Vector2(target.x,target.y+DungeonPresentation.LunaFootOffset))<.02f,"render grid alignment "+i);
   }
   Check(game.Run.Satiety==Mathf.Max(0,100-game.Run.Turns/game.Run.SatietyInterval),"satiety HUD model");
   Vector2Int adjacent=Vector2Int.zero;foreach(var d in Directions)if(game.Run.Map.Walkable(game.Run.PlayerCell+d)){adjacent=d;break;}
   game.Run.Enemies.Add(new EnemyData{Id=900,Cell=game.Run.PlayerCell+adjacent});game.Renderer.Refresh(game.Run);
   var origin=game.Run.PlayerCell;game.Move(adjacent);yield return Ready(game);
   Check(game.Run.PlayerCell==origin&&game.Run.Enemies[0].Hp==3&&game.Run.Hp==19,"bump combat damage");
   game.Attack();yield return Ready(game);game.Attack();yield return Ready(game);Check(game.Run.Enemies.Count==0&&game.Run.Hp==18,"kill removes retaliation");
   Check(HasLabel(game,"HP 18 / 20"),"HP HUD updates");
   bool corridorCaptured=false;
   foreach(var cell in PathTo(game.Run,game.Run.Map.Stairs)){
    game.Move(cell-game.Run.PlayerCell);yield return Ready(game);
    if(!corridorCaptured&&game.Run.Map.RoomIndex(game.Run.PlayerCell)<0){
     corridorCaptured=true;ScreenCapture.CaptureScreenshot(Path.Combine(output,"dungeon-corridor.png"));yield return new WaitForEndOfFrame();
    }
   }
   Check(corridorCaptured,"room to narrow corridor transition");
   Check(game.Run.CanDescend,"reachable stairs");int hp=game.Run.Hp,bag=game.Run.Bag.Count,satiety=game.Run.Satiety;
   game.Descend();yield return Ready(game);Check(game.Run.Floor==2&&game.Run.Hp==hp&&game.Run.Bag.Count==bag&&game.Run.Satiety>=satiety-1,"B2 preserves run");
   Check(HasLabel(game,"B2F"),"floor HUD updates");
   game.Wait();yield return Ready(game);Check(!game.Busy,"B2 controllable");
   ScreenCapture.CaptureScreenshot(Path.Combine(output,"dungeon-b2.png"));yield return new WaitForEndOfFrame();yield return new WaitForSeconds(.25f);
   done=true;File.WriteAllText(Path.Combine(output,"runtime-result.json"),JsonUtility.ToJson(new Report{passed=true,checks=checks,detail="100 movements, turn lock, pickup, combat, stairs, B2 continuation; isolated new session"},true));Application.Quit(0);
  }
  static readonly Vector2Int[] Directions={Vector2Int.up,Vector2Int.down,Vector2Int.left,Vector2Int.right};
  static bool HasLabel(GameManager game,string label){foreach(var t in game.GetComponentsInChildren<Text>())if(t.text==label)return true;return false;}
  static void Click(GameManager game,string name){foreach(var b in game.GetComponentsInChildren<Button>())if(b.name==name&&b.interactable){b.onClick.Invoke();return;}throw new Exception("Missing enabled UI button: "+name);}
  static List<Vector2Int> PathTo(DungeonRun run,Vector2Int end){var queue=new Queue<Vector2Int>();var seen=new Dictionary<Vector2Int,Vector2Int>();queue.Enqueue(run.PlayerCell);seen[run.PlayerCell]=run.PlayerCell;while(queue.Count>0){var p=queue.Dequeue();if(p==end)break;foreach(var d in Directions){var n=p+d;if(run.Map.Walkable(n)&&!seen.ContainsKey(n)){seen[n]=p;queue.Enqueue(n);}}}var path=new List<Vector2Int>();for(var p=end;p!=run.PlayerCell;p=seen[p])path.Add(p);path.Reverse();return path;}
  [Serializable] class Report{public bool passed;public int checks;public string detail;}
 }
}
