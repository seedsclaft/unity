using System;
using System.Collections;
using System.Collections.Generic;

namespace Ryneus
{
    public class TrainModel : BaseModel
    {
        public TrainModel()
        {
            var stageMembers = StageMembers();
            if (stageMembers.Count > 0)
            {
                SetSelectActorId(stageMembers[0].ActorId);
            }
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

        private TacticsCommandType _tacticsCommandType = TacticsCommandType.Train;
        public TacticsCommandType TacticsCommandType => _tacticsCommandType;
        public void SetTacticsCommandType(TacticsCommandType tacticsCommandType)
        {
            _tacticsCommandType = tacticsCommandType;
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
            // 新規魔法取得があるか
            var skills = actorInfo.LearningSkills(1);
            var levelUpInfo = actorInfo.LevelUp(cost,CurrentStage.Id,CurrentStage.CurrentSeek,-1,CurrentStage.WorldNo);
            PartyInfo.ChangeCurrency(Currency - cost);
            PartyInfo.SetLevelUpInfo(levelUpInfo);
            foreach (var skill in skills)
            {
                actorInfo.AddSkillTriggerSkill(skill.Id);
            }
        }

        public void LearnMagic(int skillId)
        {
            var actorInfo = TacticsActor();
            var skillInfo = new SkillInfo(skillId);
            var learningCost = TacticsUtility.LearningMagicCost(actorInfo,skillInfo.Attribute,StageMembers(),skillInfo.Master.Rank);
            PartyInfo.ChangeCurrency(Currency - learningCost);
            var levelUpInfo = actorInfo.LearnSkill(skillInfo.Id,learningCost,CurrentStage.Id,CurrentStage.CurrentSeek,-1,CurrentStage.WorldNo);
            PartyInfo.SetLevelUpInfo(levelUpInfo);
            // 作戦項目に追加
            actorInfo.AddSkillTriggerSkill(skillId);
            //actorInfo.GainNuminousCost(learningCost);
        }

        public void SaveTempBattleMembers()
        {
            TempInfo.CashBattleActors(BattleMembers());
        }

        public bool EnableAddInBattle()
        {
            var actorInfo = TacticsActor();
            if (actorInfo.BattleIndex <= 0)
            {
                var battleIndex = StageMembers().FindAll(a => a.BattleIndex >= 0).Count + 1;
                return battleIndex <= 5;
            }
            return true;
        }

        public void SetInBattle()
        {
            var actorInfo = TacticsActor();
            var battleIndex = StageMembers().FindAll(a => a.BattleIndex >= 0).Count + 1;
            if (actorInfo.BattleIndex >= 0)
            {
                RemoveBattleActor(actorInfo);
                return;
            }
            if (battleIndex > 5) 
            {
                return;
            }
            actorInfo.SetBattleIndex(battleIndex);
        }

        private void RemoveBattleActor(ActorInfo actorInfo)
        {
            actorInfo.SetBattleIndex(-1);
            var battleMembers = BattleMembers();
            for (int i = 0;i < battleMembers.Count;i++)
            {
                battleMembers[i].SetBattleIndex(i + 1);
            }
        }

        public List<ListData> SelectActorLearningMagicList(int selectedSkillId = -1)
        {
            var skillInfos = new List<SkillInfo>();
            var actorInfo = TacticsActor();
            
            foreach (var alchemyId in PartyInfo.CurrentAlchemyIdList(CurrentStage.Id,CurrentStage.CurrentSeek,CurrentStage.WorldNo))
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
                var cost = TacticsUtility.LearningMagicCost(actorInfo,skillInfo.Attribute,StageMembers(),skillInfo.Master.Rank);
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

        public List<ListData> TacticsCharacterData(int selectIndex = 0)
        {
            if (selectIndex < 0)
            {
                selectIndex = 0;
            }
            var list = new List<TacticsActorInfo>();
            foreach (var member in StageMembers())
            {
                var tacticsActorInfo = new TacticsActorInfo
                {
                    TacticsCommandType = _tacticsCommandType,
                    ActorInfo = member,
                    ActorInfos = StageMembers()
                };
                list.Add(tacticsActorInfo);
            }
            return MakeListData(list,selectIndex);
        }

        public List<ListData> TacticsBattleCharacterData(int selectIndex = 0)
        {
            if (selectIndex < 0)
            {
                selectIndex = 0;
            }
            var list = new List<TacticsActorInfo>();
            foreach (var member in StageMembers())
            {
                if (member.BattleIndex >= 1)
                {
                    var tacticsActorInfo = new TacticsActorInfo
                    {
                        TacticsCommandType = _tacticsCommandType,
                        ActorInfo = member,
                        ActorInfos = StageMembers()
                    };
                    list.Add(tacticsActorInfo);
                }
            }
            list.Sort((a,b) => a.ActorInfo.BattleIndex > b.ActorInfo.BattleIndex ? 1 : -1);
            foreach (var member in StageMembers())
            {
                if (member.BattleIndex < 1)
                {
                    var tacticsActorInfo = new TacticsActorInfo
                    {
                        TacticsCommandType = _tacticsCommandType,
                        ActorInfo = member,
                        ActorInfos = StageMembers()
                    };
                    list.Add(tacticsActorInfo);
                }
            }
            Func<TacticsActorInfo,bool> enable = (a) => 
            {
                // 同じステージ進捗度でWorldをまたげない
                var current = CurrentSelectRecord();
                var worldNo = CurrentStage.WorldNo == 0 ? 1 : 0;
                var levelUpDataInfos = a.ActorInfo.LevelUpInfos;
                if (levelUpDataInfos.Find(b => b.HasSameStageSeekBattleResultData(current.StageId,current.Seek,worldNo)) != null)
                {
                    a.DisableText = "同じ進行度の別世界でバトルを行っています";    
                    RemoveBattleActor(a.ActorInfo);
                    return false;
                }
                return true;
            };
            return MakeListData(list,enable,selectIndex);
        }

        public string TacticsCommandInputInfo()
        {
            switch (_tacticsCommandType)
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
            var tacticsCommandData = new TacticsCommandData
            {
                Title = CommandTitle()
            };
            return tacticsCommandData;
        }

        private string CommandTitle()
        {
            return DataSystem.GetText((int)_tacticsCommandType);
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

    }
}