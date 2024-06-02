using System.Collections;
using System.Collections.Generic;

namespace Ryneus
{
    public class BattleReplayModel : BattleModel
    {
        private int _replayIndex = 1;
        public void SeekReplayCounter()
        {
            _replayIndex++;
        }
        private SaveBattleInfo _saveBattleInfo;
        private UnitInfo _party;
        private UnitInfo _troop;
        private List<BattlerInfo> _battlers;
        private BattlerInfo _currentBattler;
        public void SetSaveBattleInfo(SaveBattleInfo saveBattleInfo)
        {
            _party = saveBattleInfo.Party;
            _troop = saveBattleInfo.Troop;
            var list = new List<BattlerInfo>();
            list.AddRange(_party.BattlerInfos);
            list.AddRange(_troop.BattlerInfos);
            _battlers = list;
            _saveBattleInfo = saveBattleInfo;
        }

        public ActionInfo GetSaveActionData()
        {
            if (_saveBattleInfo.actionInfos.ContainsKey(_replayIndex))
            {
                var data = _saveBattleInfo.actionInfos[_replayIndex];
                return data;
            }
            return null;
        }
        public List<ActionResultInfo> GetSaveResultData()
        {
            if (_saveBattleInfo.actionResultInfos.ContainsKey(_replayIndex))
            {
                var data = _saveBattleInfo.actionResultInfos[_replayIndex];
                return data;
            }
            return null;
        }

        public new ActionInfo CurrentActionInfo()
        {
            return GetSaveActionData();
        }

        public new void ExecCurrentActionResult()
        {
            var actionInfo = GetSaveActionData();
            if (actionInfo != null)
            {
                var subject = GetBattlerInfo(actionInfo.SubjectIndex);
                // 支払いは最後の1回
                if (actionInfo.RepeatTime == 0)
                {
                    // Hpの支払い
                    subject.GainHp(actionInfo.HpCost * -1);
                    // Mpの支払い
                    subject.GainMp(actionInfo.MpCost * -1);
                    subject.GainPayBattleMp(actionInfo.MpCost);
                }
                if (actionInfo.Master.IsHpHealFeature())
                {
                    subject.GainHealCount(1);
                }
                //var actionResultInfos = CalcDeathIndexList(actionInfo.ActionResults);
                ExecActionResultInfos(actionInfo.ActionResults);
                //actionInfo.SetTurnCount(_turnCount);
                if (actionInfo.Master.IsRevengeHpDamageFeature())
                {
                    // 受けたダメージをリセット
                    subject.SetDamagedValue(0);
                }
            }
        }
    }
}
