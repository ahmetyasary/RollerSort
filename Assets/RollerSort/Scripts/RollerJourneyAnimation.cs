using System.Collections;
using UnityEngine;
using UnityEngine.UI;
namespace RollerSort {
 public sealed partial class RollerGame {
  bool victoryTransition,journeyRewardPending;
  const float JourneyNodeStep=160f;
  bool IsHardJourneyLevel(int value)=>value>=15&&value%10==5;
  bool IsVeryHardJourneyLevel(int value)=>value>=18&&value%10==8;
  float JourneyNodeOpacity(float y)=>Mathf.Lerp(.08f,1f,Mathf.SmoothStep(0,1,Mathf.InverseLerp(60,220,y)));
  RectTransform BuildJourneyRoute(){
   var route=Panel(game,"Journey medals",Vector2.zero,Vector2.zero,Color.clear);
   route.SetSiblingIndex(Mathf.Min(3,route.parent.childCount-1));
   int count=journeyCompleted?5:4;
   for(int i=0;i<count;i++){
    int n=(journeyCompleted?level-1:level)+i;float y=540-i*JourneyNodeStep;
    bool current=n==level;
    var holder=Panel(route,"Level node "+n,new Vector2(0,y),new Vector2(150,150),Color.clear);
    var visibility=holder.gameObject.AddComponent<CanvasGroup>();visibility.alpha=JourneyNodeOpacity(y);
    if(current){
     var glow=Picture(holder,"journey-focus-ring",new Vector2(0,66),new Vector2(142,142));glow.name="Current level halo";glow.GetComponent<Image>().color=Color.white;StartCoroutine(PulseCurrentLevel(glow));
    }
    bool hard=IsHardJourneyLevel(n)||IsVeryHardJourneyLevel(n);
    var medal=Picture(holder,hard?"journey-node-hard":"journey-node",new Vector2(0,66),new Vector2(112,115));
    if(n<level&&!journeyRewardPending)Picture(medal,"journey-check",new Vector2(0,54),new Vector2(63,63));else Label(medal,n.ToString(),new Vector2(0,54),38);
    if(current){
     medal.localScale=Vector3.one*1.045f;
    }
    if(hard){
     var badge=Panel(holder,"Difficulty badge "+n,new Vector2(0,126),new Vector2(91,29),new Color32(196,24,51,255));
     Label(badge,IsVeryHardJourneyLevel(n)?"Very Hard":"Hard",new Vector2(0,14),17,bounds:new Vector2(86,27));
    }
   }
   return route;
  }
  IEnumerator PulseCurrentLevel(RectTransform halo){float time=0;while(halo){time+=Time.unscaledDeltaTime;halo.localScale=Vector3.one*(1+.045f*Mathf.Sin(time*3));halo.GetComponent<Image>().color=new Color(1,1,1,.72f+.18f*Mathf.Sin(time*3));yield return null;}}
  IEnumerator AdvanceJourney(RectTransform route){
   var blocker=Panel(game,"Journey transition input",new Vector2(0,H/2),new Vector2(W,H),Color.clear);blocker.GetComponent<Image>().raycastTarget=true;
   yield return new WaitForSecondsRealtime(.38f);
   Vector2 home=route.anchoredPosition;
   var old=route.GetChild(0) as RectTransform;var fading=old.GetComponent<CanvasGroup>();
   for(float t=0;t<.85f;){
    t+=Time.unscaledDeltaTime;float p=Mathf.Clamp01(t/.85f),ease=p*p*(3-2*p);route.anchoredPosition=home+Vector2.down*(JourneyNodeStep*ease);fading.alpha=1-ease;old.localScale=Vector3.one*(1-.3f*ease);
    for(int i=1;i<route.childCount;i++){var node=route.GetChild(i) as RectTransform;var group=node.GetComponent<CanvasGroup>();if(group)group.alpha=JourneyNodeOpacity(node.anchoredPosition.y+JourneyNodeStep*ease);}
    yield return null;
   }
   route.anchoredPosition=home+Vector2.down*JourneyNodeStep;
   HapticFeedback.Play(HapticCue.Selection);Sfx("complete",.45f,1.15f);
   yield return new WaitForSecondsRealtime(.18f);
   journeyCompleted=false;Journey();
  }
  IEnumerator JourneyRewardSequence(RectTransform route){
   var old=route.Find("Level node "+(level-1)) as RectTransform;if(!old){journeyRewardPending=false;StartCoroutine(AdvanceJourney(route));yield break;}
   var shade=Panel(game,"Journey entrance shade",new Vector2(0,H/2),new Vector2(W,H),new Color(0,0,.12f,.68f));shade.SetAsLastSibling();shade.GetComponent<Image>().raycastTarget=true;
   var layer=ScreenPanel("Reward flight",Color.clear);layer.GetComponent<Image>().raycastTarget=true;
   Vector2 oldOrigin=layer.InverseTransformPoint(old.position);var focus=Picture(layer,"journey-focus-ring",oldOrigin,new Vector2(190,190));var focusImage=focus.GetComponent<Image>();focusImage.color=new Color(1,1,1,.35f);
   var rewardCoin=Picture(layer,"coin",oldOrigin+Vector2.down*8,new Vector2(39,43));
   var rewardText=Label(layer,"+"+pendingReward,oldOrigin+Vector2.up*31,36,bounds:new Vector2(140,55),localize:false);
   for(float t=0;t<.34f;){t+=Time.unscaledDeltaTime;float p=Mathf.Clamp01(t/.34f);shade.GetComponent<Image>().color=new Color(0,0,.12f,.68f*(1-p));focus.localScale=Vector3.one*Mathf.Lerp(.65f,1.05f,1-Mathf.Pow(1-p,3));rewardText.rectTransform.localScale=rewardCoin.localScale=Vector3.one*Mathf.Lerp(.65f,1.12f,Mathf.Sin(p*Mathf.PI*.5f));yield return null;}Destroy(shade.gameObject);
   Vector2 origin=rewardCoin.anchoredPosition,target=layer.InverseTransformPoint((coinText.transform.parent.Find("coin") as RectTransform).position);const int count=12;var flying=new RectTransform[count];var arrived=new bool[count];
   for(int i=0;i<count;i++){flying[i]=Picture(layer,"coin",origin,new Vector2(24,27));flying[i].gameObject.SetActive(false);}int credited=0;
   for(float elapsed=0;elapsed<1.25f;){elapsed+=Time.unscaledDeltaTime;float pulse=Mathf.Sin(Mathf.Clamp01(elapsed/.45f)*Mathf.PI);focus.localScale=Vector3.one*(1.05f+.10f*pulse);
    for(int i=0;i<count;i++){float age=elapsed-i*.035f;if(age<0||arrived[i])continue;var coin=flying[i];coin.gameObject.SetActive(true);float p=Mathf.Clamp01(age/.72f),ease=p*p;Vector2 control=origin+new Vector2(Mathf.Sin(i*2.1f)*55,-125-Mathf.Cos(i)*25);coin.anchoredPosition=(1-ease)*(1-ease)*origin+2*(1-ease)*ease*control+ease*ease*target;coin.localScale=Vector3.one*Mathf.Lerp(1,.58f,p);coin.localRotation=Quaternion.Euler(0,0,p*300+i*17);if(p>=1){arrived[i]=true;coin.gameObject.SetActive(false);credited++;coinText.text=(coins-pendingReward+Mathf.RoundToInt((float)pendingReward*credited/count)).ToString("N0",System.Globalization.CultureInfo.InvariantCulture);coinText.transform.parent.localScale=Vector3.one*1.08f;Sfx("seat",.3f,1+credited*.025f);}}
    coinText.transform.parent.localScale=Vector3.Lerp(coinText.transform.parent.localScale,Vector3.one,Time.unscaledDeltaTime*9);rewardCoin.localScale=rewardText.rectTransform.localScale=Vector3.one*(1-.18f*Mathf.SmoothStep(0,1,Mathf.InverseLerp(.25f,1.05f,elapsed)));yield return null;
   }
   coinText.text=FormatCoins();coinText.transform.parent.localScale=Vector3.one;HapticFeedback.Play(HapticCue.LevelComplete);Sfx("complete",.55f,1.15f);Destroy(layer.gameObject);journeyRewardPending=false;Journey();
  }
  void BeginVictoryContinue(RectTransform reward){if(victoryTransition)return;victoryTransition=true;StartCoroutine(CollectVictoryReward(reward));}
  IEnumerator CollectVictoryReward(RectTransform reward){
   var layer=ScreenPanel("Reward flight",Color.clear);layer.GetComponent<Image>().raycastTarget=true;
   Vector2 origin=layer.InverseTransformPoint(reward.position);
   var targetIcon=coinText.transform.parent.Find("coin") as RectTransform;
   Vector2 target=layer.InverseTransformPoint(targetIcon?targetIcon.position:coinText.transform.position);
   const int count=12;var flying=new RectTransform[count];var arrived=new bool[count];int credited=0;
   for(int i=0;i<count;i++){flying[i]=Picture(layer,"coin",Vector2.zero,new Vector2(24,27));flying[i].anchorMin=flying[i].anchorMax=Vector2.one*.5f;flying[i].anchoredPosition=origin;flying[i].gameObject.SetActive(false);}
   for(float elapsed=0;elapsed<1.35f;){
    elapsed+=Time.unscaledDeltaTime;
    for(int i=0;i<count;i++){
     float age=elapsed-i*.036f;if(age<0||arrived[i])continue;var r=flying[i];r.gameObject.SetActive(true);float p=Mathf.Clamp01(age/.83f),ease=p*p;
     Vector2 scatter=new(Mathf.Sin(i*2.4f)*48,Mathf.Cos(i*2.4f)*36);
     Vector2 control=origin+scatter+Vector2.left*95+Vector2.up*135;
     r.anchoredPosition=(1-ease)*(1-ease)*origin+2*(1-ease)*ease*control+ease*ease*target;
     r.localScale=new Vector3((.7f+.3f*Mathf.Abs(Mathf.Cos(age*10+i)))*Mathf.Lerp(1,.65f,p),Mathf.Lerp(1,.65f,p),1);
     if(p>=1){arrived[i]=true;r.gameObject.SetActive(false);credited++;coinText.text=(coins-pendingReward+Mathf.RoundToInt((float)pendingReward*credited/count)).ToString("N0",System.Globalization.CultureInfo.InvariantCulture);Sfx("seat",.3f,1+credited*.025f);HapticFeedback.Play(HapticCue.MarbleSeat);}
    }
    coinText.transform.parent.localScale=Vector3.one*(1+.07f*Mathf.Sin(Mathf.Clamp01((elapsed-.7f)/.65f)*Mathf.PI));yield return null;
   }
   coinText.text=FormatCoins();coinText.transform.parent.localScale=Vector3.one;Destroy(layer.gameObject);
   level++;if(launchedFromJourney){journeyCompleted=true;journeyRewardPending=false;Journey();}else StartCoroutine(EnterNextLevel());
  }
  IEnumerator EnterNextLevel(){
   // Go directly to the next board after the coin flight. The large
   // "completed level → next level" interstitial interrupted the flow.
   yield return new WaitForSecondsRealtime(.12f);BuildLevel();
  }
 }
}
