using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace RollerSort {
public sealed partial class RollerGame : MonoBehaviour {
 const float W=450,H=975;
 readonly Color navy=new Color32(47,37,86,255), purple=new Color32(169,132,233,255);
 const float BeltY=790;
 readonly Dictionary<string,Sprite> art=new();
 readonly Color[] colors={new Color32(255,242,58,255),new Color32(196,58,77,255),new Color32(62,138,67,255),new Color32(220,247,255,255),new Color32(166,79,249,255),new Color32(59,99,249,255),new Color32(255,169,0,255),new Color32(252,185,255,255),new Color32(201,105,65,255),new Color32(86,84,96,255)};
 Canvas canvas; RectTransform root,game,overlay,suspendedShop; Font font; Sprite rounded,orb; Text levelText,coinText,hint; AudioSource audioSource;
 int level,coins,completed,activeBalls,capacity=36,pendingReward; float physicsAccumulator; int praiseVersion; bool paused,won,sound,haptics; float clock; float lastMatchTime,blockedBeltTime;
 readonly List<Box> boxes=new(); readonly List<Ball> balls=new(); readonly List<Tray> trays=new();
 readonly List<RectTransform> beltSockets=new(); readonly Stack<RectTransform> marblePool=new();
 class MarbleVisual { public RectTransform body,shadow; public Image image,shadowImage; }
 readonly Dictionary<RectTransform,MarbleVisual> marbleVisuals=new();
 readonly Stack<RectTransform> sparkPool=new();
 RectTransform actors;
 void IsolateCanvas(RectTransform layer,bool interactive=false){layer.gameObject.AddComponent<Canvas>();if(interactive)layer.gameObject.AddComponent<GraphicRaycaster>();} 
 class Box { public int color,row,col; public bool removed,emerging; public RectTransform view; }
 class Ball { public bool magnetic; public int color,stage,seat; public float age,phase,transferPhaseStart,radius=6.4f; public Vector2 start,position,previous,velocity,seatStart,controlA,controlB,transferEnd,bounce; public RectTransform view; public MarbleVisual visual; public Tray tray; }
 class Tray { public int color,count,landed; public bool completing; public RectTransform top; public Queue<int> next=new(); public RectTransform view; public readonly List<RectTransform> dots=new(); public readonly List<Image> covers=new(); public Image receiver; public CanvasGroup group; }
 void Start(){
  var cameraObject=new GameObject("Game Camera",typeof(Camera),typeof(AudioListener));var camera=cameraObject.GetComponent<Camera>();camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color32(99,130,184,255);camera.orthographic=true;
  Application.targetFrameRate=60; Screen.sleepTimeout=SleepTimeout.SystemSetting;
  level=PlayerPrefs.GetInt("level",1); coins=PlayerPrefs.GetInt("coins",500);
  sound=PlayerPrefs.GetInt("sound",1)==1; haptics=PlayerPrefs.GetInt("haptics",1)==1;
  GameLanguage.Load();ConfigureDiagnostics();LoadLives();LoadBoosters();
  HapticFeedback.SetEnabled(haptics);
  font=Resources.Load<Font>("Fonts/RollerRoundedFull");
  rounded=MakeSprite(false);orb=MakeSprite(true);
  audioSource=gameObject.AddComponent<AudioSource>(); audioSource.playOnAwake=false;PrepareSound();
  var c=new GameObject("Mobile Canvas",typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));
  canvas=c.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;
  var scaler=c.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(W,H);scaler.screenMatchMode=CanvasScaler.ScreenMatchMode.Expand;scaler.matchWidthOrHeight=0;canvas.scaleFactor=Mathf.Min((float)Screen.width/W,(float)Screen.height/H);
  root=Panel(c.transform,"Safe area",Vector2.zero,new Vector2(W,H),Color.clear);root.anchorMin=root.anchorMax=new Vector2(.5f,.5f);
  if(!FindFirstObjectByType<EventSystem>())new GameObject("Touch Input",typeof(EventSystem),typeof(StandaloneInputModule));
  StartCoroutine(Splash());
 }
 Sprite MakeSprite(bool circle){
  int n=128;var t=new Texture2D(n,n,TextureFormat.RGBA32,false);t.filterMode=FilterMode.Bilinear;
  for(int y=0;y<n;y++)for(int x=0;x<n;x++){
   float dx=Mathf.Max(Mathf.Abs(x-63.5f)-43,0),dy=Mathf.Max(Mathf.Abs(y-63.5f)-43,0);
   float d=circle?Vector2.Distance(new Vector2(x,y),new Vector2(63.5f,63.5f))-61:Mathf.Sqrt(dx*dx+dy*dy)-20;
   float a=Mathf.Clamp01(.7f-d);float light=circle?Mathf.Lerp(.62f,1f,Mathf.Clamp01(1-Vector2.Distance(new Vector2(x,y),new Vector2(43,87))/115)):1f;
   t.SetPixel(x,y,new Color(light,light,light,a));
  }t.Apply();return Sprite.Create(t,new Rect(0,0,n,n),new Vector2(.5f,.5f),100,0,SpriteMeshType.FullRect,new Vector4(26,26,26,26));
 }
 Sprite Art(string name){if(art.TryGetValue(name,out var sprite))return sprite;var texture=Resources.Load<Texture2D>("Art/"+name);if(!texture)return null;sprite=Sprite.Create(texture,new Rect(0,0,texture.width,texture.height),new Vector2(.5f,.5f),300,0,SpriteMeshType.FullRect,new Vector4(30,30,30,30));art[name]=sprite;return sprite;}
 int ColorIndex(Color color){int best=0;float distance=float.MaxValue;for(int i=0;i<colors.Length;i++){float d=Vector4.Distance(color,colors[i]);if(d<distance){distance=d;best=i;}}return best;}
 static readonly string[] colorNames={"yellow","pink","green","cyan","violet","blue","orange","blush","brown","charcoal"};
 string ColorName(int color)=>colorNames[color];
 RectTransform Picture(Transform parent,string asset,Vector2 pos,Vector2 size){var r=Panel(parent,asset,pos,size,Color.white);var im=r.GetComponent<Image>();im.sprite=Art(asset);im.type=Image.Type.Simple;return r;}
 RectTransform Panel(Transform parent,string name,Vector2 position,Vector2 size,Color color,bool ball=false){
  var o=new GameObject(name,typeof(RectTransform),typeof(CanvasRenderer),typeof(Image));o.transform.SetParent(parent,false);
  var r=o.GetComponent<RectTransform>();r.anchorMin=r.anchorMax=new Vector2(.5f,1);r.pivot=new Vector2(.5f,.5f);r.anchoredPosition=new Vector2(position.x,-position.y);r.sizeDelta=size;
  var im=o.GetComponent<Image>();im.sprite=ball?Art("marble-"+ColorName(ColorIndex(color))+"-v2"):rounded;im.type=ball?Image.Type.Simple:Image.Type.Sliced;im.color=ball?Color.white:color;im.raycastTarget=false;return r;
 }
 Text Label(Transform parent,string value,Vector2 pos,int size,Color? color=null,Vector2? bounds=null,bool localize=true){
  var o=new GameObject(value,typeof(RectTransform),typeof(CanvasRenderer),typeof(Text));o.transform.SetParent(parent,false);
  var r=o.GetComponent<RectTransform>();r.anchorMin=r.anchorMax=new Vector2(.5f,1);r.anchoredPosition=new Vector2(pos.x,-pos.y);r.sizeDelta=bounds??new Vector2(390,50);
  var t=o.GetComponent<Text>();t.font=font;t.text=value;t.fontSize=size;t.fontStyle=FontStyle.Normal;t.alignment=TextAnchor.MiddleCenter;t.color=color??Color.white;t.raycastTarget=false;t.resizeTextForBestFit=true;t.resizeTextMinSize=Mathf.Max(10,Mathf.RoundToInt(size*.7f));t.resizeTextMaxSize=size;if(localize)o.AddComponent<LocalizedGameText>().Bind(t,value);
  var shadow=o.AddComponent<Outline>();shadow.effectColor=new Color32(15,13,22,255);shadow.effectDistance=new Vector2(size>=25?1.45f:1f,-(size>=25?1.45f:1f));shadow.useGraphicAlpha=true;return t;
 }
 RectTransform Button(Transform parent,string text,Vector2 pos,Vector2 size,Color color,Action action,int fontSize=24){
  string asset=color==purple?(size.x/size.y>=1.6f?"hud":"button-purple"):color==colors[0]?"button-yellow":color==colors[2]?"button-green":color==colors[1]?"button-red":"button-orange";
  if(size.x/size.y<1.6f&&asset!="button-purple")asset=asset.Replace("button-","square-");
  var r=Picture(parent,asset,pos,size);var im=r.GetComponent<Image>();im.type=Image.Type.Simple;im.raycastTarget=true;
  r.gameObject.AddComponent<TouchSpring>();
  var b=r.gameObject.AddComponent<Button>();b.targetGraphic=im;var cs=b.colors;cs.pressedColor=new Color(.85f,.85f,.85f);b.colors=cs;
  b.onClick.AddListener(()=>{Sfx("click",.5f);action();});if(text!=""){var label=Label(r,text,new Vector2(0,size.y/2),fontSize,bounds:new Vector2(Mathf.Max(20,size.x-18),Mathf.Max(20,size.y-8)));label.name="Button label";label.horizontalOverflow=HorizontalWrapMode.Wrap;label.verticalOverflow=VerticalWrapMode.Truncate;}return r;
 }
 IEnumerator Splash(){
  var splash=Picture(root,"launch-scene",new Vector2(0,H/2),new Vector2(W,H));
  yield return null;NativeLaunch.Hide();
  for(int i=0;i<colors.Length;i++){Art("marble-"+ColorName(i)+"-v2");Art("receiver-"+ColorName(i)+"-v2");Art("cover-"+ColorName(i)+"-v2");Art("source-"+ColorName(i)+"-v2");yield return null;}
  yield return new WaitForSecondsRealtime(.8f);
  Destroy(splash.gameObject);if(profileScreenPreview){diagnosticsActive=false;Journey();EditProfile();}else if(settingsScreenPreview){diagnosticsActive=false;BuildLevel();Settings();}else if(journeySettingsScreenPreview){diagnosticsActive=false;Journey();Settings();}else if(diagnosticsActive||life.Count>0&&PlayerPrefs.GetInt("visitedJourney",0)==0)BuildLevel();else Journey();
 }
 RectTransform ScreenPanel(string name,Color color){var r=Panel(root,name,new Vector2(0,H/2),new Vector2(W,H),color);var image=r.GetComponent<Image>();image.sprite=null;image.type=Image.Type.Simple;r.gameObject.AddComponent<RectMask2D>();return r;}
 // Scale content independently of full-bleed screen surfaces.
 const float ContentScale=1f;
 void ScaleScreenContents(RectTransform screen,bool keepShopScenery=false){
  var children=new List<Transform>();foreach(Transform child in screen){if(child.name=="journey-bg"||keepShopScenery&&(child.name=="shop-pattern"||child.name=="awning"))continue;children.Add(child);}
  var content=Panel(screen,"Screen content",new Vector2(0,H/2),new Vector2(W,H),Color.clear);
  foreach(var child in children)child.SetParent(content,false);
  content.localScale=Vector3.one*ContentScale;
 }
 void Clear(Transform t){foreach(Transform child in t){child.gameObject.SetActive(false);Destroy(child.gameObject);}}
 void BuildLevel(){
  BeltTrack.Configure(level>=15);StopAllCoroutines();sparkPool.Clear();marbleVisuals.Clear();marblePool.Clear();beltSockets.Clear();physicsAccumulator=0;praiseVersion++;
  if(game)Destroy(game.gameObject);if(overlay)Destroy(overlay.gameObject);if(suspendedShop)Destroy(suspendedShop.gameObject);
  journey=false;failed=false;spaceOffer=false;selectingBooster=-1;boxes.Clear();portals.Clear();stones.Clear();balls.Clear();trays.Clear();completed=0;activeBalls=0;paused=false;capacity=level>=15?72:54;blockedBeltTime=0;lastMatchTime=clock;won=false;
  bool hardStage=IsHardJourneyLevel(level)||IsVeryHardJourneyLevel(level);
  game=ScreenPanel("Game",hardStage?new Color32(68,78,81,255):level>=12?new Color32(166,103,183,255):new Color32(99,130,184,255));
  var gear=Button(game,"",new Vector2(-190,107),new Vector2(50,51),purple,Settings);Picture(gear,"gear",new Vector2(0,24),new Vector2(31,31));
  levelText=Label(Picture(game,"hud",new Vector2(-6,107),new Vector2(119,43)),"Level "+level,new Vector2(0,21),23);
  var currency=Picture(game,"hud",new Vector2(142,107),new Vector2(154,43));Picture(currency,"coin",new Vector2(-57,21),new Vector2(27,29));
  coinText=Label(currency,FormatCoins(),new Vector2(-13,21),29,bounds:new Vector2(83,43));
  var add=Button(game,"",new Vector2(194,107),new Vector2(45,47),colors[0],Shop);Picture(add,"plus",new Vector2(0,22),new Vector2(31,31));
  DrawBoosters();
  var layout=LevelLayouts.Rows(level);int cols=layout[0].Length,rows=layout.Length,colorCount=Mathf.Min(3+(level-1)/2,7);
  var board=Picture(game,IsHardJourneyLevel(level)?"board-hard-reference":level<=4?"board-"+cols:"level-board-"+Mathf.Min(level,19),new Vector2(0,395),new Vector2(450,790));
  if(hardStage)board.GetComponent<Image>().color=IsHardJourneyLevel(level)?Color.white:new Color(.43f,.45f,.47f,1);else if(level>=12)board.GetComponent<Image>().color=Color.white;
  if(hardStage){var badge=Panel(game,"Difficulty badge",new Vector2(0,148),new Vector2(128,32),new Color32(212,72,75,255));Picture(badge,"skull",new Vector2(-42,16),new Vector2(23,23));Label(badge,IsVeryHardJourneyLevel(level)?"Very Hard":"Hard",new Vector2(12,16),17,bounds:new Vector2(92,28));}
  int seed=level*7919;var rng=new System.Random(seed);var sequence=LevelPlanner.BalancedColors(rows*cols,colorCount,seed);
  if(level<=2)sequence=new List<int>{0,0,2,1,1,2};
  if(level==4)sequence=new List<int>{2,0,6,6,0,6,5,2,5,6,0,0,2,5,0,0};
  sourceLayer=Panel(game,"Sources",Vector2.zero,Vector2.zero,Color.clear);
  for(int row=rows-1;row>=0;row--)for(int col=0;col<cols;col++){
   char cell=LevelLayouts.Cell(level,row,col);if(cell=='.')continue;
   if(cell=='T'){CreatePortal(row,col,rng);continue;}
   if(cell=='X'){stones.Add(new Vector2Int(col,row));Picture(sourceLayer,"stone",CellPoint(row,col),new Vector2(50,50));continue;}
   CreateSource(LevelLayouts.SourceColor(level,row,col,sequence[row*cols+col]),row,col);
  }
  RefreshSourceAccess(false);
  var lowerBoard=Picture(game,"lower-board",new Vector2(0,903),new Vector2(450,320));if(hardStage)lowerBoard.GetComponent<Image>().color=new Color(.38f,.41f,.47f,1);
  var belt=Picture(game,BeltTrack.Expanded?"belt-expanded":"belt",new Vector2(0,BeltY),new Vector2(BeltTrack.Width,69));if(hardStage)belt.GetComponent<Image>().color=new Color(.56f,.59f,.65f,1);
  var socketLayer=Panel(game,"Moving sockets",Vector2.zero,Vector2.zero,Color.clear);IsolateCanvas(socketLayer);
  for(int i=0;i<BeltTrack.SlotCount;i++){Vector2 p=BeltPoint((float)i/BeltTrack.SlotCount);var socket=Picture(socketLayer,hardStage?"belt-socket-reference":"belt-socket",p,new Vector2(19,19));beltSockets.Add(socket);}
  sequence.Clear();for(int row=0;row<rows;row++)for(int col=0;col<cols;col++){var box=boxes.Find(b=>b.row==row&&b.col==col);if(box!=null)for(int k=0;k<3;k++)sequence.Add(box.color);}
  foreach(var portal in portals)foreach(int color in portal.next)for(int k=0;k<3;k++)sequence.Add(color);
  int[][] introQueues={new[]{2,1,1,0,0},new[]{1,1,0,0,0},new[]{2,2,1,0},new[]{2,2,2,1}};
  for(int i=0;i<4;i++){
   var tray=new Tray();if(level<=2){foreach(int color in introQueues[i])tray.next.Enqueue(color);}else if(level==19){foreach(int color in ReferenceLevel19.ReceiverQueues()[i])tray.next.Enqueue(color);}else for(int k=i;k<sequence.Count;k+=4)tray.next.Enqueue(sequence[k]);
   tray.color=tray.next.Count>0?tray.next.Dequeue():-1;
   tray.view=Panel(game,"Tray column",new Vector2(-126+i*84,828),new Vector2(80,0),Color.clear);
   IsolateCanvas(tray.view);trays.Add(tray);DrawTray(tray);
  }
  actors=Panel(game,"Moving marbles",Vector2.zero,Vector2.zero,Color.clear);IsolateCanvas(actors);
  for(int i=0;i<72;i++){var r=TakeMarble(0);r.gameObject.SetActive(false);}foreach(Transform child in actors)marblePool.Push((RectTransform)child);
  Debug.Log("[RollerSort] Gameplay ready: tactile feedback + 120 Hz marble simulation");
  for(int i=0;i<36;i++){var spark=Picture(actors,"sparkle-v2",Vector2.zero,new Vector2(9,9));spark.gameObject.SetActive(false);sparkPool.Push(spark);}
  hint=Label(game,"",new Vector2(0,738),35);IsolateCanvas(hint.rectTransform);hint.GetComponent<Outline>().effectColor=new Color32(0,174,255,255);hint.GetComponent<Outline>().effectDistance=new Vector2(3,-3);ScaleScreenContents(game);StartCoroutine(UnlockIntroduction());
 }
 void Release(Box b){
  if(paused||won||b.removed||b.emerging)return;
  if(selectingBooster==0){selectingBooster=-1;magnetRelease=true;}
  if(!magnetRelease&&!Accessible(b)){HapticFeedback.Play(HapticCue.Blocked);StartCoroutine(Nudge(b.view));Toast("Önce kutuya ulaşan boş bir yol aç.");return;}
  if(!magnetRelease&&activeBalls+9>capacity){HapticFeedback.Play(HapticCue.Blocked);Toast("Bant dolu! Renkleri eşleştir veya yeniden dene.");return;}
  bool magnetic=magnetRelease;magnetRelease=false;if(magnetic){inventory[0]--;SaveBoosters();UpdateInventory();Sfx("magnet",.7f);}
  b.removed=true;RefreshSourceAccess();TryEmitPortals();
  StartCoroutine(BurstOpen(b.view));Sfx("open",.65f,1+b.color*.025f);StartCoroutine(DropSoundCascade(b.color));HapticFeedback.Play(HapticCue.OpenBox);
  Vector2 pos=b.view.anchoredPosition;pos.y=-pos.y;
  for(int i=0;i<9;i++){
   Vector2 start=pos+new Vector2((i%3-1)*13,(1-i/3)*13);
   var ball=new Ball{magnetic=magnetic,color=b.color,age=-i*.012f,start=start,position=start,velocity=new Vector2((i%3-1)*14+Mathf.Sin(i*3.1f)*9,40+(2-i/3)*12)};
   ball.previous=start;ball.view=TakeMarble(b.color);ball.visual=marbleVisuals[ball.view];ball.view.anchoredPosition=new Vector2(start.x,-start.y);balls.Add(ball);activeBalls++;
  }
 }
 RectTransform TakeMarble(int color){
  RectTransform r;
  if(marblePool.Count>0){r=marblePool.Pop();r.gameObject.SetActive(true);}
  else {r=Panel(actors,"Marble actor",Vector2.zero,Vector2.one*27,Color.clear);Picture(r,"contact-shadow",new Vector2(2,23),new Vector2(25,10));Picture(r,"marble-yellow-v2",new Vector2(0,13.5f),Vector2.one*27);}
  if(!marbleVisuals.TryGetValue(r,out var visual)){visual=new MarbleVisual{shadow=(RectTransform)r.GetChild(0),body=(RectTransform)r.GetChild(1)};visual.image=visual.body.GetComponent<Image>();visual.shadowImage=visual.shadow.GetComponent<Image>();marbleVisuals[r]=visual;}visual.image.sprite=Art("marble-"+ColorName(color)+"-v2");r.localScale=Vector3.one;r.localRotation=Quaternion.identity;r.SetAsLastSibling();return r;
 }
 void ReturnMarble(RectTransform r){r.gameObject.SetActive(false);marblePool.Push(r);}
 IEnumerator LiftLid(RectTransform lid){
  Vector2 origin=lid.anchoredPosition;var image=lid.GetComponent<Image>();
  for(float t=0;t<.24f;){if(!lid)yield break;t+=paused?0:Time.deltaTime;float p=Mathf.Clamp01(t/.24f),ease=1-Mathf.Pow(1-p,3);lid.anchoredPosition=origin+new Vector2(8*ease,29*ease);lid.localRotation=Quaternion.Euler(0,0,-14*ease);image.color=new Color(1,1,1,1-Mathf.SmoothStep(0,1,p));yield return null;}
  if(lid)Destroy(lid.gameObject);
 }
 IEnumerator Nudge(RectTransform r){Vector2 origin=r.anchoredPosition;for(float t=0;t<.22f;t+=Time.deltaTime){if(!r)yield break;r.anchoredPosition=origin+Vector2.right*Mathf.Sin(t*95)*3*(1-t/.22f);yield return null;}if(r)r.anchoredPosition=origin;}
 Vector2 BeltPoint(float phase)=>BeltTrack.Point(phase);
 void DrawTray(Tray t){
  if(!t.receiver){
   Picture(t.view,"lane",new Vector2(0,132),new Vector2(79,264));
   for(int i=4;i>=0;i--)t.covers.Insert(0,Picture(t.view,"cover-yellow-v2",new Vector2(0,50+i*35),new Vector2(81,50)).GetComponent<Image>());
   t.top=Picture(t.view,"receiver-yellow-v2",new Vector2(0,15),new Vector2(80,39));t.receiver=t.top.GetComponent<Image>();t.group=t.top.gameObject.AddComponent<CanvasGroup>();
   for(int i=0;i<3;i++)t.dots.Add(Panel(t.top,"Marble",new Vector2((i-1)*23,14),Vector2.one*21,colors[0],true));
  }
  t.top.gameObject.SetActive(t.color>=0);t.top.anchoredPosition=new Vector2(0,-15);t.top.localScale=Vector3.one;t.group.alpha=1;
  if(t.color>=0)t.receiver.sprite=Art("receiver-"+ColorName(t.color)+"-v2");
  int n=0;foreach(int color in t.next){if(n>=5)break;var cover=t.covers[n];cover.sprite=Art((IsHardJourneyLevel(level)||IsVeryHardJourneyLevel(level))&&(n+trays.IndexOf(t)+completed)%3!=1?"queue-mystery-reference":"cover-"+ColorName(color)+"-v2");cover.rectTransform.anchoredPosition=new Vector2(0,-50-n*35);cover.gameObject.SetActive(true);n++;}
  for(int i=n;i<5;i++)t.covers[i].gameObject.SetActive(false);
  for(int i=0;i<3;i++){t.dots[i].gameObject.SetActive(i<t.landed);if(t.color>=0)t.dots[i].GetComponent<Image>().sprite=Art("marble-"+ColorName(t.color)+"-v2");}
 }
 void AddSeatedMarble(Tray t,int index){var dot=t.dots[index];dot.gameObject.SetActive(true);dot.localScale=Vector3.one;}
 IEnumerator SeatPop(RectTransform dot){
  for(float t=0;t<.22f;){if(!dot)yield break;t+=GameStep;float p=Mathf.Clamp01(t/.22f);float scale=p<.58f?Mathf.Lerp(.62f,1.18f,1-Mathf.Pow(1-p/.58f,3)):Mathf.Lerp(1.18f,1f,Mathf.SmoothStep(0,1,(p-.58f)/.42f));dot.localScale=Vector3.one*scale;yield return null;}if(dot)dot.localScale=Vector3.one;
 }
 IEnumerator DropSoundCascade(int color){
  float[] waits={.045f,.055f,.075f,.10f};for(int i=0;i<waits.Length;i++){yield return new WaitForSeconds(waits[i]);if(paused)yield break;Sfx("contact",.18f+i*.035f,.88f+color*.018f+i*.055f);}
 }
 IEnumerator TrayCompleteSound(int order){
  Sfx("complete",.58f,1+(order%4)*.035f);yield return new WaitForSeconds(.065f);Sfx("seat",.34f,1.24f);yield return new WaitForSeconds(.075f);Sfx("seat",.28f,1.42f);
 }
 float GameStep=>paused?0:Time.deltaTime;
 IEnumerator Pop(RectTransform r,float duration,float amount){for(float t=0;t<duration;){if(!r)yield break;t+=GameStep;float p=Mathf.Clamp01(t/duration);r.localScale=Vector3.one*(1+Mathf.Sin(p*Mathf.PI)*amount*(1-p));yield return null;}if(r)r.localScale=Vector3.one;}
 Vector2 cachedScreen;Rect cachedSafeArea;float cachedCanvasScale=-1;
 void Update(){
  if(!root)return;
  // Fit the complete reference board on both phone and tablet displays.
  if(cachedScreen!=new Vector2(Screen.width,Screen.height)||cachedSafeArea!=Screen.safeArea||!Mathf.Approximately(cachedCanvasScale,canvas.scaleFactor)){cachedCanvasScale=canvas.scaleFactor;cachedScreen=new Vector2(Screen.width,Screen.height);cachedSafeArea=Screen.safeArea;float scale=Mathf.Min((float)Screen.width/W,(float)Screen.height/H)/canvas.scaleFactor;root.localScale=Vector3.one*scale;root.anchoredPosition=Vector2.zero;}
  LifeTick();AudioTick();DiagnosticsTick();
  if(paused||!game)return;
  physicsAccumulator+=Mathf.Min(Time.deltaTime,.1f);while(physicsAccumulator>=1f/120f){clock+=1f/120f;StepMarbles(1f/120f);physicsAccumulator-=1f/120f;}
  for(int i=0;i<beltSockets.Count;i++){var r=beltSockets[i];Vector2 p=BeltPoint((clock-1f/120f+physicsAccumulator)*ConveyorTurnsPerSecond+(float)i/BeltTrack.SlotCount);r.anchoredPosition=new Vector2(p.x,-p.y);}
  float alpha=physicsAccumulator*120;
  foreach(var b in balls){if(b.age<0){if(b.view.gameObject.activeSelf)b.view.gameObject.SetActive(false);continue;}if(!b.view.gameObject.activeSelf)b.view.gameObject.SetActive(true);Vector2 render=Vector2.Lerp(b.previous,b.position,alpha);b.view.anchoredPosition=new Vector2(render.x,-render.y);b.view.localScale=Vector3.one*(b.stage==2?Mathf.Lerp(1,.78f,Mathf.Clamp01(b.age/.22f)):b.stage==0?b.radius/MarbleRadius:1);float altitude=b.stage==0?Mathf.Clamp01((737-b.position.y)/140):0;var visual=b.visual;visual.shadow.localScale=Vector3.one*Mathf.Lerp(1,.65f,altitude);visual.shadowImage.color=new Color(1,1,1,Mathf.Lerp(.65f,.22f,altitude));float stretch=b.stage==0?Mathf.Min(.08f,Mathf.Abs(b.velocity.y)/5000):0;visual.body.localScale=new Vector3(1-stretch,1+stretch,1);}

  CheckBlockedBelt(Time.deltaTime);
  if(!won&&boxes.Count>0&&boxes.TrueForAll(b=>b.removed)&&portals.TrueForAll(p=>p.next.Count==0&&!p.emitting)&&balls.Count==0&&trays.TrueForAll(t=>t.color<0)){won=true;StartCoroutine(Victory());}
 }
 // An opened source bulges briefly, then bursts as its marbles spill out.
 IEnumerator BurstOpen(RectTransform r){for(float t=0;t<.16f;){t+=GameStep;if(!r)yield break;float p=Mathf.Clamp01(t/.16f),fade=Mathf.InverseLerp(.35f,1,p);float scale=p<.35f?Mathf.Lerp(1,1.12f,Mathf.SmoothStep(0,1,p/.35f)):Mathf.Lerp(1.12f,.55f,fade);r.localScale=new Vector3(scale,scale,1);r.GetComponent<Image>().color=new Color(1,1,1,1-fade);yield return null;}if(r)Destroy(r.gameObject);}
 IEnumerator CompleteTray(Tray t){
  completed++;StartCoroutine(TrayCompleteSound(completed));HapticFeedback.Play(HapticCue.TrayComplete);StartCoroutine(Praise(completed%2==0?"Sweet":"Neat"));
  // A detached copy of the full receiver rises above the belt and pops,
  // while the queue advances underneath and the next receiver opens its lid.
  Vector2 seat=new(t.view.anchoredPosition.x,843),popAt=seat+Vector2.down*50;string name=ColorName(t.color);
  var ghost=Picture(actors,"receiver-"+name+"-v2",seat,new Vector2(80,39));for(int i=0;i<3;i++)Picture(ghost,"marble-"+name+"-v2",new Vector2((i-1)*23,14),Vector2.one*21);
  var lidSprite=t.next.Count>0?t.covers[0].sprite:null;
  t.count=0;t.landed=0;t.color=t.next.Count>0?t.next.Dequeue():-1;DrawTray(t);t.top.localRotation=Quaternion.identity;
  RectTransform lid=null;Image lidImage=null;if(t.color>=0&&lidSprite){lid=Panel(t.top,"Opening lid",new Vector2(0,19),new Vector2(81,46),Color.white);lidImage=lid.GetComponent<Image>();lidImage.sprite=lidSprite;lidImage.type=Image.Type.Simple;}
  bool popped=false;
  for(float time=0;time<.42f;){time+=GameStep;
   float slide=1-Mathf.Pow(1-Mathf.Clamp01(time/.16f),3);
   t.top.anchoredPosition=new Vector2(0,-15-35*(1-slide));for(int i=0;i<t.covers.Count;i++)t.covers[i].rectTransform.anchoredPosition=new Vector2(0,-50-i*35-35*(1-slide));
   if(lid){float p=Mathf.Clamp01((time-.16f)/.18f),ease=1-Mathf.Pow(1-p,2);lid.anchoredPosition=new Vector2(9*ease,-19+26*ease);lid.localRotation=Quaternion.Euler(0,0,-14*ease);lidImage.color=new Color(1,1,1,1-p);}
   if(ghost){float rise=1-Mathf.Pow(1-Mathf.Clamp01(time/.22f),3),shrink=Mathf.Clamp01((time-.22f)/.14f);Vector2 p=Vector2.Lerp(seat,popAt,rise);ghost.anchoredPosition=new Vector2(p.x,-p.y);ghost.localScale=Vector3.one*Mathf.Lerp(1,1.2f,rise)*(1-shrink*shrink);
    if(shrink>0&&!popped){popped=true;StartCoroutine(PopRing(popAt));}
    if(shrink>=1){Destroy(ghost.gameObject);ghost=null;StartCoroutine(StarBurst(popAt,7));}}
   yield return null;}
  if(ghost)Destroy(ghost.gameObject);if(lid)Destroy(lid.gameObject);
  t.top.anchoredPosition=new Vector2(0,-15);for(int i=0;i<t.covers.Count;i++)t.covers[i].rectTransform.anchoredPosition=new Vector2(0,-50-i*35);
  t.completing=false;
 }
 IEnumerator PopRing(Vector2 position){var ring=Picture(actors,"pop-ring",position,new Vector2(48,48));var image=ring.GetComponent<Image>();for(float t=0;t<.2f;){t+=GameStep;if(!ring)yield break;float p=Mathf.Clamp01(t/.2f);ring.localScale=Vector3.one*Mathf.Lerp(.45f,1.35f,1-Mathf.Pow(1-p,2));image.color=new Color(1,1,1,1-p);yield return null;}if(ring)Destroy(ring.gameObject);}
 IEnumerator StarBurst(Vector2 position,int count){
  var stars=new RectTransform[count];var directions=new Vector2[count];for(int i=0;i<count;i++){float a=Mathf.Lerp(200,340,(float)i/(count-1))*Mathf.Deg2Rad;directions[i]=new Vector2(Mathf.Cos(a),Mathf.Sin(a));stars[i]=Picture(actors,"star-white",position,Vector2.one*(i%2==0?13:9));}
  for(float t=0;t<.4f;){t+=GameStep;float p=Mathf.Clamp01(t/.4f),travel=1-Mathf.Pow(1-p,2);for(int i=0;i<count;i++){var r=stars[i];if(!r)continue;Vector2 pos=position+directions[i]*(8+34*travel);r.anchoredPosition=new Vector2(pos.x,-pos.y);r.localScale=Vector3.one*(p<.2f?p/.2f:1-Mathf.InverseLerp(.55f,1,p));r.localRotation=Quaternion.Euler(0,0,(i%2==0?1:-1)*120*p);}yield return null;}
  foreach(var r in stars)if(r)Destroy(r.gameObject);
 }
 IEnumerator Settle(RectTransform r){Vector2 end=r.anchoredPosition;for(float t=0;t<.23f;){if(!r)yield break;t+=GameStep;float p=Mathf.Clamp01(t/.23f);r.anchoredPosition=end+Vector2.down*35*Mathf.Pow(1-p,3);yield return null;}if(r)r.anchoredPosition=end;}
 IEnumerator Praise(string message){int version=++praiseVersion;hint.text=GameLanguage.Text(message);hint.color=Color.white;for(float t=0;t<.85f;){t+=GameStep;if(version!=praiseVersion)yield break;float p=t/.85f;hint.rectTransform.localScale=Vector3.one*(1+.16f*Mathf.Sin(Mathf.Min(1,p*3)*Mathf.PI));hint.rectTransform.anchoredPosition=new Vector2(0,-738+18*p);hint.color=new Color(1,1,1,1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(.65f,1,p)));yield return null;}if(version==praiseVersion)hint.text="";}
 IEnumerator Burst(Vector2 position,Color color,int count){
  var sprites=new RectTransform[count];for(int i=0;i<count;i++){float a=i*2.39996f;sprites[i]=sparkPool.Count>0?sparkPool.Pop():Picture(actors,"sparkle-v2",position,new Vector2(9,9));sprites[i].gameObject.SetActive(true);sprites[i].GetComponent<Image>().color=i%3==0?Color.white:color;}
  for(float t=0;t<.45f;){t+=GameStep;float p=t/.45f;for(int i=0;i<count;i++){float a=i*2.39996f;var r=sprites[i];Vector2 d=new Vector2(Mathf.Cos(a),Mathf.Sin(a));Vector2 pos=position+d*(15+45*p);r.anchoredPosition=new Vector2(pos.x,-pos.y);r.localScale=Vector3.one*(1-p);r.localRotation=Quaternion.Euler(0,0,p*90);}yield return null;}foreach(var r in sprites)if(r){r.gameObject.SetActive(false);sparkPool.Push(r);}
 }
 void Tone(float hz,float seconds){Sfx("click",.35f,hz/660f);}
 void OnApplicationPause(bool value){if(life!=null){life.Refresh(Now);SaveLives();}if(value)HapticFeedback.Suspend();else HapticFeedback.SetEnabled(haptics);}
 void OnDestroy(){HapticFeedback.Suspend();}
 RectTransform Modal(string title,float height=510,float offset=25){
  paused=true;if(overlay){if(overlay.name=="Shop"){suspendedShop=overlay;}else {overlay.gameObject.SetActive(false);Destroy(overlay.gameObject);}}
  overlay=ScreenPanel("Modal backdrop",new Color(0.05f,.04f,.16f,.82f));overlay.GetComponent<Image>().raycastTarget=true;
  bool booster=title.Contains("Unlocked")||title=="Magnet"||title=="Shuffle"||title=="Clean Up"||title=="No Ads";var body=Picture(overlay,booster?"dialog-blue":"dialog",new Vector2(0,H/2+offset),new Vector2(title=="Level Completed!"?350:390,height));body.GetComponent<Image>().type=Image.Type.Simple;
  Panel(body,"Header divider",new Vector2(0,61),new Vector2(body.sizeDelta.x-10,2),booster?new Color32(30,76,174,255):new Color32(86,65,155,255));Label(body,title,new Vector2(0,31),32);ScaleScreenContents(overlay);StartCoroutine(ModalEntrance(body));return body;
 }
 IEnumerator ModalEntrance(RectTransform body){for(float t=0;t<.28f;t+=Time.unscaledDeltaTime){if(!body)yield break;float p=Mathf.Clamp01(t/.28f);float ease=1+2.7f*Mathf.Pow(p-1,3)+1.7f*Mathf.Pow(p-1,2);body.localScale=Vector3.one*Mathf.Lerp(.86f,1,ease);yield return null;}if(body)body.localScale=Vector3.one;}
 void Close(){if(overlay){overlay.gameObject.SetActive(false);Destroy(overlay.gameObject);}if(suspendedShop){overlay=suspendedShop;suspendedShop=null;paused=true;return;}if(game)game.gameObject.SetActive(true);paused=journey;}
 void ApplySettingState(RectTransform button,string icon,bool enabled){
  button.GetComponent<Image>().color=Color.white;var iconView=button.Find(icon) as RectTransform;if(iconView)iconView.GetComponent<Image>().color=Color.white;var old=button.Find("Off marker");if(old)Destroy(old.gameObject);if(!enabled){var marker=Label(button,"×",new Vector2(0,button.sizeDelta.y*.5f),Mathf.RoundToInt(button.sizeDelta.y*.78f),Color.white,bounds:new Vector2(button.sizeDelta.x-8,button.sizeDelta.y-8),localize:false);marker.name="Off marker";var outline=marker.GetComponent<Outline>();outline.effectColor=new Color32(25,31,31,255);outline.effectDistance=new Vector2(2.4f,-2.4f);}
  button.localScale=Vector3.one;StartCoroutine(Pop(button,.12f,.08f));
 }
 void StylePrivacyLink(Text privacy){privacy.color=new Color32(53,43,139,255);var outline=privacy.GetComponent<Outline>();if(outline)outline.enabled=false;Panel(privacy.transform,"Privacy underline",new Vector2(0,privacy.rectTransform.sizeDelta.y-4),new Vector2(Mathf.Min(205,privacy.preferredWidth),2),privacy.color);}
 void Settings(){
  if(journey){JourneySettings();return;}
  var p=Modal("Settings",650);
  var close=Button(p,"",new Vector2(152,27),new Vector2(45,46),colors[1],Close);Picture(close,"close",new Vector2(0,22),new Vector2(31,31));
  RectTransform soundButton=null,vibrationButton=null;soundButton=Button(p,"",new Vector2(-68,145),new Vector2(102,106),new Color32(255,124,35,255),()=>{sound=!sound;PlayerPrefs.SetInt("sound",sound?1:0);PlayerPrefs.Save();ApplySettingState(soundButton,"sound",sound);if(sound)Sfx("click",.7f,1.08f);});Picture(soundButton,"sound",new Vector2(0,51),new Vector2(68,68));ApplySettingState(soundButton,"sound",sound);
  vibrationButton=Button(p,"",new Vector2(68,145),new Vector2(102,106),new Color32(255,124,35,255),()=>{haptics=!haptics;PlayerPrefs.SetInt("haptics",haptics?1:0);PlayerPrefs.Save();HapticFeedback.SetEnabled(haptics);if(haptics)HapticFeedback.Play(HapticCue.Selection);ApplySettingState(vibrationButton,"haptic-touch",haptics);});Picture(vibrationButton,"haptic-touch",new Vector2(0,51),new Vector2(68,68));ApplySettingState(vibrationButton,"haptic-touch",haptics);
  Button(p,"Help & Support",new Vector2(0,309),new Vector2(300,55),new Color32(255,125,40,255),Help,25);
  var privacy=Label(p,"Privacy Policy",new Vector2(0,370),23,new Color32(52,40,128,255),new Vector2(260,42));privacy.name="Privacy Policy link";StylePrivacyLink(privacy);privacy.raycastTarget=true;var privacyButton=privacy.gameObject.AddComponent<Button>();privacyButton.targetGraphic=privacy;privacyButton.onClick.AddListener(()=>{Sfx("click",.5f);Application.OpenURL("https://www.smartlogy.com.tr/RollerSort/Privacy/");});
  Button(p,"Quit",new Vector2(0,438),new Vector2(300,57),colors[1],()=>{if(journey)Close();else ConfirmQuit();},28);
  DrawLanguageChoice(p,540);
 }
 void JourneySettings(){
  var p=Modal("Settings",470);JourneyTabs(overlay,2);
  RectTransform soundButton=null,vibrationButton=null;
  soundButton=Button(p,"",new Vector2(-58,126),new Vector2(82,86),new Color32(255,124,35,255),()=>{sound=!sound;PlayerPrefs.SetInt("sound",sound?1:0);PlayerPrefs.Save();ApplySettingState(soundButton,"sound",sound);if(sound)Sfx("click",.7f,1.08f);});Picture(soundButton,"sound",new Vector2(0,41),new Vector2(55,55));ApplySettingState(soundButton,"sound",sound);
  vibrationButton=Button(p,"",new Vector2(58,126),new Vector2(82,86),new Color32(255,124,35,255),()=>{haptics=!haptics;PlayerPrefs.SetInt("haptics",haptics?1:0);PlayerPrefs.Save();HapticFeedback.SetEnabled(haptics);if(haptics)HapticFeedback.Play(HapticCue.Selection);ApplySettingState(vibrationButton,"haptic-touch",haptics);});Picture(vibrationButton,"haptic-touch",new Vector2(0,41),new Vector2(55,55));ApplySettingState(vibrationButton,"haptic-touch",haptics);
  Button(p,"Help & Support",new Vector2(0,267),new Vector2(270,49),new Color32(255,125,40,255),Help,23);
  var privacy=Label(p,"Privacy Policy",new Vector2(0,317),18,new Color32(52,40,128,255),new Vector2(240,34));privacy.name="Privacy Policy link";StylePrivacyLink(privacy);privacy.raycastTarget=true;var privacyButton=privacy.gameObject.AddComponent<Button>();privacyButton.targetGraphic=privacy;privacyButton.onClick.AddListener(()=>{Sfx("click",.5f);Application.OpenURL("https://www.smartlogy.com.tr/RollerSort/Privacy/");});
  DrawLanguageChoice(p,390);
 }
 void ConfirmRestart(){var p=Modal("TEKRAR DENE",290);Label(p,"Bu bölümün ilerlemesi sıfırlanacak.",new Vector2(0,108),17);Button(p,"YENİDEN BAŞLA",new Vector2(0,178),new Vector2(300,54),colors[1],()=>{StopAllCoroutines();BuildLevel();},22);Button(p,"VAZGEÇ",new Vector2(0,243),new Vector2(210,42),colors[3],Close,19);}
 void Help(){var p=Modal("NASIL OYNANIR?",440);Label(p,"1. En alttaki kutulardan birine dokun.\n\n2. Dokuz bilye banda yuvarlanır.\n\n3. Bilyeler aynı renkteki tepsiye dolar.\n\n4. Tepsileri bitir, bölümü tamamla!\n\nBilyeleri uygun tepsilere ulaştır.",new Vector2(0,213),19,bounds:new Vector2(330,270));Button(p,"HADİ OYNAYALIM",new Vector2(0,382),new Vector2(300,58),colors[2],Close,21);}
 void PreviewShop(){
  paused=true;if(overlay){overlay.gameObject.SetActive(false);Destroy(overlay.gameObject);}if(suspendedShop){Destroy(suspendedShop.gameObject);suspendedShop=null;}game.gameObject.SetActive(false);
  overlay=ScreenPanel("Shop",new Color32(40,48,115,255));overlay.GetComponent<Image>().raycastTarget=true;
  Picture(overlay,"shop-pattern",new Vector2(0,H/2),new Vector2(W,H));
  Picture(overlay,"awning",new Vector2(0,120),new Vector2(450,240));
  if(journey)ProfileButton(overlay,new Vector2(-190,107));else{var gear=Button(overlay,"",new Vector2(-190,107),new Vector2(50,51),purple,Settings);Picture(gear,"gear",new Vector2(0,24),new Vector2(31,31));}
  LifeHud(overlay);
  var currency=Picture(overlay,"hud",new Vector2(142,107),new Vector2(154,43));Picture(currency,"coin",new Vector2(-57,21),new Vector2(27,29));Label(currency,FormatCoins(),new Vector2(-13,21),29,bounds:new Vector2(83,43));var add=Button(overlay,"",new Vector2(194,107),new Vector2(45,47),colors[0],()=>ShopInfo("Altın"));Picture(add,"plus",new Vector2(0,22),new Vector2(31,31));
  // Clip the scrolling product list independently of the header and close button.
  var viewport=Panel(overlay,"Products viewport",new Vector2(0,581),new Vector2(420,630),Color.clear);viewport.gameObject.AddComponent<RectMask2D>();viewport.GetComponent<Image>().raycastTarget=true;
  var content=Panel(viewport,"Products",Vector2.zero,new Vector2(420,1480),Color.clear);content.pivot=new Vector2(.5f,1);content.anchoredPosition=Vector2.zero;
  var scroll=viewport.gameObject.AddComponent<ScrollRect>();scroll.viewport=viewport;scroll.content=content;scroll.horizontal=false;scroll.vertical=true;scroll.movementType=ScrollRect.MovementType.Clamped;scroll.inertia=true;scroll.decelerationRate=.10f;scroll.scrollSensitivity=28;
  ShopCard(content,"Starter Pack",74,true,"2.500",1,1,1,"1h","₺199,99");
  var ribbon=Picture(content,"popular-ribbon",new Vector2(142,35),new Vector2(115,44));
  Picture(content,"bundle-header",new Vector2(0,207),new Vector2(396,47));Label(content,"Bundles",new Vector2(0,207),27);
  ShopCard(content,"Pocket Bundle",322,false,"1.500",1,0,0,"30m","₺299,99");
  ShopCard(content,"Small Bundle",477,false,"3.000",1,1,0,"1h","₺499,99");
  ShopCard(content,"Big Bundle",632,false,"8.000",2,2,2,"2h","₺999,99");
  ShopCard(content,"Large Bundle",787,false,"18.000",3,3,3,"4h","₺1.999,99");
  Picture(content,"bundle-header",new Vector2(0,914),new Vector2(396,47));Label(content,"Coin Packs",new Vector2(0,914),27);
  string[] amounts={"1.000","5.000","10.000","25.000","50.000","100.000"},prices={"₺99,99","₺399,99","₺799,99","₺1.999,99","₺2.999,99","₺4.999,99"};
  for(int i=0;i<6;i++){int product=i;var pack=Panel(content,"Coin pack",new Vector2(-134+(i%3)*134,1040+(i/3)*175),new Vector2(126,165),colors[0]);Picture(pack,"coins-stack",new Vector2(0,45),new Vector2(75,66));Label(pack,amounts[i],new Vector2(0,93),25,bounds:new Vector2(120,40));Button(pack,prices[i],new Vector2(0,135),new Vector2(113,44),colors[2],()=>ShopInfo(amounts[product]+" coins"),19);}
  Button(content,"Remove Ads",new Vector2(0,1397),new Vector2(390,70),colors[1],NoAdsInfo,30);

  var close=Button(overlay,"",new Vector2(149,241),new Vector2(74,77),colors[1],Close);Picture(close,"close",new Vector2(0,36),new Vector2(49,49));if(journey)JourneyTabs(overlay,0);ScaleScreenContents(overlay,true);
 }
 void ShopCard(RectTransform parent,string title,float y,bool starter,string amount,int magnet,int shuffle,int gumball,string duration,string price){
  var card=Picture(parent,starter?"shop-card-gold":"shop-card-blue",new Vector2(0,y),new Vector2(396,147));
  Picture(card,"coins-stack",new Vector2(-134,42),new Vector2(63,57));Label(card,amount,new Vector2(-134,73),22,bounds:new Vector2(90,34));
  var items=new List<string>{"magnet"};var quantities=new List<int>{magnet};if(shuffle>0){items.Add("shuffle");quantities.Add(shuffle);}if(gumball>0){items.Add("gumball");quantities.Add(gumball);}
  for(int i=0;i<items.Count;i++){float x=items.Count==1?-17:-60+i*58;Picture(card,items[i],new Vector2(x,40),new Vector2(46,49));Label(card,"×"+quantities[i],new Vector2(x,73),20,bounds:new Vector2(55,30));}
  Picture(card,"heart",new Vector2(139,40),new Vector2(64,62));Label(card,"∞",new Vector2(139,39),37,bounds:new Vector2(62,45));Label(card,duration,new Vector2(139,74),21,bounds:new Vector2(72,33));
  Label(card,title,new Vector2(-58,119),25,bounds:new Vector2(270,42));Button(card,price,new Vector2(142,117),new Vector2(88,40),colors[2],()=>ShopInfo(title),20);
 }
 void ShopInfo(string title){var p=Modal(title,270);Label(p,"Prototip mağaza\nGerçek ödeme alınmaz.",new Vector2(0,128),21,bounds:new Vector2(310,90));Button(p,"TAMAM",new Vector2(0,217),new Vector2(230,53),colors[2],Shop,24);}
 void DailyQuest(){
  string day=DateTime.Now.ToString("yyyy-MM-dd");bool today=PlayerPrefs.GetString("questDay","")==day;int count=today?PlayerPrefs.GetInt("questCount",0):0;bool claimed=today&&PlayerPrefs.GetInt("questClaimed",0)==1;
  var p=Modal("GÜNLÜK GÖREV",350);Label(p,"3 bölüm tamamla\n\n"+Mathf.Min(3,count)+" / 3   ·   Ödül: 250 altın",new Vector2(0,157),24,bounds:new Vector2(330,150));
  Button(p,count>=3&&!claimed?"250 ALTINI AL":"OYUNA DÖN",new Vector2(0,277),new Vector2(290,58),colors[2],()=>{if(count>=3&&!claimed){coins+=GameBalance.DailyQuestReward;PlayerPrefs.SetInt("coins",coins);PlayerPrefs.SetInt("questClaimed",1);PlayerPrefs.Save();coinText.text=FormatCoins();}Close();},22);
 }
 IEnumerator RewardFloat(RectTransform gift){Vector2 origin=gift.anchoredPosition;while(gift){float t=Time.unscaledTime;gift.anchoredPosition=origin+Vector2.up*Mathf.Sin(t*2.2f)*4;gift.localRotation=Quaternion.Euler(0,0,Mathf.Sin(t*1.5f)*2);yield return null;}}
 IEnumerator AnimateVictoryProgress(RectTransform fill,Text value,float from,float to){yield return new WaitForSecondsRealtime(.25f);for(float t=0;t<.75f;){t+=Time.unscaledDeltaTime;float p=1-Mathf.Pow(1-Mathf.Clamp01(t/.75f),3),v=Mathf.Lerp(from,to,p);fill.sizeDelta=new Vector2(198*v,26);fill.anchoredPosition=new Vector2(-99+99*v,-22);value.text=Mathf.RoundToInt(v*100)+"%";yield return null;}value.text=Mathf.RoundToInt(to*100)+"%";}
 void Toast(string message){StartCoroutine(Praise(message));}
 IEnumerator Victory(){
  Sfx("victory",.75f);HapticFeedback.Play(HapticCue.LevelComplete);
  yield return new WaitForSeconds(.45f);
  if(!diagnosticsActive){
  string day=DateTime.Now.ToString("yyyy-MM-dd");if(PlayerPrefs.GetString("questDay","")!=day){PlayerPrefs.SetString("questDay",day);PlayerPrefs.SetInt("questCount",0);PlayerPrefs.SetInt("questClaimed",0);}PlayerPrefs.SetInt("questCount",PlayerPrefs.GetInt("questCount",0)+1);
  pendingReward=GameBalance.LevelReward(level);coins+=pendingReward;PlayerPrefs.SetInt("coins",coins);PlayerPrefs.SetInt("level",level+1);PlayerPrefs.Save();
  }else {pendingReward=GameBalance.LevelReward(level);coins+=pendingReward;}
  victoryTransition=false;coinText.text=(coins-pendingReward).ToString("N0",System.Globalization.CultureInfo.InvariantCulture);
  var p=Modal("Level Completed!",550,50);Panel(p,"Gift inset",new Vector2(0,213),new Vector2(320,280),new Color32(126,93,205,255));string milestone=level==5?"magnet":level==7?"shuffle":level==9?"tunnel":level==10?"gumball":level==14?"belt-expanded":null;var gift=Picture(p,milestone??"gift-v2",new Vector2(0,190),new Vector2(180,190));StartCoroutine(RewardFloat(gift));if(milestone!=null)Label(p,level==9?"Tunnels Unlocked!":level==14?"Extra Space!":"New Booster · Level "+(level+1),new Vector2(0,280),21,colors[0]);
  var track=Picture(p,"progress-track",new Vector2(0,311),new Vector2(228,45));float progress=((level-1)%5+1)/5f,previous=Mathf.Max(0,progress-.2f);var fill=Picture(track,"progress-fill",new Vector2(-99+99*previous,22),new Vector2(198*previous,26));var progressText=Label(p,Mathf.RoundToInt(previous*100)+"%",new Vector2(0,311),30);StartCoroutine(AnimateVictoryProgress(fill,progressText,previous,progress));
  var reward=Picture(p,"coin",new Vector2(-38,397),new Vector2(46,50));Label(p,"+"+pendingReward,new Vector2(31,397),43,bounds:new Vector2(120,54),localize:false);Button(p,"Continue",new Vector2(0,485),new Vector2(235,80),colors[2],()=>BeginVictoryContinue(reward),36);
 }
}
}
