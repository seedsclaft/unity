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
        public static Dictionary<int,ActorData> Actors = new ();
        public static List<AdvData> Adventures = new ();
        public static List<EnemyData> Enemies = new ();
        public static List<RuleData> Rules = new ();
        public static List<HelpData> Helps = new ();
        public static Dictionary<int,SkillData> Skills = new ();
        public static List<StageData> Stages = new ();
        public static List<SymbolGroupData> SymbolGroups = new ();
        public static List<StateData> States = new ();
        public static List<TroopData> Troops = new ();
        public static List<AnimationData> Animations = new ();
        public static List<PrizeSetData> PrizeSets = new ();
        public static List<ScorePrizeData> ScorePrizes = new ();
        public static List<SkillTriggerData> SkillTriggers = new ();
        public static SystemData System;



        public static List<SystemData.CommandData> TacticsCommand => System.TacticsCommandData;
        public static List<SystemData.CommandData> TitleCommand => System.TitleCommandData;
        public static List<SystemData.CommandData> StatusCommand => System.StatusCommandData;
        public static List<SystemData.OptionCommand> OptionCommand => System.OptionCommandData;
        
        public static Color PowerUpColor => new(0,128,128);



        //private static AlcanaData _alcana;
        //public static List<AlcanaData.Alcana> Alcana => _alcana._data;





        public static void LoadData()
        {
            var ActorsData = Resources.Load<ActorDates>("Data/Actors").Data;
            foreach (var actorData in ActorsData)
            {
                Actors[actorData.Id] = actorData;
            }
            Adventures = Resources.Load<AdvDates>("Data/Adventures").Data;
            Enemies = Resources.Load<EnemyDates>("Data/Enemies").Data;
            Rules = Resources.Load<RuleDates>("Data/Rules").Data;
            Helps = Resources.Load<HelpDates>("Data/Helps").Data;
            var SkillsData = Resources.Load<SkillDates>("Data/Skills").Data;
            foreach (var skillData in SkillsData)
            {
                Skills[skillData.Id] = skillData;
            }
            Stages = Resources.Load<StageDates>("Data/Stages").Data;
            SymbolGroups = Resources.Load<StageDates>("Data/Stages").SymbolGroupData;
            States = Resources.Load<StateDates>("Data/States").Data;
            System = Resources.Load<SystemData>("Data/System");
            Troops = Resources.Load<TroopDates>("Data/Troops").Data;
            PrizeSets = Resources.Load<PrizeSetDates>("Data/PrizeSets").Data;
            ScorePrizes = Resources.Load<ScorePrizeDates>("Data/ScorePrizes").Data;
            Animations = Resources.Load<AnimationDates>("Data/Animations").Data;
            SkillTriggers = Resources.Load<SkillTriggerDates>("Data/SkillTrigger").Data;
            //DataSystem._alcana = Resources.Load<AlcanaData>("Data/Alcana");
            _data = Resources.Load<DataManager>("Data/MainData");
        }

        public static ActorData FindActor(int id)
        {
            if (Actors.ContainsKey(id))
            {
                return Actors[id];
            }
            return null;
        }

        public static SkillData FindSkill(int id)
        {
            if (Skills.ContainsKey(id))
            {
                return Skills[id];
            }
            return null;
        }
        
        public static StageData FindStage(int id)
        {
            return Stages.Find(a => a.Id == id);
        }

        public static StateData FindState(int id)
        {
            return States.Find(a => a.StateType == (StateType)id);
        }

        public static TextData GetTextData(int id)
        {
            return System.GetTextData(id);
        }

        public static string GetText(int id)
        {
            var textData = GetTextData(id);
            if (textData != null)
            {
                return textData.Text;
            }
            return "";
        }

        public static string GetHelp(int id)
        {
            var textData = GetTextData(id);
            if (textData != null)
            {
                return textData.Help;
            }
            return "";
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