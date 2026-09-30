using UnityEditor;
using UnityEngine;
public sealed class GeneratedAudioImporter:AssetPostprocessor
{
    void OnPreprocessAudio()
    {
        if(!assetPath.StartsWith("Assets/Resources/Audio/Generated/"))return;
        var importer=(AudioImporter)assetImporter;
        var settings=importer.defaultSampleSettings;
        settings.loadType=AudioClipLoadType.Streaming;
        settings.compressionFormat=AudioCompressionFormat.Vorbis;
        settings.quality=.8f;
        importer.defaultSampleSettings=settings;
        importer.forceToMono=false;
        importer.loadInBackground=true;
    }
}
