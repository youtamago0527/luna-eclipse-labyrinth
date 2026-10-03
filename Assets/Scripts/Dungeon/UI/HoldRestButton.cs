using UnityEngine;
using UnityEngine.EventSystems;
namespace LunaEclipse.Dungeon
{
 public sealed class HoldRestButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
 {
  public GameManager Manager;
  int? pointer;
  public void OnPointerDown(PointerEventData e){if(pointer.HasValue||e.button!=PointerEventData.InputButton.Left)return;pointer=e.pointerId;Manager.BeginRestHold();}
  public void OnPointerUp(PointerEventData e){if(pointer==e.pointerId)Release();}
  public void OnPointerExit(PointerEventData e){if(pointer==e.pointerId)Release();}
  void Release(){pointer=null;if(Manager!=null)Manager.EndRestHold();}
  void OnDisable(){Release();}
 }
}
