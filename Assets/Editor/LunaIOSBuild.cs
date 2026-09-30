using System;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.Rendering;

namespace LunaEclipse.EditorTools
{
    // No signing credentials or account-specific identifier is stored in this script.
    public static class LunaIOSBuild
    {
        public const string RequiredVersion = "6000.6.3f1";
        public static string[] Scenes => new[]
        {
            LunaProjectTools.StartupScenePath, LunaProjectTools.DungeonScenePath
        };

        [MenuItem("Luna/iOS/1 Prepare Export Settings")]
        public static void Prepare()
        {
            RequireVersion();
            foreach (string scene in Scenes)
                if (!File.Exists(scene)) throw new FileNotFoundException("Required scene missing", scene);

            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
            PlayerSettings.allowedAutorotateToPortrait = true;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.allowedAutorotateToLandscapeLeft = false;
            PlayerSettings.allowedAutorotateToLandscapeRight = false;
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.iOS, ScriptingImplementation.IL2CPP);
            PlayerSettings.SetArchitecture(NamedBuildTarget.iOS, 1); // ARM64
            PlayerSettings.SetManagedStrippingLevel(NamedBuildTarget.iOS, ManagedStrippingLevel.Minimal);
            PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.iOS, false);
            PlayerSettings.SetGraphicsAPIs(BuildTarget.iOS, new[] { GraphicsDeviceType.Metal });
            PlayerSettings.iOS.sdkVersion = iOSSdkVersion.DeviceSDK;
            PlayerSettings.iOS.targetOSVersionString = "15.0";
            // iOS.applicationDisplayName aliases productName in this Unity version.
            // Preserve it: changing the shared product name can change desktop save paths.
            // Keep device family, identifier, version, build number and signing decisions intact.
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(Scenes[0], true),
                new EditorBuildSettingsScene(Scenes[1], true)
            };
            AssetDatabase.SaveAssets();
            Debug.Log("Luna iOS settings prepared: portrait, Device SDK, IL2CPP/ARM64, Metal, iOS 15+, Startup first. Signing/export not performed.");
        }

        // Optional CI/local shell inputs. Validate all inputs before changing any identity fields.
        public static void ConfigureFromEnvironment()
        {
            RequireVersion();
            string bundle = Environment.GetEnvironmentVariable("LUNA_IOS_BUNDLE_ID");
            string team = Environment.GetEnvironmentVariable("LUNA_APPLE_TEAM_ID");
            string build = Environment.GetEnvironmentVariable("LUNA_IOS_BUILD_NUMBER");
            RequireIdentity(bundle, team, build);
            Prepare();
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.iOS, bundle);
            PlayerSettings.iOS.appleDeveloperTeamID = team;
            PlayerSettings.iOS.appleEnableAutomaticSigning = true;
            PlayerSettings.iOS.buildNumber = build;
            AssetDatabase.SaveAssets();
        }

        [MenuItem("Luna/iOS/2 Validate Export Readiness")]
        public static void ValidateExportReadiness()
        {
            RequireVersion();
            RequireIdentity(PlayerSettings.GetApplicationIdentifier(NamedBuildTarget.iOS),
                PlayerSettings.iOS.appleDeveloperTeamID, PlayerSettings.iOS.buildNumber);
            if (!PlayerSettings.iOS.appleEnableAutomaticSigning)
                throw new InvalidOperationException("Enable Automatic Signing for the confirmed Luna team, or use a separately reviewed manual-signing workflow.");
            if (!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.iOS, BuildTarget.iOS))
                throw new InvalidOperationException("Install iOS Build Support for Unity " + RequiredVersion + " through Unity Hub, then reopen Unity.");
            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.iOS)
                throw new InvalidOperationException("Switch the active platform to iOS and wait for import. Batch mode: start Unity with -buildTarget iOS.");
            LunaValidation.Validate();
            Debug.Log("Luna iOS export prerequisites passed. Xcode signing and TestFlight upload are not verified by this check.");
        }

        [MenuItem("Luna/iOS/3 Export Xcode Project")]
        public static void Export()
        {
            // Fail before modifying settings or output if the machine/identity is not ready.
            ValidateExportReadiness();
            Prepare();
            string output = Path.GetFullPath(Path.Combine("Builds", "iOS",
                "Luna-Xcode-" + DateTime.UtcNow.ToString("yyyyMMdd-HHmmss-fff")));
            if (Directory.Exists(output)) throw new IOException("Output already exists: " + output);
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            BuildReport report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = Scenes,
                locationPathName = output,
                target = BuildTarget.iOS,
                options = BuildOptions.None // No Development, auto-run, archive or upload.
            });
            if (report.summary.result != BuildResult.Succeeded)
                throw new InvalidOperationException("iOS export failed: " + report.summary.result
                    + "; errors: " + report.summary.totalErrors);
            Debug.Log("Luna Xcode export succeeded: " + output + ". Open Unity-iPhone.xcodeproj on Mac. Archive/upload still required.");
        }

        public static void ConfigureAndExport()
        {
            ConfigureFromEnvironment();
            Export();
        }

        internal static void RequireIdentity(string bundle, string team, string build)
        {
            if (string.IsNullOrWhiteSpace(bundle)
                || !Regex.IsMatch(bundle, @"\A[A-Za-z0-9-]+(\.[A-Za-z0-9-]+){2,}\z")
                || bundle.IndexOf("example", StringComparison.OrdinalIgnoreCase) >= 0
                || bundle.IndexOf("defaultcompany", StringComparison.OrdinalIgnoreCase) >= 0
                || bundle.IndexOf("arsene", StringComparison.OrdinalIgnoreCase) >= 0)
                throw new ArgumentException("Set a confirmed Luna-specific Bundle ID; do not reuse ARSENE or a placeholder.");
            if (string.IsNullOrEmpty(team) || !Regex.IsMatch(team, @"\A[A-Z0-9]{10}\z"))
                throw new ArgumentException("Set the confirmed 10-character Apple Team ID.");
            if (!int.TryParse(build, out int number) || number <= 0 || number > 999999999)
                throw new ArgumentException("Set a positive integer iOS build number after checking App Store Connect history.");
        }

        private static void RequireVersion()
        {
            if (Application.unityVersion != RequiredVersion)
                throw new InvalidOperationException("Use Unity " + RequiredVersion + "; current: " + Application.unityVersion);
        }
    }
}
