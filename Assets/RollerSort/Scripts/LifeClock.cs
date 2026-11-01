using System;
namespace RollerSort {
 public sealed class LifeClock {
  public const int Maximum=5;public const long RefillSeconds=1800;
  public int Count;public long Next;
  public LifeClock(int count,long next){Count=Math.Max(0,Math.Min(Maximum,count));Next=next;}
  public void Refresh(long now){if(Count>=Maximum){Next=0;return;}if(Next<=0)Next=now+RefillSeconds;while(Count<Maximum&&now>=Next){Count++;Next+=RefillSeconds;}if(Count==Maximum)Next=0;}
  public void Lose(long now){Refresh(now);if(Count==Maximum)Next=now+RefillSeconds;if(Count>0)Count--;}
  public long Remaining(long now){Refresh(now);return Count==Maximum?0:Math.Max(0,Next-now);}
 }
}
