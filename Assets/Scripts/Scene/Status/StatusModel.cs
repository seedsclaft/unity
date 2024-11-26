using System.Collections;
using System.Collections.Generic;

namespace Ryneus
{
    public class StatusModel : BaseModel
    {
        private List<ActorInfo> _actorInfos = null;
        public List<ActorInfo> ActorInfos => _actorInfos;
        public StatusModel(List<ActorInfo> actorInfos)
        {
            _actorInfos = actorInfos;
        }

        private SkillSlotType _selectingSlotType = SkillSlotType.None;
        public SkillSlotType SelectingSlotType => _selectingSlotType;
        public void SetSelectingSlotType(SkillSlotType selectingSlotType) => _selectingSlotType = selectingSlotType;
        public void SetActorSkillSlot(int changeSkillId)
        {
            CurrentActor.SetSkillSlot(_selectingSlotType,changeSkillId);
        }

        public void UpdateActorRemainMp()
        {
            var costMp = 0;
            foreach (var slotSkill in SlotSkills())
            {
                costMp += slotSkill.LearningCost;
            }
            CurrentActor.ChangeMp(CurrentActor.MaxMp - costMp);
        }

        public List<SkillInfo> SlotSkills()
        {
            var slotSkills = CurrentActor.SlotSkills();
            foreach (var slotSkill in slotSkills)
            {
                if (slotSkill.Master != null && !slotSkill.IsBattleSpecialSkill())
                {
                    var cost = TacticsUtility.LearningMagicCost(CurrentActor,slotSkill.Attribute,_actorInfos,slotSkill.Master.Rank);
                    slotSkill.SetLearningCost(cost);
                }
            }
            return slotSkills;
        }

        public List<SkillInfo> ChangeAbleSkills()
        {
            var changeAbleSkills = CurrentActor.ChangeAbleSkills();
            foreach (var changeAbleSkill in changeAbleSkills)
            {
                if (changeAbleSkill.Master != null && !changeAbleSkill.IsBattleSpecialSkill())
                {
                    var cost = TacticsUtility.LearningMagicCost(CurrentActor,changeAbleSkill.Attribute,_actorInfos,changeAbleSkill.Master.Rank);
                    changeAbleSkill.SetLearningCost(cost);
                    changeAbleSkill.SetEnable(cost <= CurrentActor.CurrentMp);
                }
            }
            return changeAbleSkills;
        }

        public string HelpText()
        {
            return DataSystem.GetText(18010);
        }

        private int _currentIndex = 0;
        public int CurrentIndex => _currentIndex;
        public void SelectActor(int actorId)
        {
            var index = _actorInfos.FindIndex(a => a.ActorId == actorId);
            _currentIndex = index;
        }

        public ActorInfo CurrentActor => _actorInfos[_currentIndex];

        public void ChangeActorIndex(int value)
        {
            _currentIndex += value;
            if (_currentIndex > _actorInfos.Count-1)
            {
                _currentIndex = 0;
            } else
            if (_currentIndex < 0)
            {
                _currentIndex = _actorInfos.Count-1;
            }
        }
        
        public void SetActorLastSkillId(int selectSkillId)
        {
            CurrentActor.SetLastSelectSkillId(selectSkillId);
        }

        public List<ActorInfo> MakeSelectActorInfos()
        {
            return new List<ActorInfo>(){CurrentActor};
        }

        public List<GetItemInfo> MakeSelectGetItemInfos()
        {
            /*
            var getItemInfos = CurrentSelectRecord().SymbolInfo.GetItemInfos.FindAll(a => a.GetItemType == GetItemType.AddActor);
            var getItemInfo = getItemInfos.Find(a => a.Param1 == CurrentActor.ActorId);
            if (getItemInfo != null)
            {
                getItemInfo.SetResultParam(CurrentActor.ActorId);
                return new List<GetItemInfo>(){getItemInfo};
            }
            getItemInfos = CurrentSelectRecord().SymbolInfo.GetItemInfos.FindAll(a => a.GetItemType == GetItemType.SelectAddActor);
            if (getItemInfos.Count > 0)
            {
                getItemInfos[0].SetResultParam(CurrentActor.ActorId);
                return getItemInfos;
            }
            */
            return new List<GetItemInfo>(){};
        }



        public List<SkillTriggerInfo> SkillTrigger(int selectIndex = -1)
        {
            return CurrentActor.SkillTriggerInfos;
        }

        public List<SystemData.CommandData> StatusCommand()
        {
            return DataSystem.StatusCommand;
        }

        public List<ListData> SelectActorLearningMagicList(int selectedSkillId = -1)
        {
            return ActorLearningMagicList(CurrentActor,-1,selectedSkillId);
        }


    }
}