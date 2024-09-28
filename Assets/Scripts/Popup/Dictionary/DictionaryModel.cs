using System;
using System.Collections.Generic;

namespace Ryneus
{
    public class DictionaryModel : BaseModel
    {
        private int _completeRate = 0;
        public int CompeteRate => _completeRate;
        public DictionaryModel()
        {
            var max = 0;
            var count = 0;
            foreach (var skill in DataSystem.Skills)
            {
                if (skill.Key < 1000) continue;
                if (skill.Key % 10 != 0) continue;
                var skillInfo = new SkillInfo(skill.Key);
                if (skillInfo.Master.Attribute == AttributeType.None)
                {
                    continue;
                }
                max++;
                if (CurrentData.PlayerInfo.SkillIds.Contains(skillInfo.Id))
                {
                    count++;
                }
            }
            _completeRate = count > 0 ? (int)((float)count*100 / max) : 0;
        }

        public List<SkillType> SkillCategory()
        {
            var skillTypes = new List<SkillType>();
            var enums = Enum.GetValues(typeof(SkillType));
            foreach (var e in enums)
            {
                var skillType = (SkillType)Enum.ToObject(typeof(SkillType), e);
                if (skillType < SkillType.Active || skillType > SkillType.Enhance) continue;
                if (!skillTypes.Contains(skillType))
                {
                    skillTypes.Add(skillType);
                }
            }
            return skillTypes;
        }

    
        public List<SkillInfo> CategorySkillList(SkillType skillType)
        {
            var skillList = new List<SkillInfo>();
            foreach (var skill in DataSystem.Skills)
            {
                if (skill.Key < 1000) continue;
                if (skill.Key % 10 != 0) continue;
                if (skill.Value.SkillType == skillType)
                {
                    var skillInfo = new SkillInfo(skill.Key);
                    if (skillInfo.Master.Attribute == AttributeType.None)
                    {
                        continue;
                    }
                    skillInfo.SetEnable(true);
                    skillList.Add(skillInfo);
                }
            }
            return skillList;
        }
    }
}
