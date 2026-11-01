using UnityEngine;
namespace RollerSort {
 public sealed class PlayButtonMotion:MonoBehaviour {
  RectTransform shine;float cycle;
  public void Bind(RectTransform target){shine=target;cycle=-.45f;}
  void Update(){
   if(!shine)return;cycle=Mathf.Repeat(cycle+Time.unscaledDeltaTime,2.65f);float p=Mathf.Clamp01(cycle/.72f);shine.anchoredPosition=new Vector2(Mathf.Lerp(-165,165,p),45);var image=shine.GetComponent<UnityEngine.UI.Image>();image.color=new Color(1,1,1,cycle<.72f?Mathf.Sin(p*Mathf.PI)*.18f:0);
  }
 }
}
