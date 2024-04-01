using System;
using System.Collections.Generic;
using UnityEngine;

namespace Ryneus
{
    [Serializable]
    public class SkillTriggerInfo
    {
        private int _actorId = -1;
        public int ActorId => _actorId;
        public void SetActorId(int actorId)
        {
            _actorId = actorId;
        }
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
        private List<TriggerType> _triggerTypes = new ();
        public List<TriggerType> TriggerTypes => _triggerTypes;
        public void UpdateTriggerType(List<TriggerType> triggerTypes)
        {
            _triggerTypes = triggerTypes;
        }

    }
}
