using System;
using System.Collections.Generic;

namespace Ryneus
{
    public class SkillTriggerModel : BaseModel
    {
        private int _actorId = -1;
        public List<ListData> SkillTrigger(int actorId)
        {
            _actorId = actorId;
            return MakeListData(PartyInfo.SkillTriggerInfos(_actorId));
        }

        public List<ListData> SkillTriggerSkillList()
        {
            var actorInfo = StageMembers().Find(a => a.ActorId == _actorId);
            var list = new List<SkillInfo>();
            if (actorInfo != null)
            {
                return actorInfo.SkillActionList();
            }
            return MakeListData(list);
        }
        
        public List<ListData> SkillTriggerCategoryList()
        {
            var list = new List<string>();
            for (int i = 24110;i <= 24122;i++)
            {
                list.Add(DataSystem.GetText(i));
            }
            return MakeListData(list);
        }

        public List<ListData> SkillTriggerDataList(int index,int category)
        {
            var list = DataSystem.SkillTriggers.FindAll(a => a.Category == -1 || a.Category == category);
            // 対象のマッチング
            var skillTriggerData = PartyInfo.SkillTriggerInfos(_actorId);
            if (skillTriggerData.Count > index)
            {
                var skill = DataSystem.FindSkill(skillTriggerData[index].SkillId);
                if (skill.TargetType == TargetType.All)
                {
                    list = list.FindAll(a => (int)a.TargetType == -1 || a.TargetType == TargetType.All || a.TargetType == TargetType.Friend || a.TargetType == TargetType.Opponent);
                } else
                if (skill.TargetType == TargetType.IsTriggerTarget)
                {
                    if (skill.IsHpHealFeature())
                    {
                        list = list.FindAll(a => (int)a.TargetType == -1 || a.TargetType == TargetType.All || a.TargetType == TargetType.Friend);
                    }
                } else
                {
                    list = list.FindAll(a => (int)a.TargetType == -1 || a.TargetType == skill.TargetType);
                }
            }
            // ソート
            list.Sort((a,b) => a.Priority >= b.Priority ? 1 : -1);
            return MakeListData(list);
        }

        public void SetSkillTriggerSkill(int index,int skillId)
        {
            PartyInfo.SetSkillTriggerSkill(_actorId,index,skillId);
        }

        public void SetSkillTrigger1(int index,SkillTriggerData triggerData)
        {
            PartyInfo.SetSkillTriggerTrigger1(_actorId,index,triggerData);
        }

        public void SetSkillTrigger2(int index,SkillTriggerData triggerData)
        {
            PartyInfo.SetSkillTriggerTrigger2(_actorId,index,triggerData);
        }

        public void SetTriggerIndexUp(int index)
        {
            PartyInfo.SetTriggerIndexUp(_actorId,index);
        }

        public void SetTriggerIndexDown(int index)
        {
            PartyInfo.SetTriggerIndexDown(_actorId,index);
        }
    }
}