using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using LunaEclipse.Dungeon;

namespace LunaEclipse.EditorTools
{
    [InitializeOnLoad]
    public static class DungeonVisualReview
    {
        static int captureFrames;
        static DungeonVisualReview() { EditorApplication.update += CaptureWhenReady; }
        // Explicit command-line entry; never captures or enters Play on ordinary project opens.
        public static void OpenPortrait()
        {
            EditorSceneManager.OpenScene(LunaProjectTools.DungeonScenePath);
            SessionState.SetBool("Luna.PortraitCaptureRequested",true);
            var gameView=EditorWindow.GetWindow(typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.GameView"));
            gameView.Focus();
            EditorApplication.isPlaying=true;
        }
        static void CaptureWhenReady()
        {
            if(!SessionState.GetBool("Luna.PortraitCaptureRequested",false)||!EditorApplication.isPlaying)return;
            var game=UnityEngine.Object.FindFirstObjectByType<GameManager>();
            if(game==null||game.Run==null||game.Busy)return;
            if(++captureFrames<90)return;
            SessionState.SetBool("Luna.PortraitCaptureRequested",false);
            Capture();
            File.WriteAllText("Screenshots/game-view-metrics.txt",$"Unity Editor Game View: {Screen.width}x{Screen.height}\nTile width: {Screen.width/9f:F2}px\nLuna idle body target: 0.85 tiles\nViewport design: 1170x1890\n");
        }
        public static void ValidateAndBuild()
        {
            DungeonCoreTests.Run();
            RelicTests.Run();
            InventoryVaultTests.Run();
            GameplayContentTests.Run();
            ProgressionIntegrationTests.Run();
            VaultLoadoutTests.Run();
            VaultCorruptionTests.Run();
            var sprite=Resources.Load<Sprite>("Luna/idle/down");
            var metrics=$"Imported sprite: rect={sprite.rect}, texture={sprite.texture.width}x{sprite.texture.height}, PPU={sprite.pixelsPerUnit}, bounds={sprite.bounds.size}.\n";
            metrics+=$"Body before normalisation={sprite.bounds.size.y*DungeonPresentation.LunaBodyCanvasRatio:F3} cells, target={DungeonPresentation.LunaHeight:F3}.\n";
            metrics+=$"Viewport: {DungeonPresentation.Width}x{DungeonPresentation.ViewHeight}; nine tiles wide; {DungeonPresentation.ViewHeight/DungeonPresentation.Width*9:F3} tiles tall.\n";
            File.WriteAllText("Library/DungeonVisualMetrics.txt",metrics);
            LunaProjectTools.BuildWindows();
        }

        [MenuItem("Luna/Capture Portrait Game View %&g")]
        public static void Capture()
        {
            if(!EditorApplication.isPlaying) { Debug.LogWarning("Enter Play Mode before capturing Game View.");return; }
            Directory.CreateDirectory("Screenshots");
            ScreenCapture.CaptureScreenshot(Path.GetFullPath("Screenshots/dungeon-game-view.png"));
        }
    }
}
