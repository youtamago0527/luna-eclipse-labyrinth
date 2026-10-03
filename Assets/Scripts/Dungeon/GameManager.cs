using System;
using System.Collections;
using UnityEngine;
namespace LunaEclipse.Dungeon
{
 public sealed class GameManager : MonoBehaviour
 {
  public DungeonRun Run {get;private set;}
  public bool Busy => turns != null && turns.Busy;
  public bool Modal {get;private set;}
  public DungeonRenderer Renderer {get;private set;}
  public DungeonUI UI {get;private set;}
  TurnManager turns; Action returnToHub; Func<DungeonRun,bool> completeRun; RunModifiers modifiers; int startFloor;
  AudioSource sound; AudioClip attackSound,hitSound,pickupSound;
  Vector2Int heldDirection,keyboardDirection; float nextHeldMove,nextKeyboardMove; bool keyboardFirst;
  bool resting;
  public void BeginRestHold(){ClearHeldInput();if(!Modal&&Run!=null&&Run.CanRest)resting=true;}
  public void EndRestHold(){resting=false;}
  public void BeginMoveHold(Vector2Int direction){if(Modal||Run==null||Run.Dead)return;resting=false;heldDirection=direction;nextHeldMove=Time.unscaledTime+DungeonRules.HoldDelay;Move(direction);}
  public void EndMoveHold(Vector2Int direction){if(heldDirection==direction)heldDirection=Vector2Int.zero;}
  void ClearHeldInput(){heldDirection=keyboardDirection=Vector2Int.zero;resting=false;}
  void OnApplicationFocus(bool focused){if(!focused)ClearHeldInput();}
  void OnApplicationPause(bool paused){if(paused)ClearHeldInput();}
  void OnDisable(){ClearHeldInput();}
  public void Initialise(RectTransform parent,Action onReturn,int? seed=null,RunModifiers runModifiers=null,int initialFloor=1,Func<DungeonRun,bool> onCompleted=null,string runId=null,System.Collections.Generic.IEnumerable<ItemData> initialItems=null)
  {
   returnToHub=onReturn;completeRun=onCompleted;modifiers=runModifiers??new RunModifiers();startFloor=initialFloor;Run=new DungeonRun(seed??Environment.TickCount,modifiers,startFloor,runId,initialItems);
   turns=gameObject.AddComponent<TurnManager>();
   UI=gameObject.AddComponent<DungeonUI>();UI.Build(parent,this);
   Renderer=gameObject.AddComponent<DungeonRenderer>();Renderer.Initialise(Run,UI.Viewport);
   sound=gameObject.AddComponent<AudioSource>();attackSound=Resources.Load<AudioClip>("Audio/attack");hitSound=Resources.Load<AudioClip>("Audio/damage");pickupSound=Resources.Load<AudioClip>("Audio/treasure");
   UI.Refresh(Run,false);
  }
  void Update()
  {
   if(Run==null||Modal||Run.Dead){ClearHeldInput();return;}
   int x=(Input.GetKey(KeyCode.D)||Input.GetKey(KeyCode.RightArrow)?1:0)-(Input.GetKey(KeyCode.A)||Input.GetKey(KeyCode.LeftArrow)?1:0);
   int y=(Input.GetKey(KeyCode.W)||Input.GetKey(KeyCode.UpArrow)?1:0)-(Input.GetKey(KeyCode.S)||Input.GetKey(KeyCode.DownArrow)?1:0);
   Vector2Int key=new Vector2Int(x,y);
   if(!Application.isFocused)key=Vector2Int.zero;
   if(key!=keyboardDirection){keyboardDirection=key;nextKeyboardMove=Time.unscaledTime;keyboardFirst=true;}
   if(Busy)return;
   if(!Run.CanRest)resting=false;
   if(resting&&key==Vector2Int.zero){Act(()=>Run.Rest());return;}
   if(heldDirection!=Vector2Int.zero&&Time.unscaledTime>=nextHeldMove){nextHeldMove=Time.unscaledTime+DungeonRules.HoldRepeat;Move(heldDirection);}
   else if(key!=Vector2Int.zero&&Time.unscaledTime>=nextKeyboardMove){nextKeyboardMove=Time.unscaledTime+(keyboardFirst?DungeonRules.HoldDelay:DungeonRules.HoldRepeat);keyboardFirst=false;Move(key);}
   else if(Input.GetKeyDown(KeyCode.Space)||Input.GetKeyDown(KeyCode.J))Attack();
   else if(Input.GetKeyDown(KeyCode.Period))Wait();
   else if(Input.GetKeyDown(KeyCode.E))PickUp();
  }
  public void Move(Vector2Int direction){resting=false;Act(()=>Run.Move(direction));}
  public void Attack(){resting=false;Act(()=>Run.Attack());}
  public void Wait(){resting=false;Act(()=>Run.Wait());}
  public void PickUp()=>Act(()=>Run.PickUp());
  public void UseItem(string id)=>Act(()=>Run.UseItem(id));
  public void DropItem(string id)=>Act(()=>Run.DropItem(id));
  public void Descend()=>Act(()=>Run.Descend());
  void Act(Func<bool> action)
  {
   if(Busy||Modal||Run==null||Run.Dead)return;
   int oldHp=Run.Hp,oldFloor=Run.Floor,oldPickup=Run.PickupSequence;bool accepted=action();
   if(!accepted){Renderer.Refresh(Run);UI.Refresh(Run,false);return;}
   turns.Begin(PlayTurn(oldHp,oldFloor,Run.PickupSequence!=oldPickup));
  }
  IEnumerator PlayTurn(int oldHp,int oldFloor,bool autoPickup)
  {
   UI.Refresh(Run,true);
   if(Run.LastAction=="attack")Play(attackSound);
   if(Run.LastAction=="pickup"||autoPickup)Play(pickupSound);
   if(Run.Floor!=oldFloor){Renderer.Refresh(Run);yield return null;}
   else yield return Renderer.Animate(Run,.14f);
   if(Run.Hp<oldHp)Play(hitSound);
   Renderer.Refresh(Run);UI.Refresh(Run,false);
  }
  void Play(AudioClip clip){if(clip!=null)sound.PlayOneShot(clip,LocalSettings.SeVolume);}
  public void SetModal(bool value){Modal=value;if(value)ClearHeldInput();}
  public void Restart(){if(Busy)return;if(completeRun!=null&&!completeRun(Run)){UI.ShowSaveFailure(Restart);return;}Modal=false;Run=new DungeonRun(Environment.TickCount,modifiers,startFloor);Renderer.Refresh(Run);UI.Refresh(Run,false);}
  public void ReturnToHub(){if(Busy)return;if(completeRun!=null&&!completeRun(Run)){UI.ShowSaveFailure(ReturnToHub);return;}returnToHub?.Invoke();}
 }
}
