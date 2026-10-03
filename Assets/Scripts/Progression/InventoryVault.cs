using System;
using System.Collections.Generic;
using UnityEngine;
namespace LunaEclipse.Progression
{
    [Serializable] public sealed class StoredItem
    {
        public string Id,Name,Kind; public int Enhancement;
        public bool Identified;
        public StoredItem Copy()=>new StoredItem{Id=Id,Name=Name,Kind=Kind,Enhancement=Enhancement,Identified=Identified};
        public string DisplayName => new Dungeon.ItemData{Id=Id,Name=Name??Id,Kind=Kind,Enhancement=Enhancement,Identified=Identified}.DisplayName;
    }
    [Serializable] public sealed class InventoryProfile
    {
        public int Version=1; public List<StoredItem> Warehouse=new List<StoredItem>(),Pending=new List<StoredItem>();
        public List<string> DepositedRuns=new List<string>(); public string LastRunId; public List<StoredItem> LastReturned=new List<StoredItem>();
        public List<StoredItem> Loadout=new List<StoredItem>(),InExpedition=new List<StoredItem>(); public string ActiveRunId;
    }
    public sealed class InventoryVault
    {
        public const string SaveKey="luna.unity.inventory.v1";
        readonly IRelicStore store;
        public InventoryProfile Profile {get;private set;}
        public string LastError {get;private set;}="";
        public InventoryVault(IRelicStore storage=null)
        {
            store=storage??new PlayerPrefsRelicStore();
            try{var raw=store.Read(SaveKey);Profile=Parse(raw);if(Profile==null){Profile=Parse(store.Read(SaveKey+".backup"));if(!string.IsNullOrEmpty(raw))LastError=Profile==null?"保存を読み込めません。元データは保持しています。":"バックアップから復元しました。";}}
            catch{LastError="保存領域にアクセスできません。";}
            Profile=Profile??new InventoryProfile();
        }
        static bool Valid(List<StoredItem> items)=>items!=null&&!items.Exists(i=>i==null||string.IsNullOrEmpty(i.Id));
        static InventoryProfile Parse(string raw)
        {
            if(string.IsNullOrWhiteSpace(raw)||!raw.TrimStart().StartsWith("{"))return null;
            try{
                var p=JsonUtility.FromJson<InventoryProfile>(raw);
                if(p==null||p.Version!=1||!Valid(p.Warehouse)||!Valid(p.Pending)||!Valid(p.LastReturned)||p.DepositedRuns==null)return null;
                p.Loadout=p.Loadout??new List<StoredItem>();p.InExpedition=p.InExpedition??new List<StoredItem>();
                if(!Valid(p.Loadout)||!Valid(p.InExpedition))return null;
                var runIds=new HashSet<string>();foreach(var id in p.DepositedRuns)if(string.IsNullOrWhiteSpace(id)||!runIds.Add(id))return null;
                if(p.InExpedition.Count>0&&string.IsNullOrWhiteSpace(p.ActiveRunId))return null;
                if(!string.IsNullOrEmpty(p.ActiveRunId)&&(string.IsNullOrWhiteSpace(p.ActiveRunId)||runIds.Contains(p.ActiveRunId)))return null;
                var ownedIds=new HashSet<string>();
                foreach(var list in new[]{p.Warehouse,p.Pending,p.Loadout,p.InExpedition})foreach(var item in list)
                {
                    if(!ownedIds.Add(item.Id))return null;
                    // Old saves may omit display fields; an unknown kind stays an inert item.
                    item.Kind=item.Kind??"unknown";item.Name=item.Name??item.Id;
                }
                foreach(var item in p.LastReturned){item.Kind=item.Kind??"unknown";item.Name=item.Name??item.Id;}
                return p;
            }catch{return null;}
        }
        static InventoryProfile Copy(InventoryProfile value)=>JsonUtility.FromJson<InventoryProfile>(JsonUtility.ToJson(value));
        bool Commit(InventoryProfile next)
        {
            string old=null;
            try{old=store.Read(SaveKey);if(Parse(old)!=null)store.Write(SaveKey+".backup",old);else if(!string.IsNullOrEmpty(old))store.Write(SaveKey+".corrupt",old);store.Write(SaveKey,JsonUtility.ToJson(next));store.Flush();Profile=next;LastError="";return true;}
            catch{try{if(old!=null){store.Write(SaveKey,old);store.Flush();}}catch{}LastError="保存できませんでした。品物は移動していません。";return false;}
        }
        public bool DepositRun(string runId,List<StoredItem> items,int capacity)
        {
            if(string.IsNullOrWhiteSpace(runId)||!Valid(items)||capacity<0)return false;
            if(Profile.DepositedRuns.Contains(runId)){LastError="この探索の持ち帰りは登録済みです。";return false;}
            if(!string.IsNullOrEmpty(Profile.ActiveRunId)&&Profile.ActiveRunId!=runId){LastError="別の探索の持込品を確認してください。";return false;}
            var ids=new HashSet<string>();foreach(var item in items)if(!ids.Add(item.Id)||Profile.Warehouse.Exists(x=>x.Id==item.Id)||Profile.Pending.Exists(x=>x.Id==item.Id)||Profile.Loadout.Exists(x=>x.Id==item.Id)){LastError="品物IDが重複しています。";return false;}
            var next=Copy(Profile);next.LastReturned.Clear();next.LastRunId=runId;
            foreach(var item in items){var target=next.Warehouse.Count<capacity?next.Warehouse:next.Pending;target.Add(item.Copy());next.LastReturned.Add(item.Copy());}
            next.DepositedRuns.Add(runId);if(next.ActiveRunId==runId){next.InExpedition.Clear();next.ActiveRunId=null;}return Commit(next);
        }
        public bool SelectForNextRun(int warehouseIndex,int bagCapacity)
        {
            if(warehouseIndex<0||warehouseIndex>=Profile.Warehouse.Count||Profile.Loadout.Count>=Math.Min(40,bagCapacity)||!string.IsNullOrEmpty(Profile.ActiveRunId))return false;
            var next=Copy(Profile);next.Loadout.Add(next.Warehouse[warehouseIndex]);next.Warehouse.RemoveAt(warehouseIndex);return Commit(next);
        }
        public bool Unselect(int index,int warehouseCapacity)
        {
            if(index<0||index>=Profile.Loadout.Count||warehouseCapacity<0)return false;
            var next=Copy(Profile);(next.Warehouse.Count<warehouseCapacity?next.Warehouse:next.Pending).Add(next.Loadout[index]);next.Loadout.RemoveAt(index);return Commit(next);
        }
        public bool BeginExpedition(string runId,int bagCapacity)
        {
            if(string.IsNullOrWhiteSpace(runId)||Profile.DepositedRuns.Contains(runId)||bagCapacity<0||Profile.Loadout.Count>Math.Min(40,bagCapacity)){LastError="出発条件を確認してください。";return false;}
            if(!string.IsNullOrEmpty(Profile.ActiveRunId)){LastError="未確定の探索があります。先に持込品を復元してください。";return false;}
            var next=Copy(Profile);next.ActiveRunId=runId;next.InExpedition=next.Loadout;next.Loadout=new List<StoredItem>();return Commit(next);
        }
        public List<StoredItem> GetExpeditionItems(string runId)
        {return Profile.ActiveRunId==runId?Profile.InExpedition.ConvertAll(item=>item.Copy()):new List<StoredItem>();}
        public bool RecoverInterrupted(int warehouseCapacity)
        {
            if(string.IsNullOrEmpty(Profile.ActiveRunId))return true;if(warehouseCapacity<0)return false;
            var next=Copy(Profile);var ids=new HashSet<string>();foreach(var item in next.Warehouse)ids.Add(item.Id);foreach(var item in next.Pending)ids.Add(item.Id);foreach(var item in next.Loadout)ids.Add(item.Id);
            foreach(var item in next.InExpedition)if(ids.Add(item.Id))(next.Warehouse.Count<warehouseCapacity?next.Warehouse:next.Pending).Add(item);
            if(!next.DepositedRuns.Contains(next.ActiveRunId))next.DepositedRuns.Add(next.ActiveRunId);
            next.ActiveRunId=null;next.InExpedition.Clear();return Commit(next);
        }
        // Withdraw keeps the object in the persistent pending tray, not outside the save.
        public bool Withdraw(int warehouseIndex)
        {
            if(warehouseIndex<0||warehouseIndex>=Profile.Warehouse.Count)return false;
            var next=Copy(Profile);next.Pending.Add(next.Warehouse[warehouseIndex]);next.Warehouse.RemoveAt(warehouseIndex);return Commit(next);
        }
        public bool Deposit(int pendingIndex,int capacity)
        {
            if(pendingIndex<0||pendingIndex>=Profile.Pending.Count||capacity<0||Profile.Warehouse.Count>=capacity)return false;
            var next=Copy(Profile);next.Warehouse.Add(next.Pending[pendingIndex]);next.Pending.RemoveAt(pendingIndex);return Commit(next);
        }
        public bool Swap(int warehouseIndex,int pendingIndex)
        {
            if(warehouseIndex<0||warehouseIndex>=Profile.Warehouse.Count||pendingIndex<0||pendingIndex>=Profile.Pending.Count)return false;
            var next=Copy(Profile);var item=next.Warehouse[warehouseIndex];next.Warehouse[warehouseIndex]=next.Pending[pendingIndex];next.Pending[pendingIndex]=item;return Commit(next);
        }
    }
}
