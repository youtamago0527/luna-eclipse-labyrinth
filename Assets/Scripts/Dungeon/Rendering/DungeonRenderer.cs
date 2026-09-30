using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace LunaEclipse.Dungeon
{
    /// <summary>Event-driven world view. Coordinates are Unity x-right, y-up.</summary>
    public sealed class DungeonRenderer : MonoBehaviour
    {
        const int WorldLayer = 30;
        const float ViewWidth = DungeonPresentation.TilesAcross;
        GameObject world;
        Camera worldCamera;
        Tilemap tiles;
        RectTransform viewport;
        SpriteRenderer player;
        readonly Dictionary<int, SpriteRenderer> enemies = new Dictionary<int, SpriteRenderer>();
        readonly Stack<SpriteRenderer> enemyPool = new Stack<SpriteRenderer>();
        readonly List<SpriteRenderer> items = new List<SpriteRenderer>();
        readonly List<UnityEngine.Object> generated = new List<UnityEngine.Object>();
        readonly Dictionary<string, Sprite[]> luna = new Dictionary<string, Sprite[]>();
        Tile[] floors;
        Tile[] walls;
        Tile stair, fog;
        readonly HashSet<Vector3Int> revealedTiles = new HashSet<Vector3Int>();
        Sprite slime, herb, healthPixel;
        object renderedMap;
        DungeonRun current;
        int displayedHp;
        readonly Vector3[] viewportCorners = new Vector3[4];
        Rect previousRect;

        public void Initialise(DungeonRun run, RectTransform area)
        {
            current = run; viewport = area;
            world = new GameObject("Dungeon World"); world.layer = WorldLayer;
            var grid = world.AddComponent<Grid>(); grid.cellSize = Vector3.one;
            var mapObject = new GameObject("Stone Tilemap", typeof(Tilemap), typeof(TilemapRenderer));
            mapObject.layer = WorldLayer; mapObject.transform.SetParent(world.transform, false);
            mapObject.transform.localPosition = new Vector3(-.5f, -.5f, 0);
            tiles = mapObject.GetComponent<Tilemap>();
            mapObject.GetComponent<TilemapRenderer>().sortingOrder = 0;
            var cameraObject = new GameObject("Dungeon Camera", typeof(Camera));
            cameraObject.transform.SetParent(world.transform, false);
            worldCamera = cameraObject.GetComponent<Camera>(); worldCamera.orthographic = true;
            worldCamera.clearFlags = CameraClearFlags.SolidColor; worldCamera.backgroundColor = new Color(.07f, .085f, .125f);
            worldCamera.cullingMask = 1 << WorldLayer; worldCamera.depth = 10;
            worldCamera.nearClipPlane = .1f; worldCamera.farClipPlane = 40;
            floors = new Tile[6];
            for(int i=0;i<floors.Length;i++){int variation=i;floors[i]=ResourceTile(i==5?"floor_single_cracked":"floor_single",()=>Stone(variation));}
            walls = new[] { ResourceTile("wall_stone",()=>Wall(0)), ResourceTile("wall_stone",()=>Wall(1)), ResourceTile("wall_stone",()=>Wall(2)) }; stair = ResourceTile("stairs_down",()=>Stairs());
            // Unexplored cells use identical, featureless mist: no hidden topology leaks.
            fog = MakeTile(Texture((x,y) => {
                float grain=(Hash(x,y,61)%13)/2400f;
                return new Color(.07f+grain,.085f+grain,.125f+grain);
            }));
            slime = Creature(false); herb = Creature(true);
            healthPixel = MakeSprite(Texture((x,y) => Color.white), new Vector2(0,.5f));
            foreach (string direction in new[] { "down", "left", "right", "up" })
            {
                luna["idle/" + direction] = new[] { Resources.Load<Sprite>("Luna/idle/" + direction) };
                foreach (string action in new[] { "walk", "attack" })
                {
                    int count = action == "walk" ? 4 : 3; var frames = new Sprite[count];
                    for (int i = 0; i < count; i++) frames[i] = Resources.Load<Sprite>("Luna/" + action + "/" + direction + "_" + (i + 1).ToString("00"));
                    luna[action + "/" + direction] = frames;
                }
            }
            player = Entity("Luna", null, 30);
            var reference = luna["idle/down"][0];
            float bodyHeight = reference.bounds.size.y * DungeonPresentation.LunaBodyCanvasRatio;
            // Normalise actual imported geometry, including any importer/atlas rescaling.
            // A single scale for every direction/frame avoids animation size jitter.
            player.transform.localScale = Vector3.one * (DungeonPresentation.LunaHeight / bodyHeight);
            Sprite shadowSprite = MakeSprite(Texture((x,y) => {
                float d=(x-32)*(x-32)/900f+(y-32)*(y-32)/90f;
                return d<1 ? new Color(.005f,.008f,.02f,.48f*(1-d)) : Color.clear;
            }), new Vector2(.5f,.5f));
            var shadow = Entity("Luna ground shadow",shadowSprite,5);
            shadow.transform.SetParent(player.transform,false);shadow.transform.localPosition=new Vector3(0,.025f,.01f);
            shadow.transform.localScale=new Vector3(.66f,.66f,1)/player.transform.localScale.x;
            Refresh(run);
        }

        public void Refresh(DungeonRun run)
        {
            if (world == null) return;
            current = run; SyncTiles(run); SyncEntities(run, false);
            displayedHp=run.Hp;player.color=Color.white;
            foreach(var renderer in enemies.Values)renderer.color=Color.white;
            player.transform.position = Position(run.PlayerCell) + new Vector3(0, DungeonPresentation.LunaFootOffset, 0);
            SetPlayerFrame(run, "idle", 0);
            FitCamera(); Follow(Position(run.PlayerCell));
        }

        public IEnumerator Animate(DungeonRun run, float duration)
        {
            if (world == null) yield break;
            current = run;
            if (!ReferenceEquals(renderedMap, run.Map)) { Refresh(run); yield break; }
            Vector3 origin = player.transform.position;
            Vector3 destination = Position(run.PlayerCell) + new Vector3(0, DungeonPresentation.LunaFootOffset, 0);
            var origins = new Dictionary<int, Vector3>();
            foreach (var pair in enemies) origins[pair.Key] = pair.Value.transform.position;
            bool walking = run.LastAction == "move", attacking = run.LastAction == "attack";
            SpriteRenderer struck=null;
            if(attacking&&run.LastAttackCell.HasValue)foreach(var renderer in enemies.Values)
                if(Vector2.Distance(renderer.transform.position,run.LastAttackCell.Value)<.1f){struck=renderer;break;}
            float elapsed = 0f; duration = attacking ? .30f : Mathf.Max(.04f, duration);
            while (elapsed < duration)
            {
                float t = Mathf.Clamp01(elapsed / duration);
                // One interpolation drives actor and camera; no second camera lag.
                float eased = t * t * (3f - 2f * t);
                player.transform.position = Vector3.Lerp(origin, destination, eased);
                SetPlayerFrame(run, walking ? "walk" : attacking ? "attack" : "idle", t);
                if(struck!=null)struck.color=Color.Lerp(Color.white,new Color(1f,.35f,.55f),Mathf.Sin(t*Mathf.PI));
                FitCamera(); Follow(player.transform.position - Vector3.up * DungeonPresentation.LunaFootOffset);
                elapsed += Time.deltaTime; yield return null;
            }
            player.transform.position = destination;
            SetPlayerFrame(run, "idle", 0);
            SyncTiles(run); SyncEntities(run, true);
            // Resolve enemies only after the player's visual action has completed.
            elapsed = 0f;
            while (elapsed < .12f)
            {
                float t = Mathf.Clamp01(elapsed / .12f);
                if(run.Hp<displayedHp)player.color=Color.Lerp(Color.white,new Color(1f,.42f,.48f),Mathf.Sin(t*Mathf.PI));
                foreach (var enemy in run.Enemies)
                {
                    if (!enemies.TryGetValue(enemy.Id, out SpriteRenderer renderer)) continue;
                    Vector3 start = origins.TryGetValue(enemy.Id, out Vector3 old) ? old : Position(enemy.Cell);
                    renderer.transform.position = Vector3.Lerp(start, Position(enemy.Cell), t);
                }
                FitCamera(); Follow(player.transform.position - Vector3.up * DungeonPresentation.LunaFootOffset);
                elapsed += Time.deltaTime; yield return null;
            }
            Refresh(run);
        }

        void LateUpdate()
        {
            if (worldCamera == null || viewport == null) return;
            if (FitCamera()) Follow(player.transform.position - Vector3.up * DungeonPresentation.LunaFootOffset);
        }

        bool FitCamera()
        {
            viewport.GetWorldCorners(viewportCorners);
            Canvas canvas = viewport.GetComponentInParent<Canvas>();
            Camera uiCamera = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay ? canvas.worldCamera : null;
            Vector2 min = RectTransformUtility.WorldToScreenPoint(uiCamera, viewportCorners[0]);
            Vector2 max = RectTransformUtility.WorldToScreenPoint(uiCamera, viewportCorners[2]);
            Rect rect = Rect.MinMaxRect(Mathf.Clamp01(min.x / Screen.width), Mathf.Clamp01(min.y / Screen.height), Mathf.Clamp01(max.x / Screen.width), Mathf.Clamp01(max.y / Screen.height));
            if (rect.width <= 0 || rect.height <= 0) return false;
            bool changed = rect != previousRect; previousRect = rect; worldCamera.rect = rect;
            float aspect = rect.width * Screen.width / (rect.height * Screen.height);
            worldCamera.orthographicSize = ViewWidth / (2f * aspect);
            return changed;
        }

        void Follow(Vector3 focus)
        {
            float halfW = ViewWidth * .5f, halfH = worldCamera.orthographicSize;
            float x = current.Map.Width < ViewWidth ? (current.Map.Width - 1) * .5f : Mathf.Clamp(focus.x, halfW - .5f, current.Map.Width - .5f - halfW);
            float y = current.Map.Height < halfH * 2 ? (current.Map.Height - 1) * .5f : Mathf.Clamp(focus.y, halfH - .5f, current.Map.Height - .5f - halfH);
            worldCamera.transform.position = new Vector3(x, y, -10);
        }

        void SyncTiles(DungeonRun run)
        {
            if (!ReferenceEquals(renderedMap, run.Map))
            {
                tiles.ClearAllTiles(); revealedTiles.Clear();
                for (int y = 0; y < run.Map.Height; y++) for (int x = 0; x < run.Map.Width; x++)
                {
                    var cell = new Vector2Int(x, y);
                    tiles.SetTile(new Vector3Int(x, y, 0), fog);
                }
                renderedMap = run.Map;
            }
            for (int y = 0; y < run.Map.Height; y++) for (int x = 0; x < run.Map.Width; x++)
            {
                var key = new Vector3Int(x,y,0);
                if(run.Map.Explored[x,y] && revealedTiles.Add(key))
                {
                    var cell = new Vector2Int(x,y); uint variation=Hash(x,y,19);
                    Tile tile = cell == run.Map.Stairs ? stair : run.Map.Walkable(cell) ? floors[variation%29==0 ? 5 : (int)(variation%5)] : walls[(int)(variation%3)];
                    tiles.SetTile(key,tile);
                }
                float light = run.Map.Visible[x, y] ? 1f : run.Map.Explored[x, y] ? .30f : 1f;
                if(run.Map.Explored[x,y])light*=run.Map.Walkable(new Vector2Int(x,y))?1.08f:.70f;
                tiles.SetColor(key, new Color(light, light, light, 1));
            }
        }

        void SyncEntities(DungeonRun run, bool preservePositions)
        {
            var alive = new HashSet<int>();
            foreach (var enemy in run.Enemies)
            {
                if (enemy.Hp <= 0) continue;
                alive.Add(enemy.Id);
                if (!enemies.TryGetValue(enemy.Id, out SpriteRenderer renderer))
                {
                    renderer = enemyPool.Count > 0 ? enemyPool.Pop() : Entity("Slime", slime, 20);
                    enemies.Add(enemy.Id, renderer); renderer.transform.position = Position(enemy.Cell);
                }
                renderer.gameObject.SetActive(true); renderer.enabled = run.Map.Visible[enemy.Cell.x, enemy.Cell.y];
                renderer.sprite=ArtLibrary.Load(ContentCatalog.FindMonster(enemy.Archetype).SpriteResource)??slime;
                // Sprite canvas is normalised by visible bounds; wide wings still fit one cell.
                renderer.transform.localScale=Vector3.one*.82f;
                var health=renderer.transform.Find("Health");
                if(health!=null){health.gameObject.SetActive(renderer.enabled);health.Find("Fill").localScale=new Vector3(.6f*Mathf.Clamp01(enemy.Hp/(float)enemy.MaxHp),.045f,1);}
                if (!preservePositions) renderer.transform.position = Position(enemy.Cell);
            }
            var remove = new List<int>();
            foreach (var pair in enemies) if (!alive.Contains(pair.Key)) { pair.Value.gameObject.SetActive(false); enemyPool.Push(pair.Value); remove.Add(pair.Key); }
            foreach (int id in remove) enemies.Remove(id);
            while (items.Count < run.Items.Count) items.Add(Entity("Herb", herb, 10));
            for (int i = 0; i < items.Count; i++)
            {
                bool active = i < run.Items.Count;
                items[i].gameObject.SetActive(active);
                if (!active) continue;
                Vector2Int cell = run.Items[i].Cell; items[i].transform.position = Position(cell);
                items[i].sprite=ArtLibrary.Load(ContentCatalog.FindItem(run.Items[i].Item.Kind)?.SpriteResource)??herb;
                items[i].transform.localScale=Vector3.one*.62f;
                items[i].enabled = run.Map.Visible[cell.x, cell.y];
            }
        }

        void SetPlayerFrame(DungeonRun run, string action, float time)
        {
            string direction = run.Facing.x < 0 ? "left" : run.Facing.x > 0 ? "right" : run.Facing.y > 0 ? "up" : "down";
            Sprite[] frames = luna[action + "/" + direction];
            player.sprite = frames[Mathf.Min(frames.Length - 1, Mathf.FloorToInt(time * frames.Length))];
            player.enabled = player.sprite != null;
        }
        static Vector3 Position(Vector2Int cell) => new Vector3(cell.x, cell.y, 0);
        SpriteRenderer Entity(string name, Sprite sprite, int order)
        {
            var go = new GameObject(name); go.layer = WorldLayer; go.transform.SetParent(world.transform, false);
            var renderer = go.AddComponent<SpriteRenderer>(); renderer.sprite = sprite; renderer.sortingOrder = order;
            if(name=="Slime"){
                var bar=new GameObject("Health");bar.layer=WorldLayer;bar.transform.SetParent(go.transform,false);bar.transform.localPosition=new Vector3(-.3f,.39f,0);
                foreach(string part in new[]{"Base","Fill"}){
                    var segment=new GameObject(part);segment.layer=WorldLayer;segment.transform.SetParent(bar.transform,false);segment.transform.localScale=new Vector3(.6f,.045f,1);
                    var sr=segment.AddComponent<SpriteRenderer>();sr.sprite=healthPixel;sr.sortingOrder=part=="Base"?40:41;sr.color=part=="Base"?new Color(.035f,.045f,.075f):new Color(.65f,.86f,.55f);
                }
            }
            return renderer;
        }
        Tile MakeTile(Texture2D texture)
        {
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, 64, 64), new Vector2(.5f, .5f), 64); generated.Add(sprite);
            Tile tile = ScriptableObject.CreateInstance<Tile>(); tile.sprite = sprite; tile.flags = TileFlags.None; generated.Add(tile); return tile;
        }
        Tile ResourceTile(string name,Func<Texture2D> fallback)
        {
            var sprite=ArtLibrary.Load("Art/Terrain/"+name);
            if(sprite==null)return MakeTile(fallback());
            var tile=ScriptableObject.CreateInstance<Tile>();tile.sprite=sprite;tile.flags=TileFlags.None;generated.Add(tile);return tile;
        }
        Texture2D Texture(Func<int, int, Color> color)
        {
            var texture = new Texture2D(64, 64, TextureFormat.RGBA32, false); texture.filterMode = FilterMode.Point; texture.wrapMode = TextureWrapMode.Clamp;
            var pixels = new Color[64 * 64]; for (int y = 0; y < 64; y++) for (int x = 0; x < 64; x++) pixels[y * 64 + x] = color(x, y);
            texture.SetPixels(pixels); texture.Apply(false, true); generated.Add(texture); return texture;
        }
        Sprite MakeSprite(Texture2D texture,Vector2 pivot){var sprite=Sprite.Create(texture,new Rect(0,0,64,64),pivot,64);generated.Add(sprite);return sprite;}
        static uint Hash(int x,int y,int seed){unchecked{uint v=(uint)(x*374761393+y*668265263+seed*1442695041);v=(v^(v>>13))*1274126177;return v^(v>>16);}}
        Texture2D Stone(int variant) => Texture((x, y) =>
        {
            int left=2+(int)(Hash(y/9,variant,4)%2),right=61-(int)(Hash(y/11,variant,9)%2);
            int bottom=2+(int)(Hash(x/8,variant,14)%2),top=61-(int)(Hash(x/10,variant,8)%2);
            if(x<left||x>right||y<bottom||y>top)return new Color(.055f,.07f,.115f);
            // Slightly chipped corners and uneven bevels instead of perfectly square plates.
            if((x<7&&y<7)||(x>57&&y>57))return new Color(.085f,.105f,.16f);
            if(y==top||x==left)return new Color(.35f,.40f,.49f);
            if(y<=bottom+1||x>=right-1)return new Color(.14f,.175f,.235f);
            float grain=(Hash(x,y,variant)%100)/100f-.5f;
            float broad=(Hash(x/8,y/8,variant+30)%100)/100f-.5f;
            float light=(y/64f)*.025f+grain*.022f+broad*.017f+(variant%3)*.007f;
            int crackX=25+(int)(Hash(y/8,variant,20)%9)+(y%8)/3;
            if(variant==5&&Math.Abs(x-crackX)<=1)return new Color(.49f,.29f,.65f);
            if(variant<5&&y>30&&y<54&&x==19+(y-30)/4+(variant*3))return new Color(.14f,.17f,.24f);
            return new Color(.215f+light,.255f+light,.335f+light);
        });
        Texture2D Wall(int variant) => Texture((x, y) =>
        {
            int row = y / 16, bx = (x + row % 2 * 16) % 32, by = y % 16;
            if (bx < 2 || by < 2) return new Color(.04f, .045f, .075f);
            if (by == 14) return new Color(.27f, .28f, .38f);
            if(bx==2)return new Color(.2f,.21f,.3f);
            float shade = (y / 64f) * .055f + (Hash(x,y,variant)%31)/1600f+(Hash(x/32,row,variant)%9)/300f;
            return new Color(.10f + shade, .105f + shade, .17f + shade);
        });
        Texture2D Stairs() => Texture((x, y) =>
        {
            if (x < 7 || x > 56 || y < 5 || y > 58) return new Color(.36f, .32f, .23f);
            if (x < 11 || x > 52) return new Color(.56f, .55f, .62f);
            if (y % 9 < 2) return new Color(.40f, .62f, .77f);
            float shade = y / 120f; return new Color(.08f + shade, .13f + shade, .21f + shade);
        });
        Sprite Creature(bool plant)
        {
            Texture2D texture = Texture((x, y) =>
            {
                if (plant)
                {
                    if (Math.Abs(x - 32) < 2 && y > 12 && y < 43) return new Color(.8f, .72f, .35f);
                    bool leaf = ((x - 24) * (x - 24) / 100f + (y - 35) * (y - 35) / 42f < 1) || ((x - 41) * (x - 41) / 75f + (y - 44) * (y - 44) / 45f < 1);
                    return leaf ? new Color(.44f, .88f, .56f) : Color.clear;
                }
                bool body = (x - 32) * (x - 32) / 625f + (y - 26) * (y - 26) / 400f < 1;
                if (!body) return Color.clear;
                if (y > 25 && y < 33 && (Math.Abs(x - 24) < 3 || Math.Abs(x - 40) < 3)) return new Color(.035f, .09f, .12f);
                if ((x - 22) * (x - 22) + (y - 37) * (y - 37) < 18) return new Color(.77f, 1f, .92f);
                return new Color(.12f + y / 250f, .48f + y / 180f, .35f + y / 200f);
            });
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, 64, 64), new Vector2(.5f, .5f), 64); generated.Add(sprite); return sprite;
        }
        void OnDestroy()
        {
            if (world != null) Destroy(world);
            foreach (UnityEngine.Object asset in generated) if (asset != null) Destroy(asset);
            generated.Clear();
        }
    }
}
