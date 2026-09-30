using System;
using UnityEngine;
using UnityEngine.UI;
namespace LunaEclipse.Progression
{
    public static class ExpeditionScreens
    {
        static readonly Color Foreground=new Color(.91f,.88f,.81f);
        public static void Departure(RectTransform parent,RelicService relics,Action<int> launch,Action back)
            =>Departure(parent,relics,null,launch,back,null);
        public static void Departure(RectTransform parent,RelicService relics,InventoryVault vault,Action<int> launch,Action back,Action openStorage)
        {
            Background(parent);Label(parent,"迷宮への出発",27,24,40,382,52);
            var bonus=relics.GetBonus();int selected=1,max=Math.Max(1,bonus.MaxStartFloor);
            Label(parent,"何階から探索しますか？",20,24,144,382,46);
            var number=Label(parent,"B1F",38,106,234,218,70);
            Button minus=null,plus=null;
            Action refresh=()=>{number.text="B"+selected+"F";minus.interactable=selected>1;plus.interactable=selected<max;};
            minus=Button(parent,"−",28,240,72,64,()=>{if(selected>1)selected--;refresh();});
            plus=Button(parent,"＋",330,240,72,64,()=>{if(selected<max)selected++;refresh();});
            refresh();
            Label(parent,"選択可能 B1〜"+max+"F\n過去最高 B"+relics.Profile.HighestFloor+"F",16,24,324,382,74);
            if(max>2){Button(parent,"1F",60,405,138,48,()=>{selected=1;refresh();});Button(parent,"最深候補",230,405,138,48,()=>{selected=max;refresh();});}
            Label(parent,"満腹度 100 から開始\n探索中の満腹度回復はできません。",18,24,490,382,82);
            Label(parent,"探索バッグ "+bonus.BagCapacity+"枠\n装備生成の強化値補正 +"+bonus.EquipmentEnhancement+"\n満腹消費まで "+(5L+bonus.SatietyIntervalBonus)+"行動",17,24,592,382,114);
            if(vault!=null)Button(parent,"持込品を選ぶ（"+vault.Profile.Loadout.Count+"個）",30,708,370,45,openStorage);
            var error=Label(parent,vault?.LastError??"",12,20,755,390,30);
            Button(parent,"探索を始める",30,790,370,58,()=>{launch?.Invoke(selected);if(vault!=null&&error!=null)error.text=vault.LastError;});
            Button(parent,"拠点へ戻る",30,864,370,48,back);
        }
        public static void Results(RectTransform parent,RelicService relics,InventoryVault vault,Action back,Action showRelics,Action showStorage)
        {
            var screen=UiKit.Rect("Expedition Results",parent);UiKit.Place(screen,0,0,430,932);
            DrawResults(screen,relics,vault,back,showRelics,showStorage);
        }
        static void DrawResults(RectTransform parent,RelicService relics,InventoryVault vault,Action back,Action showRelics,Action showStorage)
        {
            foreach(Transform child in parent){child.gameObject.SetActive(false);UnityEngine.Object.Destroy(child.gameObject);}
            Background(parent);Label(parent,"探索の記録",27,24,30,382,52);
            RelicRunRecord last=null;for(int i=relics.Profile.Runs.Count-1;i>=0;i--)if(!relics.Profile.Runs[i].RunId.StartsWith("grant:",StringComparison.Ordinal)){last=relics.Profile.Runs[i];break;}
            Label(parent,last==null?"まだ確定した探索記録はありません。":"B"+last.StartFloor+"F → B"+last.MaxFloor+"F",23,24,94,382,60);
            var viewport=UiKit.Rect("Result Scroll",parent);UiKit.Place(viewport,20,175,390,580);viewport.gameObject.AddComponent<RectMask2D>();
            viewport.gameObject.AddComponent<Image>().color=new Color(.04f,.05f,.09f);
            var scroll=viewport.gameObject.AddComponent<ScrollRect>();scroll.horizontal=false;scroll.viewport=viewport;scroll.movementType=ScrollRect.MovementType.Clamped;
            int rewards=last?.Rewards.Count??0;bool matching=last!=null&&vault.Profile.LastRunId==last.RunId;
            int itemCount=matching?vault.Profile.LastReturned.Count:0;
            var content=UiKit.Rect("Report",viewport);UiKit.Place(content,0,0,390,Math.Max(580,330+rewards*100+itemCount*53));scroll.content=content;
            float y=12;Label(content,"遺物の獲得機会 "+rewards+"件",18,12,y,366,38);y+=48;
            if(rewards==0){Label(content,"20階ごとの到達で獲得機会を記録します。",13,12,y,366,48);y+=56;}
            if(last!=null)foreach(var reward in last.Rewards)
            {
                var def=RelicService.Find(reward.RelicId);string status=reward.Claimed?"受取済み":def==null||!def.Enabled?"報酬調整待ち":"受取可能";
                Label(content,"B"+reward.Floor+"F  "+status+(def==null?"":"\n"+def.Name),14,12,y,250,82);
                if(!reward.Claimed&&def!=null&&def.Enabled)
                {string id=reward.Id;Button(content,"受け取る",274,y+14,104,48,()=>{relics.TryClaim(id);DrawResults(parent,relics,vault,back,showRelics,showStorage);});}
                y+=100;
            }
            Label(content,"持ち帰った品",18,12,y,366,38);y+=44;
            if(!matching){Label(content,"この探索の持ち帰り記録は未確定です。",13,12,y,366,48);y+=56;}
            else if(itemCount==0){Label(content,"今回の持ち帰り品はありません。",14,12,y,366,44);y+=52;}
            else foreach(var item in vault.Profile.LastReturned){Label(content,item.DisplayName,14,12,y,366,48);y+=53;}
            Label(content,"倉庫の空き待ち：保留 "+vault.Profile.Pending.Count+"個\n保留の品も保存されています。",14,12,y,366,66);y+=72;
            Label(content,relics.LastError+"\n"+vault.LastError,13,12,y,366,86);
            content.sizeDelta=new Vector2(390,Math.Max(580,y+104));
            Button(parent,"遺物",30,785,174,52,showRelics);Button(parent,"倉庫",226,785,174,52,showStorage);
            Button(parent,"拠点へ戻る",30,860,370,52,back);
        }
        static void Background(RectTransform parent){var bg=UiKit.Panel(parent,new Color(.025f,.035f,.07f));UiKit.Place(bg.rectTransform,0,0,430,932);}
        static Text Label(RectTransform parent,string text,int size,float x,float y,float w,float h){var label=UiKit.Text(parent,text,size,Foreground);UiKit.Place(label.rectTransform,x,y,w,h);return label;}
        static Button Button(RectTransform parent,string title,float x,float y,float w,float h,Action action){var button=UiKit.Button(parent,title,action);UiKit.Place(button.transform as RectTransform,x,y,w,h);return button;}
    }
}
