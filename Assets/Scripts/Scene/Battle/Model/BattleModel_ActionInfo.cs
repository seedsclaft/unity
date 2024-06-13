using System.Collections;
using System.Collections.Generic;

namespace Ryneus
{
    public partial class BattleModel : BaseModel
    {
        private List<ActionInfo> _actionInfos = new ();

        // 現在最優先の行動
        private ActionInfo _currentActionInfo = null;
        public ActionInfo CurrentActionInfo => _currentActionInfo;

        public void AddActionInfo(ActionInfo actionInfo,bool IsInterrupt)
        {
            if (IsInterrupt)
            {
                _actionInfos.Insert(0,actionInfo);
            } else
            {
                _actionInfos.Add(actionInfo);
            }
            _currentActionInfo = _actionInfos[0];
        }

        private void PopActionInfo()
        {
            _actionInfos.RemoveAt(0);
            _currentActionInfo = _actionInfos.Count > 0 ? _actionInfos[0] : null;
        }

        /// <summary>
        /// 行動を初期化
        /// </summary>
        public void ClearActionInfo()
        {
            _actionInfos.Clear();
        }

        // 行動を生成
        public ActionInfo MakeActionInfo(BattlerInfo subject,SkillInfo skillInfo,bool IsInterrupt,bool IsTrigger)
        {
            var skillData = skillInfo.Master;
            var targetIndexList = GetSkillTargetIndexList(skillInfo.Id,subject.Index,true);
            if (subject.IsState(StateType.Substitute))
            {
                int substituteId = subject.GetStateInfo(StateType.Substitute).BattlerId;
                if (targetIndexList.Contains(substituteId))
                {
                    targetIndexList.Clear();
                    targetIndexList.Add(substituteId);
                } else
                {
                    var tempIndexList = GetSkillTargetIndexList(skillInfo.Id,subject.Index,false);
                    if (tempIndexList.Contains(substituteId))
                    {
                        targetIndexList.Clear();
                        targetIndexList.Add(substituteId);
                    }
                }
            }
            int lastTargetIndex = -1;
            if (subject.IsActor)
            {
                lastTargetIndex = subject.LastTargetIndex();
                if (skillData.TargetType == TargetType.Opponent)
                {
                    var targetBattler = _troop.AliveBattlerInfos.Find(a => a.Index == lastTargetIndex && targetIndexList.Contains(lastTargetIndex));
                    if (targetBattler == null && _troop.BattlerInfos.Count > 0)
                    {
                        var containsOpponent = _troop.AliveBattlerInfos.Find(a => targetIndexList.Contains(a.Index));
                        if (containsOpponent != null)
                        {
                            lastTargetIndex = containsOpponent.Index;
                        }
                    }
                } else
                {
                    lastTargetIndex = subject.Index;
                    if (targetIndexList.Count > 0)
                    {
                        lastTargetIndex = targetIndexList[0];
                    }
                }
            }
            var actionInfo = new ActionInfo(skillInfo,_actionIndex,subject.Index,lastTargetIndex,targetIndexList);
            _actionIndex++;
            actionInfo.SetRangeType(CalcRangeType(actionInfo.Master,subject));
            var actionScopeType = CalcScopeType(subject,actionInfo);
            actionInfo.SetScopeType(actionScopeType);
            if (IsTrigger)
            {
                actionInfo.SetTriggerSkill(true);
            }
            AddTurnActionInfos(actionInfo,IsInterrupt);
            return actionInfo;
        }
    }
}
