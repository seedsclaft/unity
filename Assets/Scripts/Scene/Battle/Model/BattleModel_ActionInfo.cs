using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine.EventSystems;

namespace Ryneus
{
    public partial class BattleModel : BaseModel
    {
        // 現在の行動
        private ActionInfo _activeActionInfo = null;
        public ActionInfo ActiveActionInfo => _activeActionInfo;
        public void SetActiveActionInfo(ActionInfo actionInfo)
        {
            _activeActionInfo = actionInfo;
        }

        // 優先敵対象
        private BattlerInfo _targetBattler = null;
        public BattlerInfo TargetBattler => _targetBattler;
        public void SetTargetBattler(BattlerInfo battlerInfo) => _targetBattler = battlerInfo;

        private List<ActionInfo> _receiveActionInfos = new ();
        // 誘発した行動
        private ActionInfo _receiveActionInfo = null;
        public ActionInfo ReceiveActionInfo => _receiveActionInfo;
        /// <summary>
        /// 誘発したアクションを登録する
        /// </summary>
        /// <param name="actionInfo"></param>
        /// <param name="indexList"></param>
        /// <param name="IsInterrupt"></param>
        public void AddReceiveActionInfo(ActionInfo actionInfo,List<int> indexList,bool IsInterrupt)
        {
            SetActionInfoParameter(actionInfo);
            MakeActionResultInfo(actionInfo,indexList);
            AddActionInfo(actionInfo,IsInterrupt);
            AddTurnActionInfos(actionInfo,IsInterrupt);
        }

        public void AddActionInfo(ActionInfo actionInfo,bool IsInterrupt)
        {
            if (IsInterrupt)
            {
                //LogOutput.Log(actionInfo.Master.Id + "を割り込み");
                _receiveActionInfos.Insert(0,actionInfo);
            } else
            {
                //LogOutput.Log(actionInfo.Master.Id + "を後に追加");
                _receiveActionInfos.Add(actionInfo);
            }
            _receiveActionInfo = _receiveActionInfos[0];
        }

        public List<ActionInfo> BeforeActionInfo(ActionInfo targetActionInfo)
        {
            var list = new List<ActionInfo>();
            var findIndex = _receiveActionInfos.FindIndex(a => a == targetActionInfo);
            var idx = 0;
            foreach (var actionInfo in _receiveActionInfos)
            {
                if (idx < findIndex)
                {
                    list.Add(actionInfo);
                }
                idx++;
            }
            return list;
        }

        public void RemoveActionInfo(ActionInfo targetActionInfo)
        {
            var findIndex = _receiveActionInfos.FindIndex(a => a == targetActionInfo);
            if (findIndex > -1)
            {
                _receiveActionInfos.RemoveAt(findIndex);
            }
            if (_receiveActionInfos.Count > 0)
            {
                _receiveActionInfo = _receiveActionInfos[0];
            }
        }

        private void PopActionInfo(ActionInfo actionInfo)
        {
            var findIndex = _receiveActionInfos.FindIndex(a => a == actionInfo);
            if (findIndex > -1)
            {
                _receiveActionInfos.RemoveAt(findIndex);
            }
            _receiveActionInfo = _receiveActionInfos.Count > 0 ? _receiveActionInfos[0] : null;
        }

        /// <summary>
        /// 行動を初期化
        /// </summary>
        public void ClearActionInfo()
        {
            _receiveActionInfos.Clear();
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

        public void HitWeakPoint(int targetIndex,int skillId)
        {
            var target = GetBattlerInfo(targetIndex);
            if (!target.IsActor)
            {
                var kindType = (KindType)DataSystem.FindSkill(skillId).Attribute;
                CurrentData.PlayerInfo.AddEnemyWeakPointDict(target.EnemyData.Id,kindType);
                target.SetWeakPoint(kindType);
            }
        }
    }
}
