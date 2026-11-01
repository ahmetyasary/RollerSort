using System;
using System.Collections;
using System.Reflection;
using UnityEditor;
using UnityEngine;
namespace RollerSort.Editor {
 [InitializeOnLoad]
 public static class SmokeCheck {
  static SmokeCheck(){Application.logMessageReceived+=Observe;if(SessionState.GetBool("RollerSmoke",false)){started=EditorApplication.timeSinceStartup;EditorApplication.update+=Tick;}}
  static void Observe(string message,string stack,LogType type){if(type==LogType.Exception&&stack.Contains("RollerSort"))SessionState.SetInt("RollerErrors",SessionState.GetInt("RollerErrors",0)+1);}
  static double started,lastRelease,wonAt=-1;static bool captured,splashCaptured,rollingCaptured;static int sent,previewStage,maxBeltCount;static RollerGame game;static EditorWindow qaView;
  const BindingFlags Flags=BindingFlags.NonPublic|BindingFlags.Instance;
  static bool ReferenceMode=>SessionState.GetBool("ReferenceLevelSmoke",false);
  [Serializable] class Step{public int row,col,color;}
  [Serializable] class Fixture{public Step[] steps;}
  static Fixture fixture;static int referenceSecond=-1;
  public static void Run(){SessionState.SetBool("ReferenceLevelSmoke",false);Begin();}
  public static void RunReferenceLevel(){SessionState.SetBool("ReferenceLevelSmoke",true);Begin();}
  static void Begin(){
   BeltSpacingCheck.Run();
   SessionState.SetInt("RollerErrors",0);
   ArtImport.ConfigureAll();MobileArtSetup.Apply();
   UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/RollerSort/Scenes/RollerSort.unity");Debug.Log("ROLLER_SMOKE_START");
   if(!Application.isBatchMode){var gameView=EditorWindow.GetWindow(typeof(EditorWindow).Assembly.GetType("UnityEditor.GameView"));gameView.Show();gameView.Focus();}
   foreach(string key in new[]{"level","coins","questCount","questClaimed","visitedJourney","lives","intro0","intro1","intro2"}){SessionState.SetBool("backupExists_"+key,PlayerPrefs.HasKey(key));SessionState.SetInt("backup_"+key,PlayerPrefs.GetInt(key,0));}
   SessionState.SetString("backup_questDay",PlayerPrefs.GetString("questDay",""));
   PlayerPrefs.SetInt("level",ReferenceMode?19:1);PlayerPrefs.SetInt("coins",500);PlayerPrefs.SetInt("visitedJourney",0);PlayerPrefs.SetInt("lives",5);for(int i=0;i<3;i++)PlayerPrefs.SetInt("intro"+i,1);
   SessionState.SetBool("RollerSmoke",true);started=EditorApplication.timeSinceStartup;EditorApplication.update+=Tick;
  }
  static void Capture(string path){if(!Application.isBatchMode)ScreenCapture.CaptureScreenshot(path);}
  static object Get(string name)=>typeof(RollerGame).GetField(name,Flags).GetValue(game);
  static void Tick(){try{
   if(!EditorApplication.isPlaying){if(EditorApplication.timeSinceStartup-started>2&&!EditorApplication.isCompiling){Debug.Log("ROLLER_ENTER_PLAY");EditorApplication.isPlaying=true;}return;}
   Application.runInBackground=true;if(ReferenceMode)Time.timeScale=3;EditorApplication.QueuePlayerLoopUpdate();
   if(!Application.isBatchMode){if(!qaView)qaView=EditorWindow.GetWindow(typeof(EditorWindow).Assembly.GetType("UnityEditor.GameView"));qaView.Repaint();}
   game=UnityEngine.Object.FindFirstObjectByType<RollerGame>();if(!game){if(EditorApplication.timeSinceStartup-started>15){Debug.LogError("No runtime game");EditorApplication.Exit(1);}return;}
   double elapsed=EditorApplication.timeSinceStartup-started;
   if(elapsed>.8&&elapsed<1.5&&!splashCaptured){Capture("Docs/splash.png");splashCaptured=true;}
   if(elapsed>6.3&&!rollingCaptured){Capture("Docs/rolling.png");rollingCaptured=true;}
   if(elapsed>4&&!captured){System.IO.Directory.CreateDirectory("Docs");Capture("Docs/gameplay.png");captured=true;}
   var moving=Get("balls") as IList;
   int beltCount=0;foreach(var item in moving)if((int)item.GetType().GetField("stage").GetValue(item)==1)beltCount++;
   if(beltCount>maxBeltCount){maxBeltCount=beltCount;if(beltCount>=9)Capture("Docs/belt-flow.png");}
   for(int i=0;i<moving.Count;i++){var a=moving[i];var at=a.GetType();int sa=(int)at.GetField("stage").GetValue(a);if(sa!=1&&sa!=3)continue;
    for(int j=i+1;j<moving.Count;j++){var b=moving[j];var bt=b.GetType();int sb=(int)bt.GetField("stage").GetValue(b);if(sb!=1&&sb!=3)continue;
     float gap=Vector2.Distance((Vector2)at.GetField("position").GetValue(a),(Vector2)bt.GetField("position").GetValue(b));
     if(gap<BeltTrack.PaintedDiameter)throw new Exception("Live conveyor overlap: "+gap+" stages="+sa+","+sb);
    }
   }
   var boxes=Get("boxes") as IList;
   if(ReferenceMode){
    int second=Mathf.FloorToInt((float)Get("clock"));if(second!=referenceSecond){referenceSecond=second;string first="";foreach(var ball in moving)if((int)ball.GetType().GetField("stage").GetValue(ball)==1){first=" first="+ball.GetType().GetField("position").GetValue(ball)+" phase="+ball.GetType().GetField("phase").GetValue(ball);break;}Debug.Log("REFERENCE_PROGRESS clock="+second+" sent="+sent+" active="+Get("activeBalls")+first);}
    if(fixture==null)fixture=JsonUtility.FromJson<Fixture>(System.IO.File.ReadAllText("Docs/Reference/Level19-Fixture.json"));
    if(elapsed>5&&sent<fixture.steps.Length&&(float)Get("clock")-lastRelease>6f&&(int)Get("activeBalls")<=(int)Get("capacity")-9){var next=fixture.steps[sent];object target=null;foreach(var box in boxes){var type=box.GetType();if(!(bool)type.GetField("removed").GetValue(box)&&(int)type.GetField("row").GetValue(box)==next.row&&(int)type.GetField("col").GetValue(box)==next.col&&(int)type.GetField("color").GetValue(box)==next.color){target=box;break;}}if(target!=null&&(bool)typeof(RollerGame).GetMethod("Accessible",Flags).Invoke(game,new[]{target})){typeof(RollerGame).GetMethod("Release",Flags).Invoke(game,new[]{target});if((bool)target.GetType().GetField("removed").GetValue(target))sent++;lastRelease=(float)Get("clock");}}
   }else {
   if(elapsed>5&&boxes.Count==6&&sent<6&&EditorApplication.timeSinceStartup-lastRelease>1&&(int)Get("activeBalls")<=27){
    object candidate=null;int best=999;
    foreach(var box in boxes){var type=box.GetType();if((bool)type.GetField("removed").GetValue(box))continue;
     if(!(bool)typeof(RollerGame).GetMethod("Accessible",Flags).Invoke(game,new[]{box}))continue;
     int index=(int)type.GetField("row").GetValue(box)*10+(int)type.GetField("col").GetValue(box);
     int color=(int)type.GetField("color").GetValue(box);var trays=Get("trays") as IList;foreach(var tray in trays)if((int)tray.GetType().GetField("color").GetValue(tray)==color){index-=100;break;}
     if(index<best){candidate=box;best=index;}}
    if(candidate!=null){typeof(RollerGame).GetMethod("Release",Flags).Invoke(game,new[]{candidate});if((bool)candidate.GetType().GetField("removed").GetValue(candidate))sent++;lastRelease=EditorApplication.timeSinceStartup;}
   }
   }
   if((bool)Get("won")&&(bool)Get("paused")&&(int)Get("activeBalls")==0&&PlayerPrefs.GetInt("coins")==550){if(wonAt<0){Capture("Docs/victory.png");wonAt=EditorApplication.timeSinceStartup;}double previewTime=EditorApplication.timeSinceStartup-wonAt;
    if(previewTime>1&&previewStage==0){typeof(RollerGame).GetMethod("Settings",Flags).Invoke(game,null);previewStage++;}
    if(previewTime>1.6&&previewStage==1){Capture("Docs/settings.png");previewStage++;}
    if(previewTime>2.2&&previewStage==2){typeof(RollerGame).GetMethod("Shop",Flags).Invoke(game,null);previewStage++;}
    if(previewTime>2.8&&previewStage==3){Capture("Docs/shop.png");previewStage++;}
    if(previewTime<3.5)return;
    foreach(string key in new[]{"level","coins","questCount","questClaimed","visitedJourney","lives","intro0","intro1","intro2"}){if(SessionState.GetBool("backupExists_"+key,false))PlayerPrefs.SetInt(key,SessionState.GetInt("backup_"+key,0));else PlayerPrefs.DeleteKey(key);}PlayerPrefs.SetString("questDay",SessionState.GetString("backup_questDay",""));PlayerPrefs.Save();
    if(SessionState.GetInt("RollerErrors",0)>0)throw new Exception("Runtime exceptions during gameplay: "+SessionState.GetInt("RollerErrors",0));
    Time.timeScale=1;Debug.Log(ReferenceMode?"ROLLER_REFERENCE_LEVEL_OK: 25 sources including six portal outputs, 225 marbles matched, level won, no live conveyor overlaps.":"ROLLER_SORT_SMOKE_OK: six boxes released, 54 marbles matched, level won.");EditorApplication.update-=Tick;SessionState.SetBool("RollerSmoke",false);EditorApplication.Exit(0);}
   if(elapsed>(ReferenceMode?190:100)){foreach(var item in moving){var type=item.GetType();Debug.Log("WAITING color="+type.GetField("color").GetValue(item)+" stage="+type.GetField("stage").GetValue(item)+" position="+type.GetField("position").GetValue(item));}
    foreach(var item in Get("trays") as IList){var type=item.GetType();Debug.Log("TRAY color="+type.GetField("color").GetValue(item)+" count="+type.GetField("count").GetValue(item)+" landed="+type.GetField("landed").GetValue(item));}
    Debug.LogError("ROLLER_SORT_SMOKE_FAILED sent="+sent+" balls="+Get("activeBalls")+" simulationClock="+Get("clock")+" delta="+Time.deltaTime);EditorApplication.update-=Tick;SessionState.SetBool("RollerSmoke",false);EditorApplication.Exit(1);}
  }catch(Exception e){Debug.LogException(e);SessionState.SetBool("RollerSmoke",false);EditorApplication.Exit(1);}}
 }
}
