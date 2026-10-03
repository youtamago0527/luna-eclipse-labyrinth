using UnityEngine;
using UnityEngine.EventSystems;
namespace LunaEclipse.Dungeon
{
    public sealed class HoldMoveButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        public GameManager Manager;
        public Vector2Int Direction;
        int? pointer;
        public void OnPointerDown(PointerEventData e)
        {
            if(pointer.HasValue||e.button!=PointerEventData.InputButton.Left)return;
            pointer=e.pointerId;Manager.BeginMoveHold(Direction);
        }
        public void OnPointerUp(PointerEventData e){if(pointer==e.pointerId)Release();}
        public void OnPointerExit(PointerEventData e){if(pointer==e.pointerId)Release();}
        void Release(){pointer=null;if(Manager!=null)Manager.EndMoveHold(Direction);}
        void OnDisable(){Release();}
    }
}
