using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
namespace RollerSort {
 public static class GameLanguage {
  [Serializable] public class Entry { public string key,value; }
  [Serializable] public class Catalog {public Entry[] entries;}
  static readonly Dictionary<string,Dictionary<string,string>> languages=new();
  static readonly Dictionary<string,string> aliases=new();
  static readonly List<KeyValuePair<Regex,string>> templates=new();
  public static string Code{get;private set;}="en";
  public static event Action Changed;
  public static string NativeName=>Code=="tr"?"Türkçe":"English";
  public static void Load(){
   if(languages.Count==0){foreach(string code in new[]{"en","tr"}){var asset=Resources.Load<TextAsset>("Localization/"+code);if(!asset)throw new InvalidOperationException("Missing language resource: "+code);var catalog=JsonUtility.FromJson<Catalog>(asset.text);var table=new Dictionary<string,string>();foreach(var entry in catalog.entries){table.Add(entry.key,entry.value);aliases[entry.key]=entry.key;aliases[entry.value]=entry.key;}languages.Add(code,table);}
    foreach(var pair in aliases)if(pair.Key.Contains("{0}")){string pattern="^"+Regex.Escape(pair.Key);for(int i=0;i<4;i++)pattern=pattern.Replace(Regex.Escape("{"+i+"}"),"(.+?)");templates.Add(new KeyValuePair<Regex,string>(new Regex(pattern+"$"),pair.Value));}
   }
   string selected=PlayerPrefs.GetString("language",Application.systemLanguage==SystemLanguage.Turkish?"tr":"en");Code=languages.ContainsKey(selected)?selected:"en";
  }
  public static void Select(string code){if(languages.Count==0)Load();if(!languages.ContainsKey(code))return;Code=code;PlayerPrefs.SetString("language",code);PlayerPrefs.Save();Changed?.Invoke();}
  public static string Text(string source){if(string.IsNullOrEmpty(source))return source;if(languages.Count==0)Load();if(aliases.TryGetValue(source,out string key))return languages[Code][key];foreach(var item in templates){var match=item.Key.Match(source);if(!match.Success)continue;var args=new object[match.Groups.Count-1];for(int i=0;i<args.Length;i++)args[i]=match.Groups[i+1].Value;return string.Format(languages[Code][item.Value],args);}return source;}
 }
 public sealed class LocalizedGameText:MonoBehaviour {
  Text label;string source,rendered;
  public void Bind(Text target,string value){label=target;source=value;GameLanguage.Changed+=Refresh;Render();}
  void Render(){rendered=GameLanguage.Text(source);label.text=rendered;}
  void Refresh(){if(!label)return;if(label.text!=rendered)source=label.text;Render();}
  void OnDestroy(){GameLanguage.Changed-=Refresh;}
 }
}
