using System.Collections;
using System.Collections.Generic;

namespace Ryneus
{
    public class EnemyInfoModel : BaseModel
    {
        public EnemyInfoModel(List<BattlerInfo> enemyInfos)
        {
            _enemyBattlerInfos = enemyInfos;
        }
        
        private List<BattlerInfo> _enemyBattlerInfos = new();
        public List<BattlerInfo> EnemyBattlerInfos => _enemyBattlerInfos;

        private int _currentIndex = 0; 
        public int CurrentIndex => _currentIndex;

        public List<int> EnemyIndexes()
        {
            var list = new List<int>();
            foreach (var enemy in _enemyBattlerInfos)
            {
                list.Add(enemy.Index);
            }
            return list;
        }

        public void SelectEnemyIndex(int selectIndex)
        {
            _currentIndex = selectIndex;
        }

        public BattlerInfo CurrentEnemy => _currentIndex > -1 ? _enemyBattlerInfos[_currentIndex] : null;
        
        public List<SkillInfo> SkillActionList()
        {
            var skillInfos = new List<SkillInfo>();
            if (CurrentEnemy != null)
            {
                skillInfos = CurrentEnemy.Skills;
            }
            skillInfos.ForEach(a => a.SetEnable(true));
            skillInfos.Sort((a,b) => {return a.Id - b.Id;});
            return skillInfos;
        }

        public List<StateInfo> SelectCharacterConditions()
        {
            var stateInfos = new List<StateInfo>();
            if (CurrentEnemy != null)
            {
                stateInfos = CurrentEnemy.StateInfos;
            }
            return stateInfos;
        }

        public List<SkillTriggerInfo> EnemySkillTriggerInfo()
        {
            var skillInfos = new List<SkillInfo>();
            if (CurrentEnemy != null)
            {
                skillInfos = CurrentEnemy.Skills;
            }
            skillInfos.Sort((a,b) => a.Weight > b.Weight ? -1:1);
            var skillTriggerInfos = new List<SkillTriggerInfo>();
            foreach (var skillInfo in skillInfos)
            {
                var skillTriggerData = DataSystem.Enemies.Find(a => a.Id == CurrentEnemy.EnemyData.Id).SkillTriggerDates.Find(a => a.SkillId == skillInfo.Id);
                if (skillTriggerData == null)
                {
                    continue;
                }
                var skillTriggerInfo = new SkillTriggerInfo(CurrentEnemy.EnemyData.Id,skillInfo);
                var SkillTriggerData1 = DataSystem.SkillTriggers.Find(a => a.Id == skillTriggerData.Trigger1);
                var SkillTriggerData2 = DataSystem.SkillTriggers.Find(a => a.Id == skillTriggerData.Trigger2);
                skillTriggerInfo.UpdateTriggerDates(new List<SkillTriggerData>(){SkillTriggerData1,SkillTriggerData2});
                skillTriggerInfos.Add(skillTriggerInfo);
            }
            skillTriggerInfos = skillTriggerInfos.FindAll(a => skillInfos.Find(b => b.Id == a.SkillId) != null);
            return skillTriggerInfos;
        }
    }
}