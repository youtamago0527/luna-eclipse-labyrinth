using System;
using System.Collections.Generic;
using UnityEngine;

namespace LunaEclipse.Dungeon
{
    /// <summary>Non-destructive display crop. Original PNGs are retained unchanged.</summary>
    public static class ArtLibrary
    {
        [Serializable] sealed class Metrics { public Entry[] Entries; }
        [Serializable] sealed class Entry { public string Resource; public int Width,Height,MinX,MinY,MaxX,MaxY; }
        static readonly Dictionary<string,Sprite> cache=new Dictionary<string,Sprite>();
        static Metrics metrics;
        public static Sprite Load(string resource)
        {
            if(string.IsNullOrEmpty(resource))return null;
            if(cache.TryGetValue(resource,out var cached)&&cached!=null)return cached;
            var texture=Resources.Load<Texture2D>(resource);if(texture==null)return null;
            if(metrics==null){var json=Resources.Load<TextAsset>("Art/art-metrics");metrics=json!=null?JsonUtility.FromJson<Metrics>(json.text):new Metrics();}
            Rect rect=new Rect(0,0,texture.width,texture.height);
            if(metrics.Entries!=null)foreach(var entry in metrics.Entries)if(entry.Resource==resource&&entry.Width>0&&entry.Height>0)
            {
                // Bounds recorded in original top-left pixels; importer may rescale textures.
                float sx=texture.width/(float)entry.Width,sy=texture.height/(float)entry.Height;
                float x=Mathf.Max(0,entry.MinX-3)*sx,y=Mathf.Max(0,entry.Height-entry.MaxY-4)*sy;
                float right=Mathf.Min(entry.Width,entry.MaxX+4)*sx,top=Mathf.Min(entry.Height,entry.Height-entry.MinY+3)*sy;
                rect=Rect.MinMaxRect(x,y,right,top);break;
            }
            var sprite=Sprite.Create(texture,rect,new Vector2(.5f,.5f),Mathf.Max(rect.width,rect.height),0,SpriteMeshType.FullRect);
            sprite.name=resource;cache[resource]=sprite;return sprite;
        }
    }
}
