using System.Collections;
using UnityEngine;

namespace LunaEclipse.Audio
{
    /// <summary>Opt-in player for the three officially downloaded Suno tracks.</summary>
    public sealed class LunaMusicPlayer : MonoBehaviour
    {
        public enum Mood { Exploration, Deep, Return }
        public const string ExplorationPath = "Audio/Generated/luna-moonlit-footsteps";
        public const string DeepPath = "Audio/Generated/luna-beneath-the-eclipse";
        public const string ReturnPath = "Audio/Generated/luna-home-under-moonlight";
        private readonly AudioSource[] sources = new AudioSource[2];
        private readonly float[] gains = new float[2];
        private readonly float[] weights = new float[2];
        private Coroutine transition;
        private int active;
        private float masterVolume = 1f;
        private bool muted;

        private void Awake()
        {
            for (int i = 0; i < sources.Length; i++)
            {
                sources[i] = gameObject.AddComponent<AudioSource>();
                sources[i].playOnAwake = false;
                sources[i].spatialBlend = 0f;
                sources[i].volume = 0f;
            }
        }

        public void SetVolume(float volume) { masterVolume = Mathf.Clamp01(volume); ApplyVolumes(); }
        public void SetMuted(bool value) { muted = value; ApplyVolumes(); }

        public bool Play(Mood mood, float fadeSeconds = 0.8f)
        {
            string path = mood == Mood.Exploration ? ExplorationPath : mood == Mood.Deep ? DeepPath : ReturnPath;
            AudioClip clip = Resources.Load<AudioClip>(path);
            if (clip == null) { Debug.LogWarning("Music clip missing: " + path); return false; }
            if (sources[active].clip == clip && sources[active].isPlaying) return true;
            if (transition != null) StopCoroutine(transition);
            int previous = active; active = 1 - active;
            sources[active].Stop(); sources[active].clip = clip;
            sources[active].loop = mood != Mood.Return;
            gains[active] = mood == Mood.Deep ? 0.23f : mood == Mood.Return ? 0.35f : 0.30f;
            weights[active] = 0f; sources[active].volume = 0f; sources[active].Play();
            transition = StartCoroutine(Fade(previous, active, Mathf.Max(0.01f, fadeSeconds)));
            return true;
        }

        public void StopMusic()
        {
            if (transition != null) StopCoroutine(transition);
            transition = null;
            for (int i = 0; i < sources.Length; i++) { sources[i].Stop(); weights[i] = 0f; }
            ApplyVolumes();
        }

        private IEnumerator Fade(int previous, int next, float seconds)
        {
            float start = weights[previous];
            for (float elapsed = 0f; elapsed < seconds; elapsed += Time.unscaledDeltaTime)
            {
                float blend = elapsed / seconds;
                weights[previous] = Mathf.Lerp(start, 0f, blend); weights[next] = blend;
                ApplyVolumes(); yield return null;
            }
            sources[previous].Stop(); sources[previous].clip = null;
            weights[previous] = 0f; weights[next] = 1f; ApplyVolumes(); transition = null;
        }

        private void ApplyVolumes()
        {
            for (int i = 0; i < sources.Length; i++)
                if (sources[i] != null) sources[i].volume = muted ? 0f : masterVolume * gains[i] * weights[i];
        }

        private void OnDisable() { StopMusic(); }
    }
}
