using System;
using System.Collections.Generic;

namespace Ryneus
{
    [Serializable]
    public class SkillTriggerInfo
    {
        public SkillTriggerInfo(int actorId,SkillInfo skillInfo)
        {
            _actorId = actorId;
            _skillInfo = skillInfo;
        }

        private int _actorId = -1;
        public int ActorId => _actorId;
        private int _priority = -1;
        public int Priority => _priority;
        public void SetPriority(int priority)
        {
            _priority = priority;
        }
        public int SkillId => _skillInfo.Id;
        private SkillInfo _skillInfo = null;
        public SkillInfo SkillInfo => _skillInfo;
        public void SetSkillInfo(SkillInfo skillInfo)
        {
            _skillInfo = skillInfo;
        }
        private List<SkillTriggerData> _skillTriggerDates = new ();
        public List<SkillTriggerData> SkillTriggerDates => _skillTriggerDates;
        public void UpdateTriggerDates(List<SkillTriggerData> skillTriggerDates)
        {
            _skillTriggerDates = skillTriggerDates;
        }

    }
}
