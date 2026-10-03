using System;
using System.IO;
using System.Linq;
using LunaEclipse.Dungeon;
using LunaEclipse.Progression;
using UnityEditor;
using UnityEngine;
namespace LunaEclipse.EditorTools
{
    public static class DungeonComfortTests
    {
        static int checks;
        static void Check(bool value,string label){if(!value)throw new Exception(label);checks++;}
        public static void Run()
        {
            checks=0;var run=new DungeonRun(92);run.Enemies.Clear();
            var herb=run.Items.First(i=>DungeonRules.Distance(i.Cell,run.PlayerCell)==1);int turns=run.Turns;
            run.Move(herb.Cell-run.PlayerCell);
            Check(run.Bag.Contains(herb.Item)&&!run.Items.Contains(herb),"auto pickup");
            Check(run.Turns==turns+1&&run.LastAction=="move","pickup adds no extra turn and keeps walk animation");
            Check(!run.Logs.Any(s=>s.Contains("一歩")),"no walking spam");
            var sword=new ItemData{Id="curse",Kind="moon_sword",Name="剣",Enhancement=-2};
            var spare=new ItemData{Id="spare",Kind="moon_sword",Name="剣",Enhancement=3};
            run.Bag.Add(sword);run.Bag.Add(spare);
            Check(!sword.DisplayName.Contains("-2")&&!spare.DisplayName.Contains("+3"),"hidden enhancements");
            run.UseItem(sword.Id);Check(sword.Identified&&sword.DisplayName.Contains("-2"),"equip identifies");
            turns=run.Turns;Check(!run.DropItem(sword.Id)&&!run.UseItem(spare.Id)&&run.Turns==turns,"curse blocks drop and replace without turn");
            var shield=new ItemData{Id="shield",Kind="moon_shield",Name="盾",Enhancement=-1};run.Bag.Add(shield);run.UseItem(shield.Id);
            Check(!run.DropItem(shield.Id)&&shield.Identified,"shield curse");
            var stored=new StoredItem{Id=sword.Id,Name=sword.Name,Kind=sword.Kind,Enhancement=sword.Enhancement,Identified=sword.Identified};
            var loaded=JsonUtility.FromJson<StoredItem>(JsonUtility.ToJson(stored.Copy()));
            Check(loaded.Identified&&loaded.Enhancement==-2&&loaded.DisplayName.Contains("呪"),"identified curse persists");
            var unknown=JsonUtility.FromJson<StoredItem>("{\"Id\":\"old\",\"Kind\":\"moon_sword\",\"Name\":\"剣\",\"Enhancement\":2}");
            Check(!unknown.Identified&&!unknown.DisplayName.Contains("+2"),"old saves load safely unidentified");
            run.Bag.Add(new ItemData{Id="scroll",Kind="return_scroll"});run.Bag.Add(new ItemData{Id="potion",Kind="potion"});
            turns=run.Turns;run.SortBag();
            Check(run.Bag.Take(2).All(i=>i.Kind=="moon_sword")&&run.Bag[2].Kind=="moon_shield"&&run.Bag[3].Kind=="herb"&&run.Bag[4].Kind=="return_scroll","category sort");
            Check(run.WeaponId==sword.Id&&run.ShieldId==shield.Id&&turns==run.Turns,"sorting free and retains equipment");
            while(run.Bag.Count<run.BagCapacity)run.Bag.Add(new ItemData{Id=Guid.NewGuid().ToString()});
            var direction=DungeonRules.Directions.First(d=>run.Map.Walkable(run.PlayerCell+d));
            var excess=new FloorItem{Cell=run.PlayerCell+direction,Item=new ItemData{Id="full"}};run.Items.Add(excess);run.Move(direction);
            Check(run.Items.Contains(excess)&&run.Bag.Count==20,"full bag leaves item");
            Check(Mathf.CeilToInt(run.BagCapacity/(float)DungeonRules.ItemsPerPage)==2,"20 slots two pages");
            int negative=0,total=0;
            for(int seed=0;seed<500;seed++)foreach(var item in ItemManager.CreateItems(DungeonGenerator.Generate(seed,1),seed,1))
                if(item.Item.Equipment){total++;if(item.Item.Enhancement<0)negative++;Check(!item.Item.Identified,"generated equipment unidentified");}
            Check(negative/(float)total>.02f&&negative/(float)total<.08f,"negative frequency around 5 percent");
            Check(Resources.Load<AudioClip>("Audio/Generated/luna-moonlit-footsteps")!=null,"dungeon BGM exists");
            File.WriteAllText("Library/DungeonComfortTests.json","{\"passed\":true,\"checks\":"+checks+",\"negative\":"+negative+",\"equipment\":"+total+"}");
            DungeonCoreTests.Run();GameplayContentTests.Run();InventoryVaultTests.Run();VaultLoadoutTests.Run();
            Debug.Log("Dungeon comfort regression suite passed");
        }
    }
}
