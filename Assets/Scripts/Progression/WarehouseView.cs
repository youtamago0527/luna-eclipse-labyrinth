using System;
using UnityEngine;
using UnityEngine.UI;
namespace LunaEclipse.Progression
{
    public static class WarehouseView
    {
        public static void Build(RectTransform parent,InventoryVault vault,int capacity,Action close,int bagCapacity=20)
        {
            var view=UiKit.Rect("Warehouse",parent);UiKit.Place(view,0,0,430,932);
            Draw(view,vault,capacity,close,bagCapacity);
        }
        static void Draw(RectTransform view,InventoryVault vault,int capacity,Action close,int bagCapacity)
        {
            foreach(Transform child in view){child.gameObject.SetActive(false);UnityEngine.Object.Destroy(child.gameObject);}
            var bg=UiKit.Panel(view,new Color(.025f,.035f,.07f));UiKit.Place(bg.rectTransform,0,0,430,932);
            Label(view,"月蔵の倉庫",26,20,30,390,50);
            Label(view,"倉庫 "+vault.Profile.Warehouse.Count+" / "+capacity+"　保留 "+vault.Profile.Pending.Count+"個",16,20,88,390,40);
            Label(view,"次回持込 "+vault.Profile.Loadout.Count+" / "+bagCapacity+"個\n満杯の品は保留に保存されます。",13,20,133,390,50);
            var viewport=UiKit.Rect("Scroll",view);UiKit.Place(viewport,18,200,394,600);viewport.gameObject.AddComponent<RectMask2D>();
            viewport.gameObject.AddComponent<Image>().color=new Color(.04f,.055f,.095f);
            var scroll=viewport.gameObject.AddComponent<ScrollRect>();scroll.horizontal=false;scroll.viewport=viewport;scroll.movementType=ScrollRect.MovementType.Clamped;
            int rows=vault.Profile.Warehouse.Count+vault.Profile.Pending.Count+vault.Profile.LastReturned.Count+vault.Profile.Loadout.Count;
            var content=UiKit.Rect("Items",viewport);UiKit.Place(content,0,0,394,Mathf.Max(600,rows*108+220));scroll.content=content;
            float y=8;Label(content,"倉庫",18,12,y,370,35);y+=40;
            for(int i=0;i<vault.Profile.Warehouse.Count;i++){int index=i;Row(content,vault.Profile.Warehouse[i],y,"持込選択",()=>{vault.SelectForNextRun(index,bagCapacity);Draw(view,vault,capacity,close,bagCapacity);},vault.Profile.Loadout.Count<bagCapacity);var hold=UiKit.Button(content,"保留へ",()=>{vault.Withdraw(index);Draw(view,vault,capacity,close,bagCapacity);});UiKit.Place(hold.transform as RectTransform,278,y+56,104,40);hold.GetComponentInChildren<Text>().fontSize=13;y+=108;}
            Label(content,"次回持込",18,12,y,370,35);y+=40;
            for(int i=0;i<vault.Profile.Loadout.Count;i++){int index=i;Row(content,vault.Profile.Loadout[i],y,"選択解除",()=>{vault.Unselect(index,capacity);Draw(view,vault,capacity,close,bagCapacity);},true);y+=66;}
            Label(content,"持ち帰り保留",18,12,y,370,35);y+=40;
            for(int i=0;i<vault.Profile.Pending.Count;i++){int index=i;Row(content,vault.Profile.Pending[i],y,"倉庫へ",()=>{vault.Deposit(index,capacity);Draw(view,vault,capacity,close,bagCapacity);},vault.Profile.Warehouse.Count<capacity);y+=66;}
            Label(content,"前回持ち帰った品（履歴）",17,12,y,370,35);y+=40;
            foreach(var item in vault.Profile.LastReturned){Label(content,Name(item),14,12,y,370,58);y+=66;}
            content.sizeDelta=new Vector2(394,Mathf.Max(600,y+12));Label(view,vault.LastError,12,20,810,390,45);
            var back=UiKit.Button(view,"拠点へ戻る",close);UiKit.Place(back.transform as RectTransform,30,866,370,48);
        }
        static string Name(StoredItem item)=>item.DisplayName;
        static void Row(RectTransform parent,StoredItem item,float y,string action,Action onClick,bool enabled)
        {Label(parent,Name(item),14,12,y,256,58);var button=UiKit.Button(parent,action,onClick);UiKit.Place(button.transform as RectTransform,278,y+5,104,46);button.interactable=enabled;button.GetComponentInChildren<Text>().fontSize=13;}
        static void Label(RectTransform parent,string text,int size,float x,float y,float w,float h){var label=UiKit.Text(parent,text,size,new Color(.88f,.87f,.95f));UiKit.Place(label.rectTransform,x,y,w,h);}
    }
}
