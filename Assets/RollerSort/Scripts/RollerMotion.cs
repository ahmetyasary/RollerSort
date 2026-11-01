using UnityEngine;
namespace RollerSort {
 public sealed partial class RollerGame {
  const float MarbleRadius=BeltTrack.PaintedDiameter*.5f;
  const float Gravity=1200f,GateY=738f,ThroatHalfWidth=14.5f;
  const float TransferDuration=.155f;
  float ConveyorTurnsPerSecond=>-166f/BeltTrack.Length;
  static readonly Vector2 LeftRampTop=new(-205,680),LeftRampEnd=new(-14.5f,737);
  static readonly Vector2 RightRampTop=new(205,680),RightRampEnd=new(14.5f,737);

  bool BeltCannotProgress(){
   int circulating=0;
   foreach(var tray in trays)if(tray.completing)return false;
   foreach(var ball in balls){
    if(ball.magnetic||ball.stage==2)return false;
    if(ball.stage!=1&&ball.stage!=3)continue;
    circulating++;
    foreach(var tray in trays)if(tray.color==ball.color&&tray.count<3)return false;
   }
   return circulating>=Mathf.FloorToInt(BeltTrack.PackingLength/BeltTrack.MinimumPitch);
  }
  void CheckBlockedBelt(float dt){
   if(won||paused){blockedBeltTime=0;return;}
   blockedBeltTime=BeltCannotProgress()?blockedBeltTime+dt:0;
   if(blockedBeltTime>=2.5f){blockedBeltTime=0;OutOfSpace();}
  }

  void StepMarbles(float dt){
   MagneticStep();
   for(int i=balls.Count-1;i>=0;i--){
    var ball=balls[i];ball.previous=ball.position;ball.age+=dt;
    if(ball.age<0)continue;
    switch(ball.stage){
     case 0:
      // Small spheres emerge from the source; they reach full size before the funnel.
      float emergence=Mathf.SmoothStep(0,1,Mathf.Clamp01(ball.age/.13f));
      ball.radius=Mathf.Lerp(6.4f,MarbleRadius,emergence);ball.bounce=Vector2.zero;
      // Marbles pop out of the opened source for a moment before gravity takes them.
      if(ball.age>.08f)ball.velocity.y+=Gravity*dt;ball.velocity*=Mathf.Exp(-.35f*dt);
      ball.position+=ball.velocity*dt;
      break;
     case 1:
      ball.phase=Mathf.Repeat(ball.phase+ConveyorTurnsPerSecond*dt,1);
      ball.position=BeltPoint(ball.phase);
      foreach(var tray in trays){
       if(tray.color!=ball.color||tray.count>=3||tray.completing)continue;
       if(ball.position.y>BeltY+10&&Mathf.Abs(ball.position.x-tray.view.anchoredPosition.x)<12){
        ball.stage=2;ball.seat=tray.count++;ball.tray=tray;ball.age=0;ball.seatStart=ball.position;break;
       }
      }
      break;
     case 3:
      float entry=Mathf.Clamp01(ball.age/TransferDuration),remaining=1-entry;
      ball.phase=Mathf.Repeat(ball.transferPhaseStart+ConveyorTurnsPerSecond*Mathf.Min(ball.age,TransferDuration),1);
      ball.position=remaining*remaining*remaining*ball.seatStart+3*remaining*remaining*entry*ball.controlA+3*remaining*entry*entry*ball.controlB+entry*entry*entry*ball.transferEnd;
      if(entry>=1)ball.stage=1;
      break;
     case 2:
      float seat=1-Mathf.Pow(1-Mathf.Clamp01(ball.age/.22f),2);
      Vector2 target=new(ball.tray.view.anchoredPosition.x+(ball.seat-1)*23,838);
      Vector2 control=new(ball.seatStart.x,target.y-8);
      ball.position=(1-seat)*(1-seat)*ball.seatStart+2*(1-seat)*seat*control+seat*seat*target;
      if(seat>=1){
       var tray=ball.tray;tray.landed++;AddSeatedMarble(tray,ball.seat);
       StartCoroutine(SeatPop(tray.dots[ball.seat]));StartCoroutine(Pop(tray.top,.20f,.12f));StartCoroutine(Burst(target,colors[tray.color],5));
       Sfx("seat",.40f,1+tray.landed*.06f);HapticFeedback.Play(HapticCue.MarbleSeat);lastMatchTime=clock;
       ReturnMarble(ball.view);balls.RemoveAt(i);activeBalls--;
       if(tray.landed==3&&!tray.completing){tray.completing=true;StartCoroutine(CompleteTray(tray));}
      }
      break;
    }
   }
   SolveFunnelContacts(dt);
   SolveBeltContacts();
   ProjectBeltGaps();
   TryFeedBelt();
  }

  void SolveFunnelContacts(float dt){
   // Position-based circle contacts: no row targets, queue positions or colour ordering.
   // Iterating contacts and walls together prevents sinking through a crowded outlet.
   for(int iteration=0;iteration<10;iteration++){
    for(int i=0;i<balls.Count;i++){
     var a=balls[i];if(a.stage!=0||a.age<0)continue;
     for(int j=i+1;j<balls.Count;j++){
      var b=balls[j];if(b.stage!=0||b.age<0)continue;
      Vector2 delta=b.position-a.position;float diameter=a.radius+b.radius;
      float distanceSquared=delta.sqrMagnitude;if(distanceSquared>=diameter*diameter)continue;
      float distance=Mathf.Sqrt(distanceSquared);
      if(iteration==0){float impact=Vector2.Dot(a.velocity-b.velocity,delta.normalized);if(impact>85){ContactSound(impact);Vector2 impulse=delta.normalized*(impact*.10f);a.bounce-=impulse;b.bounce+=impulse;}} 
      Vector2 normal=distance>.0001f?delta/distance:new Vector2(.6f,.8f);
      Vector2 correction=normal*((diameter-distance)*.5f);
      a.position-=correction;b.position+=correction;
     }
    }
    foreach(var ball in balls)if(ball.stage==0&&ball.age>=0)ProjectFunnel(ball);
   }
   foreach(var ball in balls){
    if(ball.stage!=0||ball.age<0)continue;
    ball.velocity=(ball.position-ball.previous)/dt+ball.bounce;
    ball.velocity=Vector2.ClampMagnitude(ball.velocity,650);
   }
  }

  void ProjectFunnel(Ball ball){
   float side=205-ball.radius;
   ball.position.x=Mathf.Clamp(ball.position.x,-side,side);
   if(ball.position.y<660)return;
   ProjectRamp(ball,LeftRampTop,LeftRampEnd);
   ProjectRamp(ball,RightRampTop,RightRampEnd);
   if(ball.position.y>737){
    float throat=ThroatHalfWidth-ball.radius;
    ball.position.x=Mathf.Clamp(ball.position.x,-throat,throat);
   }
   // The physical outlet stays closed until there is one diameter of free belt.
   if(ball.position.y>GateY)ball.position.y=GateY;
  }

  void ProjectRamp(Ball ball,Vector2 top,Vector2 end){
   Vector2 segment=end-top;
   float along=Mathf.Clamp01(Vector2.Dot(ball.position-top,segment)/segment.sqrMagnitude);
   Vector2 nearest=top+segment*along,delta=ball.position-nearest;
   float squared=delta.sqrMagnitude;
   if(squared>=ball.radius*ball.radius)return;
   float distance=Mathf.Sqrt(squared);
   Vector2 normal=distance>.0001f?delta/distance:Vector2.down;
   ball.position=nearest+normal*ball.radius;
   float impact=Vector2.Dot(ball.velocity,normal);
   if(impact< -60&&ball.bounce==Vector2.zero){ball.bounce=normal*(-impact*.28f);ContactSound(-impact);}
  }

  void SolveBeltContacts(){
   // Spheres can roll along the conveyor when an incoming sphere presses between
   // them. This redistributes small gaps instead of leaving a half-empty jammed belt.
   for(int iteration=0;iteration<64;iteration++){
    for(int i=0;i<balls.Count;i++){
     var a=balls[i];if(a.stage!=1)continue;
     for(int j=i+1;j<balls.Count;j++){
      var b=balls[j];if(b.stage!=1)continue;
      float forward=Mathf.Repeat(b.phase-a.phase,1)*BeltTrack.Length;
      float correction=0;
      if(forward<BeltTrack.MinimumPitch)correction=(BeltTrack.MinimumPitch-forward)*.5f/BeltTrack.Length;
      else if(BeltTrack.Length-forward<BeltTrack.MinimumPitch)correction=-(BeltTrack.MinimumPitch-(BeltTrack.Length-forward))*.5f/BeltTrack.Length;
      if(correction==0)continue;
      a.phase=Mathf.Repeat(a.phase-correction,1);b.phase=Mathf.Repeat(b.phase+correction,1);
      a.position=BeltPoint(a.phase);b.position=BeltPoint(b.phase);
     }
    }
    foreach(var incoming in balls){
     if(incoming.stage!=3)continue;
     foreach(var other in balls){
      if(other.stage!=1)continue;
      float vertical=other.position.y-incoming.position.y;
      if(Mathf.Abs(vertical)>=BeltTrack.MinimumPitch)continue;
      float horizontal=other.position.x-incoming.position.x;
      float required=Mathf.Sqrt(BeltTrack.MinimumPitch*BeltTrack.MinimumPitch-vertical*vertical);
      if(Mathf.Abs(horizontal)>=required)continue;
      float direction=horizontal>=0?1:-1;
      other.phase=Mathf.Repeat(other.phase+direction*(required-Mathf.Abs(horizontal))/BeltTrack.Length,1);
      other.position=BeltPoint(other.phase);
     }
    }
   }
  }

  readonly Ball[] orderedBelt=new Ball[32];
  readonly float[] orderedDistance=new float[32];
  void ProjectBeltGaps(){
   // Finish the iterative pressure solve with an exact one-dimensional constraint
   // projection. This removes residual penetration even at maximum capacity.
   Ball entry=null,anchor=null;foreach(var b in balls){if(b.stage==3)entry=b;else if(b.stage==1&&anchor==null)anchor=b;}if(anchor==null)return;
   float gap=BeltTrack.MinimumPitch,phase=BeltTrack.PackingPhase(anchor.phase);
   bool fixedEntry=entry!=null&&Mathf.Abs(entry.position.y-(BeltY-BeltTrack.Radius))<gap;
   if(fixedEntry){float dy=entry.position.y-(BeltY-BeltTrack.Radius);gap=Mathf.Sqrt(gap*gap-dy*dy);phase=entry.position.x/BeltTrack.PackingLength;}
   int count=0;foreach(var b in balls){if(b.stage!=1||(!fixedEntry&&b==anchor))continue;float d=Mathf.Repeat(BeltTrack.PackingPhase(b.phase)-phase,1)*BeltTrack.PackingLength;int i=count;while(i>0&&orderedDistance[i-1]>d){orderedDistance[i]=orderedDistance[i-1];orderedBelt[i]=orderedBelt[i-1];i--;}orderedDistance[i]=d;orderedBelt[i]=b;count++;}
   for(int i=0;i<count;i++)orderedDistance[i]=Mathf.Max(orderedDistance[i],i==0?gap:orderedDistance[i-1]+BeltTrack.MinimumPitch);
   for(int i=count-1;i>=0;i--)orderedDistance[i]=Mathf.Min(orderedDistance[i],i==count-1?BeltTrack.PackingLength-gap:orderedDistance[i+1]-BeltTrack.MinimumPitch);
   for(int i=0;i<count;i++){var b=orderedBelt[i];b.phase=BeltTrack.FromPackingPhase(phase+orderedDistance[i]/BeltTrack.PackingLength);b.position=BeltPoint(b.phase);orderedBelt[i]=null;}
  }

  // The throat admits one sphere at a time. Two spheres resting on opposite noses
  // can lock into an arch, so the one nearest the outlet is eased toward the centre;
  // the contact solve then lifts its neighbour aside.
  void CentreThroatBall(){
   Ball nearest=null;
   foreach(var ball in balls){
    if(ball.stage!=0||ball.age<0||ball.position.y<GateY-18||Mathf.Abs(ball.position.x)>ThroatHalfWidth+ball.radius)continue;
    if(nearest==null||ball.position.y-Mathf.Abs(ball.position.x)>nearest.position.y-Mathf.Abs(nearest.position.x))nearest=ball;
   }
   if(nearest!=null)nearest.position.x=Mathf.MoveTowards(nearest.position.x,0,1.2f);
  }
  void TryFeedBelt(){
   int transported=0;foreach(var ball in balls){if(ball.stage==3)return;if(ball.stage==1)transported++;}
   if(transported>=BeltTrack.SlotCount)return;
   Ball candidate=null;
   foreach(var ball in balls){
    if(ball.stage!=0||ball.age<0||ball.position.y<GateY-2.5f||Mathf.Abs(ball.position.x)>ThroatHalfWidth-ball.radius+1)continue;
    if(candidate==null||ball.position.y>candidate.position.y)candidate=ball;
   }
   if(candidate==null){CentreThroatBall();return;}
   float entryPhase=candidate.position.x/BeltTrack.Length;
   // One continuous cubic arc, with its exit tangent matching conveyor velocity.
   candidate.stage=3;candidate.age=0;candidate.phase=Mathf.Repeat(entryPhase,1);candidate.transferPhaseStart=entryPhase;candidate.seatStart=candidate.position;
   candidate.transferEnd=BeltPoint(entryPhase+ConveyorTurnsPerSecond*TransferDuration);
   Vector2 incoming=Vector2.ClampMagnitude(candidate.velocity,220);incoming.y=Mathf.Max(45,incoming.y);
   candidate.controlA=candidate.position+incoming*(TransferDuration/3);
   candidate.controlB=candidate.transferEnd-Vector2.right*(BeltTrack.Length*ConveyorTurnsPerSecond*TransferDuration/3);
  }
 }
}
