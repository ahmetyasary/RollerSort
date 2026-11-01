namespace RollerSort {
 public static class LevelLayouts {
  public static string[] Rows(int level){
   // Hard stages use the dense six-column chamber from the reference: the two
   // tunnels sit together in the middle and the sources continue below them.
   if(level>=15&&level%10==5)return new[]{"######","######","######","##TT##","######","######"};
   if(level<3)return new[]{"##","##","##"};if(level==3)return new[]{"###","###","###"};if(level==4)return new[]{"##..","##..","####","####"};if(level<8)return new[]{"####","####","####"};if(level==8)return new[]{".###.","#####","#####",".###."};if(level==9)return new[]{"..#..",".###.","#####","#####",".###."};if(level<14)return new[]{"T##T","####","####","####"};if(level<18)return new[]{".#.#.","#####","##T##","#####","#####"};if(level==18)return new[]{"..###..",".#####.","##T#T##","#######",".#####.","..###.."};return new[]{".###.",".###T","T####","#####","###X#"};
  }
  public static char Cell(int level,int row,int col){var rows=Rows(level);return rows[rows.Length-1-row][col];}
  public static bool Occupied(int level,int row,int col)=>Cell(level,row,col)!='.';
  public static float Step(int level)=>level>=15&&level%10==5?53:Rows(level)[0].Length>5?44:53;
  public static float RowStep(int level)=>level>=15&&level%10==5?53:Rows(level)[0].Length>5?44:50;
  public static int SourceColor(int level,int row,int col,int fallback){if(level!=19)return fallback;int[][] values={new[]{6,7,9,-1,7},new[]{7,4,4,4,7},new[]{-1,7,8,4,5},new[]{-1,5,3,9,-1},new[]{-1,3,8,6,-1}};return values[row][col];}
  public static int[] TunnelColors(int level,int col,int[] fallback){if(level!=19)return fallback;return col==0?new[]{5,3,9}:new[]{6,3,8};}
 }
}
