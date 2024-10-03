using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace Ryneus
{
    public partial class BattleModel : BaseModel
    {
        private List<SkillData.TriggerData> ConvertTriggerDates(List<SkillTriggerData> skillTriggerDates)
        {
            var triggerDates = new List<SkillData.TriggerData>();
            foreach (var skillTriggerData in skillTriggerDates)
            {
                if (skillTriggerData != null && skillTriggerData.Id > 0)
                {
                    var triggerData = new SkillData.TriggerData
                    {
                        TriggerType = skillTriggerData.TriggerType,
                        Param1 = skillTriggerData.Param1,
                        Param2 = skillTriggerData.Param2,
                        Param3 = skillTriggerData.Param3,
                    };
                    triggerDates.Add(triggerData);
                }
            }
            return triggerDates;
        }

        /// <summary>
        /// 作戦結果のスキルIDと対象を取得
        /// </summary>
        /// <param name="battlerInfo"></param>
        /// <param name="skillTriggerInfos"></param>
        /// <param name="actionInfo"></param>
        /// <param name="actionResultInfos"></param>
        /// <returns></returns>
        private (int,int) SelectSkillTargetBySkillTriggerDates(BattlerInfo battlerInfo,List<SkillTriggerInfo> skillTriggerInfos,ActionInfo actionInfo = null,List<ActionResultInfo> actionResultInfos = null)
        {
            var selectSkillId = -1;
            var selectTargetIndex = -1;
            var counterSubjectIndex = actionInfo != null ? actionInfo.SubjectIndex : -1;
            foreach (var skillTriggerInfo in skillTriggerInfos)
            {
                // 条件
                var triggerDates = ConvertTriggerDates(skillTriggerInfo.SkillTriggerDates);
                if (selectSkillId == -1 && selectTargetIndex == -1 && skillTriggerInfo.SkillId > 0)
                {
                    // 条件なし
                    if (triggerDates.Count == 0)
                    {
                        selectSkillId = skillTriggerInfo.SkillId;
                    } else
                    {
                        if (triggerDates.Count == 2)
                        {
                            triggerDates[0].Param3 = 1; // and判定
                        }
                        if (CanUseSkillTrigger(triggerDates,battlerInfo))
                        {           
                            selectSkillId = skillTriggerInfo.SkillId;
                        }
                    }
                }
                // 優先指定の判定
                if (selectSkillId > -1 && selectTargetIndex == -1)
                {
                    var targetIndexList = GetSkillTargetIndexList(selectSkillId,battlerInfo.Index,true,counterSubjectIndex,actionInfo,actionResultInfos);
                    if (targetIndexList.Count == 0)
                    {
                        var triggeredSkill = DataSystem.FindSkill(selectSkillId);
                        if (actionResultInfos != null && triggeredSkill != null && triggeredSkill.TargetType == TargetType.IsTriggerTarget)
                        {
                            targetIndexList = TriggerTargetList(battlerInfo,triggeredSkill.TriggerDates[0],actionInfo,actionResultInfos,triggeredSkill.AliveType);
                        }
                        if (targetIndexList.Count == 0)
                        {
                            selectSkillId = -1;
                        }
                    }
                    var target = CanUseSkillTriggerTarget(selectSkillId,triggerDates,battlerInfo,targetIndexList);
                    if (target > -1)
                    {
                        // 対象が有効か
                        var condition = CanUseCondition(selectSkillId,battlerInfo,target);
                        if (condition)
                        {
                            selectTargetIndex = target;
                        } else
                        {
                            selectSkillId = -1;
                        }
                    } else
                    {
                        selectSkillId = -1;
                    }
                }
                if (selectSkillId > -1 && selectTargetIndex > -1)
                {
                    return (selectSkillId,selectTargetIndex);
                }
            }
            return (selectSkillId,selectTargetIndex);
        }

        private bool CanUseSkillTrigger(List<SkillData.TriggerData> triggerDates,BattlerInfo battlerInfo)
        {
            bool CanUse = true;
            if (triggerDates.Count > 0)
            {
                CanUse = IsTriggeredSkillInfo(battlerInfo,triggerDates,null,new List<ActionResultInfo>());
            }
            return CanUse;
        }
        
        private int CanUseSkillTriggerTarget(int skillId,List<SkillData.TriggerData> triggerDates,BattlerInfo battlerInfo,List<int> targetIndexes)
        {
            var skillData = DataSystem.FindSkill(skillId);
            var targeBattlerIndex = -1;
            if (skillData != null)
            {
                if (skillData.IsHpDamageFeature())
                {
                    targeBattlerIndex = _targetEnemy != null ? _targetEnemy.Index : -1;
                } else
                if (skillData.IsHpHealFeature())
                {
                    targeBattlerIndex = _targetActor != null ? _targetActor.Index : -1;
                }
            }
            // 条件なし
            if (triggerDates.Count == 0)
            {
                return BattleUtility.NearTargetIndex(battlerInfo,targetIndexes,targeBattlerIndex);
            }
            var targetIndexList1 = new List<int>();
            var targetIndexList2 = new List<int>();
            // ～を優先判定用
            var targetIndexWithInList = new List<int>();
            if (triggerDates.Count == 1)
            {
                targetIndexList2 = targetIndexes;
            }
            for (int i = 0;i < triggerDates.Count;i++)
            {
                var targetIndexList = i == 0 ? targetIndexList1 : targetIndexList2;
                foreach (var targetIndex in targetIndexes)
                {
                    var triggerDate = triggerDates[i];
                    var targetBattler = GetBattlerInfo(targetIndex);
                    var key = (int)triggerDate.TriggerType / 1000;
                    if (_checkTriggerDict.ContainsKey(key))
                    {                
                        var checkTriggerInfo2 = new CheckTriggerInfo(_turnCount,battlerInfo,BattlerActors(),BattlerEnemies());
                        _checkTriggerDict[key].AddTargetIndexList(targetIndexList,targetIndexes,targetBattler,triggerDate,skillData,checkTriggerInfo2);
                    } else
                    {
                        targetIndexList.Add(targetIndex);
                    }
                    if (triggerDate.Param2 == 1)
                    {
                        targetIndexWithInList.Add(targetIndex);
                    }
                }
            }
            var bindTargetIndexList = new List<int>();
            foreach (var targetIndex1 in targetIndexList1)
            {
                if (targetIndexList2.Contains(targetIndex1))
                {
                    bindTargetIndexList.Add(targetIndex1);
                }
            }

            // ～範囲優先の第二候補があれば変更
            if (bindTargetIndexList.Count == 0)
            {   
                bindTargetIndexList = targetIndexWithInList;
            }

            // 限定条件を満たしている方を優先する
            if (bindTargetIndexList.Count > 1)
            {
                var isDifferentParam2 = triggerDates.Count == 2 && triggerDates.FindAll(a => a.Param2 == 1).Count == 1;
                if (isDifferentParam2)
                {
                    var prioryTargetIndexList = triggerDates[0].Param2 == 0 ? targetIndexList1 : targetIndexList2;
                    if (prioryTargetIndexList.Count > 0)
                    {
                        bindTargetIndexList = prioryTargetIndexList;
                    }
                }
            }

            // ～が高い・低い順位で選択
            var friendTargets = new List<BattlerInfo>();
            var opponentTargets = new List<BattlerInfo>();
            foreach (var bindTargetIndex in bindTargetIndexList)
            {
                var bindBattlerInfo = GetBattlerInfo(bindTargetIndex);
                if (bindBattlerInfo.IsActor && battlerInfo.IsActor)
                {
                    friendTargets.Add(bindBattlerInfo);
                } else
                {
                    opponentTargets.Add(bindBattlerInfo);
                }
            }
            var checkTriggerInfo = new CheckTriggerInfo(_turnCount,battlerInfo,friendTargets,opponentTargets);
            for (int i = 0;i < triggerDates.Count;i++)
            {
                var triggerDate = triggerDates[i];
                var key = (int)triggerDate.TriggerType / 1000;
                if (_checkTriggerDict.ContainsKey(key))
                {   
                    int targetIndex = _checkTriggerDict[key].CheckTargetIndex(triggerDate,battlerInfo,checkTriggerInfo,targeBattlerIndex);
                    if (targetIndex > -1)
                    {
                        return targetIndex;
                    }
                }
            }
            if (bindTargetIndexList.Count > 0)
            {
                // 複数候補は列に近い方を選ぶ
                return BattleUtility.NearTargetIndex(battlerInfo,bindTargetIndexList,targeBattlerIndex);
            }
            return -1;
        }
    }
}
