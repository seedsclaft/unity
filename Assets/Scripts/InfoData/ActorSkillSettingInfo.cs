using System.Collections.Generic;

namespace Ryneus
{
    public class ActorSkillSettingInfo
    {
        public int X_Magic = 0;
        public int Y_Magic = 0;
        public int L1_Magic = 0;
        public int R1_Magic = 0;
        public List<int> PassiveMagic = new ();

        public void SetSkill(SkillSlotType skillSettingType,int skillId)
        {
            switch (skillSettingType)
            {
                case SkillSlotType.X:
                    X_Magic = skillId;
                    return;
                case SkillSlotType.Y:
                    Y_Magic = skillId;
                    return;
                case SkillSlotType.L1:
                    L1_Magic = skillId;
                    return;
                case SkillSlotType.R1:
                    R1_Magic = skillId;
                    return;
                case SkillSlotType.Passive:
                    if (!PassiveMagic.Contains(skillId))
                    {
                        PassiveMagic.Add(skillId);
                    }
                    return;
            }
        }

        public Dictionary<SkillSlotType,int> ActionSkillIds()
        {
            var dict = new Dictionary<SkillSlotType,int>();
            dict[SkillSlotType.X] = X_Magic;
            dict[SkillSlotType.Y] = Y_Magic;
            dict[SkillSlotType.L1] = L1_Magic;
            dict[SkillSlotType.R1] = R1_Magic;
            return dict;
        }
    }
}
