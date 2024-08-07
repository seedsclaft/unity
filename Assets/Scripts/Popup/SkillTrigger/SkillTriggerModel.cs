using System;
using System.Collections.Generic;

namespace Ryneus
{
    public class SkillTriggerModel : BaseModel
    {
        private List<int> _categoryIndexes = new ();

        public SkillTriggerModel()
        {
            int startIndex = 1;
            for (int i = startIndex;i <= 13;i++)
            {
                if (i != 4 && i != 11)
                {
                    _categoryIndexes.Add(i);
                }
            }
        }
        private int _actorId = -1;
        public ActorInfo CurrentActor => StageMembers().Find(a => a.ActorId == _actorId);
        public List<ListData> SkillTrigger(int actorId,int selectIndex = 0)
        {
            _actorId = actorId;
            Func<SkillTriggerInfo,bool> enable = (a) => 
            {
                return a.SkillId == 0 || SkillActionList(CurrentActor).Find(b => (b.Data as SkillInfo).LearningState == LearningState.Learned && (b.Data as SkillInfo).Id == a.SkillId) != null;
            };
            var listData = MakeListData(CurrentActor.SkillTriggerInfos,enable,selectIndex);
            return listData;
        }

        public int SelectCategoryIndex(int selectIndex,int skillTriggerIndex)
        {
            var categoryIndex = 1;
            var skillTriggerInfo = CurrentActor.SkillTriggerInfos[selectIndex];
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
            var list = new List<SkillInfo>();
            if (CurrentActor != null)
            {
                var skillInfo = new SkillInfo(0);
                list.Add(skillInfo);
                var listData = MakeListData(list);
                foreach (var actionInfo in SkillActionList(CurrentActor))
                {
                    listData.Add(actionInfo);
                }
                return listData;
            }
            return MakeListData(list);
        }
        
        public List<string> SkillTriggerCategoryList()
        {
            var list = new List<string>();
            foreach (var categoryIndex in _categoryIndexes)
            {
                list.Add(DataSystem.GetText(categoryIndex + 24109));
            }
            return list;
        }

        public List<SkillTriggerData> SkillTriggerDataList(int index,int category)
        {
            var list = DataSystem.SkillTriggers.FindAll(a => a.Category == -1 || a.Category == _categoryIndexes[category-1]);
            // 対象のマッチング
            var skillTriggerData = CurrentActor.SkillTriggerInfos;
            if (skillTriggerData.Count > index)
            {
                var skill = DataSystem.FindSkill(skillTriggerData[index].SkillId);
                switch (skill.TargetType)
                {
                    case TargetType.All:
                    list = list.FindAll(a => (int)a.TargetType == -1 || a.TargetType == TargetType.All || a.TargetType == TargetType.Friend || a.TargetType == TargetType.Opponent);
                    break;
                    case TargetType.IsTriggerTarget:
                    if (skill.IsHpHealFeature())
                    {
                        list = list.FindAll(a => (int)a.TargetType == -1 || a.TargetType == TargetType.All || a.TargetType == TargetType.Friend);
                    }
                    break;
                    case TargetType.Counter:
                    case TargetType.AttackTarget:
                    list = list.FindAll(a => (int)a.TargetType == -1 || a.TargetType == TargetType.All || a.TargetType == TargetType.Opponent);
                    break;
                    default:
                    list = list.FindAll(a => (int)a.TargetType == -1 || a.TargetType == skill.TargetType);
                    break;
                }
            }
            // ソート
            list.Sort((a,b) => a.Priority >= b.Priority ? 1 : -1);
            return list;
        }

        public void SetSkillTriggerSkill(int index,int skillId)
        {
            var skills = CurrentActor.SkillInfos();
            CurrentActor.SetSkillTriggerSkill(index,skills.Find(a => a.Id == skillId));
        }

        public void SetSkillTrigger1(int index,SkillTriggerData triggerData)
        {
            CurrentActor.SetSkillTriggerTrigger1(index,triggerData);
        }

        public void SetSkillTrigger2(int index,SkillTriggerData triggerData)
        {
            CurrentActor.SetSkillTriggerTrigger2(index,triggerData);
        }

        public void SetTriggerIndexUp(int index)
        {
            CurrentActor.SetTriggerIndexUp(index);
        }

        public void SetTriggerIndexDown(int index)
        {
            CurrentActor.SetTriggerIndexDown(index);
        }
    }
}