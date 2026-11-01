using UnityEditor;
using UnityEngine;
namespace RollerSort.Editor {
 public sealed class ArtImport : AssetPostprocessor {
  void OnPreprocessTexture(){if(assetPath.StartsWith("Assets/Resources/Art/")||assetPath.Contains("RollerSortLogo"))Configure((TextureImporter)assetImporter);}
  static void Configure(TextureImporter t){t.textureType=TextureImporterType.Default;t.mipmapEnabled=false;t.alphaIsTransparency=true;t.wrapMode=TextureWrapMode.Clamp;t.filterMode=FilterMode.Bilinear;t.npotScale=TextureImporterNPOTScale.None;t.maxTextureSize=4096;t.textureCompression=TextureImporterCompression.Uncompressed;}
  public static void ConfigureAll(){foreach(string guid in AssetDatabase.FindAssets("t:Texture2D",new[]{"Assets/Resources"})){var path=AssetDatabase.GUIDToAssetPath(guid);var t=AssetImporter.GetAtPath(path) as TextureImporter;if(t==null)continue;Configure(t);t.SaveAndReimport();}}
 }
}
