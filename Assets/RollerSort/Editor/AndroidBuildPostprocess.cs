using System.IO;
using System.Text.RegularExpressions;
using UnityEditor.Android;
using UnityEngine;
namespace RollerSort.Editor {
 public sealed class AndroidBuildPostprocess:IPostGenerateGradleAndroidProject {
  public int callbackOrder=>1000;
  public void OnPostGenerateGradleAndroidProject(string path){
   string manifest=Path.Combine(path,"src/main/AndroidManifest.xml");
   string xml=File.ReadAllText(manifest);
   xml=Regex.Replace(xml,"(<meta-data[^>]*android:name=\"unity\\.splash-enable\"[^>]*android:value=\")[^\"]*(\")","$1false$2");
   File.WriteAllText(manifest,xml);
   string launcher=Path.Combine(Directory.GetParent(path).FullName,"launcher/src/main/res/mipmap-mdpi/app_icon.png");
   File.Copy("Assets/RollerSort/ArtSource/android-app-icon-legacy.png",launcher,true);
   Debug.Log("ROLLER_SORT_ANDROID_BRANDING_APPLIED");
  }
 }
}
