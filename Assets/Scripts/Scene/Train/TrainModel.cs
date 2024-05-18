using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Ryneus
{
    public class TrainModel : BaseModel
    {
        public TrainModel()
        {
            _selectActorId = StageMembers()[0].ActorId;
        }

        private int _selectAttribute = 0;
        public void SetSelectAttribute(int selectAttribute)
        {
            _selectAttribute = selectAttribute;
        }
        
        private int _selectActorId = 0;
        public void SetSelectActorId(int actorId)
        {
            _selectActorId = actorId;
        }    
        public ActorInfo TacticsActor()
        {
            return StageMembers().Find(a => a.ActorId == _selectActorId);
        }

        private TacticsCommandType _TacticsCommandType = TacticsCommandType.Train;
        public TacticsCommandType TacticsCommandType => _TacticsCommandType;
        public void SetTacticsCommandType(TacticsCommandType tacticsCommandType)
        {
            _TacticsCommandType = tacticsCommandType;
        }
        private Dictionary<TacticsCommandType,bool> _tacticsCommandEnables = new ();
        public void SetTacticsCommandEnables(TacticsCommandType tacticsCommand,bool isEnable)
        {
            _tacticsCommandEnables[tacticsCommand] = isEnable;
        }

        public bool CheckActorTrain()
        {
            var cost = TacticsUtility.TrainCost(TacticsActor());
            return Currency >= cost;
        }

        public int SelectActorEvaluate()
        {
            return TacticsActor().Evaluate();
        }

        public void SelectActorTrain()
        {
            var actorInfo = TacticsActor();
            var cost = TacticsUtility.TrainCost(actorInfo);
            var levelUpInfo = actorInfo.LevelUp(cost,CurrentStage.Id,CurrentStage.CurrentTurn);
            PartyInfo.ChangeCurrency(Currency - cost);
            PartyInfo.SetLevelUpInfo(levelUpInfo);
        }

        public void LearnMagic(int skillId)
        {
            var actorInfo = TacticsActor();
            var skillInfo = new SkillInfo(skillId);
            var learningCost = TacticsUtility.LearningMagicCost(actorInfo,skillInfo.Attribute,StageMembers());
            PartyInfo.ChangeCurrency(Currency - learningCost);
            var levelUpInfo = actorInfo.LearnSkill(skillInfo.Id,learningCost,CurrentStage.Id,CurrentStage.CurrentTurn,-1);
            PartyInfo.SetLevelUpInfo(levelUpInfo);
            // 作戦項目に追加
            PartyInfo.AddSkillTriggerSkill(actorInfo.ActorId,skillId);
            //actorInfo.GainNuminousCost(learningCost);
        }

        public void SaveTempBattleMembers()
        {
            TempInfo.CashBattleActors(BattleMembers());
        }

        public void SetInBattle()
        {
            var actorInfo = TacticsActor();
            var battleIndex = StageMembers().FindAll(a => a.BattleIndex >= 0).Count + 1;
            if (actorInfo.BattleIndex >= 0)
            {
                actorInfo.SetBattleIndex(-1);
                var battleMembers = BattleMembers();
                for (int i = 0;i < battleMembers.Count;i++)
                {
                    battleMembers[i].SetBattleIndex(i + 1);
                }
                return;
            }
            if (battleIndex > 5) 
            {
                return;
            }
            actorInfo.SetBattleIndex(battleIndex);
        }

        public List<ListData> SelectActorLearningMagicList(int selectedSkillId = -1)
        {
            var skillInfos = new List<SkillInfo>();
            var actorInfo = TacticsActor();
            
            foreach (var alchemyId in PartyInfo.CurrentAlchemyIdList(CurrentStage.Id,CurrentStage.CurrentTurn))
            {
                //if (actorInfo.IsLearnedSkill(alchemyId)) continue;
                var skillInfo = new SkillInfo(alchemyId);
                if (_selectAttribute > 0)
                {
                    if ((int)skillInfo.Master.Attribute != _selectAttribute)
                    {
                        continue;
                    }
                }
                var cost = TacticsUtility.LearningMagicCost(actorInfo,skillInfo.Attribute,StageMembers());
                skillInfo.SetEnable(Currency >= cost && !actorInfo.IsLearnedSkill(alchemyId));
                skillInfo.SetLearningCost(cost);
                skillInfos.Add(skillInfo);
            }
            var selectIndex = skillInfos.FindIndex(a => a.Id == selectedSkillId);
            var listData = MakeListData(skillInfos,selectIndex);
            return listData;
        }

        public List<ListData> AttributeTabList()
        {
            var list = new List<AttributeType>();
            foreach (var attribute in Enum.GetValues(typeof(AttributeType)))
            {
                var attributeType = (AttributeType)attribute;
                list.Add(attributeType);
            }
            return MakeListData(list);
        }

        public void SetTempAddActorStatusInfos(int actorId)
        {
            var actorInfos = PartyInfo.ActorInfos.FindAll(a => a.ActorId == actorId);
            TempInfo.SetTempStatusActorInfos(actorInfos);
        }

        public List<ListData> TacticsCharacterData()
        {
            var list = new List<TacticsActorInfo>();
            foreach (var member in StageMembers())
            {
                var tacticsActorInfo = new TacticsActorInfo
                {
                    TacticsCommandType = _TacticsCommandType,
                    ActorInfo = member,
                    ActorInfos = StageMembers()
                };
                list.Add(tacticsActorInfo);
            }
            return MakeListData(list);
        }

        public string TacticsCommandInputInfo()
        {
            switch (_TacticsCommandType)
            {
                case TacticsCommandType.Train:
                    return "TRAIN";
                case TacticsCommandType.Alchemy:
                    return "ALCHEMY";
                    /*
                case TacticsCommandType.Recovery:
                    return "RECOVERY";
                    */
            }
            return "";
        }

        public TacticsCommandData TacticsCommandData()
        {
            var tacticsCommandData = new TacticsCommandData();
            tacticsCommandData.Title = CommandTitle();
            return tacticsCommandData;
        }

        private string CommandTitle()
        {
            return DataSystem.GetText((int)_TacticsCommandType);
        }

        public void ChangeBattleLineIndex(int actorId,bool isFront)
        {
            var actorInfo = TacticsActor();
            if (actorInfo.LineIndex == LineType.Front && isFront == false)
            {
                actorInfo.SetLineIndex(LineType.Back);
            } else
            if (actorInfo.LineIndex == LineType.Back && isFront == true)
            {
                actorInfo.SetLineIndex(LineType.Front);
            }
        }

        public void AssignBattlerIndex()
        {
            var idList = PartyInfo.LastBattlerIdList;
            foreach (var id in idList)
            {
                var actor = StageMembers().Find(a => a.ActorId == id);
                if (actor != null)
                {
                    actor.SetBattleIndex(id);
                }
            }
        }


        public List<ListData> ParallelCommand()
        {
            return MakeListData(BaseConfirmCommand(23050,23040));
        }
        
        public bool CanParallel()
        {
            return PartyInfo.Currency >= PartyInfo.ParallelCost();
        }

        public void ResetBattlerIndex()
        {
            foreach (var stageMember in StageMembers())
            {
                stageMember.SetBattleIndex(-1);
            }
        }

        public void SetPartyBattlerIdList()
        {
            var idList = new List<int>();
            foreach (var battleMember in BattleMembers())
            {
                idList.Add(battleMember.ActorId);
            }
            PartyInfo.SetLastBattlerIdList(idList);
        }

        public void SetSurvivalMode()
        {
            CurrentStage.SetSurvivalMode();
        }

    }
}