using UnityEditor;
using UnityEditor.Build;
using UnityEngine;
namespace RollerSort.Editor {
 public static class MobileArtSetup {
  [MenuItem("Roller Sort/Apply App Icon")]
  public static void Apply(){
   PlayerSettings.SplashScreen.show=false;
   PlayerSettings.SplashScreen.showUnityLogo=false;
   var icon=AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Resources/Art/app-icon-logo.png");
   if(icon==null)throw new System.Exception("Splash logo app icon is missing.");
   PlayerSettings.SetIconsForTargetGroup(BuildTargetGroup.Unknown,new[]{icon});
   var android=NamedBuildTarget.Android;
   var legacySizes=PlayerSettings.GetIconSizes(android,IconKind.Application);
   var legacyIcons=new Texture2D[legacySizes.Length];
   for(int i=0;i<legacyIcons.Length;i++)legacyIcons[i]=icon;
   PlayerSettings.SetIcons(android,legacyIcons,IconKind.Application);
   foreach(var kind in PlayerSettings.GetSupportedIconKinds(android)){
    var slots=PlayerSettings.GetPlatformIcons(android,kind);
    for(int i=0;i<slots.Length;i++){
     var layers=new Texture2D[slots[i].maxLayerCount];
     for(int layer=0;layer<layers.Length;layer++)layers[layer]=icon;
     slots[i].SetTextures(layers);
    }
    PlayerSettings.SetPlatformIcons(android,kind,slots);
   }
   AssetDatabase.SaveAssets();Debug.Log("ROLLER_SORT_ICON_APPLIED");
  }
 }
}
