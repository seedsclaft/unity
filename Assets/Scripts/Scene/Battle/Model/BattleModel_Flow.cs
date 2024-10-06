using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ryneus
{
    public partial class BattleModel : BaseModel
    {
        private BattlerInfo _currentBattler = null;
        public BattlerInfo CurrentBattler => _currentBattler;

        // ターンの最初の行動開始者
        private BattlerInfo _firstActionBattler = null;
        public BattlerInfo FirstActionBattler => _firstActionBattler;
        public void SetFirstActionBattler(BattlerInfo firstActionBattler)
        {
            _firstActionBattler = firstActionBattler;
        }

        public BattlerInfo CheckApCurrentBattler()
        {
            var battlerInfos = FieldBattlerInfos().FindAll(a => a.IsAlive());
            battlerInfos.Sort((a,b) => (int)a.Ap - (int)b.Ap);
            _currentBattler = battlerInfos.Find(a => a.Ap <= 0);
            return _currentBattler;
        }

        public List<int> MakeActionInfoTargetIndexes(BattlerInfo battlerInfo,int skillId,int oneTargetIndex = -1)
        {
            var skillInfo = battlerInfo.Skills.Find(a => a.Id == skillId);
            if (skillInfo == null)
            {
                skillInfo = new SkillInfo(skillId);
            }
            var actionInfo = MakeActionInfo(battlerInfo,skillInfo,false,false);
            AddActionInfo(actionInfo,false);
            // 対象を自動決定
            return MakeAutoSelectIndex(actionInfo,oneTargetIndex);
        }

        /// <summary>
        /// ActionInfoの要素を決定する
        /// </summary>
        /// <param name="actionInfo"></param>
        public void SetActionInfoParameter(ActionInfo actionInfo)
        {
            var subject = GetBattlerInfo(actionInfo.SubjectIndex);
            //int MpCost = CalcMpCost(subject,actionInfo.Master.CountTurn);
            //actionInfo.SetMpCost(MpCost);
            int HpCost = CalcHpCost(actionInfo);
            actionInfo.SetHpCost(HpCost);

            //var isPrism = PrismRepeatTime(subject,actionInfo) > 0;
            var repeatTime = CalcRepeatTime(subject,actionInfo);
            //repeatTime += PrismRepeatTime(subject,actionInfo);
            actionInfo.SetRepeatTime(repeatTime);
            actionInfo.SetBaseRepeatTime(repeatTime);
        }
    }
}
