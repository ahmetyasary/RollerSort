using System;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;
namespace RollerSort {
 public sealed partial class RollerGame {
  const string MotionRevision="video-reference";
  bool diagnosticsActive,referencePlayback,journeyAnimationPreview,profileScreenPreview,settingsScreenPreview,journeySettingsScreenPreview;int journeyPreviewStep;int referenceStep;
  [Serializable] class ReplayStep{public int row,col,color;}
  [Serializable] class ReplayRoute{public ReplayStep[] steps;}
  ReplayRoute replayRoute;double profileStart=-1,nextDiagnosticInput;int profileSamples,profileLevels,profileRetries,profileCollections;
  readonly float[] frameSamples=new float[24000];
  [Serializable] sealed class MotionReport {public string revision,device,engine;public int frames,completedLevels,retries,gcCollections,framesOver25ms,framesOver33ms;public double durationSeconds;public float averageMs,p95Ms,p99Ms,worstMs;}
  #if UNITY_IOS && !UNITY_EDITOR
  [DllImport("__Internal")] static extern int RSProfileRequested();
  #endif
  void ConfigureDiagnostics(){
   #if UNITY_IOS && !UNITY_EDITOR
   int requested=RSProfileRequested();diagnosticsActive=requested!=0;referencePlayback=requested==2;journeyAnimationPreview=requested==3;profileScreenPreview=requested==4;settingsScreenPreview=requested==5;journeySettingsScreenPreview=requested==6;
   #endif
foreach(string arg in Environment.GetCommandLineArgs()){if(arg=="-roller-journey-preview"){diagnosticsActive=true;journeyAnimationPreview=true;}if(arg=="-roller-profile-screen"){diagnosticsActive=true;profileScreenPreview=true;}if(arg=="-roller-settings-screen"){diagnosticsActive=true;settingsScreenPreview=true;}if(arg=="-roller-journey-settings-screen"){diagnosticsActive=true;journeySettingsScreenPreview=true;}if(arg=="-roller-profile")diagnosticsActive=true;if(arg=="-roller-reference"){diagnosticsActive=true;referencePlayback=true;}}if(!diagnosticsActive)return;level=journeyAnimationPreview?22:referencePlayback?19:1;coins=journeyAnimationPreview?750:referencePlayback?1025:500;if(referencePlayback)replayRoute=JsonUtility.FromJson<ReplayRoute>(Resources.Load<TextAsset>("Diagnostics/level19-route").text);Screen.sleepTimeout=SleepTimeout.NeverSleep;profileCollections=GC.CollectionCount(0);Debug.Log("ROLLER_PROFILE_ARMED "+MotionRevision);}
  void DiagnosticsTick(){
   if(!diagnosticsActive||profileScreenPreview||settingsScreenPreview||journeySettingsScreenPreview||!game)return;double now=Time.realtimeSinceStartupAsDouble;
   if(profileStart<0){profileStart=now;nextDiagnosticInput=now+1;}
   if(journeyAnimationPreview){if(now<nextDiagnosticInput)return;if(journeyPreviewStep==0){journeyPreviewStep=1;launchedFromJourney=true;won=true;StartCoroutine(Victory());nextDiagnosticInput=now+2.5;}else if(journeyPreviewStep==1&&overlay){journeyPreviewStep=2;foreach(var button in overlay.GetComponentsInChildren<UnityEngine.UI.Button>()){var text=button.GetComponentInChildren<UnityEngine.UI.Text>();if(text&&text.text==GameLanguage.Text("Continue")){button.onClick.Invoke();break;}}}return;}
   if(now-profileStart>=(referencePlayback?185:120)){FinishDiagnostics();return;}
   if(now<nextDiagnosticInput)return;nextDiagnosticInput=now+.6;
   if(paused&&won){if(activeBalls==0){profileLevels++;if(referencePlayback){FinishDiagnostics();return;}level++;}else profileRetries++;BuildLevel();return;}
   if(referencePlayback){if(paused||won||referenceStep>=replayRoute.steps.Length||activeBalls>capacity-9)return;var next=replayRoute.steps[referenceStep];var target=boxes.Find(b=>!b.removed&&b.row==next.row&&b.col==next.col&&b.color==next.color);if(target!=null&&Accessible(target)){Release(target);if(target.removed){referenceStep++;nextDiagnosticInput=now+6;}}return;}
   if(paused||won||activeBalls>18)return;
   Box candidate=null;int best=int.MaxValue;
   foreach(var box in boxes){if(box.removed||!Accessible(box))continue;int queued=0;foreach(var ball in balls)if(ball.color==box.color)queued++;
    int score=box.row*10+box.col+queued*20;foreach(var tray in trays)if(tray.color==box.color&&!tray.completing){score-=200;break;}
    if(score<best){best=score;candidate=box;}}
   if(candidate!=null)Release(candidate);
  }
  void LateUpdate(){if(!diagnosticsActive||profileStart<0)return;double elapsed=Time.realtimeSinceStartupAsDouble-profileStart;if(elapsed<5)return;if(profileSamples<frameSamples.Length)frameSamples[profileSamples++]=Time.unscaledDeltaTime*1000;}
  void FinishDiagnostics(){
   diagnosticsActive=false;var report=new MotionReport{revision=MotionRevision,device=SystemInfo.deviceModel,engine=Application.unityVersion,frames=profileSamples,completedLevels=profileLevels,retries=profileRetries,gcCollections=GC.CollectionCount(0)-profileCollections,durationSeconds=Time.realtimeSinceStartupAsDouble-profileStart};
   double sum=0;for(int i=0;i<profileSamples;i++){float value=frameSamples[i];sum+=value;if(value>25)report.framesOver25ms++;if(value>33.34f)report.framesOver33ms++;}
   if(profileSamples>0){Array.Sort(frameSamples,0,profileSamples);report.averageMs=(float)(sum/profileSamples);report.p95Ms=frameSamples[Mathf.Clamp((int)(profileSamples*.95f),0,profileSamples-1)];report.p99Ms=frameSamples[Mathf.Clamp((int)(profileSamples*.99f),0,profileSamples-1)];report.worstMs=frameSamples[profileSamples-1];}
   string json=JsonUtility.ToJson(report,true);File.WriteAllText(Path.Combine(Application.persistentDataPath,"motion-profile-"+MotionRevision+".json"),json);Debug.Log("ROLLER_PROFILE_COMPLETE "+JsonUtility.ToJson(report));
   // Profiling never writes player progression; resume the user's actual level.
   level=PlayerPrefs.GetInt("level",1);coins=PlayerPrefs.GetInt("coins",500);BuildLevel();Screen.sleepTimeout=SleepTimeout.SystemSetting;
  }
 }
}
