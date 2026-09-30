using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace LunaEclipse.Dungeon
{
    public sealed class DungeonMotionPlaytest : MonoBehaviour
    {
        [Serializable] sealed class Sample
        { public int requestedRate,step,frame; public float delta,unscaledDelta,progress; public Vector3 player,camera,scale; public bool cameraClamped,canvasBoundsInsideViewport;public Rect projectedBounds,viewport; }
        [Serializable] sealed class Report
        { public bool passed;public int checks,width,height,steps,intermediateSamples,originalVSyncCount;public string detail;public Sample[] samples;public RateSummary[] rates; }
        [Serializable] sealed class RateSummary
        { public int requestedRate,sampleCount;public float meanDelta,minDelta,maxDelta,observedMeanFps; }
        readonly List<Sample> samples=new List<Sample>();string output;bool done;int checks,steps,intermediate;float started;
        int originalVSyncCount,originalTargetFrameRate;
        static bool EdgeMode=>Array.IndexOf(Environment.GetCommandLineArgs(),"--luna-motion-edge-qa")>=0;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot(){if(Array.IndexOf(Environment.GetCommandLineArgs(),"--luna-motion-qa")>=0)new GameObject("Dungeon Motion QA").AddComponent<DungeonMotionPlaytest>();}
        void Awake()
        {
            output=Path.GetFullPath(Path.Combine(Application.dataPath,"..","QA",EdgeMode?"luna-motion-edge":"luna-motion",Screen.width+"x"+Screen.height));Directory.CreateDirectory(output);
            originalVSyncCount=QualitySettings.vSyncCount;originalTargetFrameRate=Application.targetFrameRate;
            QualitySettings.vSyncCount=0;
            started=Time.realtimeSinceStartup;Application.logMessageReceived+=OnLog;StartCoroutine(Exercise());
        }
        void OnDestroy(){Application.logMessageReceived-=OnLog;QualitySettings.vSyncCount=originalVSyncCount;Application.targetFrameRate=originalTargetFrameRate;}
        void Update(){if(!done&&Time.realtimeSinceStartup-started>90)Finish(false,"90-second timeout");}
        void OnLog(string text,string stack,LogType type){if(!done&&(type==LogType.Error||type==LogType.Exception))Finish(false,text+"\n"+stack);}
        void Check(bool value,string label){if(!value)throw new InvalidOperationException("Motion QA: "+label);checks++;}
        void Finish(bool passed,string detail)
        {
            if(done)return;done=true;File.WriteAllText(Path.Combine(output,"motion-result.json"),JsonUtility.ToJson(new Report{passed=passed,checks=checks,width=Screen.width,height=Screen.height,
                steps=steps,intermediateSamples=intermediate,detail=detail,samples=samples.ToArray(),originalVSyncCount=originalVSyncCount,rates=SummarizeRates()},true));
            QualitySettings.vSyncCount=originalVSyncCount;Application.targetFrameRate=originalTargetFrameRate;Application.Quit(passed?0:1);
        }
        RateSummary[] SummarizeRates()
        {
            var result=new List<RateSummary>();
            foreach(int rate in new[]{30,60})
            {
                var summary=new RateSummary{requestedRate=rate,minDelta=float.MaxValue};float sum=0;
                foreach(var sample in samples)if(sample.requestedRate==rate){summary.sampleCount++;sum+=sample.unscaledDelta;summary.minDelta=Mathf.Min(summary.minDelta,sample.unscaledDelta);summary.maxDelta=Mathf.Max(summary.maxDelta,sample.unscaledDelta);}
                if(summary.sampleCount>0){summary.meanDelta=sum/summary.sampleCount;summary.observedMeanFps=summary.meanDelta>0?1f/summary.meanDelta:0;}
                else summary.minDelta=0;
                result.Add(summary);
            }
            return result.ToArray();
        }
        IEnumerator Exercise()
        {
            yield return null;yield return null;var app=FindFirstObjectByType<LunaApp>();Check(app!=null,"app");app.Navigate("dungeon");yield return null;yield return null;
            var game=FindFirstObjectByType<GameManager>();Check(game!=null,"game");game.Run.Enemies.Clear();game.Renderer.Refresh(game.Run);
            var player=GameObject.Find("Luna").GetComponent<SpriteRenderer>();var camera=GameObject.Find("Dungeon Camera").GetComponent<Camera>();
            Check(player!=null&&camera!=null,"production renderers");var scale=player.transform.localScale;var origin=game.Run.PlayerCell;
            foreach(int rate in new[]{30,60})
            {
                Application.targetFrameRate=rate;yield return new WaitForSecondsRealtime(.3f);
                if(EdgeMode)
                {
                    var cells=game.Run.Map.FloorCells();
                    var edges=new[]{cells.OrderBy(c=>c.x).ThenBy(c=>c.y).First(),cells.OrderByDescending(c=>c.x).ThenBy(c=>c.y).First(),cells.OrderBy(c=>c.y).ThenBy(c=>c.x).First(),cells.OrderByDescending(c=>c.y).ThenBy(c=>c.x).First()};
                    foreach(var edge in edges)
                    {
                        PositionForTest(game,edge);yield return null;
                        var direction=DungeonRules.Directions.First(d=>game.Run.Map.Walkable(edge+d));
                        yield return ObserveMove(game,player,camera,scale,direction,rate);
                        yield return ObserveMove(game,player,camera,scale,-direction,rate);
                        Check(game.Run.PlayerCell==edge,"edge round trip");
                    }
                    continue;
                }
                foreach(var direction in DungeonRules.Directions)
                {
                    Check(game.Run.Map.Walkable(origin+direction),"connected test neighbour");
                    yield return ObserveMove(game,player,camera,scale,direction,rate);
                    yield return ObserveMove(game,player,camera,scale,-direction,rate);
                    Check(game.Run.PlayerCell==origin,"round trip returns to origin");
                }
            }
            Check(steps==16,"sixteen observed steps");
            if(EdgeMode)Check(samples.Any(sample=>sample.cameraClamped),"edge test must exercise actual camera clamp");
            Finish(true,"Actual GameManager movement: four direction round trips at requested targetFrameRate 30 and 60. Samples record actual deltas; requested rates are not measured FPS guarantees. Monotonic actor progress, orthogonal error, invariant scale, clamped camera follow, intermediate frames and final cells checked. No save completion invoked.");
        }
        void PositionForTest(GameManager game,Vector2Int target)
        {
            var parents=new Dictionary<Vector2Int,Vector2Int>{{game.Run.PlayerCell,game.Run.PlayerCell}};var queue=new Queue<Vector2Int>();queue.Enqueue(game.Run.PlayerCell);
            while(queue.Count>0&&!parents.ContainsKey(target)){var cell=queue.Dequeue();foreach(var d in DungeonRules.Directions){var next=cell+d;if(game.Run.Map.Walkable(next)&&!parents.ContainsKey(next)){parents[next]=cell;queue.Enqueue(next);}}}
            Check(parents.ContainsKey(target),"reachable edge setup");var route=new Stack<Vector2Int>();for(var p=target;p!=game.Run.PlayerCell;p=parents[p])route.Push(p);
            while(route.Count>0)Check(game.Run.Move(route.Pop()-game.Run.PlayerCell),"core setup move");
            game.Renderer.Refresh(game.Run);
        }
        IEnumerator ObserveMove(GameManager game,SpriteRenderer player,Camera camera,Vector3 scale,Vector2Int direction,int rate)
        {
            while(game.Busy)yield return null;
            var from=player.transform.position;var cell=game.Run.PlayerCell;var axis=new Vector3(direction.x,direction.y,0);var destination=from+axis;
            int turn=game.Run.Turns,localIntermediate=0;float previous=-.001f;steps++;game.Move(direction);
            Check(game.Run.PlayerCell==cell+direction&&game.Run.Turns==turn+1,"one accepted real movement");
            do
            {
                yield return new WaitForEndOfFrame();var position=player.transform.position;var displacement=position-from;float progress=Vector3.Dot(displacement,axis);
                Check(progress>=previous-.0001f&&progress>=-.0001f&&progress<=1.0001f,"monotonic non-overshooting movement");
                Check((displacement-axis*progress).magnitude<.0001f,"no orthogonal drift");
                Check(Vector3.Distance(player.transform.localScale,scale)<.0001f,"constant player scale");
                if(progress>.0001f&&progress<.9999f){localIntermediate++;intermediate++;}previous=progress;
                var focus=position-Vector3.up*DungeonPresentation.LunaFootOffset;
                float halfW=DungeonPresentation.TilesAcross*.5f,halfH=camera.orthographicSize;
                float expectedX=game.Run.Map.Width<halfW*2?(game.Run.Map.Width-1)*.5f:Mathf.Clamp(focus.x,halfW-.5f,game.Run.Map.Width-.5f-halfW);
                float expectedY=game.Run.Map.Height<halfH*2?(game.Run.Map.Height-1)*.5f:Mathf.Clamp(focus.y,halfH-.5f,game.Run.Map.Height-.5f-halfH);
                Check(Vector2.Distance(camera.transform.position,new Vector2(expectedX,expectedY))<.002f,"camera follows same interpolation with map clamp");
                var lower=camera.WorldToScreenPoint(player.bounds.min);var upper=camera.WorldToScreenPoint(player.bounds.max);var projected=Rect.MinMaxRect(Mathf.Min(lower.x,upper.x),Mathf.Min(lower.y,upper.y),Mathf.Max(lower.x,upper.x),Mathf.Max(lower.y,upper.y));var viewport=camera.pixelRect;
                samples.Add(new Sample{requestedRate=rate,step=steps,frame=Time.frameCount,delta=Time.deltaTime,unscaledDelta=Time.unscaledDeltaTime,progress=progress,
                    player=position,camera=camera.transform.position,scale=player.transform.localScale,cameraClamped=Mathf.Abs(expectedX-focus.x)>.001f||Mathf.Abs(expectedY-focus.y)>.001f,
                    projectedBounds=projected,viewport=viewport,canvasBoundsInsideViewport=projected.xMin>=viewport.xMin&&projected.yMin>=viewport.yMin&&projected.xMax<=viewport.xMax&&projected.yMax<=viewport.yMax});
            }while(game.Busy);
            Check(localIntermediate>0,"at least one measured intermediate position per step");
            Check(Vector3.Distance(player.transform.position,destination)<.0001f&&game.Run.Map.Walkable(game.Run.PlayerCell),"exact valid final position");
            Check(player.sprite!=null&&player.sprite==Resources.Load<Sprite>("Luna/idle/"+(direction.x<0?"left":direction.x>0?"right":direction.y>0?"up":"down")),"idle facing restored");
        }
    }
}
