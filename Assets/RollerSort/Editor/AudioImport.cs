using UnityEditor;
using UnityEngine;
namespace RollerSort.Editor {
 // Short effects stay uncompressed PCM: lossy codecs smear their sharp transients.
 public sealed class AudioImport : AssetPostprocessor {
  void OnPreprocessAudio(){if(!assetPath.StartsWith("Assets/Resources/Audio/"))return;var a=(AudioImporter)assetImporter;var s=a.defaultSampleSettings;s.loadType=AudioClipLoadType.DecompressOnLoad;s.compressionFormat=AudioCompressionFormat.PCM;s.sampleRateSetting=AudioSampleRateSetting.PreserveSampleRate;a.defaultSampleSettings=s;a.forceToMono=true;a.loadInBackground=false;}
 }
}
