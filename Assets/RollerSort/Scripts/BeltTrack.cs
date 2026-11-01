using UnityEngine;
namespace RollerSort {
 public static class BeltTrack {
  public const float Radius=18.5f,CenterY=790,PaintedDiameter=25.4f;
  public const float MinimumPitch=25.8f,CurvePackingScale=.9f;
  public static bool Expanded{get;private set;}
  public static int SlotCount=>Expanded?26:22;
  public static float HalfStraight=>Expanded?145:129;
  public static float Width=>Expanded?360:326;
  public static float Length=>4*HalfStraight+2*Mathf.PI*Radius;
  public static float PackingLength=>4*HalfStraight+2*Mathf.PI*Radius*CurvePackingScale;
  public static void Configure(bool expanded){Expanded=expanded;}
  public static Vector2 Point(float phase){
   float straight=2*HalfStraight,curve=Mathf.PI*Radius;
   float d=Mathf.Repeat(phase*Length+HalfStraight,Length);
   if(d<straight)return new Vector2(-HalfStraight+d,CenterY-Radius);d-=straight;
   if(d<curve){float a=d/Radius;return new Vector2(HalfStraight+Radius*Mathf.Sin(a),CenterY-Radius*Mathf.Cos(a));}d-=curve;
   if(d<straight)return new Vector2(HalfStraight-d,CenterY+Radius);
   float angle=(d-straight)/Radius;return new Vector2(-HalfStraight-Radius*Mathf.Sin(angle),CenterY+Radius*Mathf.Cos(angle));
  }
  // Packing uses tighter straight-line contacts and a larger arc distance in turns.
  // Physical phase remains arc length, so unimpeded spheres keep constant speed.
  public static float PackingPhase(float physical){float s=2*HalfStraight,c=Mathf.PI*Radius,d=Mathf.Repeat(physical*Length+HalfStraight,Length),p;
   if(d<s)p=d;else if(d<s+c)p=s+(d-s)*CurvePackingScale;else if(d<2*s+c)p=s+c*CurvePackingScale+d-s-c;else p=2*s+c*CurvePackingScale+(d-2*s-c)*CurvePackingScale;
   return Mathf.Repeat((p-HalfStraight)/PackingLength,1);
  }
  public static float FromPackingPhase(float packed){float s=2*HalfStraight,c=Mathf.PI*Radius*CurvePackingScale,p=Mathf.Repeat(packed*PackingLength+HalfStraight,PackingLength),d;
   if(p<s)d=p;else if(p<s+c)d=s+(p-s)/CurvePackingScale;else if(p<2*s+c)d=s+c/CurvePackingScale+p-s-c;else d=2*s+c/CurvePackingScale+(p-2*s-c)/CurvePackingScale;
   return Mathf.Repeat((d-HalfStraight)/Length,1);
  }
 }
}
