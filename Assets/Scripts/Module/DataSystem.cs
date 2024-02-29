using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Threading;
using Cysharp.Threading.Tasks;
using System.Linq;

namespace Ryneus
{
    abstract public class DataSystem 
    {
        private static DataManager _data;
        public static DataManager Data => _data;
        public static List<ActorData> Actors = new ();
        public static List<AdvData> Adventures = new ();
        public static List<EnemyData> Enemies = new ();
        public static List<RuleData> Rules = new ();
        public static List<HelpData> Helps = new ();
        public static List<SkillData> Skills = new ();
        public static List<StageData> Stages = new ();
        public static List<SymbolGroupData> SymbolGroups = new ();
        public static List<StateData> States = new ();
        public static List<TroopData> Troops = new ();
        public static List<AnimationData> Animations = new ();
        public static List<PrizeSetData> PrizeSets = new ();
        public static SystemData System;



        public static List<SystemData.CommandData> TacticsCommand => System.TacticsCommandData;
        public static List<SystemData.CommandData> TitleCommand => System.TitleCommandData;
        public static List<SystemData.CommandData> StatusCommand => System.StatusCommandData;
        public static List<SystemData.OptionCommand> OptionCommand => System.OptionCommandData;
        
        



        //private static AlcanaData _alcana;
        //public static List<AlcanaData.Alcana> Alcana => _alcana._data;





        public static void LoadData(){
            Actors = Resources.Load<ActorDates>("Data/Actors").Data;
            Adventures = Resources.Load<AdvDates>("Data/Adventures").Data;
            Enemies = Resources.Load<EnemyDates>("Data/Enemies").Data;
            Rules = Resources.Load<RuleDates>("Data/Rules").Data;
            Helps = Resources.Load<HelpDates>("Data/Helps").Data;
            Skills = Resources.Load<SkillDates>("Data/Skills").Data;
            Stages = Resources.Load<StageDates>("Data/Stages").Data;
            SymbolGroups = Resources.Load<StageDates>("Data/Stages").SymbolGroupData;
            States = Resources.Load<StateDates>("Data/States").Data;
            System = Resources.Load<SystemData>("Data/System");
            Troops = Resources.Load<TroopDates>("Data/Troops").Data;
            PrizeSets = Resources.Load<PrizeSetDates>("Data/PrizeSets").Data;
            Animations = Resources.Load<AnimationDates>("Data/Animations").Data;
            //DataSystem._alcana = Resources.Load<AlcanaData>("Data/Alcana");
            DataSystem._data = Resources.Load<DataManager>("Data/MainData");
        }
        public static ActorData FindActor(int id)
        {
            return Actors.Find(a => a.Id == id);
        }

        public static SkillData FindSkill(int id)
        {
            return Skills.Find(a => a.Id == id);
        }
        
        public static StageData FindStage(int id)
        {
            return Stages.Find(a => a.Id == id);
        }

        public static TextData GetTextData(int id)
        {
            return System.GetTextData(id);
        }

        public static string GetReplaceText(int id,string replace)
        {
            return System.GetReplaceText(id,replace);
        }

        public static string GetReplaceDecimalText(int value)
        {
            var numText = value.ToString();
            var index = 0;
            var charList = new List<string>();
            for (int i = numText.Length-1;i >= 0;i--)
            {
                charList.Add(numText[i].ToString());
                index++;
                if (index % 3 == 0 && i != 0)
                {
                    charList.Add(",");
                }
            }
            charList.Reverse();
            var text = "";
            foreach (var character in charList)
            {
                text += character;
            }
            return text;
        }

        public static List<ListData> HelpText(string key)
        {
            var data = Helps.Find(a => a.Key == key);
            if (data != null)
            {
                var texts = data.Help.Split("\n").ToList();
                return ListData.MakeListData(texts);
            }
            return null;
        }
    }

    [Serializable]
    public class TextData {
        public int Id;
        public string Text;
        public string Help;
    }
}