using System;
using UnityEngine;
using UnityEngine.UI;
namespace LunaEclipse.Progression
{
    public static class RelicView
    {
        public static void Build(RectTransform parent,RelicService service,Action close)
        {
            var bg=UiKit.Panel(parent,new Color(.025f,.035f,.07f));UiKit.Place(bg.rectTransform,0,0,430,932);
            Text(parent,"月蝕の遺物",27,20,36,390,48);
            var bonus=service.GetBonus();int pending=0;foreach(var run in service.Profile.Runs)pending+=run.Rewards.FindAll(r=>!r.Claimed).Count;
            Text(parent,"最高 B"+service.Profile.HighestFloor+"F  ·  獲得機会 "+pending+"件",15,20,95,390,35);
            Text(parent,"報酬内容・ランクは調整待ち。未確定分は記録のみ。",12,18,133,394,38);
            var viewport=UiKit.Rect("Relic list",parent);UiKit.Place(viewport,18,186,394,635);viewport.gameObject.AddComponent<RectMask2D>();
            var image=viewport.gameObject.AddComponent<Image>();image.color=new Color(.04f,.05f,.10f);
            var scroll=viewport.gameObject.AddComponent<ScrollRect>();scroll.horizontal=false;scroll.viewport=viewport;scroll.movementType=ScrollRect.MovementType.Clamped;
            var definitions=Array.FindAll(RelicBalance.Definitions,definition=>definition.Enabled);
            var content=UiKit.Rect("Relics",viewport);UiKit.Place(content,0,0,394,definitions.Length*173);scroll.content=content;
            for(int i=0;i<definitions.Length;i++)
            {
                var d=definitions[i];int level=service.Level(d.Id);float y=i*173;
                var panel=UiKit.Panel(content,new Color(.07f,.085f,.15f));UiKit.Place(panel.rectTransform,5,y+4,384,163);
                Text(content,d.Name+(d.Enabled?"":" [試作案]"),19,18,y+12,356,34);
                Text(content,"Lv."+level+" / "+(d.MaxLevel>0?d.MaxLevel.ToString():"未定")+"  RANK "+(service.Balance.EnableTrialRewardTable?d.Rank:"未定"),13,18,y+49,356,27);
                Text(content,d.Description,13,18,y+80,356,43);
                Text(content,d.Enabled?"現在 +"+(long)level*d.PerLevel+"  / NEXT "+(d.MaxLevel>0&&level>=d.MaxLevel?"MAX":"+"+((long)level+1)*d.PerLevel):"効果・抽選とも無効",12,18,y+126,356,27);
            }
            Text(parent,service.LastError,12,20,825,390,32);
            var back=UiKit.Button(parent,"拠点へ戻る",close);UiKit.Place(back.transform as RectTransform,30,865,370,48);
        }
        static void Text(RectTransform p,string text,int size,float x,float y,float w,float h)
        {var t=UiKit.Text(p,text,size,new Color(.88f,.87f,.95f));UiKit.Place(t.rectTransform,x,y,w,h);}
    }
}
