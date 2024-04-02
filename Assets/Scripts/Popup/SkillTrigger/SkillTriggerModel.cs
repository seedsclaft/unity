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
        
        public List<ListData> SkillTriggerDataList()
        {
            var list = DataSystem.SkillTriggers.FindAll(a => a.Category > 0);
            return MakeListData(list);
        }

        public void SetSkillTriggerSkill(int index,int skillId)
        {
            PartyInfo.SetSkillTriggerSkill(_actorId,index,skillId);
        }

        public void SetSkillTrigger1(int index,SkillTriggerData triggerType)
        {
            PartyInfo.SetSkillTriggerTrigger1(_actorId,index,triggerType);
        }
        public void SetSkillTrigger2(int index,SkillTriggerData triggerType)
        {
            PartyInfo.SetSkillTriggerTrigger2(_actorId,index,triggerType);
        }
    }
}