using System;
using UnityEngine;
using UnityEngine.UI;

namespace LunaEclipse
{
    /// <summary>Original web hub composition, drawn inside the app's safe-area parent.</summary>
    public static class HubView
    {
        private static readonly Color Gold = new Color32(215, 183, 107, 255);
        private static readonly Color Ivory = new Color32(245, 241, 232, 255);
        private static readonly Color Blue = new Color32(143, 188, 240, 255);
        private static readonly Color Panel = new Color32(5, 12, 26, 232);

        public static void Build(RectTransform parent, Action<string> navigate,int highestFloor=1,int completedRuns=0)
        {
            if (parent == null) throw new ArgumentNullException(nameof(parent));
            // Parent already excludes Screen.safeArea; never apply the inset twice.
            RectTransform root = Rect("Hub", parent);
            Stretch(root);
            root.gameObject.AddComponent<RectMask2D>();

            RectTransform background = Rect("Moonlit ruins", root);
            Stretch(background);
            Image backdrop = SpriteImage(background, "Hub/moon-ruins");
            backdrop.color = new Color(.66f, .66f, .66f, 1f);
            if (backdrop.sprite != null)
            {
                AspectRatioFitter fit = background.gameObject.AddComponent<AspectRatioFitter>();
                fit.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
                fit.aspectRatio = backdrop.sprite.rect.width / backdrop.sprite.rect.height;
            }

            RectTransform bottomShade = Rect("Lower shade", root);
            bottomShade.anchorMin = Vector2.zero;
            bottomShade.anchorMax = new Vector2(1f, .25f);
            bottomShade.offsetMin = bottomShade.offsetMax = Vector2.zero;
            Solid(bottomShade, new Color(0.005f, .015f, .045f, .58f));

            // Same original artwork, same proportions as .hub-luna in hub-layout.css.
            RectTransform luna = Rect("Luna original artwork", root);
            luna.anchorMin = new Vector2(.222f, .16f);
            luna.anchorMax = new Vector2(1.05f, .754f);
            luna.offsetMin = luna.offsetMax = Vector2.zero;
            SpriteImage(luna, "Hub/characters/luna-hub-original").preserveAspect = true;

            BuildHeader(root, navigate);
            BuildRecords(root, navigate, highestFloor, completedRuns);
            string[] quickRoutes = { "notice", "missions", "achievements", "encyclopedia" };
            string[] quickTitles = { "お知らせ", "ミッション", "実績", "図鑑" };
            string[] quickSymbols = { "✉", "✦", "♛", "▤" };
            for (int i = 0; i < quickRoutes.Length; i++)
            {
                string route = quickRoutes[i];
                RectTransform quick = Box("Quick " + route, root, new Vector2(1, 1), new Vector2(1, 1), -12, -76 - i * 52, 45, 45);
                PanelButton(quick, () => navigate?.Invoke(route));
                TextAt(quick, quickSymbols[i], 17, Blue, 0, 9, 43, 22);
                TextAt(quick, quickTitles[i], 7, Ivory, 0, -13, 43, 15);
            }

            RectTransform dialogue = Box("Luna dialogue", root, new Vector2(0, .65f), new Vector2(0, 1), 16, 0, 190, 96);
            Solid(dialogue, new Color(.015f, .035f, .075f, .88f));
            Border(dialogue);
            TextAt(dialogue, "……今日は、\nもう少し奥まで\n行けそう。", 13, Ivory, -3, 0, 155, 85, TextAnchor.MiddleLeft);
            TextAt(dialogue, "◆", 8, Blue, 79, -34, 16, 16);

            BuildNavigation(root, navigate);
            BuildCommands(root, navigate);
        }

        private static void BuildHeader(RectTransform root, Action<string> navigate)
        {
            RectTransform header = Rect("Header", root);
            header.anchorMin = new Vector2(0, 1); header.anchorMax = Vector2.one;
            header.pivot = new Vector2(.5f, 1); header.sizeDelta = new Vector2(0, 68); header.anchoredPosition = Vector2.zero;
            Solid(header, new Color(.005f, .02f, .055f, .91f));
            RectTransform line = Rect("Gold rule", header);
            line.anchorMin = Vector2.zero; line.anchorMax = new Vector2(1, 0);
            line.sizeDelta = new Vector2(0, 1); line.anchoredPosition = Vector2.zero;
            Solid(line, new Color(Gold.r, Gold.g, Gold.b, .43f));
            RectTransform crest = Box("Moon crest", header, new Vector2(0, .5f), new Vector2(.5f, .5f), 35, 0, 32, 32);
            Solid(crest, new Color32(5, 18, 38, 255)); Border(crest);
            crest.localEulerAngles = new Vector3(0, 0, 45);
            Text moon = TextAt(crest, "☾", 25, Blue, 0, 0, 38, 38);
            moon.rectTransform.localEulerAngles = new Vector3(0, 0, -45);
            RectTransform title = Box("Title", header, new Vector2(0, .5f), new Vector2(0, .5f), 66, 0, 240, 54);
            TextAt(title, "MOONLIGHT WANDERER", 8, Blue, 0, 12, 240, 17, TextAnchor.MiddleLeft);
            TextAt(title, "LUNA  ルナ", 21, Ivory, 0, -7, 240, 30, TextAnchor.MiddleLeft);
            RectTransform audio = Box("BGM settings", header, new Vector2(1, .5f), new Vector2(1, .5f), -43, 0, 30, 36);
            PlainButton(audio, "♪", 21, () => navigate?.Invoke("settings"));
            RectTransform settings = Box("Settings", header, new Vector2(1, .5f), new Vector2(1, .5f), -10, 0, 30, 36);
            PlainButton(settings, "⚙", 21, () => navigate?.Invoke("settings"));
        }

        private static void BuildRecords(RectTransform root, Action<string> navigate,int highestFloor,int completedRuns)
        {
            RectTransform records = Box("Exploration records", root, new Vector2(0, 1), new Vector2(0, 1), 12, -76, 145, 110);
            Solid(records, Panel); Border(records);
            TextAt(records, "✦ 探索記録", 11, Gold, 0, 37, 126, 22, TextAnchor.MiddleLeft);
            // Progress binding belongs to the app; these match the web's unset placeholders.
            TextAt(records, "最高到達       "+highestFloor+" F", 10, Ivory, 0, 11, 126, 21, TextAnchor.MiddleLeft);
            TextAt(records, "探索回数       "+completedRuns+" 回", 10, Ivory, 0, -10, 126, 21, TextAnchor.MiddleLeft);
            RectTransform details = Box("View records", records, new Vector2(.5f, 0), new Vector2(.5f, 0), 0, 8, 126, 24);
            PanelButton(details, () => navigate?.Invoke("results"));
            TextAt(details, "詳細を確認 ›", 9, Gold, 0, 0, 123, 23);
        }

        private static void BuildCommands(RectTransform root, Action<string> navigate)
        {
            // Web: 406px grid, 96px center gap, columns at 90% width.
            ImageCommand(root, "equipment", new Vector2(0, 0), new Vector2(0, 0), 12, 141, 139.5f, 62, navigate);
            ImageCommand(root, "shop", new Vector2(1, 0), new Vector2(1, 0), -12, 141, 139.5f, 62, navigate);
            ImageCommand(root, "storage", new Vector2(0, 0), new Vector2(0, 0), 12, 72, 139.5f, 62, navigate);
            ImageCommand(root, "relics", new Vector2(1, 0), new Vector2(1, 0), -12, 72, 139.5f, 62, navigate);
            RectTransform launch = Box("Enter dungeon", root, new Vector2(.5f, 0), new Vector2(.5f, .5f), 0, 137.5f, 214, 214);
            Image image = SpriteImage(launch, "Hub/buttons/enter-dungeon");
            image.preserveAspect = true; image.raycastTarget = true;
            Button button = launch.gameObject.AddComponent<Button>(); button.targetGraphic = image;
            button.onClick.AddListener(() => navigate?.Invoke("dungeon"));
            launch.gameObject.AddComponent<HubDiamondRaycast>();
        }

        private static void ImageCommand(RectTransform parent, string route, Vector2 anchor, Vector2 pivot, float x, float y, float width, float height, Action<string> navigate)
        {
            RectTransform rect = Box(route, parent, anchor, pivot, x, y, width, height);
            Image image = SpriteImage(rect, "Hub/buttons/" + route);
            image.preserveAspect = true; image.raycastTarget = true;
            Button button = rect.gameObject.AddComponent<Button>(); button.targetGraphic = image;
            button.onClick.AddListener(() => navigate?.Invoke(route));
        }

        private static void BuildNavigation(RectTransform root, Action<string> navigate)
        {
            RectTransform nav = Rect("Bottom navigation", root);
            nav.anchorMin = Vector2.zero; nav.anchorMax = new Vector2(1, 0);
            nav.pivot = new Vector2(.5f, 0); nav.sizeDelta = new Vector2(0, 61); nav.anchoredPosition = Vector2.zero;
            Solid(nav, new Color32(2, 6, 15, 245)); Border(nav);
            string[] names = { "拠点", "迷宮記録", "図鑑", "メニュー" };
            string[] routes = { "", "results", "encyclopedia", "settings" };
            string[] icons = { "⌂", "⌘", "▤", "☰" };
            for (int i = 0; i < names.Length; i++)
            {
                string route = routes[i];
                RectTransform item = Rect("Navigation " + names[i], nav);
                item.anchorMin = new Vector2(i / 4f, 0); item.anchorMax = new Vector2((i + 1) / 4f, 1);
                item.offsetMin = item.offsetMax = Vector2.zero;
                Solid(item, i == 0 ? new Color(Gold.r, Gold.g, Gold.b, .09f) : Color.clear).raycastTarget = true;
                Button button = item.gameObject.AddComponent<Button>();
                if (i > 0) button.onClick.AddListener(() => navigate?.Invoke(route));
                TextAt(item, icons[i], 19, i == 0 ? Gold : Blue, 0, 8, 90, 26);
                TextAt(item, names[i], 9, i == 0 ? Gold : Ivory, 0, -15, 90, 17);
            }
        }

        private static RectTransform Rect(string name, RectTransform parent)
        {
            var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
            rect.SetParent(parent, false); return rect;
        }
        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
        }
        private static RectTransform Box(string name, RectTransform parent, Vector2 anchor, Vector2 pivot, float x, float y, float width, float height)
        {
            RectTransform rect = Rect(name, parent); rect.anchorMin = rect.anchorMax = anchor;
            rect.pivot = pivot; rect.anchoredPosition = new Vector2(x, y); rect.sizeDelta = new Vector2(width, height); return rect;
        }
        private static Image Solid(RectTransform rect, Color color)
        {
            Image image = rect.gameObject.AddComponent<Image>(); image.color = color; image.raycastTarget = false; return image;
        }
        private static Image SpriteImage(RectTransform rect, string resource)
        {
            Image image = Solid(rect, Color.white); image.sprite = Resources.Load<Sprite>(resource);
            if (image.sprite == null) { image.color = Color.clear; Debug.LogWarning("Hub sprite missing: Resources/" + resource); }
            return image;
        }
        private static void Border(RectTransform rect)
        {
            Outline outline = rect.gameObject.AddComponent<Outline>();
            outline.effectColor = new Color(Gold.r, Gold.g, Gold.b, .43f); outline.effectDistance = new Vector2(1, -1);
        }
        private static void PanelButton(RectTransform rect, Action action)
        {
            Image image = Solid(rect, Panel); image.raycastTarget = true; Border(rect);
            Button button = rect.gameObject.AddComponent<Button>(); button.targetGraphic = image; button.onClick.AddListener(() => action?.Invoke());
        }
        private static void PlainButton(RectTransform rect, string label, int size, Action action)
        {
            Image image = Solid(rect, Color.clear); image.raycastTarget = true;
            Button button = rect.gameObject.AddComponent<Button>(); button.targetGraphic = image; button.onClick.AddListener(() => action?.Invoke());
            TextAt(rect, label, size, Blue, 0, 0, rect.sizeDelta.x, rect.sizeDelta.y);
        }
        private static Text TextAt(RectTransform parent, string value, int size, Color color, float x, float y, float width, float height, TextAnchor alignment = TextAnchor.MiddleCenter)
        {
            RectTransform rect = Box("Text " + value, parent, new Vector2(.5f, .5f), new Vector2(.5f, .5f), x, y, width, height);
            Text text = rect.gameObject.AddComponent<Text>(); text.font = UiKit.Font; text.fontSize = size;
            text.text = value; text.color = color; text.alignment = alignment; text.raycastTarget = false;
            text.horizontalOverflow = HorizontalWrapMode.Wrap; text.verticalOverflow = VerticalWrapMode.Truncate;
            return text;
        }
    }

    /// <summary>Transparent diamond corners must not steal taps from side commands.</summary>
    public sealed class HubDiamondRaycast : MonoBehaviour, ICanvasRaycastFilter
    {
        public bool IsRaycastLocationValid(Vector2 screenPoint, Camera eventCamera)
        {
            RectTransform rect = (RectTransform)transform;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, screenPoint, eventCamera, out Vector2 local)) return false;
            Vector2 center = rect.rect.center;
            return Mathf.Abs((local.x - center.x) / (rect.rect.width * .5f)) + Mathf.Abs((local.y - center.y) / (rect.rect.height * .5f)) <= 1.05f;
        }
    }
}
