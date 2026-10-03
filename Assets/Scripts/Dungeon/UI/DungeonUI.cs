using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LunaEclipse.Dungeon
{
    public sealed class DungeonUI : MonoBehaviour
    {
        static readonly Color Ink = new Color(.025f,.035f,.075f,.98f);
        static readonly Color Line = new Color(.35f,.28f,.53f);
        static readonly Color Pale = new Color(.89f,.9f,.98f);
        GameManager manager;
        RectTransform root, modal;
        Text floor, hp, belly, bag, logs;
        Image hpFill, logBackdrop, controlBackdrop, controlLine;
        Button pickup, stairs, attack, wait, bagButton;
        readonly List<Button> movement = new List<Button>();
        RawImage mini;
        Texture2D mapTexture;
        Color32[] mapColors;
        DungeonRun current;
        bool busy, deadShown;
        bool compactControls;
        int bagPage;
        float controlsTop = DungeonPresentation.ControlsTop;
        public RectTransform Viewport { get; private set; }

        public void Build(RectTransform parent, GameManager game)
        {
            root=parent; manager=game;
            Panel(root,0,0,1170,DungeonPresentation.HeaderHeight,Ink);
            controlBackdrop=Panel(root,0,DungeonPresentation.ControlsTop,1170,402,Ink);
            Label(root,"月蝕迷宮",32,Pale,36,12,430,44);
            floor=Label(root,"B1F",48,Pale,36,62,160,70);
            hp=Label(root,"HP 20 / 20",33,Pale,220,62,550,48);
            Panel(root,220,119,540,18,new Color(.16f,.12f,.22f));
            hpFill=Panel(root,220,119,540,18,new Color(.64f,.23f,.36f));
            belly=Label(root,"満腹 100 / 100",30,Pale,36,164,390,50);
            bag=Label(root,"バッグ 0 / 20",30,Pale,450,164,330,50);
            var back=Button(root,"拠点",680,20,115,50,()=>Confirm("探索を終えて拠点へ戻りますか？",()=>manager.ReturnToHub()));
            var miniPanel=Panel(root,824,16,310,204,new Color(.04f,.07f,.13f));
            var miniObject=new GameObject("Minimap",typeof(RectTransform),typeof(RawImage));
            miniObject.transform.SetParent(root,false);mini=miniObject.GetComponent<RawImage>();
            UiKit.Place(mini.rectTransform,834,26,290,184);mini.texture=Texture2D.blackTexture;
            var miniButton=miniObject.AddComponent<Button>();miniButton.targetGraphic=mini;miniButton.onClick.AddListener(ShowMap);
            Viewport=UiKit.Rect("Dungeon Viewport",root);UiKit.Place(Viewport,0,DungeonPresentation.HeaderHeight,1170,DungeonPresentation.ViewHeight);
            Panel(root,0,236,1170,4,Line);controlLine=Panel(root,0,DungeonPresentation.ControlsTop,1170,3,Line);
            // Compact thumb clusters. Each directional target remains >=44px at 390px width.
            movement.Add(Button(root,"▲",180,2134,140,132,()=>manager.Move(Vector2Int.up)));
            movement.Add(Button(root,"◀",34,2266,140,132,()=>manager.Move(Vector2Int.left)));
            movement.Add(Button(root,"▶",326,2266,140,132,()=>manager.Move(Vector2Int.right)));
            movement.Add(Button(root,"▼",180,2398,140,132,()=>manager.Move(Vector2Int.down)));
            var directions=new[]{Vector2Int.up,Vector2Int.left,Vector2Int.right,Vector2Int.down};
            for(int i=0;i<movement.Count;i++)
            {
                movement[i].onClick.RemoveAllListeners();
                var hold=movement[i].gameObject.AddComponent<HoldMoveButton>();hold.Manager=manager;hold.Direction=directions[i];
            }
            attack=Button(root,"攻撃",510,2148,354,232,()=>manager.Attack());
            attack.GetComponent<Image>().color=new Color(.27f,.095f,.17f,.98f);
            attack.GetComponentInChildren<Text>().fontSize=52;
            wait=Button(root,"待機",892,2134,246,132,()=>manager.Wait());
            pickup=Button(root,"拾う",892,2266,246,132,()=>Confirm("足元のアイテムを拾いますか？",()=>manager.PickUp()));
            bagButton=Button(root,"持ち物",892,2398,246,132,ShowBag);
            stairs=Button(root,"階段を降りる",510,2398,354,132,()=>Confirm("次の階へ降りますか？",()=>manager.Descend()));
            stairs.GetComponentInChildren<Text>().fontSize=30;
            // Logs overlay world pixels; they never reserve a separate footer.
            logBackdrop=Panel(root,20,1970,1130,146,new Color(.018f,.023f,.045f,.66f));
            logBackdrop.raycastTarget=false;
            logs=Label(root,"",30,Pale,42,1978,1086,134);logs.alignment=TextAnchor.UpperLeft;logs.raycastTarget=false;
            LayoutControls(true);
        }

        void LateUpdate(){if(root!=null&&logs!=null)LayoutControls(false);}
        void LayoutControls(bool force)
        {
            // Preserve the reference layout; only short/narrow screens need larger targets.
            float scale=Mathf.Min(Screen.safeArea.width/1170f,Screen.safeArea.height/2532f);
            bool compact=scale*132f<43.9f;
            if(!force&&compact==compactControls)return;
            compactControls=compact;controlsTop=compact?1998: DungeonPresentation.ControlsTop;
            floor.fontSize=compact?54:48;hp.fontSize=compact?44:33;
            belly.fontSize=bag.fontSize=compact?40:30;logs.fontSize=compact?44:30;
            foreach(var b in movement)b.GetComponentInChildren<Text>().fontSize=compact?44:36;
            foreach(var b in new[]{wait,pickup,bagButton})b.GetComponentInChildren<Text>().fontSize=compact?44:36;
            stairs.GetComponentInChildren<Text>().fontSize=compact?36:30;
            UiKit.Place(controlBackdrop.rectTransform,0,controlsTop,1170,2532-controlsTop);
            UiKit.Place(controlLine.rectTransform,0,controlsTop,1170,3);
            UiKit.Place(Viewport,0,DungeonPresentation.HeaderHeight,1170,controlsTop-DungeonPresentation.HeaderHeight);
            if(compact)
            {
                PlaceControl(movement[0],202,2002,176,176);
                PlaceControl(movement[1],16,2178,176,176);
                PlaceControl(movement[2],388,2178,176,176);
                PlaceControl(movement[3],202,2354,176,176);
                PlaceControl(attack,584,2002,330,344);
                PlaceControl(stairs,584,2354,330,176);
                PlaceControl(wait,934,2002,220,176);
                PlaceControl(pickup,934,2178,220,176);
                PlaceControl(bagButton,934,2354,220,176);
            }
            else
            {
                PlaceControl(movement[0],180,2134,140,132);
                PlaceControl(movement[1],34,2266,140,132);
                PlaceControl(movement[2],326,2266,140,132);
                PlaceControl(movement[3],180,2398,140,132);
                PlaceControl(attack,510,2148,354,232);
                PlaceControl(stairs,510,2398,354,132);
                PlaceControl(wait,892,2134,246,132);
                PlaceControl(pickup,892,2266,246,132);
                PlaceControl(bagButton,892,2398,246,132);
            }
            if(current!=null)Refresh(current,busy);
        }
        static void PlaceControl(Button button,float x,float y,float w,float h)
        {UiKit.Place((RectTransform)button.transform,x,y,w,h);}

        public void Refresh(DungeonRun run,bool isBusy)
        {
            current=run;busy=isBusy;
            floor.text="B"+run.Floor+"F";hp.text="HP "+run.Hp+" / "+run.MaxHp;
            UiKit.Place(hpFill.rectTransform,220,119,540*Mathf.Clamp01(run.MaxHp>0?(float)run.Hp/run.MaxHp:0),18);
            belly.text="満腹 "+run.Satiety+" / "+DungeonRules.MaximumSatiety;bag.text="バッグ "+run.Bag.Count+" / "+run.BagCapacity;
            int start=Mathf.Max(0,run.Logs.Count-3);logs.text="";
            for(int i=start;i<run.Logs.Count;i++)logs.text+=(i>start?"\n":"")+run.Logs[i];
            float logHeight=Mathf.Max(1,run.Logs.Count-start)*(compactControls?55:41)+20;
            float logTop=controlsTop-14-logHeight;
            UiKit.Place(logBackdrop.rectTransform,20,logTop,1130,logHeight);
            UiKit.Place(logs.rectTransform,42,logTop+8,1086,logHeight-16);
            bool enabled=!busy&&!run.Dead&&modal==null;
            foreach(var button in movement)SetEnabled(button,!run.Dead&&modal==null);
            SetEnabled(attack,enabled);SetEnabled(wait,enabled);SetEnabled(bagButton,!busy&&!run.Dead&&modal==null);
            SetEnabled(pickup,enabled&&run.CanPickUp);SetEnabled(stairs,enabled&&run.CanDescend);
            RefreshMap(run);
            if(run.Dead&&!deadShown){deadShown=true;ShowDeath();}
            if(!run.Dead)deadShown=false;
        }

        void RefreshMap(DungeonRun run)
        {
            int w=run.Map.Width,h=run.Map.Height;
            if(mapTexture==null||mapTexture.width!=w||mapTexture.height!=h)
            {
                if(mapTexture!=null)Destroy(mapTexture);
                mapTexture=new Texture2D(w,h,TextureFormat.RGBA32,false);mapTexture.filterMode=FilterMode.Point;mini.texture=mapTexture;
                mapColors=new Color32[w*h];
                float scale=Mathf.Min(290f/w,184f/h);
                UiKit.Place(mini.rectTransform,834+(290-w*scale)/2,26+(184-h*scale)/2,w*scale,h*scale);
            }
            var colors=mapColors;
            for(int y=0;y<h;y++)for(int x=0;x<w;x++)
            {
                var cell=new Vector2Int(x,y);
                colors[y*w+x]=!run.Map.Explored[x,y]?new Color32(5,8,16,255):run.Map.Walkable(cell)?new Color32(93,108,146,255):new Color32(26,32,53,255);
            }
            if(run.Map.Explored[run.Map.Stairs.x,run.Map.Stairs.y])colors[run.Map.Stairs.y*w+run.Map.Stairs.x]=new Color32(103,222,247,255);
            foreach(var item in run.Items)if(run.Map.Visible[item.Cell.x,item.Cell.y])colors[item.Cell.y*w+item.Cell.x]=new Color32(238,197,96,255);
            foreach(var enemy in run.Enemies)if(enemy.Hp>0&&run.Map.Visible[enemy.Cell.x,enemy.Cell.y])colors[enemy.Cell.y*w+enemy.Cell.x]=new Color32(237,101,133,255);
            colors[run.PlayerCell.y*w+run.PlayerCell.x]=new Color32(239,244,255,255);
            mapTexture.SetPixels32(colors);mapTexture.Apply(false);
        }

        void BeginModal(string title,bool large=false)
        {
            if(modal!=null)return;
            manager.SetModal(true);
            modal=UiKit.Rect("Dungeon Modal",root);UiKit.Place(modal,0,0,1170,2532);modal.SetAsLastSibling();
            Panel(modal,0,0,1170,2532,new Color(0,0,.025f,.88f));
            Panel(modal,64,large?120:480,1042,large?2350:1440,Ink);
            Label(modal,title,46,Pale,100,large?152:522,970,90);
        }
        void CloseModal()
        {
            if(modal!=null){modal.gameObject.SetActive(false);Destroy(modal.gameObject);modal=null;}
            manager.SetModal(false);
            if(current!=null)Refresh(current,manager.Busy);
        }
        void Confirm(string text,Action action)
        {
            if(busy||modal!=null||current==null||current.Dead)return;
            BeginModal("確認");Label(modal,text,38,Pale,122,910,926,180);
            Button(modal,"はい",122,1320,430,115,()=>{CloseModal();action();});
            Button(modal,"戻る",590,1320,430,115,CloseModal);
        }
        void ShowMap()
        {
            if(busy||modal!=null||current==null||current.Dead)return;
            BeginModal("探索マップ");
            var obj=new GameObject("Expanded Map",typeof(RectTransform),typeof(RawImage));obj.transform.SetParent(modal,false);
            var image=obj.GetComponent<RawImage>();image.texture=mapTexture;image.raycastTarget=false;
            float aspect=mapTexture.width/(float)mapTexture.height;float width=Mathf.Min(930,1000*aspect),height=width/aspect;
            UiKit.Place(image.rectTransform,(1170-width)/2,700,width,height);
            Button(modal,"閉じる",160,1760,850,104,CloseModal);
        }
        void ShowBag()
        {
            if(busy||modal!=null||current==null||current.Dead)return;
            int pages=Mathf.CeilToInt(current.BagCapacity/(float)DungeonRules.ItemsPerPage);
            bagPage=Mathf.Clamp(bagPage,0,pages-1);
            BeginModal("持ち物 "+current.Bag.Count+" / "+current.BagCapacity,true);
            Label(modal,"攻撃 "+current.AttackPower+" / 防御 "+current.DefensePower+"   アイテムを選択",36,Pale,110,260,940,70);
            Button(modal,"整頓",110,350,940,140,()=>{current.SortBag();bagPage=0;CloseModal();ShowBag();});
            for(int i=0;i<DungeonRules.ItemsPerPage;i++)
            {
                int index=bagPage*DungeonRules.ItemsPerPage+i;
                if(index>=current.Bag.Count){Label(modal,(index+1)+". ―",36,new Color(.4f,.4f,.5f),130,510+i*158,920,146);continue;}
                var item=current.Bag[index];string id=item.Id;var def=ContentCatalog.FindItem(item.Kind);
                bool equipped=current.WeaponId==id||current.ShieldId==id;
                var row=Button(modal,(index+1)+". "+item.DisplayName+(equipped?" 【装備中】":""),110,510+i*158,940,146,()=>{CloseModal();ShowItem(id);});
                var label=row.GetComponentInChildren<Text>();label.fontSize=38;
            }
            var prev=Button(modal,"前へ",110,2130,280,140,()=>{bagPage--;CloseModal();ShowBag();});SetEnabled(prev,bagPage>0);
            Label(modal,(bagPage+1)+" / "+pages,40,Pale,400,2130,360,140);
            var next=Button(modal,"次へ",770,2130,280,140,()=>{bagPage++;CloseModal();ShowBag();});SetEnabled(next,bagPage<pages-1);
            Button(modal,"閉じる",110,2300,940,140,CloseModal);
        }
        void ShowItem(string id)
        {
            var item=current.Bag.Find(i=>i.Id==id);if(item==null)return;
            var def=ContentCatalog.FindItem(item.Kind);bool equipped=current.WeaponId==id||current.ShieldId==id;
            BeginModal(item.DisplayName);
            var icon=Panel(modal,440,670,290,290,Color.white);icon.sprite=ArtLibrary.Load(def?.SpriteResource);icon.preserveAspect=true;icon.raycastTarget=false;if(icon.sprite==null)icon.color=Color.clear;
            Label(modal,item.Equipment&&!item.Identified?"強化値は未鑑定。装備すると判明します。\nマイナス装備は呪われて外せなくなります。":def?.Description??"",36,Pale,122,980,926,180);
            var use=Button(modal,equipped?"装備中":item.Equipment?"装備":"使う",122,1220,926,150,()=>{CloseModal();manager.UseItem(id);});SetEnabled(use,!equipped&&def!=null&&(def.Equipment||def.Healing>0));
            var drop=Button(modal,"置く",122,1410,926,150,()=>{CloseModal();Confirm(item.DisplayName+"を足元に置きますか？",()=>manager.DropItem(id));});SetEnabled(drop,!current.IsCursedEquipped(id));
            Button(modal,"持ち物へ",122,1700,926,150,()=>{CloseModal();ShowBag();});
        }
        void ShowDeath()
        {
            if(modal!=null){Destroy(modal.gameObject);modal=null;}
            BeginModal("探索終了");Label(modal,"ルナは力尽きた。",42,Pale,120,900,930,110);
            Button(modal,"再挑戦",160,1240,850,118,()=>{CloseModal();manager.Restart();});
            Button(modal,"拠点へ",160,1410,850,118,()=>{CloseModal();manager.ReturnToHub();});
        }
        public void ShowSaveFailure(Action retry)
        {
            if(modal!=null){modal.gameObject.SetActive(false);Destroy(modal.gameObject);modal=null;}
            BeginModal("保存を確認してください");
            Label(modal,"保存に失敗しました。探索データはまだ保持されています。\n空き容量などを確認して再試行してください。",36,Pale,120,800,930,300);
            Button(modal,"保存を再試行",160,1240,850,118,()=>{CloseModal();retry();});
        }
        static Image Panel(RectTransform parent,float x,float y,float w,float h,Color color)
        {var image=UiKit.Panel(parent,color);UiKit.Place(image.rectTransform,x,y,w,h);return image;}
        static Text Label(RectTransform parent,string text,int size,Color color,float x,float y,float w,float h)
        {var label=UiKit.Text(parent,text,size,color);UiKit.Place(label.rectTransform,x,y,w,h);return label;}
        static Button Button(RectTransform parent,string text,float x,float y,float w,float h,Action action)
        {var button=UiKit.Button(parent,text,action);UiKit.Place(button.transform as RectTransform,x,y,w,h);var label=button.GetComponentInChildren<Text>();label.fontSize=36;var colors=button.colors;colors.disabledColor=new Color(.34f,.34f,.4f,1);button.colors=colors;var border=button.GetComponent<Outline>();if(border!=null){border.effectColor=Line;border.effectDistance=new Vector2(2,2);}return button;}
        static void SetEnabled(Button button,bool enabled)
        {
            button.interactable=enabled;
            var label=button.GetComponentInChildren<Text>();
            if(label!=null)label.color=enabled?new Color(.95f,.86f,.66f):new Color(.36f,.37f,.44f);
            var border=button.GetComponent<Outline>();
            if(border!=null)border.effectColor=enabled?Line:new Color(.15f,.14f,.22f);
        }
        void OnDestroy(){if(mapTexture!=null)Destroy(mapTexture);}
    }
}
