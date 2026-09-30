using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using LunaEclipse.Progression;
namespace LunaEclipse.EditorTools
{
    public static class VaultCorruptionTests
    {
        sealed class Store:IRelicStore{public Dictionary<string,string> Data=new Dictionary<string,string>();public string Read(string k)=>Data.ContainsKey(k)?Data[k]:"";public void Write(string k,string v)=>Data[k]=v;public void Flush(){}}
        static void Check(bool pass,string message){if(!pass)throw new Exception(message);}
        static StoredItem Item()=>new StoredItem{Id="sword",Name="剣",Kind="dagger",Enhancement=2};
        static Store Corrupt(InventoryProfile broken)
        {
            var store=new Store();var valid=new InventoryProfile();valid.Warehouse.Add(Item());valid.LastReturned.Add(Item());
            store.Data[InventoryVault.SaveKey+".backup"]=JsonUtility.ToJson(valid);store.Data[InventoryVault.SaveKey]=JsonUtility.ToJson(broken);return store;
        }
        [MenuItem("Luna/Test Vault Corruption")]
        public static void Run()
        {
            var orphan=new InventoryProfile();orphan.InExpedition.Add(Item());var store=Corrupt(orphan);string raw=store.Read(InventoryVault.SaveKey);var vault=new InventoryVault(store);
            Check(vault.Profile.Warehouse.Count==1&&vault.Profile.InExpedition.Count==0,"orphan fallback");Check(vault.SelectForNextRun(0,20),"save recovery");Check(store.Read(InventoryVault.SaveKey+".corrupt")==raw,"corrupt source retained");
            var completed=new InventoryProfile{ActiveRunId="done"};completed.DepositedRuns.Add("done");completed.InExpedition.Add(Item());Check(new InventoryVault(Corrupt(completed)).Profile.Warehouse.Count==1,"completed active rejected");
            var duplicate=new InventoryProfile();duplicate.Warehouse.Add(Item());duplicate.Loadout.Add(Item());Check(new InventoryVault(Corrupt(duplicate)).Profile.Loadout.Count==0,"duplicate ownership rejected");
            var legacy=new Store();legacy.Data[InventoryVault.SaveKey]="{\"Version\":1,\"Warehouse\":[{\"Id\":\"old\"}],\"Pending\":[],\"LastReturned\":[],\"DepositedRuns\":[]}";
            var migrated=new InventoryVault(legacy);Check(migrated.Profile.Loadout.Count==0&&migrated.Profile.InExpedition.Count==0,"old fields migrate");Check(migrated.Profile.Warehouse[0].Kind=="unknown","null kind normalized");
            var history=new InventoryProfile();history.Warehouse.Add(Item());history.LastReturned.Add(Item());var historyStore=new Store();historyStore.Data[InventoryVault.SaveKey]=JsonUtility.ToJson(history);Check(new InventoryVault(historyStore).Profile.Warehouse.Count==1,"history duplicate permitted");
            Debug.Log("VaultCorruptionTests passed: orphan escrow, completed escrow, duplicate owner IDs, backup, corrupt preservation, legacy/null kind, history copies.");
        }
    }
}
