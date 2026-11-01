namespace RollerSort {
 // Connectivity is evaluated only after board changes, never per rendered frame.
 public static class BoardAccess {
  public static bool[,] Reachable(bool[,] floor,bool[,] blocked){
   int rows=floor.GetLength(0),cols=floor.GetLength(1);var seen=new bool[rows,cols];var queue=new int[rows*cols];int head=0,tail=0;
   for(int c=0;c<cols;c++)if(floor[0,c]&&!blocked[0,c]){seen[0,c]=true;queue[tail++]=c;}
   while(head<tail){int cell=queue[head++],r=cell/cols,c=cell%cols;
    Visit(r-1,c);Visit(r+1,c);Visit(r,c-1);Visit(r,c+1);
   }
   return seen;
   void Visit(int r,int c){if(r<0||c<0||r>=rows||c>=cols||seen[r,c]||!floor[r,c]||blocked[r,c])return;seen[r,c]=true;queue[tail++]=r*cols+c;}
  }
  public static bool TouchesExit(bool[,] reachable,int row,int col){if(row==0)return true;int rows=reachable.GetLength(0),cols=reachable.GetLength(1);return reachable[row-1,col]||(row+1<rows&&reachable[row+1,col])||(col>0&&reachable[row,col-1])||(col+1<cols&&reachable[row,col+1]);}
 }
}
