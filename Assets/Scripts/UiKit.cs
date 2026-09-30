using System;
using UnityEngine;
using UnityEngine.UI;
namespace LunaEclipse {
 public static class UiKit {
  static Font font;
  public static Font Font {get {if(font==null) font=Resources.Load<Font>("Fonts/NotoSansJP"); if(font==null) font=UnityEngine.Font.CreateDynamicFontFromOSFont(new[]{"Yu Gothic","Meiryo","Arial"},24); return font;}}
  public static RectTransform Rect(string name, Transform parent) {var go=new GameObject(name,typeof(RectTransform));go.transform.SetParent(parent,false);return (RectTransform)go.transform;}
  public static void Place(RectTransform r,float x,float y,float w,float h){r.anchorMin=r.anchorMax=new Vector2(0,1);r.pivot=new Vector2(0,1);r.anchoredPosition=new Vector2(x,-y);r.sizeDelta=new Vector2(w,h);}
  public static Image Panel(Transform parent,Color color){var r=Rect("Panel",parent);var v=r.gameObject.AddComponent<Image>();v.color=color;return v;}
  public static Text Text(Transform parent,string text,int size,Color color){var r=Rect("Text",parent);var v=r.gameObject.AddComponent<Text>();v.font=Font;v.text=text;v.fontSize=size;v.color=color;v.raycastTarget=false;v.alignment=TextAnchor.MiddleCenter;v.horizontalOverflow=HorizontalWrapMode.Wrap;v.verticalOverflow=VerticalWrapMode.Overflow;return v;}
  public static Button Button(Transform parent,string label,Action click){var image=Panel(parent,new Color(.025f,.06f,.12f,.96f));image.name=label;var b=image.gameObject.AddComponent<Button>();b.targetGraphic=image;b.onClick.AddListener(()=>click?.Invoke());var outline=image.gameObject.AddComponent<Outline>();outline.effectColor=new Color(.66f,.53f,.3f);outline.effectDistance=new Vector2(1,1);var t=Text(image.transform,label,18,new Color(.95f,.86f,.66f));t.rectTransform.anchorMin=Vector2.zero;t.rectTransform.anchorMax=Vector2.one;t.rectTransform.offsetMin=new Vector2(6,4);t.rectTransform.offsetMax=new Vector2(-6,-4);return b;}
  public static Image Picture(Transform parent,string path,float x,float y,float w,float h){var v=Panel(parent,Color.white);v.sprite=Resources.Load<Sprite>(path);v.preserveAspect=true;v.raycastTarget=false;Place(v.rectTransform,x,y,w,h);return v;}
 }
}
