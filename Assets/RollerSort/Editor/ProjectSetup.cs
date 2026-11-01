using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
namespace RollerSort.Editor {
 public static class ProjectSetup {
  [MenuItem("Roller Sort/Prepare Mobile Project")]
  public static void Prepare() {
   PlayerSettings.companyName="Roller Studio";
   PlayerSettings.productName="Roller Sort";
   PlayerSettings.SetApplicationIdentifier(UnityEditor.Build.NamedBuildTarget.Android,"com.smartlogy.rollersort");
   PlayerSettings.SetApplicationIdentifier(UnityEditor.Build.NamedBuildTarget.iOS,"com.smartlogy.rollersort");
   PlayerSettings.defaultInterfaceOrientation=UIOrientation.Portrait;
   PlayerSettings.allowedAutorotateToLandscapeLeft=false;
   PlayerSettings.allowedAutorotateToLandscapeRight=false;
   PlayerSettings.defaultScreenWidth=450; PlayerSettings.defaultScreenHeight=975;
   var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
   new GameObject("Roller Sort",typeof(RollerGame));
   System.IO.Directory.CreateDirectory("Assets/RollerSort/Scenes");
   EditorSceneManager.SaveScene(scene,"Assets/RollerSort/Scenes/RollerSort.unity");
   EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(scene.path,true)};
   AssetDatabase.SaveAssets();
   Debug.Log("ROLLER_SORT_SETUP_OK");
  }
 }
}
