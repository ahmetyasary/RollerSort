using System;
using UnityEngine;
namespace RollerSort.Editor {
 public static class BeltSpacingCheck {
  public static void Run(){bool before=BeltTrack.Expanded;try{BeltTrack.Configure(false);Check();BeltTrack.Configure(true);Check();}finally{BeltTrack.Configure(before);}}
  static void Check(){
   float closest=float.PositiveInfinity;
   // Sweep a fully occupied belt through every turn, not just its initial pose.
   for(int frame=0;frame<4096;frame++)for(int slot=0;slot<BeltTrack.SlotCount;slot++){
    float phase=frame/4096f+(float)slot/BeltTrack.SlotCount;
    float distance=Vector2.Distance(BeltTrack.Point(BeltTrack.FromPackingPhase(phase)),BeltTrack.Point(BeltTrack.FromPackingPhase(phase+1f/BeltTrack.SlotCount)));
    closest=Mathf.Min(closest,distance);
   }
   if(closest<BeltTrack.PaintedDiameter+.5f)throw new Exception("Belt marbles overlap at turns: "+closest);
   float continuous=float.PositiveInfinity;
   for(int frame=0;frame<4096;frame++){float phase=frame/4096f;continuous=Mathf.Min(continuous,Vector2.Distance(BeltTrack.Point(BeltTrack.FromPackingPhase(phase)),BeltTrack.Point(BeltTrack.FromPackingPhase(phase+BeltTrack.MinimumPitch/BeltTrack.PackingLength))));}
   if(continuous<BeltTrack.PaintedDiameter+.1f)throw new Exception("Continuous belt pitch overlaps at turns: "+continuous);
   Debug.Log("CONTINUOUS_BELT_SPACING_OK: "+continuous);
   Debug.Log("BELT_SPACING_OK: full belt, minimum centre distance "+closest+", painted diameter "+BeltTrack.PaintedDiameter);
  }
 }
}
