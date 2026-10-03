using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace LunaEclipse.Dungeon
{
    /// <summary>Opt-in original-sprite capture using the existing dungeon player and camera.</summary>
    public sealed class LunaFramePlaytest : MonoBehaviour
    {
        [Serializable] sealed class Frame
        {
            public string resource,screenshot; public Rect spriteRect,cameraRect;
            public Vector2 pivot,normalizedPivot;public float pixelsPerUnit,orthographicSize;
            public Vector3 position,localPosition,localScale,boundsSize;
            public Rect projectedBounds; public bool canvasBoundsInsideViewport; public Vector2Int cell;
        }
        [Serializable] sealed class Report
        {
            public bool passed;public int width,height,checks;public Rect safeArea;
            public string detail;public Frame[] frames;
        }
        readonly List<Frame> frames=new List<Frame>();string output;int checks;bool done;float started;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot()
        {
            if(Array.IndexOf(Environment.GetCommandLineArgs(),"--luna-frame-qa")>=0)
                new GameObject("Luna Frame QA").AddComponent<LunaFramePlaytest>();
        }
        void Awake()
        {
            output=Path.GetFullPath(Path.Combine(Application.dataPath,"..","QA",EdgeMode?"luna-frames-edge":"luna-frames",Screen.width+"x"+Screen.height));
            Directory.CreateDirectory(output);started=Time.realtimeSinceStartup;
            Application.logMessageReceived+=OnLog;StartCoroutine(CaptureFrames());
        }
        void OnDestroy(){Application.logMessageReceived-=OnLog;}
        void Update(){if(!done&&Time.realtimeSinceStartup-started>90)Finish(false,"90-second timeout");}
        void OnLog(string message,string stack,LogType type)
        {if(!done&&(type==LogType.Error||type==LogType.Exception))Finish(false,message+"\n"+stack);}
        void Check(bool value,string message){if(!value)throw new InvalidOperationException("Luna frame QA: "+message);checks++;}
        static bool EdgeMode=>Array.IndexOf(Environment.GetCommandLineArgs(),"--luna-frame-edge-qa")>=0;
        void Finish(bool passed,string detail)
        {
            if(done)return;done=true;
            File.WriteAllText(Path.Combine(output,"luna-frame-result.json"),JsonUtility.ToJson(new Report{passed=passed,width=Screen.width,height=Screen.height,
                safeArea=Screen.safeArea,checks=checks,detail=detail,frames=frames.ToArray()},true));Application.Quit(passed?0:1);
        }
        IEnumerator CaptureFrames()
        {
            yield return null;yield return null;
            var app=FindFirstObjectByType<LunaApp>();Check(app!=null,"app boot");
            // Title -> dungeon is direct here: this test isolates rendering, not route callbacks.
            app.Navigate("dungeon");yield return null;yield return null;
            var game=FindFirstObjectByType<GameManager>();Check(game!=null,"existing production game manager");
            while(game.Busy)yield return null;
            game.Run.AmbientEncounters=false;game.Run.Enemies.Clear();game.Renderer.Refresh(game.Run);game.SetModal(true);
            var playerObject=GameObject.Find("Luna");Check(playerObject!=null,"production player object");
            var player=playerObject.GetComponent<SpriteRenderer>();var cameraObject=GameObject.Find("Dungeon Camera");
            Check(player!=null&&cameraObject!=null,"production renderer and camera");var camera=cameraObject.GetComponent<Camera>();
            Vector3 position=player.transform.position,localPosition=player.transform.localPosition,scale=player.transform.localScale;
            Quaternion rotation=player.transform.rotation;var original=player.sprite;Check(original!=null,"original idle sprite");
            float ppu=original.pixelsPerUnit;Vector2 referencePivot=new Vector2(original.pivot.x/original.rect.width,original.pivot.y/original.rect.height);
            if(EdgeMode){yield return CaptureEdges(game,player,camera);yield break;}
            foreach(string action in new[]{"idle","walk","attack"})
                foreach(string direction in new[]{"down","left","right","up"})
                    for(int index=1;index<=(action=="idle"?1:action=="walk"?4:3);index++)
                    {
                        string resource="Luna/"+action+"/"+direction+(action=="idle"?"":"_"+index.ToString("00"));
                        var sprite=Resources.Load<Sprite>(resource);Check(sprite!=null,"load "+resource);
                        var normalized=new Vector2(sprite.pivot.x/sprite.rect.width,sprite.pivot.y/sprite.rect.height);
                        Check(Vector2.Distance(normalized,new Vector2(.5f,.1f))<.0001f,"shared foot pivot "+resource);
                        Check(Vector2.Distance(normalized,referencePivot)<.0001f&&Mathf.Abs(sprite.pixelsPerUnit-ppu)<.0001f,"import scale consistency "+resource);
                        player.sprite=sprite;yield return new WaitForEndOfFrame();
                        Check(player.sprite==sprite&&player.enabled&&camera.enabled&&player.gameObject.activeInHierarchy,"frame visible "+resource);
                        Check(Vector3.Distance(player.transform.position,position)<.00001f&&Vector3.Distance(player.transform.localPosition,localPosition)<.00001f,
                            "position unchanged "+resource);
                        Check(Vector3.Distance(player.transform.localScale,scale)<.00001f&&Quaternion.Angle(player.transform.rotation,rotation)<.0001f,"transform scale/rotation unchanged "+resource);
                        string file=Path.Combine(output,action+"-"+direction+"-"+index.ToString("00")+".png");
                        frames.Add(new Frame{resource=resource,screenshot=file,spriteRect=sprite.rect,pivot=sprite.pivot,normalizedPivot=normalized,
                            pixelsPerUnit=sprite.pixelsPerUnit,position=player.transform.position,localPosition=player.transform.localPosition,
                            localScale=player.transform.localScale,boundsSize=sprite.bounds.size,cameraRect=camera.pixelRect,orthographicSize=camera.orthographicSize});
                        ScreenCapture.CaptureScreenshot(file);yield return new WaitForEndOfFrame();yield return new WaitForSecondsRealtime(.1f);
                    }
            Check(frames.Count==32,"all 32 original frames");
            foreach(var frame in frames)Check(File.Exists(frame.screenshot)&&new FileInfo(frame.screenshot).Length>0,"PNG saved "+frame.resource);
            player.sprite=original;
            Finish(true,"32 original sprites captured on existing production SpriteRenderer at identical position/scale. Pivot and PPU verified. Static centre-frame test only: no edge clipping or continuous motion claims.");
        }
        IEnumerator CaptureEdges(GameManager game,SpriteRenderer player,Camera camera)
        {
            var cells=game.Run.Map.FloorCells();var scale=player.transform.localScale;
            var original=player.sprite;float ppu=original.pixelsPerUnit;
            string[] directions={"left","right","up","down"};
            var targets=new[]{cells.OrderBy(c=>c.x).ThenBy(c=>c.y).First(),cells.OrderByDescending(c=>c.x).ThenBy(c=>c.y).First(),
                cells.OrderByDescending(c=>c.y).ThenBy(c=>c.x).First(),cells.OrderBy(c=>c.y).ThenBy(c=>c.x).First()};
            for(int edge=0;edge<targets.Length;edge++)
            {
                var target=targets[edge];Check(game.Run.Map.Walkable(target),"edge target walkable");
                // PlayerCell has a private setter. Use real core Move on a BFS route instead of reflection/production changes.
                var parents=new Dictionary<Vector2Int,Vector2Int>{{game.Run.PlayerCell,game.Run.PlayerCell}};var queue=new Queue<Vector2Int>();queue.Enqueue(game.Run.PlayerCell);
                while(queue.Count>0&&!parents.ContainsKey(target)){var cell=queue.Dequeue();foreach(var direction in DungeonRules.Directions){var next=cell+direction;if(game.Run.Map.Walkable(next)&&!parents.ContainsKey(next)){parents[next]=cell;queue.Enqueue(next);}}}
                Check(parents.ContainsKey(target),"edge target reachable");var route=new Stack<Vector2Int>();
                for(var cell=target;cell!=game.Run.PlayerCell;cell=parents[cell])route.Push(cell);
                while(route.Count>0){var next=route.Pop();Check(game.Run.Move(next-game.Run.PlayerCell),"edge traversal accepted");}
                game.Renderer.Refresh(game.Run);yield return null;yield return new WaitForEndOfFrame();
                Vector3 expected=new Vector3(target.x,target.y+DungeonPresentation.LunaFootOffset,0);
                Check(Vector3.Distance(player.transform.position,expected)<.0001f,"edge player anchor");
                var fixedPosition=player.transform.position;var fixedLocal=player.transform.localPosition;
                for(int index=1;index<=3;index++)
                {
                    string resource="Luna/attack/"+directions[edge]+"_"+index.ToString("00");var sprite=Resources.Load<Sprite>(resource);
                    Check(sprite!=null,"edge sprite loaded");player.sprite=sprite;yield return new WaitForEndOfFrame();
                    Check(player.sprite==sprite&&player.enabled&&camera.enabled,"edge frame active");
                    Check(Vector3.Distance(player.transform.position,fixedPosition)<.0001f&&Vector3.Distance(player.transform.localPosition,fixedLocal)<.0001f&&Vector3.Distance(player.transform.localScale,scale)<.0001f,"edge transform invariant");
                    var pivot=new Vector2(sprite.pivot.x/sprite.rect.width,sprite.pivot.y/sprite.rect.height);
                    Check(Vector2.Distance(pivot,new Vector2(.5f,.1f))<.0001f&&Mathf.Abs(sprite.pixelsPerUnit-ppu)<.0001f,"edge import invariant");
                    var bounds=player.bounds;var minimum=camera.WorldToScreenPoint(bounds.min);var maximum=camera.WorldToScreenPoint(bounds.max);
                    Rect projected=Rect.MinMaxRect(Mathf.Min(minimum.x,maximum.x),Mathf.Min(minimum.y,maximum.y),Mathf.Max(minimum.x,maximum.x),Mathf.Max(minimum.y,maximum.y));
                    Rect viewport=camera.pixelRect;bool contained=projected.xMin>=viewport.xMin&&projected.yMin>=viewport.yMin&&projected.xMax<=viewport.xMax&&projected.yMax<=viewport.yMax;
                    string file=Path.Combine(output,"edge-"+directions[edge]+"-attack-"+index.ToString("00")+".png");
                    frames.Add(new Frame{resource=resource,screenshot=file,spriteRect=sprite.rect,pivot=sprite.pivot,normalizedPivot=pivot,pixelsPerUnit=sprite.pixelsPerUnit,
                        position=player.transform.position,localPosition=player.transform.localPosition,localScale=player.transform.localScale,boundsSize=sprite.bounds.size,
                        cameraRect=viewport,orthographicSize=camera.orthographicSize,projectedBounds=projected,canvasBoundsInsideViewport=contained,cell=target});
                    ScreenCapture.CaptureScreenshot(file);yield return new WaitForEndOfFrame();yield return new WaitForSecondsRealtime(.1f);
                }
            }
            Check(frames.Count==12,"four edge directions times three attack frames");
            foreach(var frame in frames)Check(File.Exists(frame.screenshot)&&new FileInfo(frame.screenshot).Length>0,"edge screenshot saved");
            Finish(true,"12 outward attack frames at four extremal reachable floor cells; real core movement, production camera follow and unchanged anchors/scales verified. Projected sprite bounds include transparent canvas and are observation-only, not a no-clipping assertion. No continuous-motion test.");
        }
    }
}
