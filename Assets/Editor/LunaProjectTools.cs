using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LunaEclipse.EditorTools
{
    /// <summary>Creates only the startup shell; gameplay starts through LunaApp.</summary>
    public static class LunaProjectTools
    {
        public const string StartupScenePath = "Assets/Scenes/Startup.unity";
        public const string DungeonScenePath = "Assets/Scenes/Dungeon.unity";
        public const string HubAssetRoot = "Assets/Resources/Hub/";

        [MenuItem("Luna/Create Startup Scene")]
        private static void CreateStartupSceneMenu()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            Prepare();
            EditorSceneManager.OpenScene(StartupScenePath);
        }

        // Batch: Unity -batchmode -quit -projectPath <path>
        //        -executeMethod LunaEclipse.EditorTools.LunaProjectTools.Prepare
        public static void Prepare()
        {
            PlayerSettings.companyName = "Luna Project";
            PlayerSettings.productName = "Luna Eclipse Labyrinth";
            PlayerSettings.defaultScreenWidth = 430;
            PlayerSettings.defaultScreenHeight = 932;
            PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
            PlayerSettings.resizableWindow = true;
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
            PlayerSettings.allowedAutorotateToPortrait = true;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.allowedAutorotateToLandscapeLeft = false;
            PlayerSettings.allowedAutorotateToLandscapeRight = false;
            PlayerSettings.runInBackground = true;

            if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
                AssetDatabase.CreateFolder("Assets", "Scenes");

            // Never replace an existing scene or the scene currently being edited.
            if (!File.Exists(StartupScenePath))
            {
                Scene current = SceneManager.GetActiveScene();
                bool emptyBatchScene = Application.isBatchMode && SceneManager.sceneCount == 1
                    && string.IsNullOrEmpty(current.path) && current.rootCount == 0;
                Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,
                    emptyBatchScene ? NewSceneMode.Single : NewSceneMode.Additive);
                bool saved = EditorSceneManager.SaveScene(scene, StartupScenePath);
                if (!emptyBatchScene) EditorSceneManager.CloseScene(scene, true);
                if (!saved) throw new IOException("Could not save " + StartupScenePath);
            }

            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(StartupScenePath, true) }
                .Concat(EditorBuildSettings.scenes.Where(scene => scene.path != StartupScenePath)).ToArray();

            if (!File.Exists(DungeonScenePath))
            {
                // A saved temporary active scene is available from Prepare above.
                if (string.IsNullOrEmpty(SceneManager.GetActiveScene().path) && Application.isBatchMode)
                    EditorSceneManager.OpenScene(StartupScenePath);
                var dungeon = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
                EditorSceneManager.SaveScene(dungeon,DungeonScenePath);
                EditorSceneManager.CloseScene(dungeon,true);
            }
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(StartupScenePath,true),new EditorBuildSettingsScene(DungeonScenePath,true) };

            foreach (string path in AssetDatabase.GetAllAssetPaths().Where(IsHubPng))
            {
                if (AssetImporter.GetAtPath(path) is TextureImporter importer && ApplyHubSpriteSettings(importer))
                    importer.SaveAndReimport();
            }
            foreach(string path in AssetDatabase.GetAllAssetPaths().Where(p=>p.StartsWith("Assets/Resources/Luna/",StringComparison.Ordinal)&&p.EndsWith(".png",StringComparison.OrdinalIgnoreCase)))
                if(AssetImporter.GetAtPath(path) is TextureImporter spriteImporter && spriteImporter.spritePixelsPerUnit!=150f)
                    AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceUpdate);
            AssetDatabase.SaveAssets();
            Debug.Log("Luna startup scene, portrait settings, and Hub sprite imports prepared.");
        }

        [MenuItem("Luna/Build Windows Preview")]
        public static void BuildWindows()
        {
            Prepare();
            LunaValidation.Validate();
            const string output = "Builds/DungeonPreview/LunaEclipseLabyrinth.exe";
            Directory.CreateDirectory("Builds/DungeonPreview");
            BuildReport report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = new[] { StartupScenePath, DungeonScenePath },
                locationPathName = output,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            });
            if (report.summary.result != BuildResult.Succeeded)
                throw new InvalidOperationException("Windows preview build failed: " + report.summary.result
                    + "; errors: " + report.summary.totalErrors);
            Debug.Log("Luna Windows preview built: " + Path.GetFullPath(output));
        }

        internal static bool IsHubPng(string path)
        {
            return path.StartsWith(HubAssetRoot, StringComparison.Ordinal)
                && path.EndsWith(".png", StringComparison.OrdinalIgnoreCase);
        }

        internal static bool ApplyHubSpriteSettings(TextureImporter importer)
        {
            bool changed = importer.textureType != TextureImporterType.Sprite
                || importer.spriteImportMode != SpriteImportMode.Single
                || !importer.alphaIsTransparency
                || importer.textureCompression != TextureImporterCompression.Uncompressed
                || importer.maxTextureSize != 4096
                || importer.mipmapEnabled
                || importer.spritePivot != new Vector2(0.5f, 0.5f);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = true;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.maxTextureSize = 4096;
            importer.mipmapEnabled = false;
            importer.spritePivot = new Vector2(0.5f, 0.5f);
            return changed;
        }
    }

    // Applies the same import defaults to newly added Hub artwork.
    // Source PNG bytes are never modified.
    internal sealed class LunaHubSpriteImporter : AssetPostprocessor
    {
        private void OnPreprocessTexture()
        {
            if (LunaProjectTools.IsHubPng(assetPath))
                LunaProjectTools.ApplyHubSpriteSettings((TextureImporter)assetImporter);
        }
    }
}
