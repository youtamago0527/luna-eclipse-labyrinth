using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using System;
using System.Collections.Generic;
using LunaEclipse.Progression;
using LunaEclipse.Audio;
namespace LunaEclipse {
 public sealed class LunaApp:MonoBehaviour {
  RectTransform safeArea,screen; Image backing; Vector2 designSize=new Vector2(430,932); AudioSource bgm,se; AudioClip click; string route="title"; Rect lastSafe; int lastWidth,lastHeight;
  RelicService relics; InventoryVault vault; LunaMusicPlayer music; Dungeon.GameManager dungeon;
  int selectedFloor=1, lastMusicFloor;
  public string CurrentRoute=>route;
  static readonly Color Gold=new Color(.92f,.81f,.56f);
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
  static void Boot(){if(FindFirstObjectByType<LunaApp>()==null)new GameObject("LunaApp").AddComponent<LunaApp>();if(Array.IndexOf(Environment.GetCommandLineArgs(),"--luna-qa")>=0)FindFirstObjectByType<LunaApp>().Navigate("dungeon");}
  void Awake(){
   Application.targetFrameRate=60;
   bool productionQA=Array.IndexOf(Environment.GetCommandLineArgs(),"--luna-production-qa")>=0||Array.IndexOf(Environment.GetCommandLineArgs(),"--luna-qa")>=0||Array.IndexOf(Environment.GetCommandLineArgs(),"--luna-soak-qa")>=0||Array.IndexOf(Environment.GetCommandLineArgs(),"--luna-frame-qa")>=0||Array.IndexOf(Environment.GetCommandLineArgs(),"--luna-motion-qa")>=0;
   var store=LocalSettings.SessionOnly?Dungeon.ProductionPlaytest.Store:null;
   AudioListener.volume=productionQA?0f:1f; // Automated tests inspect sources without sounding through the night.
   relics=new RelicService(store);vault=new InventoryVault(store);music=gameObject.AddComponent<LunaMusicPlayer>();
   vault.RecoverInterrupted(relics.GetBonus().WarehouseCapacity);
   if(FindFirstObjectByType<AudioListener>()==null)gameObject.AddComponent<AudioListener>();
   var canvasGo=new GameObject("Luna Canvas",typeof(RectTransform),typeof(Canvas),typeof(GraphicRaycaster));canvasGo.transform.SetParent(transform);var canvas=canvasGo.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;
   var scaler=canvasGo.AddComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ConstantPixelSize;
   backing=UiKit.Panel(canvasGo.transform,Color.black);backing.raycastTarget=false;backing.rectTransform.anchorMin=Vector2.zero;backing.rectTransform.anchorMax=Vector2.one;backing.rectTransform.offsetMin=backing.rectTransform.offsetMax=Vector2.zero;
   safeArea=UiKit.Rect("Safe Area",canvasGo.transform);screen=UiKit.Rect("Portrait 430x932",safeArea);screen.anchorMin=screen.anchorMax=screen.pivot=new Vector2(.5f,.5f);screen.sizeDelta=new Vector2(430,932);
   if(FindFirstObjectByType<EventSystem>()==null)new GameObject("EventSystem",typeof(EventSystem),typeof(StandaloneInputModule));
   bgm=gameObject.AddComponent<AudioSource>();bgm.loop=true;bgm.clip=Resources.Load<AudioClip>("Audio/moonlit-sanctuary");se=gameObject.AddComponent<AudioSource>();click=Resources.Load<AudioClip>("Audio/ui-select");LocalSettings.Changed+=UpdateAudio;Fit();Navigate(SceneManager.GetActiveScene().name=="Dungeon"?"dungeon":"title");
  }
  void OnDestroy(){LocalSettings.Changed-=UpdateAudio;}
  void Update(){if(Screen.width!=lastWidth||Screen.height!=lastHeight||Screen.safeArea!=lastSafe)Fit();if(route=="dungeon"&&dungeon!=null&&dungeon.Run.Floor!=lastMusicFloor){lastMusicFloor=dungeon.Run.Floor;music.Play(lastMusicFloor>=10?LunaMusicPlayer.Mood.Deep:LunaMusicPlayer.Mood.Exploration);}}
  void Fit(){lastWidth=Screen.width;lastHeight=Screen.height;lastSafe=Screen.safeArea;safeArea.anchorMin=new Vector2(lastSafe.x/Screen.width,lastSafe.y/Screen.height);safeArea.anchorMax=new Vector2(lastSafe.xMax/Screen.width,lastSafe.yMax/Screen.height);safeArea.offsetMin=safeArea.offsetMax=Vector2.zero;screen.sizeDelta=designSize;screen.localScale=Vector3.one*Mathf.Min(lastSafe.width/designSize.x,lastSafe.height/designSize.y);}
  void UpdateAudio(){if(bgm==null)return;bgm.volume=LocalSettings.BgmVolume;music.SetVolume(LocalSettings.BgmVolume);if(route=="title"||route=="dungeon"||route=="results"||bgm.volume<=0){bgm.Pause();}else if(!bgm.isPlaying&&bgm.clip!=null)bgm.Play();}
  public void Navigate(string next){
   if(next=="dungeon"&&route=="hub")next="departure";
   string expeditionId=null;var loadout=new List<Dungeon.ItemData>();
   if(next=="dungeon"){
    if(route=="dungeon"&&dungeon!=null)return;
    var limits=relics.GetBonus();expeditionId=Guid.NewGuid().ToString("N");
    if(!vault.RecoverInterrupted(limits.WarehouseCapacity)||!vault.BeginExpedition(expeditionId,limits.BagCapacity))next="departure";
    else foreach(var item in vault.GetExpeditionItems(expeditionId))loadout.Add(new Dungeon.ItemData{Id=item.Id,Kind=item.Kind,Name=item.Name,Enhancement=item.Enhancement});
   }
   route=next;foreach(Transform child in screen){child.gameObject.SetActive(false);Destroy(child.gameObject);}
   dungeon=null;designSize=next=="dungeon"?new Vector2(1170,2532):new Vector2(430,932);backing.color=next=="dungeon"?Color.clear:Color.black;Fit();
   if(next=="title")Title();
   else if(next=="hub")HubView.Build(screen,Navigate,relics.Profile.HighestFloor,relics.Profile.Runs.FindAll(r=>!r.RunId.StartsWith("grant:")).Count);
   else if(next=="settings")SettingsView.Build(screen,Navigate);
   else if(next=="relics")RelicView.Build(screen,relics,()=>Navigate("hub"));
   else if(next=="storage")WarehouseView.Build(screen,vault,relics.GetBonus().WarehouseCapacity,()=>Navigate("hub"),relics.GetBonus().BagCapacity);
   else if(next=="departure")ExpeditionScreens.Departure(screen,relics,vault,floor=>{selectedFloor=floor;Navigate("dungeon");},()=>Navigate("hub"),()=>Navigate("storage"));
   else if(next=="results")ExpeditionScreens.Results(screen,relics,vault,()=>Navigate("hub"),()=>Navigate("relics"),()=>Navigate("storage"));
   else if(next=="dungeon"){
    var host=UiKit.Rect("Dungeon",screen);host.anchorMin=Vector2.zero;host.anchorMax=Vector2.one;host.offsetMin=host.offsetMax=Vector2.zero;
    var bonus=relics.GetBonus();bool qa=Array.IndexOf(Environment.GetCommandLineArgs(),"--luna-qa")>=0;
    var modifiers=qa?null:new Dungeon.RunModifiers{EquipmentEnhancement=bonus.EquipmentEnhancement,SatietyIntervalBonus=bonus.SatietyIntervalBonus,BagCapacity=bonus.BagCapacity};
    dungeon=host.gameObject.AddComponent<Dungeon.GameManager>();dungeon.Initialise(host,()=>Navigate("results"),runModifiers:modifiers,initialFloor:qa?1:Mathf.Clamp(selectedFloor,1,bonus.MaxStartFloor),onCompleted:FinishRun,runId:expeditionId,initialItems:loadout);lastMusicFloor=0;
   }else Placeholder(next);
   foreach(var b in screen.GetComponentsInChildren<Button>())b.onClick.AddListener(()=>{if(click!=null)se.PlayOneShot(click,LocalSettings.SeVolume);});
   if(next=="results")music.Play(LunaMusicPlayer.Mood.Return);else if(next!="dungeon")music.StopMusic();UpdateAudio();
  }
  bool FinishRun(Dungeon.DungeonRun run){
   if(relics.CompleteRun(run.RunId,run.Floor,run.StartFloor)==null)return false;
   if(vault.Profile.DepositedRuns.Contains(run.RunId))return true;
   var items=new List<StoredItem>();foreach(var item in run.Bag)items.Add(new StoredItem{Id=item.Id,Name=item.Name,Kind=item.Kind,Enhancement=item.Enhancement});
   return vault.DepositRun(run.RunId,items,relics.GetBonus().WarehouseCapacity);
  }
  void Title(){UiKit.Picture(screen,"Hub/moon-ruins",0,0,430,932);var shade=UiKit.Panel(screen,new Color(0,0,.025f,.6f));UiKit.Place(shade.rectTransform,0,0,430,932);var moon=UiKit.Text(screen,"☾",72,Gold);UiKit.Place(moon.rectTransform,80,160,270,120);var title=UiKit.Text(screen,"ルナと月蝕の迷宮",32,Gold);UiKit.Place(title.rectTransform,18,312,394,85);var sub=UiKit.Text(screen,"月明かりに導かれ、姿を変える迷宮へ。",14,Color.white);UiKit.Place(sub.rectTransform,20,414,390,50);var start=UiKit.Button(screen,"冒険をはじめる",()=>Navigate("hub"));UiKit.Place((RectTransform)start.transform,55,550,320,72);var version=UiKit.Text(screen,"UNITY · ダンジョン基盤 第1段階",11,Color.gray);UiKit.Place(version.rectTransform,20,875,390,30);}
  void Placeholder(string destination){var p=UiKit.Panel(screen,new Color(.012f,.025f,.06f));UiKit.Place(p.rectTransform,0,0,430,932);string label=destination switch{"dungeon"=>"迷宮入口","equipment"=>"装備","storage"=>"倉庫","relics"=>"遺物","shop"=>"買い物","missions"=>"ミッション","results"=>"探索記録","encyclopedia"=>"図鑑","notice"=>"お知らせ","achievements"=>"実績",_=>destination};var h=UiKit.Text(screen,label,28,Gold);UiKit.Place(h.rectTransform,20,220,390,70);var t=UiKit.Text(screen,destination=="dungeon"?"ダンジョンUIは変更待ちです。\n今回のUnity移植は拠点までです。":"Unity版への移植準備中です。\n既存Web版の保存データは変更しません。",18,Color.white);UiKit.Place(t.rectTransform,35,325,360,150);var back=UiKit.Button(screen,"拠点へ戻る",()=>Navigate("hub"));UiKit.Place((RectTransform)back.transform,65,570,300,60);}
 }
}
