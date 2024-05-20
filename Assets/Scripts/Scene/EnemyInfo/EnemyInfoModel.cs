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

        private int _currentIndex = 0; 
        public int CurrentIndex => _currentIndex;

        public List<ListData> EnemyInfoListDates()
        {
            return ListData.MakeListData(_enemyBattlerInfos,true);
        }

        public List<int> EnemyIndexes(){
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

        public BattlerInfo CurrentEnemy => _enemyBattlerInfos[_currentIndex];
        
        public List<ListData> SkillActionList()
        {
            var skillInfos = CurrentEnemy.Skills;//.FindAll(a => a.Master.Attribute != AttributeType.None);
            skillInfos.ForEach(a => a.SetEnable(true));
            skillInfos.Sort((a,b) => {return a.Id - b.Id;});
            return MakeListData(skillInfos);
        }

        public List<ListData> SelectCharacterConditions()
        {
            return MakeListData(CurrentEnemy.StateInfos);
        }

        public List<ListData> EnemySkillTriggerInfo()
        {
            var skillInfos = CurrentEnemy.Skills;
            skillInfos.Sort((a,b) => a.Weight > b.Weight ? -1:1);
            var skillTriggerInfos = new List<SkillTriggerInfo>();
            foreach (var skillInfo in skillInfos)
            {
                var skillTriggerData = DataSystem.Enemies.Find(a => a.Id == CurrentEnemy.EnemyData.Id).SkillTriggerDates.Find(a => a.SkillId == skillInfo.Id);
                if (skillTriggerData == null)
                {
                    continue;
                }
                var skillTriggerInfo = new SkillTriggerInfo(CurrentEnemy.EnemyData.Id,skillTriggerData.SkillId);
                var SkillTriggerData1 = DataSystem.SkillTriggers.Find(a => a.Id == skillTriggerData.Trigger1);
                var SkillTriggerData2 = DataSystem.SkillTriggers.Find(a => a.Id == skillTriggerData.Trigger2);
                skillTriggerInfo.UpdateTriggerDates(new List<SkillTriggerData>(){SkillTriggerData1,SkillTriggerData2});
                skillTriggerInfos.Add(skillTriggerInfo);
            }
            skillTriggerInfos = skillTriggerInfos.FindAll(a => skillInfos.Find(b => b.Id == a.SkillId) != null);
            return MakeListData(skillTriggerInfos);
        }
    }
}