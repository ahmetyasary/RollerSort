using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
namespace RollerSort.Editor {
 public static class JourneyCheck {
  public static void Run(){
   var clock=new LifeClock(5,0);clock.Lose(1000);Require(clock.Count==4&&clock.Next==2800,"first loss starts refill");clock.Lose(1010);Require(clock.Count==3&&clock.Next==2800,"further loss preserves timer");clock.Refresh(2799);Require(clock.Count==3,"no early refill");clock.Refresh(6400);Require(clock.Count==5&&clock.Next==0,"offline catch up to maximum");clock=new LifeClock(0,2800);clock.Refresh(4600);Require(clock.Count==2&&clock.Next==6400,"multiple offline refills");
   for(int level=1;level<=25;level++){var rows=LevelLayouts.Rows(level);for(int i=0;i<rows.Length;i++)Require(rows[i].Length==rows[0].Length,"rectangular level definition");Require(Resources.Load<Texture2D>(level<=4?"Art/board-"+rows[0].Length:"Art/level-board-"+Mathf.Min(level,19)),"board asset "+level);}
   foreach(string name in new[]{"contact","seat","open","complete","victory","fail","shuffle","magnet","click","rolling"})Require(Resources.Load<AudioClip>("Audio/"+name),"sound "+name);
   Debug.Log("ROLLER_JOURNEY_CHECK_OK: life timing, offline refill, 25 level layouts and 10 sounds.");
  }
  static void Require(bool value,string message){if(!value)throw new Exception(message);}
 }
}
