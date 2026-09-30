using System;
using System.Collections.Generic;
using UnityEditor;
using LunaEclipse.Progression;
namespace LunaEclipse.EditorTools
{
    public static class ProgressionIntegrationTests
    {
        sealed class Store:IRelicStore
        {
            public readonly Dictionary<string,string> Values=new Dictionary<string,string>();public bool Fail;
            public string Read(string key)=>Values.ContainsKey(key)?Values[key]:"";
            public void Write(string key,string value){if(Fail)throw new Exception("simulated write failure");Values[key]=value;}
            public void Flush(){if(Fail)throw new Exception("simulated flush failure");}
        }
        static void Check(bool value,string message){if(!value)throw new Exception(message);}
        static bool Finish(RelicService relics,InventoryVault vault,string id,int floor,List<StoredItem> items)
        {
            if(relics.CompleteRun(id,floor)==null)return false;
            return vault.Profile.DepositedRuns.Contains(id)||vault.DepositRun(id,items,1);
        }
        [MenuItem("Luna/Test Progression Integration")]
        public static void Run()
        {
            var relicStore=new Store();var vaultStore=new Store();var relics=new RelicService(relicStore);var vault=new InventoryVault(vaultStore);
            var items=new List<StoredItem>{new StoredItem{Id="run:a",Name="剣",Kind="weapon",Enhancement=-2},new StoredItem{Id="run:b",Name="薬",Kind="herb"}};
            vaultStore.Fail=true;
            Check(!Finish(relics,vault,"run",20,items),"partial failure reported");
            Check(relics.Profile.Runs.Count==1&&vault.Profile.DepositedRuns.Count==0,"only first store committed");
            vaultStore.Fail=false;
            Check(Finish(relics,vault,"run",20,items),"frozen run retry");
            Check(Finish(relics,vault,"run",20,items),"idempotent completed retry");
            Check(relics.Profile.Runs.Count==1&&vault.Profile.DepositedRuns.Count==1,"no duplicate records");
            Check(vault.Profile.Warehouse.Count==1&&vault.Profile.Pending.Count==1,"all items retained exactly once");
            var reloadedRelics=new RelicService(relicStore);var reloadedVault=new InventoryVault(vaultStore);
            Check(reloadedRelics.Profile.Runs[0].MaxFloor==20&&reloadedVault.Profile.LastReturned.Count==2,"both stores persisted");
            relicStore.Fail=true;Check(!Finish(reloadedRelics,reloadedVault,"second",40,items),"first store failure aborts");
            Check(reloadedVault.Profile.DepositedRuns.Count==1,"second store untouched");
            UnityEngine.Debug.Log("ProgressionIntegrationTests passed: two-store partial failure, frozen retry, idempotency, no loss, reload, first-store abort.");
        }
    }
}
