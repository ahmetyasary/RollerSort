using System;
using System.Collections;
using System.Reflection;
using UnityEditor;
using UnityEngine;
namespace RollerSort.Editor {
 [InitializeOnLoad] public static class ProgressionCheck {
  const BindingFlags F=BindingFlags.Instance|BindingFlags.NonPublic;static RollerGame game;static int stage;static double started,ready;static string backup;
  static ProgressionCheck(){if(SessionState.GetBool("ProgressionQA",false)){started=EditorApplication.timeSinceStartup;EditorApplication.update+=Tick;}}
  public static void Run(){JourneyCheck.Run();backup=System.IO.Path.Combine("Builds","editor-progression-prefs.json");var snapshot=new PrefBackup();snapshot.Capture();System.IO.File.WriteAllText(backup,JsonUtility.ToJson(snapshot));SessionState.SetString("ProgressionBackup",backup);PlayerPrefs.SetInt("level",1);PlayerPrefs.SetInt("visitedJourney",0);PlayerPrefs.SetInt("lives",5);PlayerPrefs.SetString("lifeNext","0");for(int i=0;i<3;i++){PlayerPrefs.SetInt("intro"+i,1);PlayerPrefs.SetInt("booster"+i,1);}PlayerPrefs.Save();SessionState.SetBool("ProgressionQA",true);started=EditorApplication.timeSinceStartup;UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/RollerSort/Scenes/RollerSort.unity");EditorApplication.update+=Tick;EditorApplication.isPlaying=true;}
  [Serializable] class PrefBackup {public string[] keys={"level","coins","lives","questCount","questClaimed","visitedJourney","intro0","intro1","intro2","booster0","booster1","booster2"};public int[] values;public bool[] exists;public string lifeNext,questDay;public void Capture(){values=new int[keys.Length];exists=new bool[keys.Length];for(int i=0;i<keys.Length;i++){exists[i]=PlayerPrefs.HasKey(keys[i]);values[i]=PlayerPrefs.GetInt(keys[i],0);}lifeNext=PlayerPrefs.GetString("lifeNext","0");questDay=PlayerPrefs.GetString("questDay","");}public void Restore(){for(int i=0;i<keys.Length;i++){if(exists[i])PlayerPrefs.SetInt(keys[i],values[i]);else PlayerPrefs.DeleteKey(keys[i]);}PlayerPrefs.SetString("lifeNext",lifeNext);PlayerPrefs.SetString("questDay",questDay);PlayerPrefs.Save();}}
  static object Get(string n)=>typeof(RollerGame).GetField(n,F).GetValue(game);static void Set(string n,object v)=>typeof(RollerGame).GetField(n,F).SetValue(game,v);static void Call(string n,params object[] args)=>typeof(RollerGame).GetMethod(n,F).Invoke(game,args);static object Field(object o,string n)=>o.GetType().GetField(n).GetValue(o);static void Check(bool b,string m){if(!b)throw new Exception(m);}
  static void CheckBeltJam(){
   foreach(int level in new[]{1,21}){
    game.StopAllCoroutines();Set("level",level);Call("BuildLevel");
    var balls=(IList)Get("balls");var trays=(IList)Get("trays");
    foreach(var tray in trays){tray.GetType().GetField("color").SetValue(tray,3);tray.GetType().GetField("count").SetValue(tray,0);}
    int full=Mathf.FloorToInt(BeltTrack.PackingLength/BeltTrack.MinimumPitch);
    var type=typeof(RollerGame).GetNestedType("Ball",BindingFlags.NonPublic);
    for(int i=0;i<full;i++){var ball=Activator.CreateInstance(type);type.GetField("stage").SetValue(ball,1);type.GetField("color").SetValue(ball,0);balls.Add(ball);}
    Func<bool> blocked=()=> (bool)typeof(RollerGame).GetMethod("BeltCannotProgress",F).Invoke(game,null);
    Check(blocked(),"Full unmatched belt must stop progression");
    var first=trays[0];first.GetType().GetField("color").SetValue(first,0);Check(!blocked(),"Matching receiver must prevent premature failure");
    first.GetType().GetField("color").SetValue(first,3);first.GetType().GetField("completing").SetValue(first,true);Check(!blocked(),"Receiver transition must finish before failure");first.GetType().GetField("completing").SetValue(first,false);
    balls.RemoveAt(0);Check(!blocked(),"Belt with free space must continue");
    var extra=Activator.CreateInstance(type);type.GetField("stage").SetValue(extra,1);type.GetField("color").SetValue(extra,0);balls.Add(extra);
    Set("diagnosticsActive",true);Call("CheckBlockedBelt",2.4f);Check(!(bool)Get("spaceOffer"),"Jam grace period skipped");Call("CheckBlockedBelt",.2f);Check((bool)Get("spaceOffer")&&(bool)Get("paused"),"Full belt did not open out-of-space modal");
    balls.Clear();game.StopAllCoroutines();
   }
   Set("diagnosticsActive",false);Debug.Log("ROLLER_BELT_JAM_OK: full unmatched base/expanded belt, matching receiver, transition, free space, grace period and modal.");
  }
  static void Tick(){try{if(EditorApplication.timeSinceStartup-started>35)throw new Exception("Progression test timed out");if(!EditorApplication.isPlaying)return;EditorApplication.QueuePlayerLoopUpdate();Application.runInBackground=true;game=UnityEngine.Object.FindFirstObjectByType<RollerGame>();if(!game||Get("game")==null)return;
   if(stage==0){CheckBeltJam();Set("level",6);Call("BuildLevel");Call("Booster",0);var boxes=(IList)Get("boxes");object upper=null;foreach(var b in boxes)if((int)Field(b,"row")==2){upper=b;break;}Call("Release",upper);Check((bool)Field(upper,"removed"),"Magnet cannot select upper box");foreach(var b in (IList)Get("balls"))Check((bool)Field(b,"magnetic"),"Magnet release not homing");Set("level",8);Call("BuildLevel");Call("Booster",1);ready=EditorApplication.timeSinceStartup;stage++;return;}
   if(stage==1&&EditorApplication.timeSinceStartup-ready>.8){Check(!(bool)Get("paused"),"Shuffle left gameplay paused");foreach(var b in (IList)Get("boxes")){if((bool)Field(b,"removed"))continue;var view=(RectTransform)Field(b,"view");float expected=((int)Field(b,"col")-2)*53;Check(Mathf.Abs(view.anchoredPosition.x-expected)<.01f,"Shuffle data disagrees with visual position");}Set("level",11);Call("BuildLevel");var boxes=(IList)Get("boxes");int count=boxes.Count;object source=null;foreach(var b in boxes)if((int)Field(b,"row")==0&&(int)Field(b,"col")==3)source=b;Call("Release",source);Check(boxes.Count==count&&((IList)Get("portals")).Count==2,"Fixed tunnels must wait while their output cell is occupied");Call("Booster",2);foreach(var b in (IList)Get("balls"))Check((bool)Field(b,"magnetic"),"Cleanup did not attract waiting marble");Call("FailLevel");var life=(LifeClock)Get("life");Check(life.Count==4,"Failure did not consume exactly one life");Call("FailLevel");Check(life.Count==4,"Failure consumed twice");Call("Journey");Check((bool)Get("journey")&&(bool)Get("paused"),"Journey did not pause gameplay");Call("PlayFromJourney");Check(!(bool)Get("journey")&&!(bool)Get("paused"),"Play did not resume level");life.Count=0;Call("Journey");Call("PlayFromJourney");Check((bool)Get("journey"),"Zero lives allowed gameplay");Finish(true,null);}
  }catch(Exception e){Finish(false,e);}}
  static void Finish(bool ok,Exception e){var path=SessionState.GetString("ProgressionBackup","");if(System.IO.File.Exists(path))JsonUtility.FromJson<PrefBackup>(System.IO.File.ReadAllText(path)).Restore();SessionState.SetBool("ProgressionQA",false);EditorApplication.update-=Tick;if(ok)Debug.Log("ROLLER_PROGRESSION_OK: magnet, shuffle positions, tunnel successor, cleanup, single life loss, journey and no-life gate.");else Debug.LogException(e);EditorApplication.Exit(ok?0:1);}
 }
}
