using UnityEngine;
using UnityEngine.EventSystems;
namespace RollerSort {
 // Keep the event receiver enabled while resting; skip all transform work.
 public sealed class TouchSpring : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler {
  Vector3 velocity; bool pressed,resting=true; Vector3 initial;
  void Awake(){initial=transform.localScale;}
  public void OnPointerDown(PointerEventData data){resting=false;pressed=true;HapticFeedback.Play(HapticCue.Selection);}
  public void OnPointerUp(PointerEventData data){pressed=false;}
  public void OnPointerExit(PointerEventData data){pressed=false;}
  void Update(){if(resting)return;var target=initial*(pressed?.93f:1f);transform.localScale=Vector3.SmoothDamp(transform.localScale,target,ref velocity,pressed?.035f:.075f,Mathf.Infinity,Time.unscaledDeltaTime);if(!pressed&&(transform.localScale-initial).sqrMagnitude<.000001f&&velocity.sqrMagnitude<.0001f){transform.localScale=initial;velocity=Vector3.zero;resting=true;}}
  void OnDisable(){pressed=false;resting=true;velocity=Vector3.zero;transform.localScale=initial;}
 }
}
