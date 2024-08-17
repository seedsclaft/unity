using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Unity.VisualScripting;

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

        public int SelectActorEvaluate()
        {
            return TacticsActor().Evaluate();
        }

        public void LearnMagic(int skillId)
        {
            ActorLearnMagic(TacticsActor(),skillId);
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

        public int SelectedActorIdBySelectIndex(List<ListData> listData,int selectIndex)
        {
            var actorInfo = (TacticsActorInfo)listData[selectIndex].Data;
            SetSelectActorId(actorInfo.ActorInfo.ActorId);
            return actorInfo.ActorInfo.ActorId;
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
            return ActorLearningMagicList(TacticsActor(),_selectAttribute,selectedSkillId);
        }

        public List<AttributeType> AttributeTabList()
        {
            var list = new List<AttributeType>();
            foreach (var attribute in Enum.GetValues(typeof(AttributeType)))
            {
                var attributeType = (AttributeType)attribute;
                list.Add(attributeType);
            }
            return list;
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