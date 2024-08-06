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
            int textId = -1;
            if (CurrentStage == null)
            {
                textId = 11090;
            } else
            {
                textId = 11050;
            }
            return DataSystem.GetText(textId);
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

        public void SelectAddActor()
        {
            if (CurrentStage == null)
            {
                CurrentSaveData.SetResumeStage(true);
                PartyInfo.ChangeCurrency(DataSystem.System.InitCurrency);
            }
        }

        public List<ActorInfo> MakeSelectActorInfos()
        {
            return new List<ActorInfo>(){CurrentActor};
        }

        public List<GetItemInfo> MakeSelectGetItemInfos()
        {
            var getItemInfo = CurrentSelectRecord().SymbolInfo.GetItemInfos.Find(a => a.GetItemType == GetItemType.SelectAddActor);
            if (getItemInfo != null)
            {
                getItemInfo.SetParam1(CurrentActor.ActorId);
                getItemInfo.SetParam2(CurrentActor.ActorId);
            }
            return new List<GetItemInfo>(){getItemInfo};
        }

        public bool EnableLvReset()
        {
            return CurrentActor.EnableLvReset();
        }

        public int ActorLvReset()
        {
            var tempActor = _actorInfos.Find(a => a.ActorId == CurrentActor.ActorId);
            var currency = tempActor.ActorLevelReset();
            PartyInfo.ChangeCurrency(PartyInfo.Currency + currency);
            return currency;
        }

        public List<ListData> SkillTrigger(int selectIndex = -1)
        {
            var listData = MakeListData(CurrentActor.SkillTriggerInfos,selectIndex);
            return listData;
        }

        public List<ListData> StatusCommand()
        {
            return MakeListData( DataSystem.StatusCommand );
        }

        public bool CheckActorTrain()
        {
            return Currency >= ActorLevelUpCost(CurrentActor);
        }

        public int SelectActorEvaluate()
        {
            return CurrentActor.Evaluate();
        }

        public void CommandLevelUp()
        {
            ActorLevelUp(CurrentActor);
        }

        public List<ListData> SelectActorLearningMagicList(int selectedSkillId = -1)
        {
            return ActorLearningMagicList(CurrentActor,-1,selectedSkillId);
        }

        public void LearnMagic(int skillId)
        {
            ActorLearnMagic(CurrentActor,skillId);
        }

        public int LevelUpCost()
        {
            return ActorLevelUpCost(CurrentActor);
        }
    }
}