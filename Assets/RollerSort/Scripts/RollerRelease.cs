using UnityEngine;
namespace RollerSort {
 public sealed partial class RollerGame {
  // The first public release uses earned coins only. No real-money products.
  void Shop(){
   var p=Modal("Boosters",540);
   Label(p,"Earn coins by completing levels.",new Vector2(0,91),20,bounds:new Vector2(330,52));
   for(int i=0;i<3;i++){int index=i;float y=165+i*105;Picture(p,BoosterIcons[i],new Vector2(-119,y),new Vector2(64,67));Label(p,BoosterNames[i],new Vector2(20,y-20),23,bounds:new Vector2(210,34));Button(p,"Get ×3\n1.800 coins",new Vector2(27,y+23),new Vector2(220,55),colors[2],()=>{
    if(level<UnlockLevels[index]){Toast("Bölüm "+UnlockLevels[index]+" ile açılır");return;}
    if(coins<1800){Toast("Yeterli altın yok");return;}
    coins-=1800;inventory[index]+=3;SaveBoosters();PlayerPrefs.SetInt("coins",coins);PlayerPrefs.Save();if(coinText)coinText.text=FormatCoins();UpdateInventory();Close();
   },20);}
   Button(p,"Close",new Vector2(0,487),new Vector2(250,54),colors[3],Close,25);
  }
 }
}
