using System.Collections;
using System.Collections.Generic;

namespace Ryneus
{
    [System.Serializable]
    public class ActionInfo 
    {
        private int _index;
        
        private int _subjectIndex = 0;
        public int SubjectIndex => _subjectIndex;

        private int _lastTargetIndex = 0;
        public int LastTargetIndex => _lastTargetIndex;
        public SkillData Master => DataSystem.FindSkill(_skillInfo.Id);
        private SkillInfo _skillInfo = null;
        public SkillInfo SkillInfo => _skillInfo;
        
        private RangeType _rangeType = RangeType.None;
        public RangeType RangeType => _rangeType;
        private TargetType _targetType = TargetType.None;
        public TargetType TargetType => _targetType;
        private ScopeType _scopeType = ScopeType.None;
        public ScopeType ScopeType => _scopeType;

        private List<ActionResultInfo> _actionResult = new ();
        public List<ActionResultInfo> ActionResults => _actionResult;

        private int _mpCost;
        public int MpCost => _mpCost;
        private int _hpCost;
        public int HpCost => _hpCost;
        private int _repeatTime;
        public int RepeatTime => _repeatTime;
        public void SetRepeatTime(int repeatTime)
        {
            _repeatTime = repeatTime;
        }
        // 選択可能な対象情報
        private List<int> _candidateTargetIndexList;
        public List<int> CandidateTargetIndexList => _candidateTargetIndexList;
        public void SetCandidateTargetIndexList(List<int> candidateTargetIndexList)
        {
            _candidateTargetIndexList = candidateTargetIndexList;
        }

        private bool _triggeredSkill = false;
        public bool TriggeredSkill => _triggeredSkill;
        
        private int _turnCount;
        public int TurnCount => _turnCount;
        public void SetTurnCount(int turnCount) {_turnCount = turnCount;}

        public ActionInfo(SkillInfo skillInfo,int index,int subjectIndex,int lastTargetIndex,List<int> targetIndexList)
        {
            _index = index;
            _skillInfo = skillInfo;
            _scopeType = Master.Scope;
            _rangeType = Master.Range;
            _targetType = Master.TargetType;
            _subjectIndex = subjectIndex;
            _lastTargetIndex = lastTargetIndex;
            _candidateTargetIndexList = targetIndexList;
        }

        public void SetRangeType(RangeType rangeType)
        {
            _rangeType = rangeType;
        }

        public void SetScopeType(ScopeType scopeType)
        {
            _scopeType = scopeType;
        }

        public void SetHpCost(int hpCost)
        {
            _hpCost = hpCost;
        }

        public void SetMpCost(int mpCost)
        {
            _mpCost = mpCost;
        }

        public void SetActionResult(List<ActionResultInfo> actionResult)
        {
            _actionResult = actionResult;
        }

        public List<ActionInfo> CheckPlusSkill()
        {
            // 行動後スキル
            var featureDates = SkillInfo.FeatureDates;
            var PlusSkill = featureDates.FindAll(a => a.FeatureType == FeatureType.PlusSkill);
            
            var actionInfos = new List<ActionInfo>();
            for (var i = 0;i < PlusSkill.Count;i++)
            {
                var skillInfo = new SkillInfo(PlusSkill[i].Param1);
                var actionInfo = new ActionInfo(skillInfo,_index,SubjectIndex,-1,null);
                actionInfo.SetTriggerSkill(true);
                actionInfos.Add(actionInfo);
            }
            return actionInfos;
        }

        public List<SkillInfo> CheckPlusSkillTrigger()
        {
            var featureDates = SkillInfo.FeatureDates;
            var skillInfos = new List<SkillInfo>();
            var PlusSkillTrigger = featureDates.FindAll(a => a.FeatureType == FeatureType.PlusSkillTrigger);
            for (var i = 0;i < PlusSkillTrigger.Count;i++)
            {
                var skillInfo = new SkillInfo(PlusSkillTrigger[i].Param1);
                skillInfos.Add(skillInfo);
            }
            return skillInfos;
        }

        public void SetTriggerSkill(bool triggeredSkill)
        {
            _triggeredSkill = triggeredSkill;
        }

        public bool IsUnison()
        {
            return SkillInfo.FeatureDates.Find(a => a.FeatureType == FeatureType.AddState && (StateType)a.Param1 == StateType.Wait) != null;
        }
    }
}