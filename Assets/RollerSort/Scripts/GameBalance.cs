namespace RollerSort {
 public static class GameBalance {
  public const int DailyQuestReward=250;
  public static int LevelReward(int level){
   int reward=50+((level-1)/5)*10;
   if(level>=15&&level%10==5)reward+=25;
   if(level>=18&&level%10==8)reward+=50;
   return reward>175?175:reward;
  }
 }
}
