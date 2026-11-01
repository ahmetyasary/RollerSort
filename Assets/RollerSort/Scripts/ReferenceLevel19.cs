namespace RollerSort {
 // The board and four initial receiver colours match the supplied recording.
 // Queue suffixes are our own colour-conserving level design: future receiver
 // dependencies follow an accessible source route and cannot require a colour
 // trapped behind a later source. The reference game does not reveal all queues.
 public static class ReferenceLevel19 {
  public static int[][] ReceiverQueues()=>new[]{
   new[]{5,5,4,4,8,4,9,3,3,7,4,6,7,8,7,3,5,6,9},
   new[]{5,6,4,4,8,4,9,3,5,7,4,8,7,8,9,3,5,3,9},
   new[]{6,6,4,4,8,4,9,3,5,7,6,8,7,7,9,3,6,3,9},
   new[]{7,7,7,7,7,7,3,3,5,4,6,8,8,7,9,5,6,3}
  };
 }
}
