using System;
using System.Collections.Generic;
using LunaEclipse.Progression;
using UnityEditor;
namespace LunaEclipse.EditorTools
{
    public static class RelicTests
    {
        sealed class Store:IRelicStore
        {public readonly Dictionary<string,string> Data=new Dictionary<string,string>();public bool Fail;public string Read(string k)=>Data.ContainsKey(k)?Data[k]:"";public void Write(string k,string v){if(Fail)throw new Exception("quota");Data[k]=v;}public void Flush(){if(Fail)throw new Exception("quota");}}
        static void Check(bool value,string message){if(!value)throw new Exception(message);}
        [MenuItem("Luna/Test Relics")]
        public static void Run()
        {
            var store=new Store();var s=new RelicService(store);
            Check(s.Level("bag_expansion")==0,"new profile");
            Check(s.Grant("first","bag_expansion"),"grant");Check(!s.Grant("first","bag_expansion"),"duplicate grant");
            for(int i=0;i<30;i++)s.Grant("bag"+i,"bag_expansion");Check(s.GetBonus().BagCapacity==40,"bag cap");
            Check(new RelicService(store).Level("bag_expansion")==20,"reload");
            var report=s.CompleteRun("run",41);Check(report.Rewards.Count==2,"20F milestones");Check(report.Rewards[0].RelicId==null&&report.Rewards[0].Rank==null,"pending no manufactured reward");Check(!s.TryClaim(report.Rewards[0].Id),"pending cannot claim");
            Check(s.CompleteRun("run",100).MaxFloor==41,"idempotent report");
            s.Grant("compass","deep_compass");Check(s.GetBonus().MaxStartFloor==2,"compass");s.Grant("smith","blacksmith_memory");Check(-3+s.GetBonus().EquipmentEnhancement==-2,"negative equipment additive");
            store.Fail=true;Check(!s.Grant("fail","blacksmith_memory")&&s.Level("blacksmith_memory")==1,"failure rollback");store.Fail=false;
            store.Data[RelicService.SaveKey]="broken";var restored=new RelicService(store);Check(restored.Profile.HighestFloor==41,"backup recovery");Check(restored.LastError.Length>0,"recovery notice");
            var preview=new RelicService(new Store(),new RelicBalance{EnableTrialRewardTable=true});var reward=preview.CompleteRun("preview",20).Rewards[0];Check(preview.TryClaim(reward.Id),"preview claim");Check(!preview.TryClaim(reward.Id),"double claim");
            Check(!s.Grant("disabled","blade_prism"),"disabled candidate");
            var approved=new RelicService(new Store());var pending=approved.CompleteRun("approval",20).Rewards[0];Check(approved.Grant(pending.Id,"bag_expansion"),"approve pending");Check(pending.Claimed&&!approved.TryClaim(pending.Id),"pending resolved without double claim");
            UnityEngine.Debug.Log("RelicTests passed: default migration, acquisition, cap, persistence, pending opportunities, duplication, additive quality, failure rollback, backup recovery, preview claims.");
        }
    }
}
