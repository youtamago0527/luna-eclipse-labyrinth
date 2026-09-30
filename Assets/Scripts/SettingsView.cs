using System;
using UnityEngine;
using UnityEngine.UI;

namespace LunaEclipse
{
    public static class SettingsView
    {
        private static readonly Color Ivory = new Color(.94f, .89f, .78f);
        private static readonly Color Muted = new Color(.65f, .72f, .82f);
        private static readonly Color Gold = new Color(.74f, .58f, .30f);

        public static void Build(RectTransform parent, Action<string> navigate)
        {
            var background = UiKit.Panel(parent, new Color(.025f, .045f, .085f, .98f));
            UiKit.Place(background.transform as RectTransform, 0, 0, 430, 932);
            Label(parent, "設定・クレジット", 28, Ivory, 28, 44, 374, 46);
            Label(parent, "月蝕の迷宮で過ごす時間を、あなた好みに。", 15, Muted, 28, 96, 374, 44);

            AddVolume(parent, "BGM", 174, LocalSettings.BgmVolume, value => LocalSettings.BgmVolume = value);
            AddVolume(parent, "効果音", 280, LocalSettings.SeVolume, value => LocalSettings.SeVolume = value);

            Label(parent, "操作ガイド", 21, Gold, 28, 404, 374, 34);
            bool nativeIos = Application.platform == RuntimePlatform.IPhonePlayer;
            string instructions = nativeIos
                ? "iOSアプリ版\n画面の方向ボタンをタップして移動します。\n各操作ボタンをタップして行動します。"
                : Application.platform == RuntimePlatform.WebGLPlayer
                  ? "ブラウザ版\n方向キー／WASDで移動、Spaceで攻撃。\n画面のボタンでも操作できます。"
                  : "PC版\n方向キー／WASDで移動、Spaceで攻撃。\nEで拾う、ピリオドで待機します。";
            Label(parent, instructions, 17, Ivory, 28, 446, 374, 104);

            Label(parent, "サウンドクレジット", 21, Gold, 28, 578, 374, 34);
            Label(parent, "効果音提供：OtoLogic\nライセンス：CC BY 4.0", 18, Ivory, 28, 620, 374, 66);
            Link(parent, "OtoLogic 公式サイト ↗", "https://otologic.jp/", 702);
            Link(parent, "CC BY 4.0 ライセンス ↗", "https://creativecommons.org/licenses/by/4.0/", 760);
            var back = UiKit.Button(parent, "拠点へ戻る", () => navigate?.Invoke("hub"));
            UiKit.Place(back.transform as RectTransform, 28, 848, 374, 54);
        }

        private static void Label(RectTransform parent, string text, int size, Color color,
            float x, float y, float width, float height)
        {
            var label = UiKit.Text(parent, text, size, color);
            UiKit.Place(label.transform as RectTransform, x, y, width, height);
        }

        private static void Link(RectTransform parent, string label, string url, float y)
        {
            var button = UiKit.Button(parent, label, () => Application.OpenURL(url));
            UiKit.Place(button.transform as RectTransform, 28, y, 374, 46);
        }

        private static void AddVolume(RectTransform parent, string title, float y, float initial, Action<float> onChange)
        {
            Label(parent, title, 21, Ivory, 28, y, 180, 32);
            var valueText = UiKit.Text(parent, Mathf.RoundToInt(initial * 100) + "%", 18, Muted);
            UiKit.Place(valueText.transform as RectTransform, 325, y, 77, 32);

            var sliderObject = new GameObject(title + " Volume", typeof(RectTransform), typeof(Image), typeof(Slider));
            sliderObject.GetComponent<Image>().color = Color.clear;
            var rect = sliderObject.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            UiKit.Place(rect, 28, y + 36, 374, 44);

            var track = Graphic(rect, "Track", new Color(.15f, .20f, .29f), new Vector2(0, .4f), new Vector2(1, .6f));
            // Slider drives child anchors across their full parent height.
            // Keep the fill inside the thin track and the handle on a zero-height rail.
            var fill = Graphic(track.rectTransform, "Fill", Gold, Vector2.zero, new Vector2(initial, 1));
            var handleRail = new GameObject("Handle Rail", typeof(RectTransform)).GetComponent<RectTransform>();
            handleRail.SetParent(rect, false);
            handleRail.anchorMin = new Vector2(0, .5f);
            handleRail.anchorMax = new Vector2(1, .5f);
            handleRail.offsetMin = handleRail.offsetMax = Vector2.zero;
            var handle = Graphic(handleRail, "Handle", Ivory, new Vector2(initial, .5f), new Vector2(initial, .5f));
            handle.rectTransform.sizeDelta = new Vector2(24, 34);
            var slider = sliderObject.GetComponent<Slider>();
            slider.minValue = 0;
            slider.maxValue = 1;
            slider.wholeNumbers = false;
            slider.fillRect = fill.rectTransform;
            slider.handleRect = handle.rectTransform;
            slider.targetGraphic = handle;
            slider.direction = Slider.Direction.LeftToRight;
            slider.SetValueWithoutNotify(initial);
            slider.onValueChanged.AddListener(value =>
            {
                // Resolve either a Text or another Component-returning UiKit implementation.
                var label = valueText.GetComponent<Text>();
                if (label != null) label.text = Mathf.RoundToInt(value * 100) + "%";
                onChange(value);
            });
        }

        private static Image Graphic(RectTransform parent, string name, Color color, Vector2 min, Vector2 max)
        {
            var obj = new GameObject(name, typeof(RectTransform), typeof(Image));
            var rect = obj.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = min;
            rect.anchorMax = max;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            var image = obj.GetComponent<Image>();
            image.color = color;
            return image;
        }
    }
}
