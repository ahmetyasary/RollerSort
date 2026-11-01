using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace RollerSort {
 public sealed partial class RollerGame {
  string FormatCoins()=>coins.ToString(System.Globalization.CultureInfo.InvariantCulture);
  void ProfileButton(Transform parent,Vector2 point){var b=Button(parent,"",point,new Vector2(50,51),purple,EditProfile);Picture(b,"avatar-"+PlayerPrefs.GetInt("avatar",5),new Vector2(0,25),new Vector2(42,44));}

  void EditProfile(){
   int choice=Mathf.Clamp(PlayerPrefs.GetInt("avatar",5),0,8);string pendingName=PlayerPrefs.GetString("playerName","Player1429");var p=Modal("Edit Profile",690);
   var close=Button(p,"",new Vector2(155,29),new Vector2(43,43),colors[1],Close);Picture(close,"close",new Vector2(0,21),new Vector2(29,29));
   var identity=Panel(p,"Profile identity",new Vector2(0,118),new Vector2(338,82),new Color32(132,91,211,255));
   var portraitFrame=Panel(identity,"Current avatar frame",new Vector2(-128,41),new Vector2(72,72),new Color32(255,169,30,255));
   var portrait=Picture(portraitFrame,"avatar-"+choice,new Vector2(0,36),new Vector2(65,65));
   var nameField=Panel(identity,"Profile name field",new Vector2(16,41),new Vector2(215,51),new Color32(91,66,193,255));
   var nameText=Label(nameField,pendingName,new Vector2(-7,24),21,bounds:new Vector2(190,42),localize:false);nameText.alignment=TextAnchor.MiddleLeft;
   var pencil=Button(identity,"",new Vector2(143,41),new Vector2(43,44),new Color32(198,83,222,255),()=>OpenNameEditor(nameText,value=>pendingName=value));DrawPencil(pencil);
   var chooser=Panel(p,"Avatar chooser",new Vector2(0,345),new Vector2(342,350),new Color32(70,52,150,255));
   Panel(chooser,"Chooser header",new Vector2(0,24),new Vector2(342,48),new Color32(176,132,235,255));Label(chooser,"Choose Avatar:",new Vector2(0,24),20,bounds:new Vector2(310,40));
   var viewport=Panel(chooser,"Avatar viewport",new Vector2(0,190),new Vector2(330,270),Color.clear);viewport.GetComponent<Image>().raycastTarget=true;viewport.gameObject.AddComponent<RectMask2D>();
   var content=Panel(viewport,"Avatar list content",new Vector2(0,183),new Vector2(330,366),Color.clear);
   var scroll=chooser.gameObject.AddComponent<ScrollRect>();scroll.viewport=viewport;scroll.content=content;scroll.horizontal=false;scroll.vertical=true;scroll.movementType=ScrollRect.MovementType.Elastic;scroll.elasticity=.12f;scroll.decelerationRate=.12f;scroll.scrollSensitivity=28;
   var options=new RectTransform[9];var checks=new RectTransform[9];
   Action refresh=()=>{portrait.GetComponent<Image>().sprite=Art("avatar-"+choice);for(int j=0;j<options.Length;j++){bool selected=j==choice;options[j].GetComponent<Image>().color=selected?new Color32(56,235,65,255):Color.white;checks[j].gameObject.SetActive(selected);if(selected){options[j].localScale=Vector3.one;StartCoroutine(Pop(options[j],.20f,.11f));}}};
   for(int i=0;i<9;i++){int index=i;var option=Button(content,"",new Vector2(-106+(i%3)*106,55+(i/3)*112),new Vector2(88,89),purple,()=>{if(choice==index)return;choice=index;HapticFeedback.Play(HapticCue.Selection);refresh();});option.name="Avatar option "+i;options[i]=option;Picture(option,"avatar-"+i,new Vector2(0,44),new Vector2(79,80));var check=Picture(option,"journey-check",new Vector2(33,75),new Vector2(28,28));check.name="Selected avatar check";checks[i]=check;}
   refresh();
   Canvas.ForceUpdateCanvases();scroll.verticalNormalizedPosition=1;
   Button(p,"Save",new Vector2(0,625),new Vector2(235,65),colors[2],()=>{PlayerPrefs.SetInt("avatar",choice);pendingName=pendingName.Trim();if(pendingName.Length>0)PlayerPrefs.SetString("playerName",pendingName);PlayerPrefs.Save();if(journey)Journey();else Close();},29);
  }

  void DrawPencil(RectTransform parent){var shaft=Panel(parent,"Pencil shaft",new Vector2(-1,22),new Vector2(24,7),Color.white);shaft.localRotation=Quaternion.Euler(0,0,-45);var tip=Panel(parent,"Pencil tip",new Vector2(9,32),new Vector2(7,7),new Color32(255,211,92,255));tip.localRotation=Quaternion.Euler(0,0,-45);}

  void OpenNameEditor(Text nameText,Action<string> accepted){
   var shade=Panel(overlay,"Name editor",new Vector2(0,H/2),new Vector2(W,H),new Color(0.025f,.02f,.08f,.86f));shade.GetComponent<Image>().raycastTarget=true;shade.SetAsLastSibling();
   var dialog=Picture(shade,"dialog",new Vector2(0,H/2-95),new Vector2(390,330));dialog.GetComponent<Image>().type=Image.Type.Simple;
   Panel(dialog,"Header divider",new Vector2(0,61),new Vector2(380,2),new Color32(86,65,155,255));Label(dialog,"Enter Name",new Vector2(0,31),32);
   var close=Button(dialog,"",new Vector2(155,29),new Vector2(43,43),colors[1],()=>Destroy(shade.gameObject));Picture(close,"close",new Vector2(0,21),new Vector2(29,29));
   Label(dialog,"Enter your new name:",new Vector2(0,108),19,bounds:new Vector2(320,36));
   var field=Panel(dialog,"Name input",new Vector2(0,158),new Vector2(318,58),Color.white);field.GetComponent<Image>().raycastTarget=true;
   var inputText=Label(field,nameText.text,new Vector2(0,29),25,new Color32(42,35,59,255),new Vector2(286,50),false);inputText.GetComponent<Outline>().enabled=false;
   var input=field.gameObject.AddComponent<InputField>();input.textComponent=inputText;input.text=nameText.text;input.characterLimit=16;input.lineType=InputField.LineType.SingleLine;input.targetGraphic=field.GetComponent<Image>();
   Button(dialog,"Save",new Vector2(0,260),new Vector2(220,61),colors[2],()=>{string value=input.text.Trim();if(value.Length==0)return;nameText.text=value;accepted(value);HapticFeedback.Play(HapticCue.Selection);Destroy(shade.gameObject);},28);
   StartCoroutine(FocusNameInput(input));StartCoroutine(ModalEntrance(dialog));
  }
  IEnumerator FocusNameInput(InputField input){yield return null;yield return null;if(!input)yield break;input.Select();input.ActivateInputField();input.MoveTextEnd(false);}

  void FeatureInfo(int milestone){var p=Modal("Level "+milestone,290);Label(p,"Bu özellik "+milestone+". bölümde açılır.",new Vector2(0,128),22,bounds:new Vector2(330,90));Button(p,"Journey",new Vector2(0,225),new Vector2(245,58),colors[2],Journey,26);}
  void NoAdsInfo(){var p=Modal("No Ads",405);Picture(p,"no-ads",new Vector2(0,148),new Vector2(135,135));Label(p,"Bölümler arasında kesintisiz oyna.\nBu sürümde satın alma önizlemesidir.",new Vector2(0,266),21,bounds:new Vector2(330,100));Button(p,"Close",new Vector2(0,346),new Vector2(240,58),colors[2],Close,26);}
 }
}
