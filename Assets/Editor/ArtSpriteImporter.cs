using UnityEditor;
using UnityEngine;

// Generated originals remain unchanged. Import as cell-friendly transparent sprites.
public sealed class ArtSpriteImporter : AssetPostprocessor
{
    public override uint GetVersion() => 1;
    void OnPreprocessTexture()
    {
        if(!assetPath.StartsWith("Assets/Resources/Art/")) return;
        var importer=(TextureImporter)assetImporter;
        importer.textureType=TextureImporterType.Sprite;
        importer.spriteImportMode=SpriteImportMode.Single;
        importer.spritePixelsPerUnit=1024;
        importer.maxTextureSize=1024;
        importer.mipmapEnabled=false;
        importer.alphaIsTransparency=true;
        importer.textureCompression=TextureImporterCompression.Uncompressed;
        importer.npotScale=TextureImporterNPOTScale.None;
        importer.filterMode=FilterMode.Bilinear;
        var settings=new TextureImporterSettings(); importer.ReadTextureSettings(settings);
        settings.spriteMeshType=SpriteMeshType.FullRect;
        settings.spriteAlignment=(int)SpriteAlignment.Center;
        importer.SetTextureSettings(settings);
    }
}
