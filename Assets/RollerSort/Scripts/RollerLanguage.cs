using System.Collections;
using UnityEngine;
using UnityEngine.UI;
namespace RollerSort {
 public sealed partial class RollerGame {
  Text languageName;
  void DrawLanguageChoice(RectTransform parent,float y=414){
   var row=Picture(parent,"hud",new Vector2(0,y),new Vector2(310,54));row.name="Language selector";
   languageName=Label(row,GameLanguage.NativeName,new Vector2(0,27),25,bounds:new Vector2(200,46),localize:false);
   Button(row,"◀",new Vector2(-130,27),new Vector2(42,46),new Color32(255,124,35,255),CycleLanguage,23);
   Button(row,"▶",new Vector2(130,27),new Vector2(42,46),new Color32(255,124,35,255),CycleLanguage,23);
  }
  void CycleLanguage(){GameLanguage.Select(GameLanguage.Code=="en"?"tr":"en");if(languageName){languageName.text=GameLanguage.NativeName;StartCoroutine(AnimateLanguageChoice(languageName.rectTransform));}HapticFeedback.Play(HapticCue.Selection);}
  IEnumerator AnimateLanguageChoice(RectTransform label){for(float t=0;t<.22f;){if(!label)yield break;t+=Time.unscaledDeltaTime;label.localScale=Vector3.one*(1+.08f*Mathf.Sin(Mathf.Clamp01(t/.22f)*Mathf.PI));yield return null;}if(label)label.localScale=Vector3.one;}
 }
}
