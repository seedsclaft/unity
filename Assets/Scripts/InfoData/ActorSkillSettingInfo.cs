using System.Collections.Generic;
using System.Diagnostics;

namespace Ryneus
{
    public class ActorSkillSettingInfo
    {
        public int X_Magic = 0;
        public int Y_Magic = 0;
        public int L1_Magic = 0;
        public int R1_Magic = 0;
        public int PassiveSlot = 1;
        public List<int> PassiveMagic = new ();

        public void SetSkill(SkillSlotType skillSettingType,int skillId)
        {
            switch (skillSettingType)
            {
                case SkillSlotType.Y:
                    Y_Magic = skillId;
                    return;
                case SkillSlotType.X:
                    X_Magic = skillId;
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
            var dict = new Dictionary<SkillSlotType,int>
            {
                [SkillSlotType.Y] = Y_Magic,
                [SkillSlotType.X] = X_Magic,
                [SkillSlotType.L1] = L1_Magic,
                [SkillSlotType.R1] = R1_Magic,
            };
            return dict;
        }

        public List<int> PassiveSkillIds()
        {
            var list = new List<int>();
            for (int i = 0;i < PassiveSlot;i++)
            {
                if (PassiveMagic.Count > i)
                {
                    list.Add(PassiveMagic[i]);
                } else
                {
                    list.Add(0);
                }
            }
            return list;
        }
    }
}
