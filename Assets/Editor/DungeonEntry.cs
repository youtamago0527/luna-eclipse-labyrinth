using UnityEditor;
using UnityEditor.SceneManagement;
namespace LunaEclipse.EditorTools {
 public static class DungeonEntry {
  [MenuItem("Luna/Play Dungeon")]
  public static void Play(){if(!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())return;EditorSceneManager.OpenScene(LunaProjectTools.DungeonScenePath);EditorApplication.isPlaying=true;}
 }
}
