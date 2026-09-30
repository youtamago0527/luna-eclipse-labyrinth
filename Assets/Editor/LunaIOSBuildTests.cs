using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;
using UnityEngine.Rendering;

namespace LunaEclipse.EditorTools
{
    public static class LunaIOSBuildTests
    {
        [MenuItem("Luna/iOS/Validate Settings Tests")]
        public static void Run()
        {
            string bundle = PlayerSettings.GetApplicationIdentifier(NamedBuildTarget.iOS);
            string product = PlayerSettings.productName;
            string team = PlayerSettings.iOS.appleDeveloperTeamID;
            string build = PlayerSettings.iOS.buildNumber;
            bool signing = PlayerSettings.iOS.appleEnableAutomaticSigning;
            var windowsBackend = PlayerSettings.GetScriptingBackend(NamedBuildTarget.Standalone);
            var activeTarget = EditorUserBuildSettings.activeBuildTarget;
            LunaIOSBuild.Prepare();
            LunaIOSBuild.Prepare(); // Idempotent; no target switch or scene recreation.
            Check(bundle == PlayerSettings.GetApplicationIdentifier(NamedBuildTarget.iOS), "Identity preserved");
            Check(product == PlayerSettings.productName, "Shared product name preserved");
            Check(team == PlayerSettings.iOS.appleDeveloperTeamID, "Team preserved");
            Check(build == PlayerSettings.iOS.buildNumber, "Build number preserved");
            Check(signing == PlayerSettings.iOS.appleEnableAutomaticSigning, "Signing preserved");
            Check(windowsBackend == PlayerSettings.GetScriptingBackend(NamedBuildTarget.Standalone), "Windows backend preserved");
            Check(activeTarget == EditorUserBuildSettings.activeBuildTarget, "Active target preserved");
            Check(PlayerSettings.GetScriptingBackend(NamedBuildTarget.iOS) == ScriptingImplementation.IL2CPP, "IL2CPP");
            Check(PlayerSettings.GetArchitecture(NamedBuildTarget.iOS) == 1, "ARM64");
            Check(PlayerSettings.GetManagedStrippingLevel(NamedBuildTarget.iOS) == ManagedStrippingLevel.Minimal, "Minimal stripping");
            Check(PlayerSettings.iOS.sdkVersion == iOSSdkVersion.DeviceSDK, "Device SDK");
            Check(PlayerSettings.iOS.targetOSVersionString == "15.0", "Minimum iOS");
            Check(PlayerSettings.defaultInterfaceOrientation == UIOrientation.Portrait, "Portrait");
            Check(!PlayerSettings.allowedAutorotateToLandscapeLeft && !PlayerSettings.allowedAutorotateToLandscapeRight, "No landscape");
            Check(!PlayerSettings.GetUseDefaultGraphicsAPIs(BuildTarget.iOS)
                && PlayerSettings.GetGraphicsAPIs(BuildTarget.iOS).SequenceEqual(new[] { GraphicsDeviceType.Metal }), "Metal");
            Check(EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).SequenceEqual(LunaIOSBuild.Scenes), "Startup then Dungeon");
            LunaIOSBuild.RequireIdentity("com.lunatest.labyrinth", "ABCDE12345", "1");
            Reject(null, "ABCDE12345", "1");
            Reject("com.example.luna", "ABCDE12345", "1");
            Reject("com.company.arsene", "ABCDE12345", "1");
            Reject("com.luna.*", "ABCDE12345", "1");
            Reject("com.lunatest.labyrinth", "", "1");
            Reject("com.lunatest.labyrinth", "ABCDE12345", "0");
            Reject("com.lunatest.labyrinth", "ABCDE12345", "-1");
            Reject("com.lunatest.labyrinth", "ABCDE12345", "not-a-number");
            LunaValidation.Validate();
            File.WriteAllText("Library/LunaIOSSettingsTests.json", "{\"passed\":true,\"checks\":25,\"xcodeExportTested\":false}");
            Debug.Log("Luna iOS settings tests passed: 25 checks; Xcode export not performed.");
        }

        private static void Reject(string bundle, string team, string build)
        {
            try { LunaIOSBuild.RequireIdentity(bundle, team, build); }
            catch (ArgumentException) { return; }
            throw new Exception("Unsafe identity accepted");
        }

        private static void Check(bool success, string name)
        {
            if (!success) throw new Exception("iOS settings check failed: " + name);
        }
    }
}
