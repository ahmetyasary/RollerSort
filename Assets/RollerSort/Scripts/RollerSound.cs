using UnityEngine;
using System.Collections.Generic;
namespace RollerSort {
 public sealed partial class RollerGame {
  // Each cue may ship several takes (name, name-2, name-3...); repeated hits pick a different one each time.
  readonly Dictionary<string,AudioClip[]> soundClips=new();readonly Dictionary<string,int> lastTake=new();AudioSource[] voices;int voiceIndex;float nextContact;
  void PrepareSound(){voices=new AudioSource[12];for(int i=0;i<voices.Length;i++){voices[i]=gameObject.AddComponent<AudioSource>();voices[i].playOnAwake=false;voices[i].spatialBlend=0;}foreach(var name in new[]{"contact","seat","open","complete","complete-up1","complete-up2","complete-up3","victory","fail","shuffle","magnet","click","rolling"}){var takes=new List<AudioClip>();for(int k=1;;k++){var clip=Resources.Load<AudioClip>("Audio/"+(k==1?name:name+"-"+k));if(!clip)break;clip.LoadAudioData();takes.Add(clip);}soundClips[name]=takes.ToArray();}}
  AudioClip Take(string name){if(!soundClips.TryGetValue(name,out var takes)||takes.Length==0)return null;if(takes.Length==1)return takes[0];lastTake.TryGetValue(name,out int last);int pick=Random.Range(0,takes.Length-1);if(pick>=last)pick++;lastTake[name]=pick;return takes[pick];}
  void Sfx(string name,float volume=1,float pitch=1){if(!sound||voices==null)return;var clip=Take(name);if(!clip)return;var source=voices[voiceIndex++%voices.Length];source.pitch=pitch;source.PlayOneShot(clip,volume);}
  void ContactSound(float speed){if(Time.unscaledTime<nextContact||paused)return;nextContact=Time.unscaledTime+.045f;Sfx("contact",Mathf.Lerp(.13f,.45f,Mathf.InverseLerp(85,500,speed)),Random.Range(.93f,1.08f));}
  // The belt has no continuous bed: between hits the reference is silent, which keeps each knock distinct.
  void AudioTick(){}
 }
}
