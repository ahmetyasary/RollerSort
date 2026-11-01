using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
namespace RollerSort.Editor {
 public static class LanguageCheck {
  static void Require(bool value,string reason){if(!value)throw new Exception(reason);}
  public static void Run(){bool had=PlayerPrefs.HasKey("language");string before=PlayerPrefs.GetString("language");GameObject sample=null;try{
   GameLanguage.Load();GameLanguage.Select("en");Require(GameLanguage.Text("Settings")=="Settings","English settings");Require(GameLanguage.Text("Bölüm 11 ile açılır")=="Unlocks at level 11","Dynamic English unlock");Require(GameLanguage.Text("ŞİMDİ · 11")=="NOW · 11","Journey localization");
   sample=new GameObject("Language QA",typeof(Text));var text=sample.GetComponent<Text>();sample.AddComponent<LocalizedGameText>().Bind(text,"Settings");
   GameLanguage.Select("tr");Require(text.text=="Ayarlar","Live language refresh");Require(GameLanguage.Text("Level 11")=="Bölüm 11","Dynamic Turkish level");Require(GameLanguage.Text("Play · 11")=="Oyna · 11","Play number preserved");Require(GameLanguage.Text("Yeni can: 12:34\nCanlar 30 dakikada bir yenilenir.").Contains("12:34"),"Timer value lost");Require(GameLanguage.Text("Unknown player name")=="Unknown player name","Unknown fallback");
   GameLanguage.Load();Require(GameLanguage.Code=="tr","Language persistence");GameLanguage.Select("invalid");Require(GameLanguage.Code=="tr","Invalid language changed selection");GameLanguage.Select("en");Require(text.text=="Settings","Reverse live refresh");Require(GameLanguage.Text("3 bölüm tamamla\n\n2 / 3   ·   Ödül: 250 altın").Contains("2 / 3"),"Quest format");
   Debug.Log("ROLLER_LANGUAGE_OK: Resources catalogs, live refresh both directions, persistence, numeric templates, timer/quest formats and fallback.");
  }finally{if(sample)UnityEngine.Object.DestroyImmediate(sample);if(had)PlayerPrefs.SetString("language",before);else PlayerPrefs.DeleteKey("language");PlayerPrefs.Save();GameLanguage.Load();}EditorApplication.Exit(0);}
 }
}
