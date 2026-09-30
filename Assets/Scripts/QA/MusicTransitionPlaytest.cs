using System;
using System.Collections;
using System.Linq;
using LunaEclipse.Audio;
using UnityEngine;

namespace LunaEclipse.Dungeon
{
    /// <summary>Caller-owned audio transition exercise. No automatic entry or persistent settings.</summary>
    public static class MusicTransitionPlaytest
    {
        public static IEnumerator Exercise(Action<bool,string> check)
        {
            if(check==null)throw new ArgumentNullException(nameof(check));
            float originalTimeScale=Time.timeScale;
            GameObject host=null;
            try
            {
                host=new GameObject("Temporary Music Transition QA");
                var music=host.AddComponent<LunaMusicPlayer>();
                var sources=host.GetComponents<AudioSource>();
                check(sources.Length==2,"music QA owns exactly two sources");
                music.SetVolume(.8f);
                check(music.Play(LunaMusicPlayer.Mood.Exploration),"exploration clip loads");
                yield return new WaitForSecondsRealtime(.1f);
                check(music.Play(LunaMusicPlayer.Mood.Deep),"early deep transition accepted");
                yield return new WaitForSecondsRealtime(.1f);
                check(music.Play(LunaMusicPlayer.Mood.Return),"early return transition accepted");
                check(host.GetComponents<AudioSource>().Length==2,"rapid transitions reuse sources");
                yield return new WaitForSecondsRealtime(.1f);
                music.SetMuted(true);
                check(sources.All(s=>Mathf.Abs(s.volume)<.0001f),"mute immediately zeros both fade sources");
                yield return new WaitForSecondsRealtime(.15f);
                check(sources.All(s=>Mathf.Abs(s.volume)<.0001f),"running fade cannot override mute");
                music.SetVolume(.2f);music.SetMuted(false);
                check(sources.All(s=>s.volume>=0&&s.volume<=.0701f),"unmute uses current master volume during fade");
                yield return new WaitForSecondsRealtime(1f);
                var playing=sources.Where(s=>s.isPlaying).ToArray();
                check(playing.Length==1,"interrupted fades settle to one playing source");
                check(playing.Length==1&&playing[0].clip==Resources.Load<AudioClip>(LunaMusicPlayer.ReturnPath),"latest return track wins");
                check(playing.Length==1&&Mathf.Abs(playing[0].volume-.07f)<.001f&&!playing[0].loop,"settled return gain and nonlooping mode");
                music.StopMusic();
                check(sources.All(s=>!s.isPlaying&&Mathf.Abs(s.volume)<.0001f),"stop immediately silences both sources");
                yield return new WaitForSecondsRealtime(1f);
                check(sources.All(s=>!s.isPlaying&&Mathf.Abs(s.volume)<.0001f),"stopped fade never revives audio");
                Time.timeScale=0f;
                check(music.Play(LunaMusicPlayer.Mood.Exploration,.25f),"paused-time transition starts");
                yield return new WaitForSecondsRealtime(.5f);
                playing=sources.Where(s=>s.isPlaying).ToArray();
                check(playing.Length==1&&playing[0].clip==Resources.Load<AudioClip>(LunaMusicPlayer.ExplorationPath),"unscaled fade finishes with timescale zero");
                check(playing.Length==1&&Mathf.Abs(playing[0].volume-.06f)<.001f&&playing[0].loop,"paused-time fade reaches target gain");
                music.SetVolume(0f);
                yield return new WaitForSecondsRealtime(.1f);
                check(sources.All(s=>Mathf.Abs(s.volume)<.0001f),"zero master volume stays silent");
                music.SetVolume(.4f);
                check(sources.Any(s=>s.isPlaying&&Mathf.Abs(s.volume-.12f)<.001f),"restored master volume applies without new playback source");
                music.StopMusic();
            }
            finally
            {
                Time.timeScale=originalTimeScale;
                if(host!=null)UnityEngine.Object.Destroy(host);
            }
        }
    }
}
