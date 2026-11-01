using System;
using System.Collections;
using System.Reflection;
using UnityEditor;
using UnityEngine;
namespace RollerSort.Editor {
 [InitializeOnLoad] public static class DenseMotionCheck {
  const BindingFlags F=BindingFlags.Instance|BindingFlags.NonPublic;static RollerGame game;static double started;static bool done;
  static DenseMotionCheck(){if(SessionState.GetBool("DenseMotionQA",false)){started=EditorApplication.timeSinceStartup;EditorApplication.update+=Tick;}}
  public static void Run(){SessionState.SetBool("DenseMotionQA",true);started=EditorApplication.timeSinceStartup;UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/RollerSort/Scenes/RollerSort.unity");EditorApplication.update+=Tick;EditorApplication.isPlaying=true;}
  static object Get(string n)=>typeof(RollerGame).GetField(n,F).GetValue(game);static void Set(string n,object v)=>typeof(RollerGame).GetField(n,F).SetValue(game,v);static void Call(string n,params object[] args)=>typeof(RollerGame).GetMethod(n,F).Invoke(game,args);static void Field(object o,string n,object v)=>o.GetType().GetField(n).SetValue(o,v);static object Field(object o,string n)=>o.GetType().GetField(n).GetValue(o);
  static void Tick(){try{if(EditorApplication.timeSinceStartup-started>40)throw new Exception("Dense motion test timeout");if(!EditorApplication.isPlaying)return;EditorApplication.QueuePlayerLoopUpdate();Application.runInBackground=true;game=UnityEngine.Object.FindFirstObjectByType<RollerGame>();if(!game||Get("game")==null||done)return;done=true;game.enabled=false;Set("diagnosticsActive",true);Set("level",19);Scenario(false,false);Scenario(true,false);Scenario(true,true);FunnelStress();Debug.Log("ROLLER_DENSE_MOTION_OK: base/full expanded belts, full circuit and admission into one remaining gap, no overlaps.");Finish(0);}catch(Exception e){Debug.LogException(e);Finish(1);}}
  static void Scenario(bool expanded,bool admission){Set("level",expanded?19:1);Call("BuildLevel");var moving=(IList)Get("balls");foreach(var t in (IList)Get("trays"))Field(t,"color",-1);int number=BeltTrack.SlotCount;
   for(int i=0;i<number;i++){if(admission&&i==0)continue;float phase=BeltTrack.FromPackingPhase((float)i/number);Add(1,phase,BeltTrack.Point(phase));}
   if(admission)Add(0,0,new Vector2(0,738));Set("activeBalls",number);
   for(int step=0;step<600;step++){Call("StepMarbles",1f/120);for(int i=0;i<moving.Count;i++){var a=moving[i];int sa=(int)Field(a,"stage");if(sa!=1&&sa!=3)continue;for(int j=i+1;j<moving.Count;j++){var b=moving[j];int sb=(int)Field(b,"stage");if(sb!=1&&sb!=3)continue;float d=Vector2.Distance((Vector2)Field(a,"position"),(Vector2)Field(b,"position"));if(d<BeltTrack.PaintedDiameter)throw new Exception("Dense overlap "+d+" expanded="+expanded+" admission="+admission+" step="+step);}}}
   if(admission)foreach(var b in moving)if((int)Field(b,"stage")==0||(int)Field(b,"stage")==3)throw new Exception("Admission never finished");
   void Add(int stage,float phase,Vector2 point){var type=typeof(RollerGame).GetNestedType("Ball",BindingFlags.NonPublic);var b=Activator.CreateInstance(type,true);Field(b,"color",0);Field(b,"stage",stage);Field(b,"phase",phase);Field(b,"age",1f);Field(b,"radius",BeltTrack.PaintedDiameter*.5f);Field(b,"position",point);Field(b,"previous",point);Field(b,"start",point);Field(b,"velocity",new Vector2(0,100));var view=(RectTransform)typeof(RollerGame).GetMethod("TakeMarble",F).Invoke(game,new object[]{0});Field(b,"view",view);Field(b,"visual",((IDictionary)Get("marbleVisuals"))[view]);moving.Add(b);}
  }
  static void FunnelStress(){Set("level",19);Call("BuildLevel");foreach(var t in (IList)Get("trays"))Field(t,"color",-1);var moving=(IList)Get("balls");var type=typeof(RollerGame).GetNestedType("Ball",BindingFlags.NonPublic);for(int i=0;i<36;i++){var b=Activator.CreateInstance(type,true);Vector2 p=new((i%6-2.5f)*26,570+(i/6)*26);Field(b,"color",0);Field(b,"stage",0);Field(b,"age",1f);Field(b,"radius",BeltTrack.PaintedDiameter*.5f);Field(b,"position",p);Field(b,"previous",p);Field(b,"start",p);Field(b,"velocity",Vector2.zero);var view=(RectTransform)typeof(RollerGame).GetMethod("TakeMarble",F).Invoke(game,new object[]{0});Field(b,"view",view);Field(b,"visual",((IDictionary)Get("marbleVisuals"))[view]);moving.Add(b);}Set("activeBalls",36);
   for(int i=0;i<1800;i++){Call("StepMarbles",1f/120);if(i<480||i%60!=0)continue;for(int j=moving.Count-1;j>=0;j--)if((int)Field(moving[j],"stage")==1){Call("ReturnMarble",Field(moving[j],"view"));moving.RemoveAt(j);}}
   foreach(var b in moving)if((int)Field(b,"stage")!=1)throw new Exception("Funnel retained a stranded sphere: "+Field(b,"position"));
   Debug.Log("ROLLER_FUNNEL_STRESS_OK: 36-sphere pile drains after outlet pressure is released.");
  }
  static void Finish(int code){SessionState.SetBool("DenseMotionQA",false);EditorApplication.update-=Tick;EditorApplication.Exit(code);}
 }
}
