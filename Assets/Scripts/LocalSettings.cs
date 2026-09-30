using System;
using System.Collections.Generic;
using UnityEngine;

namespace LunaEclipse
{
    public static class LocalSettings
    {
        private const string BgmKey = "luna.settings.bgm-volume";
        private const string SeKey = "luna.settings.se-volume";
        private static readonly Dictionary<string,float> SessionValues = new Dictionary<string,float>();
        public static bool SessionOnly => Array.Exists(Environment.GetCommandLineArgs(), argument => argument.StartsWith("--luna-", StringComparison.Ordinal));
        public static event Action Changed;

        public static float BgmVolume
        {
            get => Read(BgmKey);
            set => Write(BgmKey, value);
        }

        public static float SeVolume
        {
            get => Read(SeKey);
            set => Write(SeKey, value);
        }

        private static float Read(string key)
        {
            if (SessionOnly && SessionValues.TryGetValue(key, out float sessionValue)) return sessionValue;
            float value = PlayerPrefs.GetFloat(key, .35f);
            value = float.IsNaN(value) || float.IsInfinity(value) ? .35f : Mathf.Clamp01(value);
            if (SessionOnly) SessionValues[key] = value;
            return value;
        }

        private static void Write(string key, float value)
        {
            if (float.IsNaN(value) || float.IsInfinity(value)) return;
            value = Mathf.Clamp01(value);
            if (Mathf.Approximately(Read(key), value)) return;
            if (SessionOnly) SessionValues[key] = value;
            else
            {
                PlayerPrefs.SetFloat(key, value);
                PlayerPrefs.Save();
            }
            Changed?.Invoke();
        }
    }
}
