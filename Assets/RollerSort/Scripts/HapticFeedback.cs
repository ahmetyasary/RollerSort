using System.Runtime.InteropServices;
using UnityEngine;
namespace RollerSort {
 public enum HapticCue { Selection, OpenBox, MarbleSeat, TrayComplete, LevelComplete, Blocked }
 public static class HapticFeedback {
  static bool enabled=true;
  static float nextImpact;
#if UNITY_IOS && !UNITY_EDITOR
  [DllImport("__Internal")] static extern void RSHapticsEnable(int value);
  [DllImport("__Internal")] static extern void RSHapticsPlay(int kind);
  [DllImport("__Internal")] static extern void RSHapticsSuspend();
#endif
  public static void SetEnabled(bool value){enabled=value;
#if UNITY_IOS && !UNITY_EDITOR
   RSHapticsEnable(value?1:0);
#endif
  }
  public static void Suspend(){
#if UNITY_IOS && !UNITY_EDITOR
   RSHapticsSuspend();
#endif
  }
  public static void Play(HapticCue cue){
   if(!enabled)return;
   if(cue==HapticCue.MarbleSeat&&Time.unscaledTime<nextImpact)return;
   nextImpact=Time.unscaledTime+(cue==HapticCue.TrayComplete?.22f:.055f);
#if UNITY_IOS && !UNITY_EDITOR
   RSHapticsPlay((int)cue);
#elif UNITY_ANDROID && !UNITY_EDITOR
   // Android's semantic touch feedback respects the device's own haptic preference.
   using(var unity=new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
   {
    var activity=unity.GetStatic<AndroidJavaObject>("currentActivity");
    activity.Call("runOnUiThread",new AndroidJavaRunnable(()=>{
     using(activity)
     using(var window=activity.Call<AndroidJavaObject>("getWindow"))
     using(var view=window.Call<AndroidJavaObject>("getDecorView"))
     using(var version=new AndroidJavaClass("android.os.Build$VERSION")) {
      int id=cue==HapticCue.Selection?3:4;
      if(version.GetStatic<int>("SDK_INT")>=30&&(cue==HapticCue.TrayComplete||cue==HapticCue.LevelComplete))id=16;
      view.Call<bool>("performHapticFeedback",id);
     }
    }));
   }
#endif
  }
 }
}
