using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;
using System.Linq;

namespace Ryneus
{
    public class ResultModel : BaseModel
    {
        public List<ListData> ResultCommand()
        {
            var listDates = MakeListData(BaseConfirmCommand(3040,6));
            foreach (var listDate in listDates)
            {
                var data = (SystemData.CommandData)listDate.Data;
                /*
                if (data.Id == 1)
                {
                    listDate.SetEnable(false);
                }
                */
            }
            return listDates;
        }

        public string EndingTypeText()
        {
            return DataSystem.GetText(16080) + CurrentStage.EndingType;
        }

        public bool IsNewRecord()
        {
            return TotalEvaluate() > CurrentData.PlayerInfo.GetBestScore(CurrentStage.Id);
        }

        public void ApplyScore()
        {
            CurrentData.PlayerInfo.SetBestScore(CurrentStage.Id,TotalEvaluate());
        }

        public List<ListData> StageEndCommand()
        {
            var listDates = MakeListData(BaseConfirmCommand(16020,6));
            foreach (var listDate in listDates)
            {
                var data = (SystemData.CommandData)listDate.Data;
                /*
                if (data.Id == 1)
                {
                    listDate.SetEnable(false);
                }
                */
            }
            return listDates;
        }

        public void SetActors()
        {
        }

        // アルカナ新規入手
        public bool CheckIsAlcana()
        {
            return CurrentStage.Master.Alcana;
        }

        private List<SkillInfo> AddMagicRebornSkill()
        {
            var list = new List<SkillInfo>();
            return list;
        }

        private int _rebornActorIndex = 0;
        
        public List<ListData> ActorInfos()
        {
            var listDates = MakeListData(CurrentData.PlayerInfo.SaveActorList);
            foreach (var listDate in listDates)
            {
                var actorInfo = (ActorInfo)listDate.Data;
                if (actorInfo.Master.ClassId == DataSystem.FindActor(PartyInfo.ActorInfos[0].ActorId).ClassId)
                {
                    //listDate.SetEnable(false);
                }
            }
            return listDates;
        }

        public List<ListData> EraseActorInfos()
        {
            var listDates = MakeListData(CurrentData.PlayerInfo.SaveActorList);
            var disable = DisableActorIndexes();
            foreach (var listDate in listDates)
            {
                var actorInfo = (ActorInfo)listDate.Data;
                if (disable.Contains(actorInfo.Master.ClassId))
                {
                    listDate.SetEnable(false);
                }
            }
            return listDates;
        }

        public List<int> DisableActorIndexes()
        {
            var list = new List<int>();
            var evaluateActorInfo = StageMembers()[0];
            var actorListIndexes = new List<int>();
            foreach (var listData in ActorInfos())
            {
                var actorInfo = (ActorInfo)listData.Data;
                if (!actorListIndexes.Contains(actorInfo.Master.ClassId))
                {
                    actorListIndexes.Add(actorInfo.Master.ClassId);
                }
            }
            // 2キャラしかいない
            if (actorListIndexes.Count == 2)
            {
                if (actorListIndexes[0] == evaluateActorInfo.Master.ClassId)
                {
                    list.Add(actorListIndexes[1]);
                } else
                {
                    list.Add(actorListIndexes[0]);
                }
            }
            return list;
        }

        public ActorInfo RebornActorInfo()
        {
            var actorInfos = ActorInfos();
            if (actorInfos.Count > _rebornActorIndex)
            {
                return (ActorInfo)actorInfos[_rebornActorIndex].Data;
            }
            return null;
        }

        public void SetRebornActorIndex(int index)
        {
            _rebornActorIndex = index;
        }

        public void EraseReborn()
        {
            CurrentData.PlayerInfo.EraseReborn(_rebornActorIndex);
        }

        public SlotInfo MakeSlotInfo()
        {
            var list = new List<ActorInfo>();
            foreach (var evaluateMember in StageMembers())
            {
                evaluateMember.ResetData();
                list.Add(evaluateMember);
            }
            var slotInfo = new SlotInfo(StageMembers());
            slotInfo.SetTimeRecord();
            return slotInfo;
        }
    }
}