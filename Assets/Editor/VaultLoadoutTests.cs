using System;
using System.Collections.Generic;
using UnityEditor;
using LunaEclipse.Progression;
namespace LunaEclipse.EditorTools
{
    public static class VaultLoadoutTests
    {
        sealed class Store:IRelicStore
        {public Dictionary<string,string> Data=new Dictionary<string,string>();public bool Fail;public string Read(string k)=>Data.ContainsKey(k)?Data[k]:"";public void Write(string k,string v){if(Fail)throw new Exception("quota");Data[k]=v;}public void Flush(){if(Fail)throw new Exception("quota");}}
        static void Check(bool value,string text){if(!value)throw new Exception(text);}
        [MenuItem("Luna/Test Vault Loadout")]
        public static void Run()
        {
            var store=new Store();var v=new InventoryVault(store);var sword=new StoredItem{Id="first:sword",Name="剣",Kind="dagger",Enhancement=2};
            Check(v.DepositRun("first",new List<StoredItem>{sword},20),"initial deposit");Check(v.SelectForNextRun(0,20),"select");Check(v.Profile.Warehouse.Count==0,"removed selected from warehouse");
            store.Fail=true;Check(!v.BeginExpedition("second",20)&&v.Profile.Loadout.Count==1,"start failure retained");store.Fail=false;
            Check(v.BeginExpedition("second",20),"begin");Check(!v.BeginExpedition("other",20),"active conflict");
            var items=v.GetExpeditionItems("second");Check(items[0].Enhancement==2&&items[0].Id==sword.Id,"identity");items[0].Name="mutated";Check(v.Profile.InExpedition[0].Name=="剣","copy");
            items=v.GetExpeditionItems("second");items.Add(new StoredItem{Id="second:new",Name="薬",Kind="herb"});
            store.Fail=true;Check(!v.DepositRun("second",items,1)&&v.Profile.InExpedition.Count==1,"return failure escrow");store.Fail=false;
            Check(v.DepositRun("second",items,1),"return");Check(v.Profile.Warehouse.Count==1&&v.Profile.Pending.Count==1&&v.Profile.InExpedition.Count==0,"return exactly once");Check(!v.DepositRun("second",items,1),"duplicate return");
            Check(v.SelectForNextRun(0,1)&&v.BeginExpedition("interrupted",1),"next run");v=new InventoryVault(store);Check(v.RecoverInterrupted(0),"recover");Check(v.Profile.Pending.Count==2&&v.Profile.InExpedition.Count==0,"recover overflow safe");Check(v.RecoverInterrupted(0)&&v.Profile.Pending.Count==2,"recovery idempotency");Check(!v.DepositRun("interrupted",new List<StoredItem>{sword},20),"stale return rejected");
            var oldStore=new Store();oldStore.Data[InventoryVault.SaveKey]="{\"Version\":1,\"Warehouse\":[],\"Pending\":[],\"DepositedRuns\":[],\"LastReturned\":[]}";var old=new InventoryVault(oldStore);Check(old.Profile.Loadout!=null&&old.Profile.InExpedition!=null,"legacy migration");
            UnityEngine.Debug.Log("VaultLoadoutTests passed: selection, start failure, escrow conflict, identity, copy, return failure, return idempotency, interrupted recovery, legacy migration.");
        }
    }
}
