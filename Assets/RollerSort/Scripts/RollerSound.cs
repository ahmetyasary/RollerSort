using UnityEngine;
using System.Collections.Generic;
namespace RollerSort {
 public sealed partial class RollerGame {
  readonly Dictionary<string,AudioClip> soundClips=new();AudioSource[] voices;AudioSource rolling;int voiceIndex;float nextContact;
  void PrepareSound(){voices=new AudioSource[12];for(int i=0;i<voices.Length;i++){voices[i]=gameObject.AddComponent<AudioSource>();voices[i].playOnAwake=false;voices[i].spatialBlend=0;}foreach(var name in new[]{"contact","seat","open","complete","victory","fail","shuffle","magnet","click","rolling"})soundClips[name]=Resources.Load<AudioClip>("Audio/"+name);foreach(var clip in soundClips.Values)if(clip)clip.LoadAudioData();rolling=gameObject.AddComponent<AudioSource>();rolling.clip=soundClips["rolling"];rolling.loop=true;rolling.playOnAwake=false;rolling.volume=0;rolling.Play();}
  void Sfx(string name,float volume=1,float pitch=1){if(!sound||voices==null||!soundClips.TryGetValue(name,out var clip)||!clip)return;var source=voices[voiceIndex++%voices.Length];source.pitch=pitch;source.PlayOneShot(clip,volume);}
  void ContactSound(float speed){if(Time.unscaledTime<nextContact||paused)return;nextContact=Time.unscaledTime+.045f;Sfx("contact",Mathf.Lerp(.13f,.45f,Mathf.InverseLerp(85,500,speed)),Random.Range(.93f,1.08f));}
  void AudioTick(){if(!rolling)return;float target=!sound||paused||journey||balls.Count==0?0:Mathf.Min(.22f,.025f*balls.Count);rolling.volume=Mathf.MoveTowards(rolling.volume,target,Time.unscaledDeltaTime*.7f);}
 }
}
