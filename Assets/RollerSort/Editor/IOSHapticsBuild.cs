using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
namespace RollerSort.Editor {
 public static class IOSHapticsBuild {
  [PostProcessBuild(100)]
  public static void Configure(BuildTarget target,string path){
   if(target!=BuildTarget.iOS)return;
   string file=PBXProject.GetPBXProjectPath(path);var project=new PBXProject();project.ReadFromFile(file);
   project.AddFrameworkToProject(project.GetUnityFrameworkTargetGuid(),"CoreHaptics.framework",false);
   project.WriteToFile(file);
  }
 }
}
