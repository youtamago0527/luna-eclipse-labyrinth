using System;
using System.Collections.Generic;
using UnityEngine;
namespace LunaEclipse.Progression
{
    public interface IRelicStore { string Read(string key); void Write(string key,string value); void Flush(); }
    public sealed class PlayerPrefsRelicStore:IRelicStore
    { public string Read(string key)=>PlayerPrefs.GetString(key,""); public void Write(string key,string value)=>PlayerPrefs.SetString(key,value); public void Flush()=>PlayerPrefs.Save(); }
    public sealed class RelicService
    {
        public const string SaveKey="luna.unity.relics.v1";
        readonly IRelicStore store; public RelicBalance Balance {get;}
        public RelicProfile Profile {get;private set;} public string LastError {get;private set;}="";
        public RelicService(IRelicStore storage=null,RelicBalance balance=null)
        {store=storage??new PlayerPrefsRelicStore();Balance=balance??new RelicBalance();Load();}
        static RelicProfile Parse(string json)
        {
            if(string.IsNullOrWhiteSpace(json)||!json.TrimStart().StartsWith("{"))return null;
            try{var p=JsonUtility.FromJson<RelicProfile>(json);if(p==null||p.Version!=1||p.Levels==null||p.Runs==null)return null;
                p.HighestFloor=Math.Max(1,p.HighestFloor);var seen=new HashSet<string>();
                foreach(var l in p.Levels){if(l==null||string.IsNullOrEmpty(l.Id)||!seen.Add(l.Id))return null;var d=Find(l.Id);l.Level=Math.Max(0,l.Level);if(d!=null&&d.MaxLevel>0)l.Level=Math.Min(l.Level,d.MaxLevel);}
                var runs=new HashSet<string>();foreach(var r in p.Runs)if(r==null||string.IsNullOrEmpty(r.RunId)||!runs.Add(r.RunId)||r.Rewards==null||r.Rewards.Exists(v=>v==null||string.IsNullOrEmpty(v.Id)))return null;
                return p;}catch{return null;}
        }
        void Load(){try{string raw=store.Read(SaveKey);Profile=Parse(raw);if(Profile==null){Profile=Parse(store.Read(SaveKey+".backup"));if(!string.IsNullOrEmpty(raw))LastError=Profile!=null?"バックアップから復元しました。":"保存データを読み込めませんでした。元データは保持しています。";}Profile=Profile??new RelicProfile();}catch{Profile=new RelicProfile();LastError="保存領域にアクセスできません。";}}
        public bool Save()
        {
            string old=null;
            try{old=store.Read(SaveKey);if(Parse(old)!=null)store.Write(SaveKey+".backup",old);else if(!string.IsNullOrEmpty(old))store.Write(SaveKey+".corrupt",old);store.Write(SaveKey,JsonUtility.ToJson(Profile));store.Flush();LastError="";return true;}
            catch{try{if(old!=null){store.Write(SaveKey,old);store.Flush();}}catch{}LastError="保存できませんでした。変更は確定していません。";return false;}
        }
        public static RelicDefinition Find(string id)=>Array.Find(RelicBalance.Definitions,d=>d.Id==id);
        public int Level(string id)=>Profile.Levels.Find(l=>l.Id==id)?.Level??0;
        // Reserve arithmetic headroom for consumers adding their base values.
        static int SafeSum(int current,long addition)=>(int)Math.Max(0,Math.Min((long)int.MaxValue-1024,current+addition));
        public RelicBonus GetBonus()
        {
            var b=new RelicBonus();int start=1;
            foreach(var d in RelicBalance.Definitions){if(!d.Enabled)continue;long n=(long)Level(d.Id)*d.PerLevel;switch(d.Effect){case RelicEffect.EquipmentQuality:b.EquipmentEnhancement=SafeSum(b.EquipmentEnhancement,n);break;case RelicEffect.SatietyInterval:b.SatietyIntervalBonus=SafeSum(b.SatietyIntervalBonus,n);break;case RelicEffect.StartFloor:start=SafeSum(start,n);break;case RelicEffect.Warehouse:b.WarehouseBonus=SafeSum(b.WarehouseBonus,n);break;case RelicEffect.Bag:b.BagBonus=SafeSum(b.BagBonus,n);break;case RelicEffect.WeaponQuality:b.WeaponEnhancement=SafeSum(b.WeaponEnhancement,n);break;case RelicEffect.ArmorQuality:b.ArmorEnhancement=SafeSum(b.ArmorEnhancement,n);break;case RelicEffect.MissionChance:b.MissionChanceBonus+=n*.01f;break;}}
            b.MaxStartFloor=Math.Max(1,Math.Min(start,Profile.HighestFloor));return b;
        }
        public string RankForFloor(int floor){string rank="D";for(int i=0;i<Math.Min(Balance.Ranks.Length,Balance.RankFloors.Length);i++)if(floor>=Balance.RankFloors[i])rank=Balance.Ranks[i];return rank;}
        public RelicRunRecord CompleteRun(string runId,int maxFloor,int startFloor=1)
        {
            if(string.IsNullOrWhiteSpace(runId)||startFloor<1||maxFloor<startFloor)throw new ArgumentException("Invalid run report");
            var existing=Profile.Runs.Find(r=>r.RunId==runId);if(existing!=null)return existing;
            string before=JsonUtility.ToJson(Profile);var record=new RelicRunRecord{RunId=runId,MaxFloor=maxFloor,StartFloor=startFloor};
            int interval=Math.Max(1,Balance.RewardInterval);
            for(long floor=((long)startFloor/interval+1)*interval;floor<=maxFloor;floor+=interval)
            {string rank=Balance.EnableTrialRewardTable?RankForFloor((int)floor):null;var candidates=Array.FindAll(RelicBalance.Definitions,d=>d.Enabled&&RankIndex(d.Rank)<=RankIndex(rank));string item=null;if(Balance.EnableTrialRewardTable&&candidates.Length>0)item=candidates[StableHash(runId+":"+floor)%candidates.Length].Id;
                record.Rewards.Add(new RelicReward{Id=runId+":"+floor,Floor=(int)floor,Rank=rank,RelicId=item});}
            Profile.HighestFloor=Math.Max(Profile.HighestFloor,maxFloor);Profile.Runs.Add(record);
            if(!Save()){Profile=JsonUtility.FromJson<RelicProfile>(before);return null;}return record;
        }
        int RankIndex(string rank){int i=Array.IndexOf(Balance.Ranks,rank);return i<0?0:i;}
        static int StableHash(string value){uint h=2166136261;foreach(char c in value)h=(h^c)*16777619;return (int)(h&0x7fffffff);}
        public bool TryClaim(string rewardId)
        {
            RelicReward reward=null;foreach(var run in Profile.Runs){reward=run.Rewards.Find(r=>r.Id==rewardId);if(reward!=null)break;}
            if(reward==null||reward.Claimed)return false;var def=Find(reward.RelicId);if(def==null||!def.Enabled)return false;
            string before=JsonUtility.ToJson(Profile);var level=Profile.Levels.Find(l=>l.Id==def.Id);if(level==null){level=new RelicLevel{Id=def.Id};Profile.Levels.Add(level);}if(level.Level<int.MaxValue&&(def.MaxLevel<=0||level.Level<def.MaxLevel))level.Level++;reward.Claimed=true;
            if(Save())return true;Profile=JsonUtility.FromJson<RelicProfile>(before);return false;
        }
        // Only call with a designer-approved reward, never as an automatic drop table.
        public bool Grant(string grantId,string relicId)
        {
            if(string.IsNullOrWhiteSpace(grantId))return false;
            var def=Find(relicId);if(def==null||!def.Enabled)return false;
            string id="grant:"+grantId;if(Profile.Runs.Exists(r=>r.RunId==id))return false;
            string before=JsonUtility.ToJson(Profile);
            RelicReward pending=null;foreach(var run in Profile.Runs){pending=run.Rewards.Find(r=>r.Id==grantId);if(pending!=null)break;}
            if(pending!=null&&pending.Claimed)return false;
            var level=Profile.Levels.Find(l=>l.Id==relicId);if(level==null){level=new RelicLevel{Id=relicId};Profile.Levels.Add(level);}
            if(level.Level<int.MaxValue&&(def.MaxLevel<=0||level.Level<def.MaxLevel))level.Level++;
            if(pending!=null){pending.RelicId=relicId;pending.Claimed=true;}
            Profile.Runs.Add(new RelicRunRecord{RunId=id,StartFloor=1,MaxFloor=Profile.HighestFloor,Rewards=new List<RelicReward>{new RelicReward{Id=id,RelicId=relicId,Claimed=true}}});
            if(Save())return true;Profile=JsonUtility.FromJson<RelicProfile>(before);return false;
        }
    }
}
