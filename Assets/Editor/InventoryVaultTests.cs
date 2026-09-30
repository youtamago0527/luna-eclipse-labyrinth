using System;
using System.Collections.Generic;
using UnityEditor;
using LunaEclipse.Progression;
namespace LunaEclipse.EditorTools
{
    public static class InventoryVaultTests
    {
        sealed class Store:IRelicStore
        {public Dictionary<string,string> Values=new Dictionary<string,string>();public bool Fail;public string Read(string k)=>Values.ContainsKey(k)?Values[k]:"";public void Write(string k,string v){if(Fail)throw new Exception("full");Values[k]=v;}public void Flush(){if(Fail)throw new Exception("full");}}
        static void Check(bool valid,string message){if(!valid)throw new Exception(message);}
        [MenuItem("Luna/Test Inventory Vault")]
        public static void Run()
        {
            var store=new Store();var vault=new InventoryVault(store);var items=new List<StoredItem>{new StoredItem{Id="a",Name="剣",Kind="weapon",Enhancement=-2},new StoredItem{Id="b",Name="薬",Kind="herb"}};
            Check(vault.DepositRun("run",items,1),"deposit run");Check(vault.Profile.Warehouse.Count==1&&vault.Profile.Pending.Count==1,"overflow retained");items[0].Name="changed";Check(vault.Profile.Warehouse[0].Name=="剣","copy ownership");
            Check(!vault.DepositRun("run",items,1),"run idempotency");Check(!vault.Deposit(0,1),"full");Check(vault.Swap(0,0),"swap full");
            Check(vault.Withdraw(0),"withdraw");Check(vault.Profile.Pending.Count==2,"withdraw retained");Check(vault.Deposit(1,1),"redeposit");
            vault=new InventoryVault(store);Check(vault.Profile.Warehouse.Count==1&&vault.Profile.Pending.Count==1,"reload");Check(vault.Profile.LastReturned[0].Enhancement==-2,"enhancement/history");
            store.Fail=true;Check(!vault.Withdraw(0)&&vault.Profile.Warehouse.Count==1,"rollback");store.Fail=false;
            store.Values[InventoryVault.SaveKey]="corrupt";vault=new InventoryVault(store);Check(vault.Profile.DepositedRuns.Contains("run"),"backup");
            UnityEngine.Debug.Log("InventoryVaultTests passed: capacity, overflow, copy isolation, duplicate run, swap, persistence, enhancement, failure rollback, backup.");
        }
    }
}
