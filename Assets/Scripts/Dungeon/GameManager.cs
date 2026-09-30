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
   if(Run==null||Busy||Modal||Run.Dead)return;
   if(Input.GetKeyDown(KeyCode.W)||Input.GetKeyDown(KeyCode.UpArrow))Move(Vector2Int.up);
   else if(Input.GetKeyDown(KeyCode.S)||Input.GetKeyDown(KeyCode.DownArrow))Move(Vector2Int.down);
   else if(Input.GetKeyDown(KeyCode.A)||Input.GetKeyDown(KeyCode.LeftArrow))Move(Vector2Int.left);
   else if(Input.GetKeyDown(KeyCode.D)||Input.GetKeyDown(KeyCode.RightArrow))Move(Vector2Int.right);
   else if(Input.GetKeyDown(KeyCode.Space)||Input.GetKeyDown(KeyCode.J))Attack();
   else if(Input.GetKeyDown(KeyCode.Period))Wait();
   else if(Input.GetKeyDown(KeyCode.E))PickUp();
  }
  public void Move(Vector2Int direction)=>Act(()=>Run.Move(direction));
  public void Attack()=>Act(()=>Run.Attack());
  public void Wait()=>Act(()=>Run.Wait());
  public void PickUp()=>Act(()=>Run.PickUp());
  public void UseItem(string id)=>Act(()=>Run.UseItem(id));
  public void DropItem(string id)=>Act(()=>Run.DropItem(id));
  public void Descend()=>Act(()=>Run.Descend());
  void Act(Func<bool> action)
  {
   if(Busy||Modal||Run==null||Run.Dead)return;
   int oldHp=Run.Hp,oldFloor=Run.Floor;bool accepted=action();
   if(!accepted){Renderer.Refresh(Run);UI.Refresh(Run,false);return;}
   turns.Begin(PlayTurn(oldHp,oldFloor));
  }
  IEnumerator PlayTurn(int oldHp,int oldFloor)
  {
   UI.Refresh(Run,true);
   if(Run.LastAction=="attack")Play(attackSound);
   if(Run.LastAction=="pickup")Play(pickupSound);
   if(Run.Floor!=oldFloor){Renderer.Refresh(Run);yield return null;}
   else yield return Renderer.Animate(Run,.14f);
   if(Run.Hp<oldHp)Play(hitSound);
   Renderer.Refresh(Run);UI.Refresh(Run,false);
  }
  void Play(AudioClip clip){if(clip!=null)sound.PlayOneShot(clip,LocalSettings.SeVolume);}
  public void SetModal(bool value){Modal=value;}
  public void Restart(){if(Busy)return;if(completeRun!=null&&!completeRun(Run)){UI.ShowSaveFailure(Restart);return;}Modal=false;Run=new DungeonRun(Environment.TickCount,modifiers,startFloor);Renderer.Refresh(Run);UI.Refresh(Run,false);}
  public void ReturnToHub(){if(Busy)return;if(completeRun!=null&&!completeRun(Run)){UI.ShowSaveFailure(ReturnToHub);return;}returnToHub?.Invoke();}
 }
}
