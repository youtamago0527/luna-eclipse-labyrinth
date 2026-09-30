using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace LunaEclipse.EditorTools
{
    public static class LunaValidation
    {
        private static readonly string[] RequiredHubSprites =
        {
            "Hub/moon-ruins", "Hub/characters/luna-hub-original",
            "Hub/buttons/equipment", "Hub/buttons/shop", "Hub/buttons/storage",
            "Hub/buttons/relics", "Hub/buttons/enter-dungeon"
        };

        [Serializable]
        private sealed class ValidationReport
        {
            public bool passed;
            public string utc;
            public string unityVersion;
            public string[] errors;
        }

        [MenuItem("Luna/Validate Project")]
        public static void Validate()
        {
            var errors = new List<string>();
            if (EditorApplication.isCompiling)
                errors.Add("Scripts are still compiling. Run validation after compilation finishes.");

            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(LunaProjectTools.StartupScenePath) == null)
                errors.Add("Startup scene is missing. Run Luna/Create Startup Scene.");
            if (!EditorBuildSettings.scenes.Any(scene => scene.enabled && scene.path == LunaProjectTools.StartupScenePath))
                errors.Add("Startup scene is not enabled in Build Settings.");
            if (PlayerSettings.defaultScreenWidth != 430 || PlayerSettings.defaultScreenHeight != 932)
                errors.Add("Default player resolution must be 430 x 932.");
            if (PlayerSettings.defaultInterfaceOrientation != UIOrientation.Portrait)
                errors.Add("Default interface orientation must be Portrait.");
            if (!PlayerSettings.runInBackground) errors.Add("Run In Background must be enabled.");

            foreach (string resourcePath in RequiredHubSprites)
            {
                string path = "Assets/Resources/" + resourcePath + ".png";
                if (!File.Exists(path)) { errors.Add("Required artwork missing: " + path); continue; }
                if (!(AssetImporter.GetAtPath(path) is TextureImporter importer))
                { errors.Add("Texture importer missing: " + path); continue; }
                if (importer.textureType != TextureImporterType.Sprite || importer.spriteImportMode != SpriteImportMode.Single)
                    errors.Add("Artwork must import as a Single Sprite: " + path);
                if (!importer.alphaIsTransparency || importer.textureCompression != TextureImporterCompression.Uncompressed
                    || importer.maxTextureSize != 4096 || importer.spritePivot != new Vector2(0.5f, 0.5f))
                    errors.Add("Artwork import settings differ from the project defaults: " + path);
                if (Resources.Load<Sprite>(resourcePath) == null)
                    errors.Add("Sprite cannot be loaded through Resources: " + resourcePath);
            }

            bool appFound = AppDomain.CurrentDomain.GetAssemblies().SelectMany(GetLoadableTypes)
                .Any(type => type.Name == "LunaApp" && typeof(MonoBehaviour).IsAssignableFrom(type));
            if (!appFound) errors.Add("Compiled LunaApp MonoBehaviour was not found.");

            var report = new ValidationReport
            {
                passed = errors.Count == 0,
                utc = DateTime.UtcNow.ToString("O"),
                unityVersion = Application.unityVersion,
                errors = errors.ToArray()
            };
            Directory.CreateDirectory("Library");
            File.WriteAllText("Library/LunaValidationReport.json", JsonUtility.ToJson(report, true));
            if (errors.Count > 0)
                throw new InvalidOperationException("Luna validation failed:\n" + string.Join("\n", errors));
            Debug.Log("Luna validation passed: startup scene, compiled app, seven required Hub sprites, and portrait settings.");
        }

        private static IEnumerable<Type> GetLoadableTypes(Assembly assembly)
        {
            try { return assembly.GetTypes(); }
            catch (ReflectionTypeLoadException exception) { return exception.Types.Where(type => type != null); }
        }
    }
}
