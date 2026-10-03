using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LunaEclipse.Audio;
using LunaEclipse.Progression;
using UnityEngine;
using UnityEngine.UI;

namespace LunaEclipse.Dungeon
{
    /// <summary>Explicit command-line integration QA; production saves are never used.</summary>
    public sealed class ProductionPlaytest : MonoBehaviour
    {
        sealed class MemoryStore : IRelicStore
        {
            readonly Dictionary<string,string> values = new Dictionary<string,string>();
            public string Read(string key) => values.TryGetValue(key,out var value) ? value : "";
            public void Write(string key,string value) { values[key]=value; }
            public void Flush() { }
        }
        public static IRelicStore Store { get; } = new MemoryStore();
        [Serializable] sealed class BoundaryObservation
        {
            public string screen; public int width,height,checkedButtons,excludedButtons;
            public Rect safeArea; public string[] outsideButtons;
            public GameplayControlSize[] gameplayControls; public string[] gameplayControlsBelow44;
        }
        [Serializable] sealed class GameplayControlSize
        { public string name; public float width,height; }
        [Serializable] sealed class Report
        {
            public bool passed; public int checks,width,height; public Rect safeArea;
            public string detail; public string[] screenshots; public bool invalidLabelIgnored;
            public BoundaryObservation[] boundaryObservations;
        }
        readonly List<string> screenshots=new List<string>();
        readonly List<BoundaryObservation> boundaryObservations=new List<BoundaryObservation>();
        bool invalidLabelIgnored;
        string output; int checks; bool done; float started; LunaApp app;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot()
        {
            if(Array.IndexOf(Environment.GetCommandLineArgs(),"--luna-production-qa")<0)return;
            new GameObject("Production Runtime QA").AddComponent<ProductionPlaytest>();
        }
        void Awake()
        {
            string folder="production";
            foreach(string argument in Environment.GetCommandLineArgs())
            {
                const string prefix="--luna-qa-label=";
                if(!argument.StartsWith(prefix,StringComparison.Ordinal))continue;
                string label=argument.Substring(prefix.Length);
                bool valid=label.Length>0&&label.Length<=64&&label.All(c=>(c>='a'&&c<='z')||(c>='A'&&c<='Z')||(c>='0'&&c<='9')||c=='-');
                if(valid)folder="production-"+label;else invalidLabelIgnored=true;
                break;
            }
            output=Path.GetFullPath(Path.Combine(Application.dataPath,"..","QA",folder));
            Directory.CreateDirectory(output);started=Time.realtimeSinceStartup;
            Application.logMessageReceived+=OnLog;StartCoroutine(Exercise());
        }
        void OnDestroy(){Application.logMessageReceived-=OnLog;}
        void Update(){if(!done&&Time.realtimeSinceStartup-started>180)Finish(false,"Timeout after 180 seconds");}
        void OnLog(string message,string stack,LogType type)
        {if(!done&&(type==LogType.Error||type==LogType.Exception))Finish(false,message+"\n"+stack);}
        void Finish(bool passed,string detail)
        {
            if(done)return;done=true;
            File.WriteAllText(Path.Combine(output,"production-result.json"),JsonUtility.ToJson(new Report{passed=passed,checks=checks,detail=detail,screenshots=screenshots.ToArray(),
                width=Screen.width,height=Screen.height,safeArea=Screen.safeArea,invalidLabelIgnored=invalidLabelIgnored,boundaryObservations=boundaryObservations.ToArray()},true));
            Application.Quit(passed?0:1);
        }
        void Check(bool value,string label){if(!value)throw new InvalidOperationException("Production QA: "+label);checks++;}
        IEnumerator Ready(GameManager game){while(game.Busy)yield return null;yield return null;}
        IEnumerator Capture(string name)
        {
            yield return new WaitForEndOfFrame();
            ObserveBoundaries(name);
            string file=Path.Combine(output,name+".png");ScreenCapture.CaptureScreenshot(file);
            yield return new WaitForEndOfFrame();yield return new WaitForSecondsRealtime(.12f);screenshots.Add(file);
        }
        void ObserveBoundaries(string name)
        {
            var observation=new BoundaryObservation{screen=name,width=Screen.width,height=Screen.height,safeArea=Screen.safeArea};
            var outside=new List<string>();var corners=new Vector3[4];
            var controls=new List<GameplayControlSize>();var smallControls=new List<string>();
            var gameplayNames=new HashSet<string>{"▲","◀","▶","▼","↖","↗","↙","↘","足踏み\n長押し","攻撃","待機","拾う","持ち物","階段を降りる"};
            foreach(var button in app.GetComponentsInChildren<Button>(true))
            {
                // Scroll contents may legitimately extend beyond their clipped viewport.
                bool hidden=!button.isActiveAndEnabled||button.GetComponentInParent<ScrollRect>()!=null;
                foreach(var group in button.GetComponentsInParent<CanvasGroup>())if(group.alpha<=.001f)hidden=true;
                var rect=button.transform as RectTransform;var canvas=button.GetComponentInParent<Canvas>();
                if(hidden||rect==null||canvas==null||!canvas.isActiveAndEnabled||rect.rect.width<=0||rect.rect.height<=0)
                {observation.excludedButtons++;continue;}
                observation.checkedButtons++;rect.GetWorldCorners(corners);
                var camera=canvas.renderMode==RenderMode.ScreenSpaceOverlay?null:canvas.worldCamera;
                float minX=float.PositiveInfinity,minY=float.PositiveInfinity,maxX=float.NegativeInfinity,maxY=float.NegativeInfinity;
                foreach(var corner in corners){var point=RectTransformUtility.WorldToScreenPoint(camera,corner);minX=Mathf.Min(minX,point.x);minY=Mathf.Min(minY,point.y);maxX=Mathf.Max(maxX,point.x);maxY=Mathf.Max(maxY,point.y);}
                if(gameplayNames.Contains(button.name))
                {
                    float width=maxX-minX,height=maxY-minY;
                    controls.Add(new GameplayControlSize{name=button.name,width=width,height=height});
                    if(width<44f||height<44f)smallControls.Add(button.name);
                }
                // Two pixels absorb layout rounding. Observations do not fail functional QA.
                if(minX< -2||minY< -2||maxX>Screen.width+2||maxY>Screen.height+2)
                    outside.Add(button.name+" ["+minX.ToString("F1")+","+minY.ToString("F1")+" → "+maxX.ToString("F1")+","+maxY.ToString("F1")+"]");
            }
            observation.outsideButtons=outside.ToArray();observation.gameplayControls=controls.ToArray();
            observation.gameplayControlsBelow44=smallControls.ToArray();boundaryObservations.Add(observation);
        }
        void Route(string route){Check(app.CurrentRoute==route,"route "+route);}
        void ClickBagItem(GameManager game,string kind)
        {
            var item=game.Run.Bag.First(i=>i.Kind==kind);int index=game.Run.Bag.IndexOf(item);
            for(int page=0;page<index/DungeonRules.ItemsPerPage;page++)Click("次へ");
            Click((index+1)+". "+item.DisplayName+((game.Run.WeaponId==item.Id||game.Run.ShieldId==item.Id)?" 【装備中】":""));
        }
        void Click(string label)
        {
            foreach(var button in app.GetComponentsInChildren<Button>())
                if(button.isActiveAndEnabled&&button.interactable&&(button.name==label||button.GetComponentsInChildren<Text>().Any(t=>t.text==label)))
                {button.onClick.Invoke();checks++;return;}
            throw new InvalidOperationException("Production QA missing enabled button: "+label);
        }
        void Audio(string expected)
        {
            var tracks=new HashSet<string>{"moonlit-sanctuary","luna-moonlit-footsteps","luna-beneath-the-eclipse","luna-home-under-moonlight"};
            var playing=FindObjectsByType<AudioSource>(FindObjectsSortMode.None).Where(s=>s.isPlaying&&s.clip!=null&&tracks.Contains(s.clip.name)).ToArray();
            Check(playing.Length<=1,"single settled BGM source");
            if(expected==null)Check(playing.Length==0,"silent title");
            else if(LocalSettings.BgmVolume>0)Check(playing.Length==1&&playing[0].clip.name==expected,"BGM route "+expected);
            else Check(playing.All(s=>s.volume==0),"muted preference respected");
        }
        void VerifyAudioSettingsIsolation()
        {
            Check(LocalSettings.SessionOnly,"QA session settings isolation enabled");
            const string bgmKey="luna.settings.bgm-volume",seKey="luna.settings.se-volume";
            bool hadBgm=PlayerPrefs.HasKey(bgmKey),hadSe=PlayerPrefs.HasKey(seKey);
            float savedBgm=PlayerPrefs.GetFloat(bgmKey,-123f),savedSe=PlayerPrefs.GetFloat(seKey,-123f);
            float initialBgm=LocalSettings.BgmVolume,initialSe=LocalSettings.SeVolume;
            try
            {
                LocalSettings.BgmVolume=.17f;LocalSettings.SeVolume=.27f;
                Check(Mathf.Approximately(LocalSettings.BgmVolume,.17f)&&Mathf.Approximately(LocalSettings.SeVolume,.27f),"session audio settings change");
                LocalSettings.BgmVolume=float.NaN;LocalSettings.SeVolume=float.PositiveInfinity;
                Check(Mathf.Approximately(LocalSettings.BgmVolume,.17f)&&Mathf.Approximately(LocalSettings.SeVolume,.27f),"invalid audio values ignored");
                LocalSettings.BgmVolume=-1;LocalSettings.SeVolume=2;
                Check(LocalSettings.BgmVolume==0&&LocalSettings.SeVolume==1,"session audio bounds");
                Check(PlayerPrefs.HasKey(bgmKey)==hadBgm&&PlayerPrefs.HasKey(seKey)==hadSe,"QA creates no persisted audio keys");
                Check(PlayerPrefs.GetFloat(bgmKey,-123f).Equals(savedBgm)&&PlayerPrefs.GetFloat(seKey,-123f).Equals(savedSe),"QA leaves persisted audio values unchanged");
            }
            finally{LocalSettings.BgmVolume=initialBgm;LocalSettings.SeVolume=initialSe;}
        }
        IEnumerator VerifySettingsScreen()
        {
            float originalBgm=LocalSettings.BgmVolume,originalSe=LocalSettings.SeVolume;
            Click("⚙");yield return null;Route("settings");
            var sliders=app.GetComponentsInChildren<Slider>();
            Check(sliders.Length==2,"two visible settings sliders");
            var bgmSlider=sliders.Single(s=>s.name=="BGM Volume");
            var seSlider=sliders.Single(s=>s.name=="効果音 Volume");
            try
            {
                bgmSlider.value=0;seSlider.value=.42f;yield return null;
                Check(LocalSettings.BgmVolume==0&&Mathf.Approximately(LocalSettings.SeVolume,.42f),"slider callbacks update session settings");
                Audio("moonlit-sanctuary");
                Check(app.GetComponentsInChildren<Text>().Any(t=>t.text=="42%"),"slider percentage label updates");
                bgmSlider.value=.23f;yield return null;Audio("moonlit-sanctuary");
                var hubTrack=app.GetComponents<AudioSource>().Single(s=>s.clip!=null&&s.clip.name=="moonlit-sanctuary");
                Check(Mathf.Approximately(hubTrack.volume,.23f),"BGM slider updates live source volume");
                Canvas.ForceUpdateCanvases();
                foreach(var slider in sliders)
                {
                    Check(slider.fillRect.rect.height<=10,"slider fill stays inside thin track");
                    Check(slider.handleRect.rect.height<=34.1f,"slider handle does not stretch into label");
                }
                yield return Capture("02b-settings");
                Click("拠点へ戻る");yield return null;Route("hub");
                Click("⚙");yield return null;Route("settings");
                Check(app.GetComponentsInChildren<Slider>().Any(s=>s.name=="BGM Volume"&&Mathf.Approximately(s.value,.23f)),"settings rebuilt with session volume");
            }
            finally{LocalSettings.BgmVolume=originalBgm;LocalSettings.SeVolume=originalSe;}
            Click("拠点へ戻る");yield return null;Route("hub");Audio("moonlit-sanctuary");
        }
        IEnumerator Exercise()
        {
            yield return null;yield return null;app=FindFirstObjectByType<LunaApp>();Check(app!=null,"app booted");
            VerifyAudioSettingsIsolation();
            yield return MusicTransitionPlaytest.Exercise(Check);
            yield return null; // Destroy the temporary audio sources before route checks.
            Check(Screen.height>Screen.width,"portrait target");
            Route("title");Audio(null);yield return Capture("01-title");
            foreach(var monster in ContentCatalog.Monsters)Check(ArtLibrary.Load(monster.SpriteResource)!=null,"art "+monster.Id);
            foreach(var kind in new[]{"herb","potion","moon_sword","moon_shield","return_scroll","depth_compass"})
                Check(ArtLibrary.Load("Art/Items/"+kind)!=null,"art "+kind);
            foreach(var terrain in new[]{"floor_single","floor_single_cracked","wall_stone","stairs_down"})
            {
                var tile=ArtLibrary.Load("Art/Terrain/"+terrain);
                Check(tile!=null,"terrain resource "+terrain);
                Check(Mathf.Abs(tile.bounds.size.x-1)<.0001f&&Mathf.Abs(tile.bounds.size.y-1)<.0001f,"one-cell terrain "+terrain);
            }
            Click("冒険をはじめる");yield return null;Route("hub");Audio("moonlit-sanctuary");yield return Capture("02-hub");
            yield return VerifySettingsScreen();
            Click("Enter dungeon");yield return null;Route("departure");yield return Capture("03-departure");
            Click("探索を始める");yield return null;Route("dungeon");
            var game=FindFirstObjectByType<GameManager>();Check(game!=null,"runtime manager");
            yield return new WaitForSecondsRealtime(1.1f);Audio("luna-moonlit-footsteps");yield return Capture("04-dungeon");
            game.Run.Enemies.Clear();game.Renderer.Refresh(game.Run);
            var center=game.Run.Map.FloorCells().First(c=>DungeonRules.MovementDirections.All(d=>DungeonRules.CanStep(game.Run.Map,c,c+d)));
            foreach(var cell in PathTo(game.Run,center)){game.Move(cell-game.Run.PlayerCell);yield return Ready(game);}
            foreach(var d in DungeonRules.MovementDirections.Where(d=>d.x!=0&&d.y!=0))
            {
                var origin=game.Run.PlayerCell;game.Move(d);yield return Ready(game);
                Check(game.Run.PlayerCell==origin+d,"diagonal runtime move");
                yield return Capture("04-diagonal-"+d.x+"-"+d.y);
                game.Move(-d);yield return Ready(game);
            }
            typeof(DungeonRun).GetProperty("Hp").SetValue(game.Run,game.Run.MaxHp-2);
            game.UI.Refresh(game.Run,false);
            var restButton=game.GetComponentInChildren<HoldRestButton>();
            var restPointer=new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current){pointerId=8};
            int restTurns=game.Run.Turns;restButton.OnPointerDown(restPointer);
            yield return new WaitForSecondsRealtime(.75f);restButton.OnPointerUp(restPointer);yield return Ready(game);
            Check(game.Run.Turns>restTurns,"rest pointer repeats turns");
            restTurns=game.Run.Turns;yield return new WaitForSecondsRealtime(.5f);Check(game.Run.Turns==restTurns,"rest release stops");
            var held=game.GetComponentsInChildren<HoldMoveButton>().First(h=>game.Run.Map.Walkable(game.Run.PlayerCell+h.Direction*2));
            var pointer=new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current){pointerId=7};
            int heldTurns=game.Run.Turns;held.OnPointerDown(pointer);
            yield return new WaitForSecondsRealtime(.62f);held.OnPointerUp(pointer);yield return Ready(game);
            Check(game.Run.Turns>=heldTurns+2,"pointer hold repeats grid steps");
            heldTurns=game.Run.Turns;yield return new WaitForSecondsRealtime(.4f);
            Check(game.Run.Turns==heldTurns,"pointer release stops movement");
            // Generated items, actual walking and confirmation UI. Only enemies are isolated.
            foreach(var kind in new[]{"herb","moon_sword"})
            {
                if(game.Run.Bag.Any(i=>i.Kind==kind))continue;
                var item=game.Run.Items.Find(i=>i.Item.Kind==kind);Check(item!=null,"generated "+kind);
                foreach(var cell in PathTo(game.Run,item.Cell)){game.Move(cell-game.Run.PlayerCell);yield return Ready(game);}
                Check(game.Run.Bag.Any(i=>i.Id==item.Item.Id)&&!game.Run.Items.Contains(item),"walking collects item once");
            }
            Click("持ち物");Click("整頓");yield return Capture("05-bag");Click("次へ");yield return Capture("05-page2");Click("前へ");ClickBagItem(game,"moon_sword");yield return Capture("05-item-detail");Click("装備");yield return Ready(game);
            Check(game.Run.WeaponId!=null,"UI sword equip");
            Click("持ち物");yield return Capture("05b-equipped-bag");Click("閉じる");
            // A controlled adjacent attacker creates a reproducible healing opportunity.
            var adjacent=DungeonRules.Directions.First(d=>game.Run.Map.Walkable(game.Run.PlayerCell+d));
            game.Run.Enemies.Add(new EnemyData{Id=999,Cell=game.Run.PlayerCell+adjacent,Hp=50,MaximumHp=50,AttackPower=9});
            game.Wait();yield return Ready(game);game.Run.Enemies.Clear();game.Renderer.Refresh(game.Run);
            int hp=game.Run.Hp,satiety=game.Run.Satiety;Check(hp<game.Run.MaxHp,"fixed enemy damages player");
            Click("持ち物");ClickBagItem(game,"herb");Click("使う");yield return Ready(game);
            Check(game.Run.Hp>hp&&game.Run.Satiety<=satiety,"UI healing without satiety recovery");yield return Capture("06-healed");
            var stairPath=PathTo(game.Run,game.Run.Map.Stairs);
            // Stand next to the stairs so the art remains unobscured by Luna.
            foreach(var cell in stairPath.Take(Math.Max(0,stairPath.Count-1))){game.Move(cell-game.Run.PlayerCell);yield return Ready(game);}
            Check(game.Run.Map.Visible[game.Run.Map.Stairs.x,game.Run.Map.Stairs.y],"stairs revealed at approach");
            yield return Capture("06b-stairs-approach");
            string runId=game.Run.RunId;int carried=game.Run.Bag.Count;
            Click("拠点");Check(game.Modal,"return confirmation");Click("はい");yield return null;
            Route("results");yield return new WaitForSecondsRealtime(1.1f);Audio("luna-home-under-moonlight");yield return Capture("07-results");
            var savedRelics=new RelicService(Store);var savedVault=new InventoryVault(Store);
            Check(savedRelics.Profile.Runs.Count(r=>r.RunId==runId)==1,"completion persisted once to memory");
            Check(savedVault.Profile.LastRunId==runId&&savedVault.Profile.LastReturned.Count==carried,"inventory deposited to memory");
            Click("倉庫");yield return null;Route("storage");Audio("moonlit-sanctuary");yield return Capture("08-storage");
            Click("拠点へ戻る");yield return null;Route("hub");
            // Hub image command has no text label; stable GameObject name is the route.
            Click("relics");yield return null;Route("relics");yield return Capture("09-relics");
            Click("拠点へ戻る");yield return null;Route("hub");Audio("moonlit-sanctuary");yield return Capture("10-hub-returned");
            var returningSword=new InventoryVault(Store).Profile.Warehouse.First(i=>i.Kind=="moon_sword");
            Click("storage");yield return null;Route("storage");
            int swordIndex=new InventoryVault(Store).Profile.Warehouse.FindIndex(i=>i.Id==returningSword.Id);
            app.GetComponentsInChildren<Button>().Where(b=>b.interactable&&b.GetComponentsInChildren<Text>().Any(t=>t.text=="持込選択")).ElementAt(swordIndex).onClick.Invoke();yield return null;
            var selected=new InventoryVault(Store).Profile;
            Check(selected.Loadout.Count==1&&selected.Loadout[0].Id==returningSword.Id,"warehouse UI selects the returned sword");
            Check(!selected.Warehouse.Any(i=>i.Id==returningSword.Id),"selection removes warehouse ownership");
            yield return Capture("11-loadout");
            Click("拠点へ戻る");yield return null;Click("Enter dungeon");yield return null;Route("departure");
            Click("探索を始める");yield return null;Route("dungeon");
            var second=FindFirstObjectByType<GameManager>();Check(second!=null&&second.Run.RunId!=runId,"second expedition is a fresh run");
            var brought=second.Run.Bag.Single(i=>i.Id==returningSword.Id);
            Check(brought.Kind==returningSword.Kind&&brought.Enhancement==returningSword.Enhancement,"loadout keeps identity and enhancement without reroll");
            Check(second.Run.Hp==second.Run.MaxHp&&second.Run.Satiety==100,"loadout does not alter fresh health or satiety");
            var reserved=new InventoryVault(Store).Profile;
            Check(reserved.Loadout.Count==0&&reserved.ActiveRunId==second.Run.RunId&&reserved.InExpedition.Count==1,"selected ownership reserved for active expedition");
            yield return new WaitForSecondsRealtime(1.1f);Audio("luna-moonlit-footsteps");
            Click("持ち物");yield return Capture("12-carried-bag");Click("閉じる");
            string secondId=second.Run.RunId;Click("拠点");Click("はい");yield return null;Route("results");
            var returned=new InventoryVault(Store).Profile;
            Check(returned.LastRunId==secondId&&returned.DepositedRuns.Count(id=>id==secondId)==1,"second return deposits once");
            Check(string.IsNullOrEmpty(returned.ActiveRunId)&&returned.InExpedition.Count==0,"return releases expedition reservation");
            Check(returned.Warehouse.Concat(returned.Pending).Concat(returned.Loadout).Concat(returned.InExpedition).Count(i=>i.Id==returningSword.Id)==1,"returned sword has exactly one owning location");
            Check(returned.LastReturned.Single(i=>i.Id==returningSword.Id).Enhancement==returningSword.Enhancement,"second return retains original enhancement");
            yield return Capture("13-second-results");Click("拠点へ戻る");yield return null;Route("hub");
            foreach(var file in screenshots)Check(File.Exists(file)&&new FileInfo(file).Length>0,"screenshot saved "+Path.GetFileName(file));
            Finish(true,"Actual route buttons, generated pickup, equip/heal, two returns, warehouse loadout preserves ID/enhancement without duplicates, relics; isolated in-memory stores. Audio checked after fades; all 12 art resources loaded. No PlayerPrefs writes.");
        }
        static List<Vector2Int> PathTo(DungeonRun run,Vector2Int end)
        {
            var queue=new Queue<Vector2Int>();var seen=new Dictionary<Vector2Int,Vector2Int>();queue.Enqueue(run.PlayerCell);seen[run.PlayerCell]=run.PlayerCell;
            while(queue.Count>0){var cell=queue.Dequeue();if(cell==end)break;foreach(var dir in DungeonRules.Directions){var next=cell+dir;if(run.Map.Walkable(next)&&!seen.ContainsKey(next)){seen[next]=cell;queue.Enqueue(next);}}}
            var path=new List<Vector2Int>();for(var cell=end;cell!=run.PlayerCell;cell=seen[cell])path.Add(cell);path.Reverse();return path;
        }
    }
}
