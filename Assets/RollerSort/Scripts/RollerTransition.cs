using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace RollerSort {
 // Hosts the curtain coroutine so BuildLevel's StopAllCoroutines cannot cut it short.
 public sealed class ScreenTransition:MonoBehaviour {}
 public sealed partial class RollerGame {
  Sprite logoSprite;
  Sprite Logo(){if(logoSprite)return logoSprite;var texture=Resources.Load<Texture2D>("RollerSortLogoV2");if(texture)logoSprite=Sprite.Create(texture,new Rect(0,0,texture.width,texture.height),new Vector2(.5f,.5f),100);return logoSprite;}
  // The splash shutters close over the current screen around the logo, the
  // next screen is built underneath, then the shutters open onto it.
  void LogoTransition(Action swap){
   var curtain=Panel(root,"Logo transition",new Vector2(0,H/2),new Vector2(W,H),Color.clear);curtain.GetComponent<Image>().raycastTarget=true;
   var halves=new RectTransform[2];
   for(int i=0;i<2;i++){var half=Panel(curtain,i==0?"Upper shutter":"Lower shutter",new Vector2(0,H/4+i*H/2),new Vector2(W,H/2),Color.clear);half.gameObject.AddComponent<RectMask2D>();Picture(half,"splash-bg",new Vector2(0,H/2-i*H/2),new Vector2(W,H));halves[i]=half;}
   var logo=Panel(curtain,"Logo",new Vector2(0,465),new Vector2(306,204),Color.white);var image=logo.GetComponent<Image>();image.sprite=Logo();image.type=Image.Type.Simple;image.preserveAspect=true;
   curtain.gameObject.AddComponent<ScreenTransition>().StartCoroutine(RunLogoTransition(curtain,halves,logo,image,swap));
  }
  IEnumerator RunLogoTransition(RectTransform curtain,RectTransform[] halves,RectTransform logo,Image image,Action swap){
   void Shutter(float closed){float gap=(1-closed)*H/2;halves[0].anchoredPosition=new Vector2(0,-H/4+gap);halves[1].anchoredPosition=new Vector2(0,-3*H/4-gap);}
   Shutter(0);logo.localScale=Vector3.zero;Sfx("shuffle",.45f,1.15f);
   bool met=false;
   for(float t=0;t<.44f;){t+=Time.unscaledDeltaTime;float p=Mathf.Clamp01(t/.3f);Shutter(p*p*(3-2*p));
    if(p>=1&&!met){met=true;HapticFeedback.Play(HapticCue.Selection);}
    float q=Mathf.Clamp01((t-.16f)/.28f);logo.localScale=Vector3.one*(q<=0?0:1+2.7f*Mathf.Pow(q-1,3)+1.7f*Mathf.Pow(q-1,2));
    yield return null;}
   Shutter(1);logo.localScale=Vector3.one;
   swap();curtain.SetAsLastSibling();
   yield return new WaitForSecondsRealtime(.12f);
   Sfx("open",.4f,1.2f);
   for(float t=0;t<.36f;){t+=Time.unscaledDeltaTime;float p=Mathf.Clamp01(t/.36f),fade=Mathf.Clamp01(t/.22f);Shutter(1-p*p*(3-2*p));logo.localScale=Vector3.one*Mathf.Lerp(1,1.25f,fade);image.color=new Color(1,1,1,1-fade);yield return null;}
   Destroy(curtain.gameObject);
  }
 }
}
