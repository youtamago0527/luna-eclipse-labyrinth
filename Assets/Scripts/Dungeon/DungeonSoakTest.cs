using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.UI;

namespace LunaEclipse.Dungeon
{
    /// <summary>Bounded, explicitly opt-in lifetime stress check; not proof of absence of leaks.</summary>
    public sealed class DungeonSoakTest : MonoBehaviour
    {
        [Serializable] sealed class Sample
        {
            public int cycle,cameras,worldRoots,worldObjects,audioSources,textures,sprites;
            public int materials,meshes,fonts,renderTextures,audioClips;
            public long allocatedBytes,managedBytes;
        }
        [Serializable] sealed class Report
        {
            public bool passed;public int completedCycles,checks,floors;public float seconds;
            public string detail;public long startAllocatedBytes,endAllocatedBytes,startManagedBytes,endManagedBytes;
            public Sample[] samples;
        }
        int cycles=25;float timeout=900;
        readonly List<Sample> samples=new List<Sample>();
        LunaApp app;Sample baseline,warm;string output;float started;int checks,floors,completed;bool done;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot()
        {
            if(Array.IndexOf(Environment.GetCommandLineArgs(),"--luna-soak-qa")>=0)
                new GameObject("Dungeon Soak QA").AddComponent<DungeonSoakTest>();
        }
        void Awake()
        {
            foreach(var argument in Environment.GetCommandLineArgs())
                if(argument.StartsWith("--luna-soak-cycles=",StringComparison.Ordinal))
                {cycles=argument=="--luna-soak-cycles=50"?50:25;break;}
            timeout=cycles==50?1800:900;
            output=Path.GetFullPath(Path.Combine(Application.dataPath,"..","QA",cycles==50?"soak-50":"soak"));Directory.CreateDirectory(output);
            started=Time.realtimeSinceStartup;Application.logMessageReceived+=OnLog;StartCoroutine(Exercise());
        }
        void OnDestroy(){Application.logMessageReceived-=OnLog;}
        void Update(){if(!done&&Time.realtimeSinceStartup-started>timeout)Finish(false,timeout+"-second time budget exceeded");}
        void OnLog(string message,string stack,LogType type){if(!done&&(type==LogType.Exception||type==LogType.Error))Finish(false,message+"\n"+stack);}
        void Check(bool value,string label){if(!value)throw new InvalidOperationException("Soak QA: "+label);checks++;}
        Sample Snapshot(int cycle)
        {
            var objects=FindObjectsByType<GameObject>(FindObjectsInactive.Include,FindObjectsSortMode.None);
            return new Sample{cycle=cycle,cameras=FindObjectsByType<Camera>(FindObjectsInactive.Include,FindObjectsSortMode.None).Length,
                worldRoots=objects.Count(o=>o.name=="Dungeon World"),worldObjects=objects.Count(o=>o.layer==30),
                audioSources=FindObjectsByType<AudioSource>(FindObjectsInactive.Include,FindObjectsSortMode.None).Length,
                textures=Resources.FindObjectsOfTypeAll<Texture2D>().Length,sprites=Resources.FindObjectsOfTypeAll<Sprite>().Length,
                materials=Resources.FindObjectsOfTypeAll<Material>().Length,meshes=Resources.FindObjectsOfTypeAll<Mesh>().Length,
                fonts=Resources.FindObjectsOfTypeAll<Font>().Length,renderTextures=Resources.FindObjectsOfTypeAll<RenderTexture>().Length,
                audioClips=Resources.FindObjectsOfTypeAll<AudioClip>().Length,
                allocatedBytes=Profiler.GetTotalAllocatedMemoryLong(),managedBytes=GC.GetTotalMemory(false)};
        }
        void Finish(bool passed,string detail)
        {
            if(done)return;done=true;var end=Snapshot(completed);
            File.WriteAllText(Path.Combine(output,"soak-result.json"),JsonUtility.ToJson(new Report{passed=passed,completedCycles=completed,checks=checks,floors=floors,
                seconds=Time.realtimeSinceStartup-started,detail=detail,startAllocatedBytes=baseline?.allocatedBytes??0,endAllocatedBytes=end.allocatedBytes,
                startManagedBytes=baseline?.managedBytes??0,endManagedBytes=end.managedBytes,samples=samples.ToArray()},true));Application.Quit(passed?0:1);
        }
        void Click(string label)
        {
            foreach(var button in app.GetComponentsInChildren<Button>())
                if(button.isActiveAndEnabled&&button.interactable&&(button.name==label||button.GetComponentsInChildren<Text>().Any(t=>t.text==label)))
                {button.onClick.Invoke();return;}
            throw new InvalidOperationException("Soak missing button "+label);
        }
        IEnumerator Ready(GameManager game){while(game.Busy)yield return null;yield return null;}
        IEnumerator Settle(){yield return null;yield return null;yield return null;yield return new WaitForSecondsRealtime(.1f);}
        IEnumerator Walk(GameManager game,Vector2Int target)
        {
            foreach(var cell in PathTo(game.Run,target))
            {
                var delta=cell-game.Run.PlayerCell;Check(Mathf.Abs(delta.x)+Mathf.Abs(delta.y)==1&&game.Run.Map.Walkable(cell),"valid path edge");
                int turn=game.Run.Turns;game.Move(delta);yield return Ready(game);
                Check(game.Run.PlayerCell==cell&&game.Run.Turns==turn+1,"movement cell and single turn");
            }
        }
        IEnumerator Exercise()
        {
            yield return null;yield return null;app=FindFirstObjectByType<LunaApp>();Check(app!=null,"app exists");
            Check(app.CurrentRoute=="title","initial title");Click("冒険をはじめる");yield return Settle();
            baseline=Snapshot(0);samples.Add(baseline);
            for(int cycle=1;cycle<=cycles;cycle++)
            {
                Check(app.CurrentRoute=="hub","hub entry "+cycle);Click("Enter dungeon");yield return null;
                Check(app.CurrentRoute=="departure","departure route");Click("探索を始める");yield return null;
                var game=FindFirstObjectByType<GameManager>();Check(game!=null&&app.CurrentRoute=="dungeon","dungeon entered");
                Check(FindObjectsByType<GameManager>(FindObjectsSortMode.None).Length==1,"one manager");
                Check(Snapshot(cycle).worldRoots==1,"one world root");
                for(int floor=1;floor<=3;floor++)
                {
                    game.Run.Enemies.Clear();game.Renderer.Refresh(game.Run);
                    Check(game.Run.Floor==floor&&game.Run.Map.Walkable(game.Run.PlayerCell),"floor entry valid");floors++;
                    if(floor==1)
                    {
                        var herb=game.Run.Items.First(i=>i.Item.Kind=="herb");yield return Walk(game,herb.Cell);
                        Check(game.Run.Bag.Any(i=>i.Id==herb.Item.Id),"automatic pickup");
                        int turns=game.Run.Turns;game.UseItem(herb.Item.Id);yield return Ready(game);
                        Check(game.Run.Turns==turns&&game.Run.Bag.Any(i=>i.Id==herb.Item.Id),"full HP use preserves item");
                    }
                    if(floor<3)
                    {
                        yield return Walk(game,game.Run.Map.Stairs);game.Descend();yield return Ready(game);
                        Check(game.Run.Floor==floor+1,"stairs accepted");
                    }
                }
                game.ReturnToHub();yield return Settle();Check(app.CurrentRoute=="results","results route");
                Click("拠点へ戻る");yield return Settle();Check(app.CurrentRoute=="hub","returned hub");
                var sample=Snapshot(cycle);samples.Add(sample);completed=cycle;
                Check(sample.cameras==baseline.cameras,"camera count returns to baseline");
                Check(sample.worldRoots==baseline.worldRoots&&sample.worldObjects==baseline.worldObjects,"world objects released");
                Check(sample.audioSources==baseline.audioSources,"audio sources released");
                Check(FindObjectsByType<GameManager>(FindObjectsInactive.Include,FindObjectsSortMode.None).Length==0,"dungeon manager released");
                if(cycle==3)warm=sample;
                if(warm!=null){Check(sample.textures<=warm.textures+16,"bounded live texture count after warmup");Check(sample.sprites<=warm.sprites+16,"bounded live sprite count after warmup");}
            }
            Finish(true,cycles+" hub/departure/dungeon/results cycles, "+(cycles*3)+" floor visits, actual animated movement; destruction counts return to baseline. Texture/sprite populations bounded after three warmup cycles. Memory and additional resource counts are observations, not leak-free proof. In-memory progression stores only.");
        }
        static List<Vector2Int> PathTo(DungeonRun run,Vector2Int end)
        {
            var queue=new Queue<Vector2Int>();var parents=new Dictionary<Vector2Int,Vector2Int>{{run.PlayerCell,run.PlayerCell}};queue.Enqueue(run.PlayerCell);
            while(queue.Count>0&&!parents.ContainsKey(end)){var p=queue.Dequeue();foreach(var d in DungeonRules.Directions){var n=p+d;if(run.Map.Walkable(n)&&!parents.ContainsKey(n)){parents[n]=p;queue.Enqueue(n);}}}
            var path=new List<Vector2Int>();for(var p=end;p!=run.PlayerCell;p=parents[p])path.Add(p);path.Reverse();return path;
        }
    }
}
