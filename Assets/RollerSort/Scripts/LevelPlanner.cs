using System;
using System.Collections.Generic;
namespace RollerSort {
 public static class LevelPlanner {
  public static List<int> BalancedColors(int count,int colorCount,int seed){
   var values=new List<int>(count);int offset=Math.Abs(seed)%colorCount;
   for(int i=0;i<count;i++)values.Add((i+offset)%colorCount);
   var random=new Random(seed);for(int i=values.Count-1;i>0;i--){int j=random.Next(i+1);int hold=values[i];values[i]=values[j];values[j]=hold;}
   for(int i=2;i<values.Count;i++)if(values[i]==values[i-1]&&values[i]==values[i-2]){int swap=i+1;while(swap<values.Count&&values[swap]==values[i])swap++;if(swap<values.Count){int hold=values[i];values[i]=values[swap];values[swap]=hold;}}
   return values;
  }
 }
}
