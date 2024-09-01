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

        public string HelpText()
        {
            return DataSystem.GetText(18010);
        }

        private int _currentIndex = 0;
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
            return new List<GetItemInfo>(){};
        }

        public bool EnableLvReset()
        {
            return CurrentActor.EnableLvReset();
        }

        public int ActorLvReset()
        {
            var tempActor = _actorInfos.Find(a => a.ActorId == CurrentActor.ActorId);
            var currency = tempActor.ActorLevelReset();
            PartyInfo.ChangeCurrency(Currency+ currency);
            return currency;
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

        public int LevelUpCost()
        {
            return ActorLevelUpCost(CurrentActor);
        }
    }
}