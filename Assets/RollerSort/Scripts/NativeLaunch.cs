using System.Runtime.InteropServices;
namespace RollerSort {
 static class NativeLaunch {
  #if UNITY_IOS && !UNITY_EDITOR
  [DllImport("__Internal")] static extern void RSHideLaunchScreen();
  #endif
  public static void Hide(){
   #if UNITY_IOS && !UNITY_EDITOR
   RSHideLaunchScreen();
   #endif
  }
 }
}
