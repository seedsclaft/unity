using System;
using System.Collections.Generic;

namespace Ryneus
{
    [Serializable]
    public class SkillTriggerInfo
    {
        public SkillTriggerInfo(int actorId,int skillId)
        {
            _actorId = actorId;
            _skillId = skillId;
        }

        private int _actorId = -1;
        public int ActorId => _actorId;
        private int _priority = -1;
        public int Priority => _priority;
        public void SetPriority(int priority)
        {
            _priority = priority;
        }
        private int _skillId = -1;
        public int SkillId => _skillId;
        public void SetSkillId(int skillId)
        {
            _skillId = skillId;
        }
        private List<SkillTriggerData> _skillTriggerDates = new ();
        public List<SkillTriggerData> SkillTriggerDates => _skillTriggerDates;
        public void UpdateTriggerDates(List<SkillTriggerData> skillTriggerDates)
        {
            _skillTriggerDates = skillTriggerDates;
        }

    }
}
