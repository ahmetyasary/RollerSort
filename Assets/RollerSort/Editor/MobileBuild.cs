using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
namespace RollerSort.Editor {
 public static class MobileBuild {
  public static void ExportIOSSimulator(){
   var sdk=PlayerSettings.iOS.sdkVersion;
   var architecture=typeof(PlayerSettings.iOS).GetProperty("simulatorSdkArchitecture");
   object previous=architecture.GetValue(null);
   try {
    PlayerSettings.iOS.sdkVersion=iOSSdkVersion.SimulatorSDK;
    string arm64=System.Array.Find(System.Enum.GetNames(architecture.PropertyType),name=>name.ToUpperInvariant().Contains("ARM64"));
    if(arm64==null)throw new System.Exception("ARM64 simulator architecture unavailable");
    architecture.SetValue(null,System.Enum.Parse(architecture.PropertyType,arm64));
    PlayerSettings.SetScriptingBackend(UnityEditor.Build.NamedBuildTarget.iOS,ScriptingImplementation.IL2CPP);
    MobileArtSetup.Apply();
    var result=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{"Assets/RollerSort/Scenes/RollerSort.unity"},locationPathName="Builds/iOSSimulator",target=BuildTarget.iOS,options=BuildOptions.None});
    if(result.summary.result!=BuildResult.Succeeded)throw new System.Exception("Simulator export failed: "+result.summary.result);
    Debug.Log("ROLLER_SORT_SIMULATOR_EXPORT_OK");
   } finally {PlayerSettings.iOS.sdkVersion=sdk;architecture.SetValue(null,previous);}
  }
  public static void ExportIOS(){
   PlayerSettings.iOS.sdkVersion=iOSSdkVersion.DeviceSDK;
   PlayerSettings.iOS.targetOSVersionString="15.0";
   PlayerSettings.iOS.appleDeveloperTeamID="9VSUBY2W33";
   PlayerSettings.iOS.appleEnableAutomaticSigning=true;
   PlayerSettings.SetScriptingBackend(UnityEditor.Build.NamedBuildTarget.iOS,ScriptingImplementation.IL2CPP);
   MobileArtSetup.Apply();
   var result=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{"Assets/RollerSort/Scenes/RollerSort.unity"},locationPathName="Builds/iOS",target=BuildTarget.iOS,options=BuildOptions.None});
   if(result.summary.result!=BuildResult.Succeeded)throw new System.Exception("iOS export failed: "+result.summary.result);
   Debug.Log("ROLLER_SORT_IOS_EXPORT_OK");
  }
  public static void ExportAndroidAPK(){
   PlayerSettings.SetScriptingBackend(UnityEditor.Build.NamedBuildTarget.Android,ScriptingImplementation.IL2CPP);
   PlayerSettings.Android.targetArchitectures=AndroidArchitecture.ARM64;
   PlayerSettings.Android.useCustomKeystore=false;
   EditorUserBuildSettings.buildAppBundle=false;
   MobileArtSetup.Apply();
   System.IO.Directory.CreateDirectory("Builds/Android");
   var result=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{"Assets/RollerSort/Scenes/RollerSort.unity"},locationPathName="Builds/Android/RollerSort-1.0-2.apk",target=BuildTarget.Android,options=BuildOptions.None});
   if(result.summary.result!=BuildResult.Succeeded)throw new System.Exception("Android APK build failed: "+result.summary.result);
   Debug.Log("ROLLER_SORT_ANDROID_APK_OK "+result.summary.outputPath);
  }
 }
}
