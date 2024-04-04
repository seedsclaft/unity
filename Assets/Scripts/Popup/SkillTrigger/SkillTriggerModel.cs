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
            list.Add("隊列・状況");
            list.Add("種族");
            list.Add("HP");
            list.Add("MP");
            list.Add("状態");
            list.Add("攻撃タイミング");
            list.Add("編成人数");
            list.Add("自身の状態");
            list.Add("自身のHp");
            list.Add("自身のMp");
            list.Add("一番高いステータス");
            list.Add("一番低いステータス");
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
                list = list.FindAll(a => (int)a.TargetType == -1 || a.TargetType == skill.TargetType);
            }
            // ソート
            list.Sort((a,b) => a.Priority > b.Priority ? 1 : -1);
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
    }
}