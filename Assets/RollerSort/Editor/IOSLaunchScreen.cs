using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
namespace RollerSort.Editor {
 // Show the game's own splash while iOS loads the Unity runtime.
 public static class IOSLaunchScreen {
  [PostProcessBuild(110)] public static void Configure(BuildTarget target,string path){
   if(target!=BuildTarget.iOS)return;
   File.Copy("Assets/Resources/Art/launch-scene.png",Path.Combine(path,"RollerSplashBackground.png"),true);
   File.Copy("Assets/Resources/RollerSortLogoV2.png",Path.Combine(path,"RollerSplashLogo.png"),true);
   string projectPath=PBXProject.GetPBXProjectPath(path);var project=new PBXProject();project.ReadFromFile(projectPath);
   foreach(string asset in new[]{"RollerSplashBackground.png","RollerSplashLogo.png"}){string guid=project.FindFileGuidByProjectPath(asset);if(string.IsNullOrEmpty(guid))guid=project.AddFile(asset,asset,PBXSourceTree.Source);project.AddFileToBuild(project.GetUnityMainTargetGuid(),guid);}
   string launch="RollerLaunchScreen.storyboard";File.WriteAllText(Path.Combine(path,launch),Storyboard);
   string launchGuid=project.FindFileGuidByProjectPath(launch);if(string.IsNullOrEmpty(launchGuid))launchGuid=project.AddFile(launch,launch,PBXSourceTree.Source);project.AddFileToBuild(project.GetUnityMainTargetGuid(),launchGuid);
   project.WriteToFile(projectPath);
   string plistPath=Path.Combine(path,"Info.plist");var plist=new PlistDocument();plist.ReadFromFile(plistPath);
   foreach(string key in new[]{"UILaunchStoryboardName","UILaunchStoryboardName~iphone","UILaunchStoryboardName~ipod","UILaunchStoryboardName~ipad"})plist.root.SetString(key,"RollerLaunchScreen");var configurations=plist.root["UIApplicationSceneManifest"].AsDict()["UISceneConfigurations"].AsDict()["UIWindowSceneSessionRoleApplication"].AsArray();
   foreach(var entry in configurations.values){entry.AsDict().SetString("UISceneDelegateClassName","RollerLaunchScene");entry.AsDict().SetString("UISceneStoryboardFile","RollerLaunchScreen");}
   plist.WriteToFile(plistPath);
   foreach(string device in new[]{"iPhone","iPad"})File.WriteAllText(Path.Combine(path,"LaunchScreen-"+device+".storyboard"),Storyboard);
  }
  const string Storyboard=@"<?xml version=""1.0"" encoding=""UTF-8""?>
<document type=""com.apple.InterfaceBuilder3.CocoaTouch.Storyboard.XIB"" version=""3.0"" toolsVersion=""15702"" targetRuntime=""iOS.CocoaTouch"" propertyAccessControl=""none"" useAutolayout=""YES"" launchScreen=""YES"" useTraitCollections=""YES"" colorMatched=""YES"" initialViewController=""roller-controller"">
 <device id=""retina6_7"" orientation=""portrait"" appearance=""light""/>
 <dependencies><deployment identifier=""iOS""/><plugIn identifier=""com.apple.InterfaceBuilder.IBCocoaTouchPlugin"" version=""15704""/><capability name=""documents saved in the Xcode 8 format"" minToolsVersion=""8.0""/></dependencies>
 <scenes><scene sceneID=""roller-scene""><objects><viewController id=""roller-controller"" sceneMemberID=""viewController""><view key=""view"" contentMode=""scaleToFill"" id=""roller-root""><rect key=""frame"" x=""0"" y=""0"" width=""450"" height=""975""/><autoresizingMask key=""autoresizingMask"" widthSizable=""YES"" heightSizable=""YES""/>
 <subviews>
 <imageView userInteractionEnabled=""NO"" contentMode=""scaleAspectFill"" image=""RollerSplashBackground.png"" translatesAutoresizingMaskIntoConstraints=""NO"" id=""roller-background""><rect key=""frame"" x=""0"" y=""0"" width=""450"" height=""975""/></imageView>
 <imageView userInteractionEnabled=""NO"" hidden=""YES"" contentMode=""scaleAspectFit"" image=""RollerSplashLogo.png"" translatesAutoresizingMaskIntoConstraints=""NO"" id=""roller-logo""><rect key=""frame"" x=""70"" y=""360"" width=""310"" height=""210""/><constraints><constraint firstAttribute=""height"" secondItem=""roller-logo"" secondAttribute=""width"" multiplier=""0.677419355"" id=""logo-aspect""/></constraints></imageView>
 </subviews><color key=""backgroundColor"" red=""0.1333"" green=""0.1725"" blue=""0.5137"" alpha=""1"" colorSpace=""custom"" customColorSpace=""sRGB""/>
 <constraints>
 <constraint firstItem=""roller-background"" firstAttribute=""top"" secondItem=""roller-root"" secondAttribute=""top"" id=""bg-top""/><constraint firstItem=""roller-background"" firstAttribute=""bottom"" secondItem=""roller-root"" secondAttribute=""bottom"" id=""bg-bottom""/><constraint firstItem=""roller-background"" firstAttribute=""leading"" secondItem=""roller-root"" secondAttribute=""leading"" id=""bg-leading""/><constraint firstItem=""roller-background"" firstAttribute=""trailing"" secondItem=""roller-root"" secondAttribute=""trailing"" id=""bg-trailing""/>
 <constraint firstItem=""roller-logo"" firstAttribute=""centerX"" secondItem=""roller-root"" secondAttribute=""centerX"" id=""logo-x""/><constraint firstItem=""roller-logo"" firstAttribute=""centerY"" secondItem=""roller-root"" secondAttribute=""centerY"" multiplier=""0.953846154"" id=""logo-y""/><constraint firstItem=""roller-logo"" firstAttribute=""width"" secondItem=""roller-root"" secondAttribute=""width"" multiplier=""0.688888889"" id=""logo-width""/>
 </constraints></view></viewController><placeholder placeholderIdentifier=""IBFirstResponder"" id=""roller-responder"" sceneMemberID=""firstResponder""/></objects></scene></scenes>
 <resources><image name=""RollerSplashBackground.png"" width=""450"" height=""975""/><image name=""RollerSplashLogo.png"" width=""310"" height=""210""/></resources>
</document>";
 }
}
