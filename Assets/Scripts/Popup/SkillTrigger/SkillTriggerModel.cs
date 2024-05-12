using System;
using System.Collections.Generic;

namespace Ryneus
{
    public class SkillTriggerModel : BaseModel
    {
        private int _actorId = -1;
        public List<ListData> SkillTrigger(int actorId,int selectIndex = -1)
        {
            _actorId = actorId;
            var listData = MakeListData(PartyInfo.SkillTriggerInfos(_actorId),selectIndex);
            return listData;
        }

        public int SelectCategoryIndex(int selectIndex,int skillTriggerIndex)
        {
            var categoryIndex = 1;
            var skillTriggerInfo = PartyInfo.SkillTriggerInfos(_actorId)[selectIndex];
            if (skillTriggerInfo != null)
            {
                categoryIndex = skillTriggerInfo.SkillTriggerDates[skillTriggerIndex].Category;
            }
            if (categoryIndex <= 0)
            {
                categoryIndex = 1;
            }
            return categoryIndex;
        }

        public List<ListData> SkillTriggerSkillList()
        {
            var actorInfo = StageMembers().Find(a => a.ActorId == _actorId);
            var list = new List<SkillInfo>();
            if (actorInfo != null)
            {
                var skillInfo = new SkillInfo(0);
                list.Add(skillInfo);
                var listData = MakeListData(list);
                foreach (var actionInfo in actorInfo.SkillActionList())
                {
                    listData.Add(actionInfo);
                }
                return listData;
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