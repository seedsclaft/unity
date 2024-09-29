using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Ryneus
{
    [Serializable]
    public class SkillInfo 
    {
        public SkillData Master => DataSystem.FindSkill(_id);
        private int _id;
        public int Id => _id;

        private bool _enable;
        public bool Enable => _enable;
        public void SetEnable(bool IsEnable)
        {
            _enable = IsEnable;
        }
        public AttributeType Attribute => Master.Attribute;

        private LearningState _learningState;
        public LearningState LearningState => _learningState;
        public void SetLearningState(LearningState learningState)
        {
            _learningState = learningState;
        }

        private int _learningCost = 0;
        public int LearningCost => _learningCost;
        public void SetLearningCost(int cost)
        {
            _learningCost = cost;
        }

        private int _learningLv = 0;
        public int LearningLv => _learningLv;
        public void SetLearningLv(int learningLv)
        {
            _learningLv = learningLv;
        }

        private List<SkillData.FeatureData> _featureDates = new();
        public List<SkillData.FeatureData> FeatureDates => _featureDates;

        private List<SkillData.TriggerData> _triggerDates = new();
        public List<SkillData.TriggerData> TriggerDates => _triggerDates;

        private int _weight = 100;
        public int Weight => _weight;
        public void SetWeight(int weight)
        {
            _weight = weight;
        }

        private int _useCount = 0;
        public int UseCount => _useCount;
        public void SetUseCount(int useCount)
        {
            _useCount = useCount;
        }
        public void GainUseCount()
        {
            _useCount++;
        }

        private int _countTurn = 0;
        public int CountTurn => _countTurn;
        public void SetCountTurn(int countTurn)
        {
            _countTurn = countTurn;
        }
        public void SeekCountTurn()
        {
            if (_countTurn > 0)
            {
                _countTurn--;
            }
        }

        public SkillInfo(int id)
        {
            _id = id;
            _learningState = LearningState.None;
            if (Master != null && Master.FeatureDates != null)
            {
                var list = new List<SkillData.FeatureData>();
                foreach (var featureData in Master.FeatureDates)
                {
                    list.Add(featureData.CopyData());
                }
                _featureDates = list;
            }
        }
        
        public void SetTriggerDates(List<SkillData.TriggerData> triggerDates)
        {
            _triggerDates = triggerDates;
        }

        public bool IsUnison()
        {
            return FeatureDates.Find(a => a.FeatureType == FeatureType.AddState && (StateType)a.Param1 == StateType.Wait) != null;
        }

        public int ActionAfterGainAp()
        {
            var gainAp = 0;
            foreach (var featureData in FeatureDates)
            {
                if (featureData.FeatureType == FeatureType.ActionAfterGainAp)
                {
                    gainAp += featureData.Param1;
                }
            }
            return gainAp;
        }

        public bool IBattleActiveSkill()
        {
            return Master.SkillType != SkillType.Passive && Master.SkillType != SkillType.UseAlcana && Master.SkillType != SkillType.Relic;
        }

        public bool IBattlePassiveSkill()
        {
            return Master.SkillType == SkillType.Passive || Master.SkillType == SkillType.UseAlcana || Master.SkillType == SkillType.Relic;
        }

        public bool IsEnhanceSkill()
        {
            return Master.SkillType == SkillType.Enhance;
        }

        public bool IsBattleActiveSkill()
        {
            return Master.SkillType == SkillType.Active || Master.SkillType == SkillType.Awaken;
        }


        public bool IsBattlePassiveSkill()
        {
            return Master.SkillType == SkillType.Passive || Master.SkillType == SkillType.Messiah;
        }

        public string ConvertHelpText()
        {
            var help = Master.ConvertHelpText(Master.Help);
            var regex = new Regex(@"\[.+?\]");
            var splits = regex.Matches(help);
            if (splits.Count > 0)
            {
                foreach (var split in splits)
                {
                    var paramText = "";
                    var array = split.ToString().Substring(1,5).Split(",");
                    
                    var p1 = array[0];
                    var p2 = int.Parse(array[1]);
                    var p3 = int.Parse(array[2]);
                    if (p1 == "f")
                    {
                        var targetFeature = FeatureDates[p2];
                        
                        if (p3 == 1)
                        {
                            paramText = targetFeature.Param1.ToString();
                        } else
                        if (p3 == 2)
                        {
                            paramText = targetFeature.Param2.ToString();
                        } else
                        if (p3 == 3)
                        {
                            paramText = targetFeature.Param3.ToString();
                        }
                        Regex reg1 = new Regex("/f");
                        help = reg1.Replace(help,paramText,1);
                    }
                    help = help.Replace(split.ToString(),"");
                }
            }
            if (Master.Name.Contains("+"))
            {
                help = help.Replace("/n",Master.Name.Replace("+",""));
            }
            // ステート説明挿入
            var state = FeatureDates.Find(a => a.FeatureType == FeatureType.AddState);
            if (state != null)
            {
                var stateMaster = DataSystem.States.Find(a => (int)a.StateType == state.Param1);        
                help = help.Replace("/s","【" + stateMaster.Name + "】" + "\n" + stateMaster.Help);
            }
            return help;
        }
    }
}