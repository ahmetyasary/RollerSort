using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
namespace RollerSort.Editor {
 [InitializeOnLoad] public static class VisualCheck {
  const BindingFlags Flags=BindingFlags.Instance|BindingFlags.NonPublic;
  static double started;static int stage;static RollerGame game;
  static VisualCheck(){if(SessionState.GetBool("RollerVisual",false)){started=EditorApplication.timeSinceStartup;EditorApplication.update+=Tick;}}
  public static void Run(){UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/RollerSort/Scenes/RollerSort.unity");SessionState.SetBool("RollerVisual",true);started=EditorApplication.timeSinceStartup;EditorApplication.update+=Tick;EditorApplication.isPlaying=true;}
  static void Tick(){try{
   if(EditorApplication.timeSinceStartup-started>45)throw new Exception("Visual render timed out");
   if(!EditorApplication.isPlaying)return;Application.runInBackground=true;EditorApplication.QueuePlayerLoopUpdate();
   game=UnityEngine.Object.FindFirstObjectByType<RollerGame>();if(!game)return;
   var board=typeof(RollerGame).GetField("game",Flags).GetValue(game) as RectTransform;if(!board)return;
   if(stage==0){CheckScaleInitialization();game.StopAllCoroutines();game.enabled=false;Render("Docs/layout-gameplay.png");typeof(RollerGame).GetMethod("Shop",Flags).Invoke(game,null);stage++;return;}
   if(stage==1){if(board.gameObject.activeSelf)throw new Exception("Shop exposes gameplay layer");Render("Docs/layout-shop.png");typeof(RollerGame).GetMethod("Settings",Flags).Invoke(game,null);stage++;return;}
   if(stage==2){game.StopAllCoroutines();Render("Docs/layout-settings.png");typeof(RollerGame).GetMethod("Close",Flags).Invoke(game,null);var restored=typeof(RollerGame).GetField("overlay",Flags).GetValue(game) as RectTransform;if(!restored||restored.name!="Shop")throw new Exception("Settings did not restore shop");typeof(RollerGame).GetMethod("Close",Flags).Invoke(game,null);if(!board.gameObject.activeSelf)throw new Exception("Shop did not restore gameplay");Debug.Log("ROLLER_VISUAL_OK");SessionState.SetBool("RollerVisual",false);EditorApplication.update-=Tick;EditorApplication.Exit(0);}
  }catch(Exception e){Debug.LogException(e);SessionState.SetBool("RollerVisual",false);EditorApplication.Exit(1);}}
  static void CheckScaleInitialization(){
   // Reproduce CanvasScaler becoming ready one frame after the game creates it.
   var canvas=typeof(RollerGame).GetField("canvas",Flags).GetValue(game) as Canvas;
   var root=typeof(RollerGame).GetField("root",Flags).GetValue(game) as RectTransform;
   var update=typeof(RollerGame).GetMethod("Update",Flags);float original=canvas.scaleFactor;
   canvas.scaleFactor=1;update.Invoke(game,null);canvas.scaleFactor=3;update.Invoke(game,null);
   float expected=Mathf.Max(Screen.width/450f,Screen.height/975f);
   if(Mathf.Abs(root.localScale.x*canvas.scaleFactor-expected)>.001f)throw new Exception("Layout retained stale CanvasScaler scale");
   canvas.scaleFactor=original;update.Invoke(game,null);Debug.Log("ROLLER_SCALE_INITIALIZATION_OK");
  }
  static void Render(string path){
   var canvas=typeof(RollerGame).GetField("canvas",Flags).GetValue(game) as Canvas;canvas.GetComponent<CanvasScaler>().enabled=false;canvas.renderMode=RenderMode.WorldSpace;canvas.scaleFactor=3;
   var cr=canvas.GetComponent<RectTransform>();cr.sizeDelta=new Vector2(450,975);cr.position=Vector3.zero;cr.localScale=Vector3.one;cr.rotation=Quaternion.identity;
   var root=typeof(RollerGame).GetField("root",Flags).GetValue(game) as RectTransform;root.sizeDelta=new Vector2(450,975);root.anchoredPosition=Vector2.zero;root.localScale=Vector3.one;
   var cameraObject=new GameObject("Visual QA camera",typeof(Camera));var camera=cameraObject.GetComponent<Camera>();camera.transform.position=new Vector3(0,0,-100);camera.orthographic=true;camera.orthographicSize=487.5f;camera.nearClipPlane=.1f;camera.farClipPlane=200;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color32(99,130,184,255);
   var rt=new RenderTexture(945,2048,24,RenderTextureFormat.ARGB32);camera.targetTexture=rt;foreach(var text in canvas.GetComponentsInChildren<Text>(true))text.SetAllDirty();Canvas.ForceUpdateCanvases();camera.Render();var old=RenderTexture.active;RenderTexture.active=rt;var texture=new Texture2D(945,2048,TextureFormat.RGB24,false);texture.ReadPixels(new Rect(0,0,945,2048),0,0);texture.Apply();System.IO.File.WriteAllBytes(path,texture.EncodeToPNG());RenderTexture.active=old;camera.targetTexture=null;rt.Release();UnityEngine.Object.Destroy(cameraObject);UnityEngine.Object.Destroy(texture);UnityEngine.Object.Destroy(rt);
  }
 }
}
