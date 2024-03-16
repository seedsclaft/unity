using System;
using System.Collections;
using System.Collections.Generic;

namespace Ryneus
{
    [Serializable]
    public class AlcanaInfo{
        private bool _IsAlcana = true;
        public bool IsAlcana => _IsAlcana;
        private List<SkillInfo> _ownAlcanaList = new ();
        public List<SkillInfo> OwnAlcanaList => _ownAlcanaList;
        public List<SkillInfo> EnableOwnAlcanaList => _ownAlcanaList.FindAll(a => a.Enable);


        public AlcanaInfo(){
            InitData(false);
        }

        public void InitData(bool isTestMode)
        {
            _ownAlcanaList.Clear();
            if (isTestMode)
            {
                for (var i = 1;i <= 22;i++)
                {
                    var alcana = new SkillInfo(500000 + i * 10);
                    alcana.SetEnable(false);
                    _ownAlcanaList.Add(alcana);
                }
            }
        }

        public List<SkillInfo> CheckAlcanaSkillInfo(TriggerTiming triggerTiming)
        {
            return _ownAlcanaList.FindAll(a => a.Enable && a.Master.TriggerDates.Find(b => b.TriggerTiming == triggerTiming) != null);
        }

        public void DisableAlcana(SkillInfo skillInfo)
        {
            skillInfo.SetEnable(false);
        }

        public void SetIsAlcana(bool isAlcana)
        {
            _IsAlcana = isAlcana;
        }

        public void RefreshOwnAlcana(List<SkillInfo> ownAlcanaSkillInfos)
        {
            _ownAlcanaList = ownAlcanaSkillInfos;
        }

        public void CheckEnableSkillTrigger()
        {
            foreach (var alcana in _ownAlcanaList)
            {
                var triggerDates = alcana.Master.TriggerDates;
                foreach (var triggerData in triggerDates)
                {
                    if (triggerData.TriggerType == TriggerType.ExtendStageTurn)
                    {
                        var enable = GameSystem.CurrentStageData.CurrentStage.CurrentTurn <= 0;
                        alcana.SetEnable(enable);
                    }
                }
            }
        }
    }
}