using System.Collections;
using UnityEngine;
namespace LunaEclipse.Dungeon
{
 public sealed class TurnManager : MonoBehaviour
 {
  public bool Busy {get;private set;}
  public bool Begin(IEnumerator action){if(Busy||action==null)return false;Busy=true;StartCoroutine(Execute(action));return true;}
  IEnumerator Execute(IEnumerator action){try{yield return action;}finally{Busy=false;}}
  void OnDisable(){StopAllCoroutines();Busy=false;}
 }
}
