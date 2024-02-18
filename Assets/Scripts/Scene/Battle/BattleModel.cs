using System;
using System.Collections.Generic;
using Effekseer;
using Cysharp.Threading.Tasks;
using System.Linq;
using UnityEngine;

public class BattleModel : BaseModel
{
    private int _actionIndex = 0;
    private int _turnCount = 0;
    public void SeekTurnCount(){_turnCount++;}

    private List<BattlerInfo> _battlers = new List<BattlerInfo>();
    public List<BattlerInfo> Battlers => _battlers;

    private UnitInfo _party;
    private UnitInfo _troop;

    private List<BattleRecord> _battleRecords = new ();
    public List<BattleRecord> BattleRecords => _battleRecords;

    private List<ActionInfo> _battleActionRecords = new ();
    public List<ActionInfo> BattleActionRecords => _battleActionRecords;

    // 行動したバトラー
    private BattlerInfo _currentTurnBattler = null;
    public BattlerInfo CurrentTurnBattler => _currentTurnBattler;
    public void SetCurrentTurnBattler(BattlerInfo currentTurnBattler)
    {
        _currentTurnBattler = currentTurnBattler;
    }
    private BattlerInfo _currentBattler = null;
    public BattlerInfo CurrentBattler => _currentBattler;

    private List<ActionInfo> _actionInfos = new ();
    private Dictionary<int,List<int>> _passiveSkillInfos = new ();
    private Dictionary<int,List<int>> _usedPassiveSkillInfos = new ();

    public UniTask<List<UnityEngine.AudioClip>> GetBattleBgm()
    {
        return GetBgmData("TACTICS2");
    }

    public GameObject BattleBackGroundObject()
    {
        return ResourceSystem.LoadBattleBackGround(CurrentStage.Master.BackGround);
    }

    public void CreateBattleData()
    {
        _actionIndex = 0;
        _battlers.Clear();
        var battleMembers = BattleMembers();
        foreach (var battleMember in battleMembers)
        {
            var battlerInfo = new BattlerInfo(battleMember,battleMember.BattleIndex);
            _battlers.Add(battlerInfo);
            var battlerRecord = new BattleRecord(battlerInfo.Index);
            _battleRecords.Add(battlerRecord);
        }
        var enemies = CurrentStage.CurrentBattleInfos();
        foreach (var enemy in enemies)
        {
            enemy.ResetData();
            //baseEnemy.GainHp(-9999);
            _battlers.Add(enemy);
            var battlerRecord = new BattleRecord(enemy.Index);
            _battleRecords.Add(battlerRecord);
        }
        // アルカナ
        StageAlcana.CheckEnableSkillTrigger();
        var alcana = new BattlerInfo(StageAlcana,true,1);
        _battlers.Add(alcana);
        var alcanaBattlerRecord = new BattleRecord(1001);
        _battleRecords.Add(alcanaBattlerRecord);

        foreach (var battlerInfo1 in _battlers)
        {
            _passiveSkillInfos[battlerInfo1.Index] = new ();
            _usedPassiveSkillInfos[battlerInfo1.Index] = new ();
        }
        _party = new UnitInfo();
        _party.SetBattlers(BattlerActors());
        _troop = new UnitInfo();
        _troop.SetBattlers(BattlerEnemies());
    }

    public List<BattlerInfo> FieldBattlerInfos()
    {
        return _battlers.FindAll(a => a.isAlcana == false);
    }

    public List<StateInfo> UpdateAp()
    {
        var removeStateList = new List<StateInfo>();
        foreach (var battler in FieldBattlerInfos())
        {
            if (battler.IsAlive())
            {
                battler.UpdateAp();
                var removeStates = battler.UpdateState(RemovalTiming.UpdateAp);
                if (removeStates.Count > 0)
                {
                    removeStateList.AddRange(removeStates);
                }
            }
        }
        MakeActionBattler();
        return removeStateList;
    }

    public void UpdateApModify(BattlerInfo battlerInfo)
    {
        var minusAp = 0f;
        if (battlerInfo.Ap < 0)
        {
            minusAp = battlerInfo.Ap;
        }
        foreach (var battler in FieldBattlerInfos())
        {
            if (battler.IsAlive())
            {
                battler.ChangeAp(minusAp * -1f);
            }
        }
    }
    
    public List<ActionResultInfo> UpdateChainState()
    {
        var actionResultInfos = new List<ActionResultInfo>();
        for (int i = 0;i < FieldBattlerInfos().Count;i++)
        {
            var chainStateInfos = FieldBattlerInfos()[i].UpdateChainState();
            for (int j = chainStateInfos.Count-1;j >= 0;j--)
            {
                var stateInfo = chainStateInfos[j];
                var subject = GetBattlerInfo(stateInfo.BattlerId);
                var target = GetBattlerInfo(stateInfo.TargetIndex);
                if (subject.IsAlive() && target != null)
                {
                    int chainDamage = stateInfo.Effect;
                    if (subject.IsState(StateType.ChainDamageUp))
                    {
                        chainDamage += subject.ChainSuccessCount;
                    }
                    var featureData = new SkillData.FeatureData(){
                        FeatureType = FeatureType.HpDefineDamage,
                        Param1 = chainDamage
                    };
                    var actionResultInfo = new ActionResultInfo(subject,target,new List<SkillData.FeatureData>(){featureData},-1);
                        
                    if ((target.Hp - actionResultInfo.HpDamage) <= 0)
                    {
                        FieldBattlerInfos()[i].RemoveState(stateInfo,true);
                    }
                    actionResultInfos.Add(actionResultInfo);
                }
            }
        }
        return actionResultInfos;
    }

    public List<ActionResultInfo> UpdateBenedictionState()
    {
        var actionResultInfos = new List<ActionResultInfo> ();
        for (int i = 0;i < FieldBattlerInfos().Count;i++)
        {
            var benedictionStateInfos = FieldBattlerInfos()[i].GetStateInfoAll(StateType.Benediction);
            for (int j = benedictionStateInfos.Count-1;j >= 0;j--)
            {
                var stateInfo = benedictionStateInfos[j];
                if (stateInfo.Turns % stateInfo.BaseTurns == 0)
                {
                    var subject = FieldBattlerInfos().Find(a => a.Index == stateInfo.BattlerId);
                    if (subject.IsAlive())
                    {
                        var targets = new List<BattlerInfo>();
                        if (subject.isActor)
                        {
                            targets = _party.AliveBattlerInfos.FindAll(a => a.Index != subject.Index);
                        } else{
                            targets = _troop.AliveBattlerInfos.FindAll(a => a.Index != subject.Index);
                        }
                        var featureData = new SkillData.FeatureData(){
                            FeatureType = FeatureType.HpHeal,
                            Param1 = stateInfo.Effect
                        };
                        foreach (var target in targets)
                        {
                            var actionResultInfo = new ActionResultInfo(FieldBattlerInfos()[i],target,new List<SkillData.FeatureData>(){featureData},-1);
                            actionResultInfos.Add(actionResultInfo);
                        }
                    }
                }
            }
        }
        return actionResultInfos;
    }

    public void SetLastSkill(int skillId)
    {
        _currentBattler.SetLastSelectSkillId(skillId);
    }

    public void WaitUnison()
    {
        _currentBattler.SetAp(1);
        _currentBattler.AddState(new StateInfo(StateType.Wait,999,0,_currentBattler.Index,_currentBattler.Index,10),true);
        _currentBattler = null;
        _actionInfos.Clear();
    }

    public void AssignWaitBattler()
    {
        if (_currentTurnBattler != null && _currentTurnBattler.isActor)
        {
            var waitBattlerIndex = _party.AliveBattlerInfos.FindIndex(a => a.IsState(StateType.Wait));
            if (waitBattlerIndex > -1)
            {
                _party.AliveBattlerInfos[waitBattlerIndex].SetAp(0);
                _party.AliveBattlerInfos[waitBattlerIndex].EraseStateInfo(StateType.Wait);
            }
        }
    }

    public void RemoveOneMemberWaitBattlers()
    {
        var partyWaitBattlers = _party.AliveBattlerInfos.FindAll(a => a.CanMove());
        if (partyWaitBattlers.Count < 1)
        {
            foreach (var battlerInfo in _party.AliveBattlerInfos)
            {
                battlerInfo.EraseStateInfo(StateType.Wait);
            }
        }
        var troopWaitBattlers = _troop.AliveBattlerInfos.FindAll(a => a.CanMove());
        if (troopWaitBattlers.Count < 1)
        {
            foreach (var battlerInfo in _troop.AliveBattlerInfos)
            {
                battlerInfo.EraseStateInfo(StateType.Wait);
            }
        }
    }

    public void MakeActionBattler()
    {
        FieldBattlerInfos().Sort((a,b) => (int)a.Ap - (int)b.Ap);
        _currentBattler = FieldBattlerInfos().Find(a => a.Ap <= 0);
    }

    public void SetActionBattler(int targetIndex)
    {
        var battlerInfo = GetBattlerInfo(targetIndex);
        if (battlerInfo != null)
        {
            _currentBattler = battlerInfo;
        }
    }

    public List<int> CheckChainBattler(BattlerInfo subject)
    {
        var targetIndexes = new List<int>();
        foreach (var battler in FieldBattlerInfos())
        {
            var stateInfos = battler.GetStateInfoAll(StateType.Chain);
            for (var i = stateInfos.Count-1; 0 <= i;i--)
            {
                if (stateInfos[i].BattlerId == subject.Index)
                {
                    if (battler.IsAlive())
                    {
                        targetIndexes.Add(stateInfos[i].TargetIndex);
                    } else
                    {
                        battler.RemoveState(stateInfos[i],true);
                    }
                }
            }
        }
        return targetIndexes;
    }

    // 攻撃を受けた対象が付与したステートを取得
    private List<StateInfo> CheckAttackedBattlerState(StateType stateType,int targetIndex)
    {
        var stateInfos = new List<StateInfo>();
        foreach (var battler in FieldBattlerInfos())
        {
            var chainStateInfos = battler.GetStateInfoAll(stateType);
            // 末尾から取得
            for (int i = chainStateInfos.Count-1; 0 <= i;i--)
            {
                // 付与者と攻撃対象が同じ
                if (chainStateInfos[i].BattlerId == targetIndex)
                {
                    stateInfos.Add(chainStateInfos[i]);
                }
            }
        }
        return stateInfos;
    }

    public List<BattlerInfo> BattlerActors(){
        return FieldBattlerInfos().FindAll(a => a.isActor == true);
    }

    public List<BattlerInfo> BattlerEnemies(){
        return FieldBattlerInfos().FindAll(a => a.isActor == false);
    }

    public BattlerInfo GetBattlerInfo(int index)
    {
        return _battlers.Find(a => a.Index == index);
    }

    public BattleRecord GetBattlerRecord(int index)
    {
        return _battleRecords.Find(a => a.BattlerIndex == index);
    }

    public List<ListData> SkillActionList()
    {
        var skillInfos = _currentBattler.Skills.FindAll(a => a.Master.SkillType != SkillType.None && a.Master.Id > 100);
        for (int i = 0; i < skillInfos.Count;i++)
        {
            skillInfos[i].SetEnable(CheckCanUse(skillInfos[i],_currentBattler));
        }
        var sortList1 = new List<SkillInfo>();
        var sortList2 = new List<SkillInfo>();
        var sortList3 = new List<SkillInfo>();
        skillInfos.Sort((a,b) => {return a.Master.Id > b.Master.Id ? 1 : -1;});
        foreach (var skillInfo in skillInfos)
        {
            if (skillInfo.Master.IconIndex >= MagicIconType.Elementarism && skillInfo.Master.IconIndex <= MagicIconType.Psionics)
            {
                sortList1.Add(skillInfo);
            } else
            if (skillInfo.Master.IconIndex >= MagicIconType.Demigod && skillInfo.Master.IconIndex < MagicIconType.Other)
            {
                sortList2.Add(skillInfo);
            } else
            {
                sortList3.Add(skillInfo);
            }
        }
        skillInfos.Clear();
        skillInfos.AddRange(sortList1);
        skillInfos.AddRange(sortList2);
        skillInfos.AddRange(sortList3);
        return MakeListData(skillInfos);
    }

    public int SelectSkillIndex(List<ListData> skillInfos)
    {
        int selectIndex = 0;
        if (_currentBattler != null && _currentBattler.isActor == true)
        {
            var skillIndex = skillInfos.FindIndex(a => ((SkillInfo)a.Data).Id == _currentBattler.LastSelectSkillId);
            if (skillIndex > -1)
            {
                selectIndex = skillIndex;
            }
        }
        return selectIndex;
    }

    private bool CheckCanUse(SkillInfo skillInfo,BattlerInfo battlerInfo)
    {
        if (skillInfo.Master.MpCost > battlerInfo.Mp)
        {
            return false;
        }
        if (skillInfo.Master.SkillType == SkillType.Passive)
        {
            return false;
        }
        if (skillInfo.Master.SkillType == SkillType.Demigod)
        {
            return false;
        }
        if (skillInfo.Master.SkillType == SkillType.Awaken)
        {
            if (!battlerInfo.IsState(StateType.Demigod))
            {
                return false;
            }
        }
        if (skillInfo.IsUnison())
        {
            return FieldBattlerInfos().FindAll(a => a.IsAlive() && a.isActor == battlerInfo.isActor && a.CanMove()).Count > 1;
        }
        if (CanUseTrigger(skillInfo,battlerInfo) == false)
        {
            return false;
        }
        var targetIndexList = GetSkillTargetIndexes(skillInfo.Master.Id,battlerInfo.Index,true);
        if (targetIndexList.Count == 0)
        {
            return false;
        }
        return true;
    }

    /// <summary>
    /// 行動を初期化
    /// </summary>
    public void ClearActionInfo()
    {
        _actionInfos.Clear();
    }

    public bool EnableCurrentBattler()
    {
        if (_currentBattler.CanMove() == false)
        {
            return false;
        }
        // 使用可能な魔法がない
        var skillInfos = _currentBattler.ActiveSkills().FindAll(a => CheckCanUse(a,_currentBattler));
        if (skillInfos.Count == 0)
        {
            return false;
        }
        return true;
    }

    // 行動を生成
    public ActionInfo MakeActionInfo(BattlerInfo subject,SkillInfo skillInfo,bool IsInterrupt,bool IsTrigger)
    {
        var skillData = skillInfo.Master;
        var targetIndexList = GetSkillTargetIndexes(skillInfo.Id,subject.Index,true);
        if (subject.IsState(StateType.Substitute))
        {
            int substituteId = subject.GetStateInfo(StateType.Substitute).BattlerId;
            if (targetIndexList.Contains(substituteId))
            {
                targetIndexList.Clear();
                targetIndexList.Add(substituteId);
            } else{
                var tempIndexList = GetSkillTargetIndexes(skillInfo.Id,subject.Index,false);
                if (tempIndexList.Contains(substituteId))
                {
                    targetIndexList.Clear();
                    targetIndexList.Add(substituteId);
                }
            }
        }
        int lastTargetIndex = -1;
        if (subject.isActor)
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
        var actionInfo = new ActionInfo(skillInfo,_actionIndex,skillInfo.Id,subject.Index,lastTargetIndex,targetIndexList);
        _actionIndex++;
        if (subject.IsState(StateType.Extension))
        {
            actionInfo.SetRangeType(RangeType.L);
        }
        if (IsTrigger)
        {
            actionInfo.SetTriggerSkill(true);
        }
        if (IsInterrupt)
        {
            _actionInfos.Insert(0,actionInfo);
        } else
        {
            _actionInfos.Add(actionInfo);
        }
        return actionInfo;
    }

    // 選択可能な対象のインデックスを取得
    public List<int> GetSkillTargetIndexes(int skillId,int subjectIndex,bool checkCondition)
    {
        var skillData = DataSystem.FindSkill(skillId);
        var subject = GetBattlerInfo(subjectIndex);
        
        var rangeType = skillData.Range;
        if (subject.IsState(StateType.Extension))
        {
            rangeType = RangeType.L;
        }

        var targetIndexList = new List<int>();
        if (skillData.TargetType == TargetType.All)
        {
            targetIndexList = TargetIndexAll(subject.isActor,targetIndexList,rangeType);
        } else
        if (skillData.TargetType == TargetType.Opponent)
        {
            targetIndexList = TargetIndexOpponent(subject.isActor,targetIndexList,rangeType,subject.LineIndex);
        } else
        if (skillData.TargetType == TargetType.Friend)
        {
            targetIndexList = TargetIndexFriend(subject.isActor,targetIndexList);
            if (skillData.Scope == ScopeType.WithoutSelfOne || skillData.Scope == ScopeType.WithoutSelfAll)
            {
                targetIndexList.Remove(subject.Index);
            }
        } else 
        if (skillData.TargetType == TargetType.Self)
        {
            targetIndexList.Add(subject.Index);
        }

        if (skillData.AliveOnly)
        {
            targetIndexList = targetIndexList.FindAll(a => FieldBattlerInfos().Find(b => a == b.Index).IsAlive());
        } else
        {
            targetIndexList = targetIndexList.FindAll(a => !FieldBattlerInfos().Find(b => a == b.Index).IsAlive());
        }
        if (skillData.ScopeTriggers.Count > 0)
        {
            targetIndexList = CheckScopeTriggers(targetIndexList,skillData.ScopeTriggers);
        }
        if (checkCondition == true)
        {
            for (int i = targetIndexList.Count-1;i >= 0;i--)
            {
                if (CanUseCondition(skillId,targetIndexList[i]) == false)
                {
                    targetIndexList.Remove(targetIndexList[i]);
                }
            }
        }
        return targetIndexList;
    }

    // 選択範囲が敵味方全員の場合
    private List<int> TargetIndexAll(bool isActor,List<int> targetIndexList,RangeType rangeType)
    {
        foreach (var battlerInfo in _party.BattlerInfos)
        {
            targetIndexList.Add(battlerInfo.Index);
        }
        foreach (var battlerInfo in _troop.BattlerInfos)
        {
            targetIndexList.Add(battlerInfo.Index);
        }
        return targetIndexList;
    }

    // 選択範囲が相手
    private List<int> TargetIndexOpponent(bool isActor,List<int> targetIndexList,RangeType rangeType,LineType lineType)
    {   
        if (isActor)
        {
            foreach (var battlerInfo in _troop.BattlerInfos)
            {
                targetIndexList.Add(battlerInfo.Index);
                }
        } else{
            foreach (var battlerInfo in _party.BattlerInfos)
            {
                targetIndexList.Add(battlerInfo.Index);
            }
        }
        return targetIndexList;
    }

    private List<int> TargetIndexFriend(bool isActor,List<int> targetIndexList)
    {
        var battlerInfos = isActor ? _party.BattlerInfos : _troop.BattlerInfos;
        foreach (var battlerInfo in battlerInfos)
        {
            targetIndexList.Add(battlerInfo.Index);
        }
        return targetIndexList;
    }

    // 選択可能なBattlerInfoを取得
    public List<ListData> TargetBattlerPartyInfos(ActionInfo actionInfo)
    {
        var targetBattlerInfos = new List<ListData>();
        foreach (var battlerInfo in _party.BattlerInfos)
        {
            var listData = new ListData(battlerInfo);
            listData.SetEnable(actionInfo.TargetIndexList.Contains(battlerInfo.Index));
            listData.SetSelected(actionInfo.LastTargetIndex == battlerInfo.Index);
            targetBattlerInfos.Add(listData);
        }
        return targetBattlerInfos;
    }

    // 選択可能なBattlerInfoを取得
    public List<ListData> TargetBattlerEnemyInfos(ActionInfo actionInfo)
    {
        var targetBattlerInfos = new List<ListData>();
        foreach (var battlerInfo in _troop.BattlerInfos)
        {
            var listData = new ListData(battlerInfo);
            listData.SetEnable(actionInfo.TargetIndexList.Contains(battlerInfo.Index));
            listData.SetSelected(actionInfo.LastTargetIndex == battlerInfo.Index);
            targetBattlerInfos.Add(listData);
        }
        return targetBattlerInfos;
    }

    public List<int> CurrentActionTargetIndexes(int selectIndex)
    {
        var actionInfo = CurrentActionInfo();
        var targetIndexList = GetSkillTargetIndexes(actionInfo.Master.Id,_currentBattler.Index,true);
        var scopeType = actionInfo.ScopeType;
        var subject = GetBattlerInfo(actionInfo.SubjectIndex);
        if (subject.IsState(StateType.EffectLine))
        {
            scopeType = ScopeType.Line;
        }
        if (subject.IsState(StateType.EffectAll))
        {
            scopeType = ScopeType.All;
        }
        var TargetBattler = GetBattlerInfo(selectIndex);
        switch (scopeType)
        {
            case ScopeType.All:
                break;
            case ScopeType.WithoutSelfAll:
                break;
            case ScopeType.Line:
            case ScopeType.FrontLine:
                targetIndexList = targetIndexList.FindAll(a => GetBattlerInfo(a).LineIndex == TargetBattler.LineIndex);
                break;
            case ScopeType.One:
            case ScopeType.WithoutSelfOne:
            case ScopeType.Self:
                targetIndexList.Clear();
                targetIndexList.Add(selectIndex);
                break;
        }
        return targetIndexList;
    }

    public bool CanUseCondition(int skillId,int targetIndex)
    {
        bool IsEnable = false;
        var skill = DataSystem.FindSkill(skillId);
        var target = GetBattlerInfo(targetIndex);
        foreach (var featureData in skill.FeatureDates)
        {
            switch (featureData.FeatureType)
            {
                case FeatureType.HpDamage:
                if (target.Hp > 0)
                {
                    IsEnable = true;
                }
                break;
                case FeatureType.HpHeal:
                case FeatureType.KindHeal:
                if (_currentBattler != null && _currentBattler.isActor)
                {
                    {
                        if (!target.isActor)
                        {
                            IsEnable = target.Kinds.Contains(KindType.Undead);
                        } else
                        {
                            IsEnable = true;
                        }
                    }
                } else 
                {
                    if (target.Hp < target.MaxHp)
                    {
                        if (!target.isActor && !target.Kinds.Contains(KindType.Undead))
                        {
                            IsEnable = true;
                        }
                    }
                }
                break;
                case FeatureType.MpHeal:
                if (_currentBattler != null)
                {
                    if (_currentBattler.isActor)
                    {
                        IsEnable = true;
                    } else
                    {
                        if (target.Mp < target.MaxMp)
                        {
                            IsEnable = true;
                        }
                    }
                }
                break;
                case FeatureType.MpDamage:
                if (target.Mp > 0)
                {
                    IsEnable = true;
                }
                break;
                case FeatureType.AddState:
                case FeatureType.AddStateNextTurn:
                if ((StateType)featureData.Param1 == StateType.RemoveBuff)
                {
                    if (target.GetRemovalBuffStates().Count > 0)
                    {
                        IsEnable = true;
                    }
                }
                else
                {
                    if (!target.IsState((StateType)featureData.Param1) && !target.IsState(StateType.Barrier))
                    {
                        IsEnable = true;
                    } else
                    if (_currentBattler != null && _currentBattler.isActor || (StateType)featureData.Param1 == StateType.DamageUp || (StateType)featureData.Param1 == StateType.Prism)
                    {
                        IsEnable = true;
                    } else
                    if (_currentBattler != null && !_currentBattler.isActor && !target.IsState((StateType)featureData.Param1) && target.IsState(StateType.Barrier))
                    {
                        if (UnityEngine.Random.Range(0,100) > 50)
                        {
                            IsEnable = true;
                        }
                    }
                }
                break;
                case FeatureType.RemoveState:
                if (target.IsState((StateType)featureData.Param1))
                {
                    IsEnable = true;
                }
                break;
                case FeatureType.RemoveAbnormalState:
                if (target.StateInfos.Find(a => a.Master.Abnormal) != null)
                {
                    IsEnable = true;
                }
                break;
                case FeatureType.BreakUndead:
                if (_currentBattler.isActor)
                {
                    IsEnable = true;
                } else 
                {
                    if (!target.isActor && !target.Kinds.Contains(KindType.Undead))
                    {
                        IsEnable = true;
                    }
                }
                break;
                default:
                IsEnable = true;
                break;
            }
        }
        return IsEnable;
    }

    public List<int> CheckScopeTriggers(List<int> targetIndexList,List<SkillData.TriggerData> scopeTriggers)
    {
        for (int i = targetIndexList.Count-1;i >= 0;i--)
        {
            var target = GetBattlerInfo(targetIndexList[i]);
            var remove = false;
            foreach (var scopeTrigger in scopeTriggers)
            {
                if (scopeTrigger.TriggerType == TriggerType.DemigodMagicAttribute)
                {
                    if (target.Skills.Find(a => a.Master.SkillType == SkillType.Demigod && a.Attribute == (AttributeType)scopeTrigger.Param1) == null)
                    {
                        remove = true;
                    }
                } else
                {
                    if (!scopeTrigger.IsTriggeredSkillInfo(target,BattlerActors(),BattlerEnemies()))
                    {
                        remove = true;
                    }
                }
            }
            if (remove)
            {
                targetIndexList.RemoveAt(i);
            }
        }
        return targetIndexList;
    }

    public ActionInfo CurrentActionInfo()
    {
        if (_actionInfos.Count < 1){
            return null;
        }
        return _actionInfos[0];
    }

    // indexListにActionを使ったときのリザルトを生成
    public void MakeActionResultInfo(ActionInfo actionInfo,List<int> indexList)
    {
        var subject = GetBattlerInfo(actionInfo.SubjectIndex);
        if (indexList.Count > 0)
        {
            if (subject.isActor)
            {
                if (actionInfo.Master.TargetType == TargetType.Opponent)
                {
                    subject.SetLastTargetIndex(indexList[0]);
                }
            }
            if (actionInfo.Master.TargetType == TargetType.All)
            {
                if (indexList[0] > 100)
                {
                    subject.SetLastTargetIndex(indexList[0]);
                }
            }
        }
        int MpCost = CalcMpCost(actionInfo);
        actionInfo.SetMpCost(MpCost);

        var isPrism = PrismRepeatTime(subject,actionInfo) > 0;
        var repeatTime = CalcRepeatTime(actionInfo);
        repeatTime += PrismRepeatTime(subject,actionInfo);

        // 攻撃前の攻撃無効回数を管理
        var noDamageDict = new Dictionary<int,int>();
        for (int i = 0; i < indexList.Count;i++)
        {
            noDamageDict[indexList[i]] = GetBattlerInfo(indexList[i]).StateTurn(StateType.NoDamage);
        }
        var actionResultInfos = new List<ActionResultInfo>();

        for (int i = 0; i < repeatTime;i++)
        {
            foreach (var targetIndex in indexList)
            {
                var target = GetBattlerInfo(targetIndex);
                var featureDates = new List<SkillData.FeatureData>();
                foreach (var featureData in actionInfo.SkillInfo.FeatureDates)
                {
                    if (isPrism)
                    {
                        // 攻撃増加分のダメージ種別を追加
                        if (featureData.FeatureType == FeatureType.HpDamage || featureData.FeatureType == FeatureType.HpDefineDamage)
                        {
                            var prismCount = (subject.GetStateInfoAll(StateType.Prism).Count + 1);
                            var prismTime = (i+1) % prismCount;
                            if (prismTime == 0)
                            {
                                prismTime = prismCount;
                            }
                            var damageFeature = featureData.CopyData();
                            var ratio = 1.0f / (prismTime);
                            damageFeature.Param1 = (int)(damageFeature.Param1 * ratio);
                            featureDates.Add(damageFeature);
                            continue;
                        }
                    }
                    featureDates.Add(featureData);
                }

                var actionResultInfo = new ActionResultInfo(subject,target,featureDates,actionInfo.Master.Id,actionInfo.Master.Scope == ScopeType.One);
                
                if (actionResultInfo.RemoveAttackStateDamage())            
                {
                    var chainStateInfos = CheckAttackedBattlerState(StateType.Chain,actionResultInfo.TargetIndex);
                    if (chainStateInfos.Count > 0)
                    {
                        for (int k = 0; k < chainStateInfos.Count;k++)
                        {
                            var chainedBattlerInfo = GetBattlerInfo(chainStateInfos[k].TargetIndex);
                            chainedBattlerInfo.RemoveState(chainStateInfos[k],true);
                            actionResultInfo.AddRemoveState(chainStateInfos[k]);
                        }
                    }
                    if (target.IsState(StateType.Benediction))
                    {
                        var benedictStateInfos = target.GetStateInfoAll(StateType.Benediction);
                        for (int k = 0; k < benedictStateInfos.Count;k++)
                        {
                            target.RemoveState(benedictStateInfos[k],true);
                            target.ResetAp(false);
                            actionResultInfo.AddRemoveState(benedictStateInfos[k]);
                        }
                    }
                }

                // 無敵回数
                int noDamageCount = target.StateTurn(StateType.NoDamage);
                // このリザルトでの無敵進行回数
                int seekCount = actionResultInfo.SeekCount(target,StateType.NoDamage);
                if (seekCount > 0)
                {
                    // 今までのリザルトでの無敵進行回数
                    int seekCountAll = actionResultInfos.Sum(a => a.SeekCount(target,StateType.NoDamage));
                    if ((seekCountAll+1) >= noDamageDict[targetIndex])
                    {
                        var noDamageState = target.GetStateInfo(StateType.NoDamage);
                        if (noDamageState != null)
                        {
                            target.RemoveState(noDamageState,true);
                            actionResultInfo.AddRemoveState(noDamageState);
                            var displayedResults = actionResultInfos.FindAll(a => a.DisplayStates.Find(b => b.Master.StateType == StateType.NoDamage) != null);

                            foreach (var displayedResult in displayedResults)
                            {
                                for (int j = displayedResult.DisplayStates.Count-1; j >= 0;j--)
                                {
                                    if (displayedResult.DisplayStates[j] == noDamageState)
                                    {
                                        displayedResult.DisplayStates.Remove(noDamageState);
                                    }
                                }
                            }
                        }
                    }
                }
                actionResultInfos.Add(actionResultInfo);
            }
        }
        AdjustActionResultInfo(actionResultInfos);
        actionInfo.SetActionResult(actionResultInfos);
    }

    private int CalcMpCost(ActionInfo actionInfo)
    {
        return actionInfo.Master.MpCost;
    }

    private int CalcRepeatTime(ActionInfo actionInfo)
    {
        return actionInfo.Master.RepeatTime;
    }

    private int PrismRepeatTime(BattlerInfo subject,ActionInfo actionInfo)
    {
        if (actionInfo.Master.Attribute == AttributeType.Shine && subject.IsState(StateType.Prism))
        {
            var damageFeatures = actionInfo.SkillInfo.FeatureDates.FindAll(a => a.FeatureType == FeatureType.HpDamage || a.FeatureType == FeatureType.HpDefineDamage);
            if (damageFeatures.Count > 0)
            {
                return (subject.GetStateInfoAll(StateType.Prism).Count + 1);
            }
        }
        return 0;
    }

    public void MakeCurseActionResults(ActionInfo actionInfo,List<int> indexList)
    {
        var actionResultInfos = actionInfo.ActionResults;
        var beforeCurseIndex = 0;
        for (int i = 0;i < actionResultInfos.Count;i++)
        {
            if (actionResultInfos[i].CursedDamage == true)
            {
                beforeCurseIndex = i;
            }
        }
        
        for (int i = 0; i < indexList.Count;i++)
        {
            // 呪い
            var curseStateInfos = CheckAttackedBattlerState(StateType.Curse,indexList[i]);
            if (curseStateInfos.Count > 0)
            {
                for (int j = 0; j < curseStateInfos.Count;j++)
                {
                    var hpDamage = 0;
                    for (int k = 0;k < actionResultInfos.Count;k++)
                    {
                        if (actionResultInfos[k].CursedDamage) continue;
                        var cursedDamage = (actionResultInfos[k].OverkillHpDamage > actionResultInfos[k].HpDamage) ? actionResultInfos[k].OverkillHpDamage : actionResultInfos[k].HpDamage;
                        if (k >= beforeCurseIndex && cursedDamage > 0 && curseStateInfos[j].BattlerId == actionResultInfos[k].TargetIndex)
                        {
                            hpDamage += cursedDamage;
                        }
                    }
                    if (hpDamage > 0)
                    {
                        var featureData = new SkillData.FeatureData();
                        featureData.FeatureType = FeatureType.HpCursedDamage;
                        featureData.Param1 = (int)MathF.Floor(hpDamage * curseStateInfos[j].Effect * 0.01f);

                        var curseBattlerInfo = GetBattlerInfo(curseStateInfos[j].TargetIndex);
                        var curseActionResultInfo = new ActionResultInfo(GetBattlerInfo(curseStateInfos[j].BattlerId),curseBattlerInfo,new List<SkillData.FeatureData>(){featureData},-1);
                        //curseActionResultInfo.RemovedStates.Add(curseStateInfos[j]);
                        curseActionResultInfo.SetCursedDamage(true);
                        actionResultInfos.Add(curseActionResultInfo);
                    }
                }
            }
        }
    }

    public EffekseerEffectAsset SkillActionAnimation(string animationName)
    {
        var result = ResourceSystem.LoadResourceEffect(animationName);
        return result;
    }

    public List<MakerEffectData.SoundTimings> SkillActionSoundTimings(string animationName)
    {
        var makerEffectPath = animationName.Replace("MakerEffect/","");
        var path = "Animations/AnimationData/" + makerEffectPath;
        var result = UnityEngine.Resources.Load<MakerEffectAssetData>(path);
        if (result != null)
        {
            return result.AssetData.soundTimings;
        }
        return null;
    }

    public void ExecCurrentActionResult()
    {
        var actionInfo = CurrentActionInfo();
        if (actionInfo != null)
        {
            // Mpの支払い
            _currentBattler.GainMp(actionInfo.MpCost * -1);
            _currentBattler.GainPayBattleMp(actionInfo.MpCost);
            if (actionInfo.Master.IsHpHealFeature())
            {
                _currentBattler.GainHealCount(1);
            }
            var actionResultInfos = CalcDeathIndexList(actionInfo.ActionResults);
            foreach (var actionResultInfo in actionResultInfos)
            {
                ExecActionResultInfo(actionResultInfo);
            }
            actionInfo.SetTurnCount(_turnCount);
            _battleActionRecords.Add(actionInfo);
        }
    }

    // 複数のActionResultのreDamageとreHealを1つにまとめる
    public void AdjustActionResultInfo(List<ActionResultInfo> actionResultInfos)
    {
        // ドレイン回復をまとめる
        var reHealResults = actionResultInfos.FindAll(a => a.ReHeal > 0);
        if (reHealResults.Count > 1)
        {
            int reHeal = 0;
            foreach (var reHealResult in reHealResults)
            {
                reHeal += reHealResult.ReHeal;
                reHealResult.SetReHeal(0);
            }
            reHealResults[reHealResults.Count-1].SetReHeal(reHeal);
        }
        // カウンターダメージをまとめる
        var counterResults = actionResultInfos.FindAll(a => a.ReDamage > 0);
        if (counterResults.Count > 1)
        {
            int reDamage = 0;
            foreach (var counterResult in counterResults)
            {
                reDamage += counterResult.ReDamage;
                counterResult.SetReDamage(0);
            }
            counterResults[counterResults.Count-1].SetReDamage(reDamage);
        }
        if (reHealResults.Count > 0 && counterResults.Count > 0)
        {
            int heal = reHealResults[reHealResults.Count-1].ReHeal;
            int damage = counterResults[counterResults.Count-1].ReDamage;
            
            if (heal > damage)
            {
                reHealResults[reHealResults.Count-1].SetReHeal(heal - damage);
                counterResults[counterResults.Count-1].SetReDamage(0);
            } else
            {
                reHealResults[reHealResults.Count-1].SetReHeal(0);
                counterResults[counterResults.Count-1].SetReDamage(damage - heal);
            }
        }
        // ReDamageによってDeadIndexを変更
        if (counterResults.Count > 0)
        {
            var result = counterResults[counterResults.Count-1];
            if (result.ReDamage > GetBattlerInfo(result.SubjectIndex).Hp && !result.DeadIndexList.Contains(result.SubjectIndex))
            {
                counterResults[counterResults.Count-1].DeadIndexList.Add(result.SubjectIndex);
            }
        }

        // Stateの重複をまとめる
        var addStates = new List<StateInfo>();
        var removeStates = new List<StateInfo>();
        var displayStates = new List<StateInfo>();
        foreach (var actionResultInfo in actionResultInfos)
        {
            for (var i = actionResultInfo.AddedStates.Count-1;i >= 0;i--)
            {
                if (addStates.Find(a => a.CheckSameStateType(actionResultInfo.AddedStates[i]) == true) == null)
                {
                    addStates.Add(actionResultInfo.AddedStates[i]);
                } else
                {
                    actionResultInfo.AddedStates.RemoveAt(i);
                }
            }
            for (var i = actionResultInfo.RemovedStates.Count-1;i >= 0;i--)
            {
                if (removeStates.Find(a => a.CheckSameStateType(actionResultInfo.RemovedStates[i]) == true) == null)
                {
                    removeStates.Add(actionResultInfo.RemovedStates[i]);
                } else
                {
                    actionResultInfo.RemovedStates.RemoveAt(i);
                }
            }
            for (var i = actionResultInfo.DisplayStates.Count-1;i >= 0;i--)
            {
                if (displayStates.Find(a => a.CheckSameStateType(actionResultInfo.DisplayStates[i]) == true) == null)
                {
                    displayStates.Add(actionResultInfo.DisplayStates[i]);
                } else
                {
                    actionResultInfo.DisplayStates.RemoveAt(i);
                }
            }
        }
    }

    public void ExecActionResultInfo(ActionResultInfo actionResultInfo)
    {
        var subject = GetBattlerInfo(actionResultInfo.SubjectIndex);
        var target = GetBattlerInfo(actionResultInfo.TargetIndex);
        var subjectRecord = GetBattlerRecord(actionResultInfo.SubjectIndex);
        var targetRecord = GetBattlerRecord(actionResultInfo.TargetIndex);
        foreach (var addState in actionResultInfo.AddedStates)
        {
            var addTarget = GetBattlerInfo(addState.TargetIndex);
            addTarget.AddState(addState,true);
        }
        foreach (var removeState in actionResultInfo.RemovedStates)
        {
            var removeTarget = GetBattlerInfo(removeState.TargetIndex);
            removeTarget.RemoveState(removeState,true);
        }
        if (actionResultInfo.HpDamage != 0)
        {
            target.GainHp(-1 * actionResultInfo.HpDamage);
            targetRecord.GainDamagedValue(actionResultInfo.HpDamage);
            subjectRecord.GainDamageValue(actionResultInfo.HpDamage);
        }
        if (actionResultInfo.HpHeal != 0 && (!actionResultInfo.DeadIndexList.Contains(target.Index) || actionResultInfo.AliveIndexList.Contains(target.Index)))
        {
            target.GainHp(actionResultInfo.HpHeal);
            subjectRecord.GainHealValue(actionResultInfo.HpHeal);
        }
        if (actionResultInfo.MpDamage != 0)
        {
            target.GainMp(-1 * actionResultInfo.MpDamage);
        }
        if (actionResultInfo.MpHeal != 0)
        {
            target.GainMp(actionResultInfo.MpHeal);
        }
        if (actionResultInfo.ApHeal != 0)
        {
            target.ChangeAp(actionResultInfo.ApHeal * -1);
        }
        if (actionResultInfo.ApDamage != 0)
        {
            target.ChangeAp(actionResultInfo.ApDamage);
        }
        if (actionResultInfo.ReHeal != 0)
        {
            subject.GainHp(actionResultInfo.ReHeal);
        }
        if (actionResultInfo.ReDamage != 0)
        {
            subject.GainHp(-1 * actionResultInfo.ReDamage);
        }
        if (actionResultInfo.Missed == true)
        {
            target.GainDodgeCount(1);
        }
        foreach (var targetIndex in actionResultInfo.ExecStateInfos)
        {
            var execTarget = GetBattlerInfo(targetIndex.Key);
            if (execTarget != null)
            {
                foreach (var stateInfo in targetIndex.Value)
                {
                    execTarget.UpdateStateCount(RemovalTiming.UpdateCount,stateInfo);
                }
            }
        }
        //if (actionResultInfo.AddedStates.Find(a => a.Master.Id == (int)StateType.Stun) != null
        //    || actionResultInfo.DeadIndexList.Contains(actionResultInfo.TargetIndex))  
        if (actionResultInfo.HpDamage > 0 || actionResultInfo.ExecStateInfos[target.Index].Find(a => a.StateType == StateType.CounterAura) != null)
        {
            if (target.IsState(StateType.CounterAura))
            {
                var counterAuraStateInfos = target.GetStateInfoAll(StateType.CounterAura);
                for (int j = 0; j < counterAuraStateInfos.Count;j++)
                {
                    target.RemoveState(counterAuraStateInfos[j],true);
                    actionResultInfo.AddRemoveState(counterAuraStateInfos[j]);
                }
            }
        }
        
        actionResultInfo.SetTurnCount(_turnCount);
    }
    
    public List<int> DeathBattlerIndex(List<ActionResultInfo> actionResultInfos)
    {
        var deathBattlerIndex = new List<int>();
        foreach (var actionResultInfo in actionResultInfos)
        {
            foreach (var deadIndexList in actionResultInfo.DeadIndexList)
            {
                // 例外
                if (!GetBattlerInfo(deadIndexList).IsState(StateType.Death))
                {

                } else
                {
                    deathBattlerIndex.Add(deadIndexList);
                }
            }
        }
        return deathBattlerIndex;
    }

    public List<int> AliveBattlerIndex(List<ActionResultInfo> actionResultInfos)
    {
        var aliveBattlerIndex = new List<int>();
        foreach (var actionResultInfo in actionResultInfos)
        {
            foreach (var aliveIndexList in actionResultInfo.AliveIndexList)
            {
                aliveBattlerIndex.Add(aliveIndexList);
            }
        }
        return aliveBattlerIndex;
    }

    public List<StateInfo> UpdateTurn()
    {
        var result = _currentTurnBattler.UpdateState(RemovalTiming.UpdateTurn);
        _currentTurnBattler.TurnEnd();
        return result;
    }

    public List<StateInfo> UpdateNextSelfTurn()
    {
        var result = _currentBattler.UpdateState(RemovalTiming.NextSelfTurn);
        return result;
    }

    public void TurnEnd()
    {
        var reAction = false;
        var actionInfo = CurrentActionInfo();
        if (actionInfo.TriggeredSkill == false)
        {    
            var noResetAp = actionInfo.SkillInfo.FeatureDates.Find(a => a.FeatureType == FeatureType.NoResetAp);
            if (noResetAp == null)
            {
                _currentBattler.ResetAp(false);
            }
            var afterAp = actionInfo.SkillInfo.FeatureDates.Find(a => a.FeatureType == FeatureType.SetAfterAp);
            if (afterAp != null)
            {
                _currentBattler.SetAp(afterAp.Param1);
                if (afterAp.Param1 == 0)
                {
                    reAction = true;
                }
            }
        }
        _actionInfos.RemoveAt(0);
        if (reAction == false)
        {
            _currentBattler = null;
        }
    }

    public void CheckPlusSkill()
    {
        var actionInfo = CurrentActionInfo();
        if (actionInfo != null)
        {
            var plusActionInfos = actionInfo.CheckPlusSkill();
            if (GetBattlerInfo(actionInfo.SubjectIndex).IsState(StateType.Extension))
            {
                foreach (var plusActionInfo in plusActionInfos)
                {
                    plusActionInfo.SetRangeType(RangeType.L);
                }
            }
            foreach (var plusActionInfo in plusActionInfos)
            {
                if (plusActionInfo.Master.SkillType == SkillType.Passive)
                {
                    var useLimit = plusActionInfo.Master.TriggerDates.Find(a => a.Param2 >= 1);
                    if (useLimit != null)
                    {
                        var usedCount = _usedPassiveSkillInfos[actionInfo.SubjectIndex].FindAll(a => a == plusActionInfo.Master.Id);
                        if (usedCount.Count < useLimit.Param2)
                        {
                            _usedPassiveSkillInfos[actionInfo.SubjectIndex].Add(plusActionInfo.Master.Id);
                        } else
                        {
                            continue;
                        }
                    }
                    if (!_passiveSkillInfos[actionInfo.SubjectIndex].Contains(plusActionInfo.Master.Id))
                    {
                        _passiveSkillInfos[actionInfo.SubjectIndex].Add(plusActionInfo.Master.Id);
                    }
                }
                _actionInfos.Add(plusActionInfo);
            }
        }
    }

    public List<ActionResultInfo> CheckRegenerate()
    {
        var results = RegenerateActionResults();
        results.AddRange(AfterHealActionResults());
        results.AddRange(UndeadHealActionResults());
        if (CurrentActionInfo() != null && CurrentActionInfo().ActionResults.Find(a => a.HpDamage > 0) != null)
        {
            results.AddRange(AssistHealActionResults());
        }
        return results;
    }

    private List<ActionResultInfo> RegenerateActionResults()
    {
        var RegenerateResults = MakeStateActionResult(_currentBattler,StateType.Regenerate,FeatureType.HpHeal);
        return RegenerateResults;
    }

    private List<ActionResultInfo> AfterHealActionResults()
    {
        var afterHealResults = new List<ActionResultInfo>();
        var afterSkillInfo = _currentBattler.Skills.Find(a => a.FeatureDates.Find(b => b.FeatureType == FeatureType.AddState && (StateType)b.Param1 == StateType.AfterHeal) != null);
        if (_currentBattler.IsState(StateType.AfterHeal) && afterSkillInfo != null)
        {
            var stateInfo = _currentBattler.GetStateInfo(StateType.AfterHeal);
            var skillInfo = new SkillInfo(afterSkillInfo.Id);
            var actionInfo = MakeActionInfo(_currentBattler,skillInfo,false,false);
            
            if (actionInfo != null)
            {
                _actionInfos.Remove(actionInfo);
                var party = _currentBattler.isActor ? _party.AliveBattlerInfos : _troop.AliveBattlerInfos;
                var targetIndexes = new List<int>();
                foreach (var member in party)
                {
                    if (_currentBattler.Index != member.Index)
                    {
                        targetIndexes.Add(member.Index);
                    }   
                }
                foreach (var targetIndex in targetIndexes)
                {
                    var featureData = new SkillData.FeatureData();
                    featureData.FeatureType = FeatureType.HpHeal;
                    featureData.Param1 = stateInfo.Effect;

                    var actionResultInfo = new ActionResultInfo(GetBattlerInfo(targetIndex),GetBattlerInfo(targetIndex),new List<SkillData.FeatureData>(){featureData},-1);
                    afterHealResults.Add(actionResultInfo);
                }
            }
        }
        return afterHealResults;
    }

    private List<ActionResultInfo> UndeadHealActionResults()
    {
        var UndeadHealResults = MakeStateActionResult(_currentBattler,StateType.Undead,FeatureType.HpHeal,(int)HpHealType.RateValue);
        return UndeadHealResults;
    }

    private List<ActionResultInfo> AssistHealActionResults()
    {
        var assistHealResults = new List<ActionResultInfo>();
        var afterSkillInfo = _currentBattler.Skills.Find(a => a.FeatureDates.Find(b => b.FeatureType == FeatureType.AddState && (StateType)b.Param1 == StateType.AssistHeal) != null);
        if (_currentBattler.IsState(StateType.AssistHeal) && afterSkillInfo != null)
        {
            var stateInfo = _currentBattler.GetStateInfo(StateType.AssistHeal);
            var skillInfo = new SkillInfo(afterSkillInfo.Id);
            var actionInfo = MakeActionInfo(_currentBattler,skillInfo,false,false);
            
            if (actionInfo != null)
            {
                _actionInfos.Remove(actionInfo);
                var party = _currentBattler.isActor ? _party.AliveBattlerInfos : _troop.AliveBattlerInfos;
                party = party.FindAll(a => a.IsAlive());
                var targetIndexes = new List<int>();
                foreach (var member in party)
                {
                    //if (_currentBattler.Index != member.Index)
                    //{
                        targetIndexes.Add(member.Index);
                    //}
                    
                }
                var healValue = CurrentActionInfo().ActionResults.FindAll(a => a.HpDamage > 0).Count;
                foreach (var targetIndex in targetIndexes)
                {
                    var featureData = new SkillData.FeatureData();
                    featureData.FeatureType = FeatureType.HpHeal;
                    featureData.Param1 = healValue * stateInfo.Effect;

                    var actionResultInfo = new ActionResultInfo(GetBattlerInfo(targetIndex),GetBattlerInfo(targetIndex),new List<SkillData.FeatureData>(){featureData},-1);
                    assistHealResults.Add(actionResultInfo);
                }
            }
        }
        return assistHealResults;
    }

    public List<ActionResultInfo> CheckBurnDamage()
    {
        var results = MakeStateActionResult(_currentBattler,StateType.BurnDamage,FeatureType.HpDefineDamage);
        var results2 = MakeStateActionResult(_currentBattler,StateType.BurnDamagePer,FeatureType.NoEffectHpPerDamage);
        results.AddRange(results2);
        // 対象ごとにHpダメージでまとめる
        var targetIndexes = new List<int>();
        foreach (var result in results)
        {
            if (!targetIndexes.Contains(result.TargetIndex))
            {
                targetIndexes.Add(result.TargetIndex);
            }
        }
        foreach (var targetIndex in targetIndexes)
        {
            var damageResults = results.FindAll(a => a.HpDamage > 0 && a.TargetIndex == targetIndex);
            if (damageResults.Count > 1)
            {
                int hpDamage = 0;
                foreach (var damageResult in damageResults)
                {
                    hpDamage += damageResult.HpDamage;
                    damageResult.SetHpDamage(0);
                }
                damageResults[damageResults.Count-1].SetHpDamage(hpDamage);
            }
        }
        // HpDamageによってDeadIndexを変更
        if (results.Count > 0)
        {
            var result = results[results.Count-1];
            if (result.HpDamage > GetBattlerInfo(result.TargetIndex).Hp && !result.DeadIndexList.Contains(result.TargetIndex))
            {
                results[results.Count-1].DeadIndexList.Add(result.TargetIndex);
            }
        }
        return results;
    }

    /// <summary>
    /// battlerInfoが付与したステート効果の結果を取得
    /// </summary>
    public List<ActionResultInfo> MakeStateActionResult(BattlerInfo battlerInfo,StateType stateType,FeatureType featureType,int param3Value = 0)
    {
        var actionResultInfos = new List<ActionResultInfo>();
        var stateInfos = battlerInfo.GetStateInfoAll(stateType);
        
        var featureData = new SkillData.FeatureData();
        featureData.FeatureType = featureType;

        for (int i = 0;i < stateInfos.Count;i++)
        {
            featureData.Param1 = stateInfos[i].Effect;
            if (param3Value > 0)
            {
                featureData.Param3 = param3Value;
            }
            var target = GetBattlerInfo(stateInfos[i].BattlerId);
            if (target.IsAlive())
            {
                var actionResultInfo = new ActionResultInfo(GetBattlerInfo(stateInfos[i].BattlerId),GetBattlerInfo(stateInfos[i].TargetIndex),new List<SkillData.FeatureData>(){featureData},-1);
                actionResultInfos.Add(actionResultInfo);
            }
        }
        return actionResultInfos;
    }

    // リザルトから発生するトリガースキルを生成
    public List<ActionInfo> CheckTriggerSkillInfos(TriggerTiming triggerTiming,ActionInfo actionInfo,List<ActionResultInfo> actionResultInfos)
    {
        var madeActionInfos = new List<ActionInfo>();
        /*
        if (_currentBattler == null)
        {
            return false;
        }
        */
        var actionInfos = new List<ActionInfo>();
        var triggeredSkills = new List<SkillInfo>();
        
        for (var i = 0;i < _battlers.Count;i++)
        {
            var checkBattler = _battlers[i];
            triggeredSkills.Clear();
            foreach (var skillInfo in checkBattler.ActiveSkills())
            {
                if (actionInfo == null || (actionInfo.Master.Id != skillInfo.Id))
                {
                    var triggerDates = skillInfo.Master.TriggerDates.FindAll(a => a.TriggerTiming == triggerTiming);
                    if (IsTriggeredSkillInfo(checkBattler,triggerDates,actionInfo,actionResultInfos))
                    {
                        triggeredSkills.Add(skillInfo);
                    }
                }
            }
            if (triggeredSkills.Count > 0)
            {
                for (var j = 0;j < triggeredSkills.Count;j++)
                {
                    if (triggeredSkills[j].Master.SkillType == SkillType.Demigod){
                        if (checkBattler.IsAwaken == false)
                        {
                            checkBattler.SetAwaken();
                            var makeActionInfo = MakeActionInfo(checkBattler,triggeredSkills[j],triggerTiming == TriggerTiming.Interrupt,true);
                            madeActionInfos.Add(makeActionInfo);
                        }
                    } else{
                        var makeActionInfo = MakeActionInfo(checkBattler,triggeredSkills[j],triggerTiming == TriggerTiming.Interrupt,true);
                        madeActionInfos.Add(makeActionInfo);
                    }
                }
            }
        }

        return madeActionInfos;
    }

    public List<ActionResultInfo> CheckTriggerPassiveInfos(List<TriggerTiming> triggerTimings,ActionInfo actionInfo = null, List<ActionResultInfo> actionResultInfos = null)
    {
        var makeActionResults = new List<ActionResultInfo>();
        foreach (var battlerInfo in _battlers)
        {
            foreach (var passiveInfo in battlerInfo.PassiveSkills())
            {
                if (battlerInfo.IsState(StateType.NoPassive))
                {
                    continue;
                }
                var triggerDates = passiveInfo.Master.TriggerDates.FindAll(a => triggerTimings.Contains(a.TriggerTiming));
                // バトル中〇回以下使用
                var inBattleUseCountUnder = triggerDates.Find(a => a.TriggerType == TriggerType.InBattleUseCountUnder);
                if (inBattleUseCountUnder != null)
                {
                    if (inBattleUseCountUnder.Param1 <= passiveInfo.UseCount)
                    {
                        continue;
                    }
                }
                if (IsTriggeredSkillInfo(battlerInfo,triggerDates,actionInfo,actionResultInfos))
                {                
                    bool usable = CanUsePassiveCount(battlerInfo,passiveInfo.Id,triggerDates);
                    if (usable)
                    {
                        var makeResultInfos = MakePassiveSkillActionResults(battlerInfo,triggerDates,actionResultInfos,passiveInfo);
                        makeActionResults.AddRange(makeResultInfos);
                    }
                }
            }
        }
        return makeActionResults;
    }

    private bool CanUsePassiveCount(BattlerInfo battlerInfo,int skillId,List<SkillData.TriggerData> triggerDates)
    {
        bool usable = true;
        // トリガーのParam2を使用回数制限にする
        var useLimit = triggerDates.Find(a => a.Param2 >= 1);
        if (useLimit != null)
        {
            var usedCount = _usedPassiveSkillInfos[battlerInfo.Index].FindAll(a => a == skillId);
            if (usedCount.Count < useLimit.Param2)
            {
                _usedPassiveSkillInfos[battlerInfo.Index].Add(skillId);
            } else
            {
                usable = false;
            }
        }
        return usable;
    }

    private List<ActionResultInfo> MakePassiveSkillActionResults(BattlerInfo battlerInfo,List<SkillData.TriggerData> triggerDates,List<ActionResultInfo> actionResultInfos,SkillInfo passiveInfo)
    {
        var makeResultInfos = new List<ActionResultInfo>();
        if (passiveInfo.Master.Scope == ScopeType.Self)
        {
            var actionResultInfo = new ActionResultInfo(battlerInfo,battlerInfo,passiveInfo.FeatureDates,passiveInfo.Id);
            makeResultInfos.Add(actionResultInfo);
        } else
        if (passiveInfo.Master.Scope == ScopeType.All)
        {
            var partyMember = battlerInfo.isActor ? BattlerActors() : BattlerEnemies();
            if (passiveInfo.Master.TargetType == TargetType.Opponent){
                partyMember = battlerInfo.isActor ? BattlerEnemies() : BattlerActors();
            }
            if (passiveInfo.Master.AliveOnly)
            {
                partyMember = partyMember.FindAll(a => a.IsAlive());
            } else
            {
                partyMember = partyMember.FindAll(a => !a.IsAlive());
            }
            var targetIndexList = new List<int>();
            foreach (var member in partyMember)
            {
                targetIndexList.Add(member.Index);
            }
            targetIndexList = CheckScopeTriggers(targetIndexList,passiveInfo.Master.ScopeTriggers);
            foreach (var targetIndex in targetIndexList)
            {
                var actionResultInfo = new ActionResultInfo(battlerInfo,GetBattlerInfo(targetIndex),passiveInfo.FeatureDates,passiveInfo.Id);
                makeResultInfos.Add(actionResultInfo);
            }
        } else
        if (passiveInfo.Master.Scope == ScopeType.One){
            if (passiveInfo.Master.TargetType == TargetType.IsTriggerTarget)
            {
                var targetList = TriggerTargetList(battlerInfo,triggerDates,actionResultInfos);
                if (targetList.Count > 0)
                {
                    var actionResultInfo = new ActionResultInfo(battlerInfo,targetList[0],passiveInfo.FeatureDates,passiveInfo.Id);
                    makeResultInfos.Add(actionResultInfo);
                }
            }
            if (passiveInfo.Master.TargetType == TargetType.AttackTarget)
            {
                foreach (var actionResult in actionResultInfos)
                {
                    if (actionResult.Missed == false)
                    {
                        var actionResultInfo = new ActionResultInfo(battlerInfo,GetBattlerInfo(actionResult.TargetIndex),passiveInfo.FeatureDates,passiveInfo.Id);
                        makeResultInfos.Add(actionResultInfo);
                    }
                }
            }
        } else
        if (passiveInfo.Master.Scope == ScopeType.RandomOne)
        {
            var partyMember = battlerInfo.isActor ? BattlerActors() : BattlerEnemies();
            if (passiveInfo.Master.TargetType == TargetType.Opponent){
                partyMember = battlerInfo.isActor ? BattlerEnemies() : BattlerActors();
            }
            if (passiveInfo.Master.AliveOnly)
            {
                partyMember = partyMember.FindAll(a => a.IsAlive());
            } else
            {
                partyMember = partyMember.FindAll(a => !a.IsAlive());
            }
            var rand = UnityEngine.Random.Range(0,partyMember.Count);
            var actionResultInfo = new ActionResultInfo(battlerInfo,partyMember[rand],passiveInfo.FeatureDates,passiveInfo.Id);
            makeResultInfos.Add(actionResultInfo);
        } else
        {
            if (passiveInfo.Master.TargetType == TargetType.AttackTarget)
            {
                foreach (var actionResult in actionResultInfos)
                {
                    if (actionResult.Missed == false)
                    {
                        var actionResultInfo = new ActionResultInfo(battlerInfo,GetBattlerInfo(actionResult.TargetIndex),passiveInfo.FeatureDates,passiveInfo.Id);
                        makeResultInfos.Add(actionResultInfo);
                    }
                }
            }
        }
        if (!_passiveSkillInfos[battlerInfo.Index].Contains(passiveInfo.Master.Id))
        {
            _passiveSkillInfos[battlerInfo.Index].Add(passiveInfo.Master.Id);
        }
        passiveInfo.GainUseCount();
        return makeResultInfos;
    }

    public List<ActionResultInfo> CheckPlusPassiveInfos(ActionResultInfo actionResultInfo,int skillId)
    {
        var actionResultInfos = new List<ActionResultInfo>();
        var skill = DataSystem.FindSkill(skillId);
        var triggerDates = skill.TriggerDates;
        var battlerInfo = GetBattlerInfo(actionResultInfo.SubjectIndex);
        var target = GetBattlerInfo(actionResultInfo.TargetIndex);
        var enable = true;
        // 簡易判定
        foreach (var triggerData in triggerDates)
        {
            if (!triggerData.IsTriggeredSkillInfo(battlerInfo,BattlerActors(),BattlerEnemies()))
            {
                enable = false;
            }
        }
        if (enable)
        {
            var makeResultInfos = MakePassiveSkillActionResults(battlerInfo,triggerDates,new List<ActionResultInfo>(){actionResultInfo},new SkillInfo(skillId));
            actionResultInfos.AddRange(makeResultInfos);
        }
        return actionResultInfos;
    }
    
    public List<ActionResultInfo> CheckRemovePassiveInfos()
    {
        var actionResultInfos = new List<ActionResultInfo>();
        for (int i = 0;i < _battlers.Count;i++)
        {
            var battlerInfo = _battlers[i];
            var passiveSkillIds = _passiveSkillInfos[battlerInfo.Index];
            for (int j = 0;j < passiveSkillIds.Count;j++)
            {
                var passiveSkillData = DataSystem.FindSkill(passiveSkillIds[j]);
                bool IsRemove = false;
                
                foreach (var feature in passiveSkillData.FeatureDates)
                {
                    if (feature.FeatureType == FeatureType.AddState)
                    {
                        var triggerDates = passiveSkillData.TriggerDates.FindAll(a => a.TriggerTiming == TriggerTiming.After || a.TriggerTiming == TriggerTiming.StartBattle || a.TriggerTiming == TriggerTiming.AfterAndStartBattle);
                        if (IsRemove == false && !IsTriggeredSkillInfo(battlerInfo,triggerDates,null,new List<ActionResultInfo>()))
                        {
                            IsRemove = true;
                            
                            var featureData = new SkillData.FeatureData();
                            featureData.FeatureType = FeatureType.RemoveStatePassive;
                            featureData.Param1 = feature.Param1;
                            if (passiveSkillData.Scope == ScopeType.Self)
                            {
                                var actionResultInfo = new ActionResultInfo(battlerInfo,battlerInfo,new List<SkillData.FeatureData>(){featureData},passiveSkillData.Id);
                                if (actionResultInfos.Find(a => a.RemovedStates.Find(b => b.Master.StateType == (StateType)featureData.FeatureType) != null) != null)
                                {
                                    
                                } else{
                                    var stateInfos = battlerInfo.GetStateInfoAll((StateType)feature.Param1);
                                    if (battlerInfo.IsAlive() && stateInfos.Find(a => a.SkillId == passiveSkillData.Id) != null)
                                    {
                                        actionResultInfos.Add(actionResultInfo);
                                    }
                                }
                            } else
                            if (passiveSkillData.Scope == ScopeType.All)
                            {
                                var partyMember = battlerInfo.isActor ? BattlerActors() : BattlerEnemies();
                                foreach (var member in partyMember)
                                {
                                    if ((passiveSkillData.AliveOnly && member.IsAlive()) || (!passiveSkillData.AliveOnly && !member.IsAlive()))
                                    {

                                        var actionResultInfo = new ActionResultInfo(battlerInfo,member,new List<SkillData.FeatureData>(){featureData},passiveSkillData.Id);
                                        if (actionResultInfos.Find(a => a.RemovedStates.Find(b => b.Master.StateType == (StateType)featureData.FeatureType) != null) != null)
                                        {
                                            
                                        } else{
                                            var stateInfos = battlerInfo.GetStateInfoAll((StateType)feature.Param1);
                                            if (member.IsAlive() && stateInfos.Find(a => a.SkillId == passiveSkillData.Id) != null)
                                            {
                                                actionResultInfos.Add(actionResultInfo);
                                            }
                                        }
                                    }
                                }
                            }

                        }
                    }
                }
            }
        }
        return actionResultInfos;
    }

    private bool IsTriggeredSkillInfo(BattlerInfo battlerInfo,List<SkillData.TriggerData> triggerDates,ActionInfo actionInfo,List<ActionResultInfo> actionResultInfos)
    {
        bool IsTriggered = false;
        if (triggerDates.Count > 0)
        {
            for (var j = 0;j < triggerDates.Count;j++)
            {
                /*
                if (triggerTiming == TriggerTiming.Use)
                {
                    if (actionInfo == null)
                    {
                        IsTriggered = false;
                        break;
                    }
                    if (actionInfo != null && actionInfo.SubjectIndex != battlerInfo.Index)
                    {
                        IsTriggered = false;
                        break;
                    }
                }
                */
                // 簡易判定
                if (triggerDates[j].IsTriggeredSkillInfo(battlerInfo,BattlerActors(),BattlerEnemies()))
                {
                    IsTriggered = true;
                }
                if (triggerDates[j].TriggerType == TriggerType.AttackState)
                {
                    if (battlerInfo.IsAlive() && actionInfo != null && actionInfo.SubjectIndex == battlerInfo.Index && actionInfo.ActionResults.Find(a => a.HpDamage > 0) != null)
                    {
                        if (triggerDates[j].Param1 > UnityEngine.Random.Range(0,100))
                        {
                            IsTriggered = true;
                        }
                    }
                }
                if (triggerDates[j].TriggerType == TriggerType.ActionMpCost)
                {
                    if (battlerInfo.IsAlive() && actionInfo != null && actionInfo.MpCost == triggerDates[j].Param1)
                    {
                        IsTriggered = true;
                    }
                }
                if (triggerDates[j].TriggerType == TriggerType.TargetHpRateUnder)
                {
                    if (battlerInfo.IsAlive() && actionResultInfos != null && actionResultInfos.Count > 0)
                    {
                        foreach (var actionResultInfo in actionResultInfos)
                        {
                            if (actionResultInfo.HpDamage > 0 && actionResultInfo.TargetIndex != actionResultInfo.SubjectIndex)
                            {
                                var targetBattlerInfo = GetBattlerInfo(actionResultInfo.TargetIndex);
                                if (battlerInfo.isActor == targetBattlerInfo.isActor)
                                {
                                    if (targetBattlerInfo.Hp == 0)
                                    {
                                        if (0 == triggerDates[j].Param1)
                                        {
                                            IsTriggered = true;
                                        }
                                    }else
                                    {
                                        if (((float)targetBattlerInfo.Hp / (float)targetBattlerInfo.MaxHp) <= triggerDates[j].Param1 * 0.01f)
                                        {
                                            IsTriggered = true;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                if (triggerDates[j].TriggerType == TriggerType.PayBattleMp)
                {
                    if (battlerInfo.IsAlive() && battlerInfo.PayBattleMp >= triggerDates[j].Param1)
                    {
                        IsTriggered = true;
                    }
                }
                if (battlerInfo.IsAwaken == false && triggerDates[j].TriggerType == TriggerType.ActionResultDeath)
                {
                    if (battlerInfo.IsAlive())
                    {
                        if (battlerInfo.isActor)
                        {
                            if (actionResultInfos.Find(a => _party.AliveBattlerInfos.Find(b => a.DeadIndexList.Contains(b.Index)) != null) != null)
                            {
                                IsTriggered = true;
                            }
                        } else
                        {
                            if (actionResultInfos.Find(a => _troop.AliveBattlerInfos.Find(b => a.DeadIndexList.Contains(b.Index)) != null) != null)
                            {
                                IsTriggered = true;
                            }  
                        }
                    }
                }
                if (battlerInfo.IsAwaken == false && triggerDates[j].TriggerType == TriggerType.DeadWithoutSelf)
                {
                    if (battlerInfo.isActor)
                    {
                        int count = 0;
                        for (var i = 0;i < _battlers.Count;i++)
                        {
                            if (_battlers[i].isActor && _battlers[i].IsState(StateType.Death))
                            {
                                count++;
                            }
                        }
                        if (battlerInfo.IsAlive() && count > 0 && (count+1) >= _battlers.FindAll(a => a.isActor).Count)
                        {
                            IsTriggered = true;
                        }
                    } else
                    {
                        int count = 0;
                        for (var i = 0;i < _battlers.Count;i++)
                        {
                            if (!_battlers[i].isActor && _battlers[i].IsState(StateType.Death))
                            {
                                count++;
                            }
                        }
                        if (battlerInfo.IsAlive() && count > 0 && (count+1) >= _battlers.FindAll(a => !a.isActor).Count)
                        {
                            IsTriggered = true;
                        }
                    } 
                }
                if (battlerInfo.IsAwaken == false && triggerDates[j].TriggerType == TriggerType.SelfDead)
                {
                    if (actionResultInfos.Find(a => a.DeadIndexList.Contains(battlerInfo.Index) == true) != null)
                    {
                        IsTriggered = true;
                        var stateInfos = battlerInfo.GetStateInfoAll(StateType.Death);
                        for (var i = 0;i < stateInfos.Count;i++){
                            battlerInfo.RemoveState(stateInfos[i],true);
                            battlerInfo.SetPreserveAlive(true);
                        }
                    }
                }
                if (battlerInfo.IsAwaken == false && triggerDates[j].TriggerType == TriggerType.AttackedCount)
                {
                    if (battlerInfo.IsAlive() && battlerInfo.AttackedCount >= triggerDates[j].Param1)
                    {
                        IsTriggered = true;
                    }
                }
                if (battlerInfo.IsAwaken == false && triggerDates[j].TriggerType == TriggerType.AllEnemyCurseState)
                {
                    var opponents = battlerInfo.isActor ? _troop.AliveBattlerInfos : _party.AliveBattlerInfos;
                    if (battlerInfo.IsAlive() && opponents.Find(a => !a.IsState(StateType.Curse)) == null && opponents.FindAll(a => a.IsAlive()).Count > 0)
                    {
                        IsTriggered = true;
                    }
                }
                if (battlerInfo.IsAwaken == false && triggerDates[j].TriggerType == TriggerType.ActionResultAddState)
                {
                    if (battlerInfo.IsAlive())
                    {
                        if (actionInfo != null && battlerInfo.isActor != GetBattlerInfo(actionInfo.SubjectIndex).isActor)
                        {
                            var states = actionInfo.SkillInfo.FeatureDates.FindAll(a => a.FeatureType == FeatureType.AddState);
                            foreach (var state in states)
                            {
                                if (state.Param1 == (int)StateType.Stun || state.Param1 == (int)StateType.Slow || state.Param1 == (int)StateType.Curse || state.Param1 == (int)StateType.BurnDamage || state.Param1 == (int)StateType.Blind || state.Param1 == (int)StateType.Freeze)
                                {
                                    IsTriggered = true;
                                }
                            }
                        }
                    }
                }
                if (battlerInfo.IsAwaken == false && triggerDates[j].TriggerType == TriggerType.DefeatEnemyByAttack)
                {
                    var attackBattler = GetBattlerInfo(actionInfo.SubjectIndex);
                    if (battlerInfo.IsAlive() && attackBattler != null && battlerInfo.Index == attackBattler.Index)
                    {
                        foreach (var actionResultInfo in actionResultInfos)
                        {
                            foreach (var deadIndex in actionResultInfo.DeadIndexList)
                            {
                                if (battlerInfo.isActor != GetBattlerInfo(deadIndex).isActor)
                                {
                                    IsTriggered = true;
                                }
                            }
                        }
                    }
                }
                if (triggerDates[j].TriggerType == TriggerType.LessTroopMembers)
                {
                    var troops = _battlers.FindAll(a => !a.isActor);
                    var party = _battlers.FindAll(a => a.isActor);
                    if ( troops.Count >= party.Count )
                    {
                        IsTriggered = true;
                    }
                }
                if (triggerDates[j].TriggerType == TriggerType.MoreTroopMembers)
                {
                    var troops = _battlers.FindAll(a => !a.isActor);
                    var party = _battlers.FindAll(a => a.isActor);
                    if ( troops.Count <= party.Count )
                    {
                        IsTriggered = true;
                    }
                }
                
                if (triggerDates[j].TriggerType == TriggerType.DodgeCountOver)
                {
                    if (battlerInfo.IsAlive() && battlerInfo.DodgeCount >= triggerDates[j].Param1)
                    {
                        IsTriggered = true;
                    }
                }
                if (triggerDates[j].TriggerType == TriggerType.HpHealCountOver)
                {
                    if (battlerInfo.IsAlive() && battlerInfo.HealCount >= triggerDates[j].Param1)
                    {
                        IsTriggered = true;
                    }
                }
                if (triggerDates[j].TriggerType == TriggerType.AwakenDemigodAttribute)
                {
                    var friends = battlerInfo.isActor ? _party.AliveBattlerInfos : _troop.AliveBattlerInfos;
                    friends = friends.FindAll(a => a.IsAwaken);
                    if (battlerInfo.IsAlive() && friends.Count > 0 && friends.Find(a => a.Skills.Find(b => (b.Attribute == (AttributeType)triggerDates[j].Param1 && b.Master.SkillType == SkillType.Demigod)) != null) != null)
                    {
                        IsTriggered = true;
                    }
                }
                // Param3をAnd条件フラグにする
                if (triggerDates[j].Param3 == 1)
                {
                    if (IsTriggered)
                    {
                        IsTriggered = false;
                    } else{
                        break;
                    }
                }
            }
        }

        return IsTriggered;
    }

    private List<BattlerInfo> TriggerTargetList(BattlerInfo battlerInfo,List<SkillData.TriggerData> triggerDates,List<ActionResultInfo> actionResultInfos)
    {
        var list = new List<BattlerInfo>();
        for (int i = 0;i < triggerDates.Count;i++)
        {
            if (triggerDates[i].TriggerType == TriggerType.TargetHpRateUnder)
            {
                if (battlerInfo.IsAlive())
                {
                    foreach (var actionResultInfo in actionResultInfos)
                    {
                        var targetBattlerInfo = GetBattlerInfo(actionResultInfo.TargetIndex);
                        if (targetBattlerInfo.Hp == 0)
                        {
                            if (0 == triggerDates[i].Param1)
                            {
                                list.Add(targetBattlerInfo);
                            }
                        }else
                        if (((float)targetBattlerInfo.Hp / (float)targetBattlerInfo.MaxHp) < triggerDates[i].Param1 * 0.01f)
                        {
                            list.Add(targetBattlerInfo);
                        }
                    }
                }
            }
            if (triggerDates[i].TriggerType == TriggerType.ActionResultDeath)
            {
                if (battlerInfo.IsAlive())
                {
                    if (battlerInfo.isActor)
                    {
                        var deathTarget = actionResultInfos.Find(a => _party.AliveBattlerInfos.Find(b => a.DeadIndexList.Contains(b.Index)) != null);
                        if (deathTarget != null)
                        {
                            var targetBattlerInfo = GetBattlerInfo(deathTarget.TargetIndex);
                            list.Add(targetBattlerInfo);
                        }
                    } else
                    {
                        var deathTarget = actionResultInfos.Find(a => _troop.AliveBattlerInfos.Find(b => a.DeadIndexList.Contains(b.Index)) != null);
                        if (deathTarget != null)
                        {
                            var targetBattlerInfo = GetBattlerInfo(deathTarget.TargetIndex);
                            list.Add(targetBattlerInfo);
                        }
                    }
                }
            }
        }
        return list;
    }

    private bool CanUseTrigger(SkillInfo skillInfo,BattlerInfo battlerInfo)
    {
        bool CanUse = true;
        if (skillInfo.TriggerDates.Count > 0)
        {
            CanUse = false;
            CanUse = IsTriggeredSkillInfo(battlerInfo,skillInfo.TriggerDates,null,new List<ActionResultInfo>());
        }
        return CanUse;
    }

    public List<int> MakeAutoSelectIndex(ActionInfo actionInfo,int oneTargetIndex = -1)
    {
        var indexList = new List<int>();
        // interruptされた行動の対象を引き継ぐ
        if (actionInfo.ActionResults.Count > 0)
        {
            foreach (var actionResultInfo in actionInfo.ActionResults)
            {
                if (actionResultInfo.CursedDamage == false && !indexList.Contains(actionResultInfo.TargetIndex))
                {
                    indexList.Add(actionResultInfo.TargetIndex);
                } 
            }
            return indexList;
        }
        var targetIndexList = GetSkillTargetIndexes(actionInfo.Master.Id,actionInfo.SubjectIndex,true);
        if (targetIndexList.Count == 0)
        {
            return targetIndexList;
        }
        
        int targetRand = 0;
        for (int i = 0;i < targetIndexList.Count;i++)
        {
            var battlerInfo = GetBattlerInfo(targetIndexList[i]);
            if (actionInfo.Master.Scope == ScopeType.WithoutSelfOne || actionInfo.Master.Scope == ScopeType.WithoutSelfAll)
            {
                if (battlerInfo.Index == actionInfo.SubjectIndex)
                {
                    continue;
                }
            }
            targetRand += battlerInfo.TargetRate();
        }
        targetRand = UnityEngine.Random.Range (0,targetRand);
        int targetIndex = -1;
        for (int i = 0;i < targetIndexList.Count;i++)
        {
            var battlerInfo = GetBattlerInfo(targetIndexList[i]);
            if (battlerInfo.Index == actionInfo.SubjectIndex)
            {
                continue;
            }
            targetRand -= battlerInfo.TargetRate();
            if (targetRand <= 0 && targetIndex == -1)
            {
                targetIndex = targetIndexList[i];
            }
        }
        // 挑発
        if (_currentBattler.IsState(StateType.Substitute))
        {
            int substituteId = _currentBattler.GetStateInfo(StateType.Substitute).BattlerId;
            if (targetIndexList.Contains(substituteId))
            {
                targetIndex = substituteId;
            } else
            {
                var tempIndexList = GetSkillTargetIndexes(actionInfo.Master.Id,actionInfo.SubjectIndex,false);
                if (tempIndexList.Contains(substituteId))
                {
                    targetIndex = substituteId;
                }
            }
        }
        if (targetIndex == -1)
        {
            targetIndex = targetIndexList [UnityEngine.Random.Range (0, targetIndexList.Count)];
        }
        //int targetIndex = targetIndexList [UnityEngine.Random.Range (0, targetIndexList.Count)];
        
        var scopeType = actionInfo.Master.Scope;
        if (_currentBattler.IsState(StateType.EffectLine))
        {
            scopeType = ScopeType.Line;
        }
        if (_currentBattler.IsState(StateType.EffectAll))
        {
            scopeType = ScopeType.All;
        }
        if (scopeType == ScopeType.All)
        {
            indexList = targetIndexList;
        }
        if (scopeType == ScopeType.Self)
        {
            indexList = targetIndexList;
        }
        if (scopeType == ScopeType.One)
        {
            if (!_currentBattler.IsState(StateType.Substitute) && oneTargetIndex > -1)
            {
                indexList.Add (oneTargetIndex);
            } else
            {
                indexList.Add (targetIndex);
            }
        }
        if (scopeType == ScopeType.WithoutSelfOne)
        {
            indexList.Add (targetIndex);
        }
        if (scopeType == ScopeType.WithoutSelfAll)
        {
            indexList.Clear();
            for (int i = 0;i < targetIndexList.Count;i++)
            {
                var battlerInfo = GetBattlerInfo(targetIndexList[i]);
                if (battlerInfo.Index == actionInfo.SubjectIndex)
                {
                    continue;
                }
                indexList.Add(targetIndexList[i]);
            }
        }
        if (scopeType == ScopeType.Line || scopeType == ScopeType.FrontLine)
        {
            indexList.Add (targetIndex);
            var battlerInfo = GetBattlerInfo(targetIndex);
            foreach (var targetBattlerInfo in FieldBattlerInfos())
            {   
                if (battlerInfo.isActor && targetBattlerInfo.isActor)
                {
                    if (battlerInfo.LineIndex == targetBattlerInfo.LineIndex)
                    {
                        if (indexList.IndexOf(targetBattlerInfo.Index) == -1)
                        {                        
                            indexList.Add (targetBattlerInfo.Index);
                        }
                    }
                }
                if (!battlerInfo.isActor && !targetBattlerInfo.isActor)
                {
                    if (battlerInfo.LineIndex == targetBattlerInfo.LineIndex)
                    {
                        if (indexList.IndexOf(targetBattlerInfo.Index) == -1)
                        {                        
                            indexList.Add (targetBattlerInfo.Index);
                        }
                    }
                }
            }
        }
        // 生存判定
        if (actionInfo.Master.AliveOnly)
        {
            indexList = indexList.FindAll(a => _battlers.Find(b => a == b.Index).IsAlive());
        } else
        {
            indexList = indexList.FindAll(a => !_battlers.Find(b => a == b.Index).IsAlive());
        }
        return indexList;
    }

    public int MakeAutoSkillId(BattlerInfo battlerInfo)
    {
        var skillInfos = battlerInfo.ActiveSkills().FindAll(a => CheckCanUse(a,battlerInfo));
        if (skillInfos.Count == 0)
        {
            return 0;
        }
        int weight = 0;
        for (int i = 0;i < skillInfos.Count;i++)
        {
            weight += skillInfos[i].Weight;
        }
        weight = UnityEngine.Random.Range(0,weight);
        int skillIndex = -1;
        for (int i = 0;i < skillInfos.Count;i++)
        {
            weight -= skillInfos[i].Weight;
            if (weight <= 0 && skillIndex == -1)
            {
                skillIndex = i;
            }
        }
        
        return skillInfos[skillIndex].Id;
    }

    public (int,int) MakeAutoActorSkillId(BattlerInfo battlerInfo)
    {
        var skillInfos = battlerInfo.ActiveSkills().FindAll(a => CheckCanUse(a,battlerInfo));
        var (skillId,targetIndex) = BattleActorAI.MakeAutoActorSkillId(skillInfos,battlerInfo,BattlerActors(),BattlerEnemies());
        return (skillId,targetIndex);
    }

    public List<ActionResultInfo> CalcDeathIndexList(List<ActionResultInfo> actionResultInfos)
    {
        // 複数回ダメージで戦闘不能になるかチェック
        var deathIndexes = new List<int>();
        var damageData = new Dictionary<int,int>();
        foreach (var actionResultInfo in actionResultInfos)
        {
            if (!damageData.TryGetValue(actionResultInfo.TargetIndex ,out var value))
            {
                damageData[actionResultInfo.TargetIndex] = 0;
            }
            if (!damageData.TryGetValue(actionResultInfo.SubjectIndex ,out var value2))
            {
                damageData[actionResultInfo.SubjectIndex] = 0;
            }
            if (actionResultInfo.HpDamage != 0)
            {
                damageData[actionResultInfo.TargetIndex] += actionResultInfo.HpDamage;
            }
            if (actionResultInfo.ReDamage != 0)
            {
                damageData[actionResultInfo.SubjectIndex] += actionResultInfo.ReDamage;
            }
            foreach (var battlerInfo in _battlers)
            {
                if (actionResultInfo.TargetIndex == battlerInfo.Index)
                {
                    if (damageData[actionResultInfo.TargetIndex] >= battlerInfo.Hp)
                    {
                        if (!deathIndexes.Contains(battlerInfo.Index) && !actionResultInfo.DeadIndexList.Contains(battlerInfo.Index))
                        {
                            deathIndexes.Add(battlerInfo.Index);
                            actionResultInfo.DeadIndexList.Add(battlerInfo.Index);
                        }
                    }
                }
                if (actionResultInfo.SubjectIndex == battlerInfo.Index)
                {
                    if (damageData[actionResultInfo.SubjectIndex] >= battlerInfo.Hp)
                    {
                        if (!deathIndexes.Contains(battlerInfo.Index) && !actionResultInfo.DeadIndexList.Contains(battlerInfo.Index))
                        {
                            deathIndexes.Add(battlerInfo.Index);
                            actionResultInfo.DeadIndexList.Add(battlerInfo.Index);
                        }
                    }
                }
            }
        }
        return actionResultInfos;
    }

    public List<BattlerInfo> PreservedAliveEnemies()
    {
        var list = new List<BattlerInfo>();
        foreach (var battlerInfo in FieldBattlerInfos())
        {
            if (battlerInfo.PreserveAlive)
            {
                list.Add(battlerInfo);
                battlerInfo.SetPreserveAlive(false);
            }
        }
        return list;
    }

    public List<BattlerInfo> NotDeadMembers()
    {
        var list = new List<BattlerInfo>();
        foreach (var battlerInfo in FieldBattlerInfos())
        {
            if (battlerInfo.Hp > 0 && battlerInfo.IsState(StateType.Death))
            {
                var states = battlerInfo.GetStateInfoAll(StateType.Death);
                foreach (var state in states)
                {                
                    battlerInfo.RemoveState(state,true);
                }
                list.Add(battlerInfo);
            }
        }
        return list;
    }

    // 戦闘不能の付与者のステート効果を解除する
    public List<StateInfo> EndRemoveState()
    {
        var removeStateInfos = new List<StateInfo>();
        //var StateTypes = RemoveDeathStateTypes();
        foreach (var battler in FieldBattlerInfos())
        {
            if (battler.IsAlive() == false)
            {
                for (var i = battler.StateInfos.Count-1;i >= 0;i--)
                {
                    if (battler.StateInfos[i].Master.RemoveByDeath)
                    {
                        if (battler.StateInfos[i].IsStartPassive() == false)
                        {
                            removeStateInfos.Add(battler.StateInfos[i]);
                            battler.RemoveState(battler.StateInfos[i],true);
                        }
                    }
                }
            }
        }
        /*
        foreach (var stateType in StateTypes)
        {
            for (int i = 0;i < _battlers.Count;i++)
            {
                var stateInfos = _battlers[i].GetStateInfoAll(stateType);
                if (stateInfos.Count > 0)
                {
                    foreach (var stateInfo in stateInfos)
                    {
                        BattlerInfo subject = GetBattlerInfo(stateInfo.BattlerId);
                        if (subject.IsAlive() == false)
                        {
                            _battlers[i].RemoveState(stateInfo,true);
                            removeStateInfos.Add(stateInfo);
                        }
                    }
                }
            }
        }
        */
        return removeStateInfos;
    }

    // 戦闘不能に聖棺がいたら他の対象に移す
    public List<StateInfo> EndHolyCoffinState()
    {
        var addStateInfos = new List<StateInfo>();
        //var StateTypes = RemoveDeathStateTypes();
        foreach (var battler in FieldBattlerInfos())
        {
            if (battler.IsAlive() == false)
            {
                for (var i = battler.StateInfos.Count-1;i >= 0;i--)
                {
                    if (battler.StateInfos[i].StateType == StateType.HolyCoffin)
                    {
                        battler.RemoveState(battler.StateInfos[i],true);
                        var randTargets = battler.isActor ? _party.AliveBattlerInfos : _troop.AliveBattlerInfos;
                        if (randTargets.Count > 0)
                        {
                            var rand = UnityEngine.Random.Range(0,randTargets.Count);
                            battler.StateInfos[i].SetTargetIndex(randTargets[rand].Index);
                            randTargets[rand].AddState(battler.StateInfos[i],true);
                            addStateInfos.Add(battler.StateInfos[i]);
                        }
                    }
                }
            }
        }
        return addStateInfos;
    }
    

    public void GainAttackCount(int targetIndex)
    {
        GetBattlerInfo(targetIndex).GainAttackedCount(1);
    }


    public bool CheckVictory()
    {
        bool isVictory = _troop.BattlerInfos.Find(a => a.IsAlive()) == null;
        return isVictory;
    }

    public void MakeBattleScore()
    {
        if (CheckVictory())
        {
            PartyInfo.SetBattleResultVictory(true);
            float score = 100;
            float damageAll = 0;
            float healAll = 0;
            float damagedAll = 0;
            foreach (var battleRecord in _battleRecords)
            {
                if (battleRecord.BattlerIndex < 100)
                {
                    damageAll += battleRecord.DamageValue;
                    healAll += battleRecord.HealValue;
                    damagedAll += battleRecord.DamagedValue;
                }
            }
            if (damageAll == 0 && damagedAll == 0)
            {
                PartyInfo.SetBattleScore((int)score);
            } else
            {
                var scoreRate = 1f - ((damagedAll-(healAll/2)) / (damageAll+damagedAll));
                score *= scoreRate;
                score = Math.Max(0,score);
                score = Math.Min(100,score);
                PartyInfo.SetBattleScore((int)score);
            }
        }
    }

    public List<GetItemInfo> MakeBattlerResult()
    {
        var list = new List<GetItemInfo>();
        list.AddRange(CurrentStage.CurrentSelectSymbol().GetItemInfos);
        if (CheckVictory() && CurrentStage.RecordStage == false)
        {
            foreach (var battlerInfo in _party.BattlerInfos)
            {
                
            }
        }
        return list;
    }

    public bool CheckDefeat()
    {
        bool isDefeat = _party.BattlerInfos.Find(a => a.IsAlive()) == null;
        if (isDefeat)
        {
            PartyInfo.SetBattleResultVictory(false);
            PartyInfo.SetBattleScore(0);
        }
        return isDefeat;
    }

    public bool EnableEscape()
    {
        var troopData = CurrentTroopInfo();
        if (troopData != null)
        {
            return troopData.EscapeEnable;
        }
        return false;
    }

    public void EscapeBattle()
    {
        PartyInfo.SetBattleResultVictory(false);
        PartyInfo.SetBattleScore(0);
    }

    public void EndBattle()
    {
        foreach (var battler in _party.BattlerInfos)
        {
            var actorInfo = Actors().Find(a => a.ActorId == battler.CharaId);
            actorInfo.ChangeHp(battler.Hp);
            actorInfo.ChangeMp(battler.Mp);
        }
        SaveSystem.SaveConfigStart(GameSystem.ConfigData);
    }

    public List<ListData> SideMenu()
    {
        var list = new List<SystemData.CommandData>();
        var escapeCommand = new SystemData.CommandData();
        escapeCommand.Id = 1;
        escapeCommand.Name = DataSystem.GetTextData(411).Text;
        escapeCommand.Key = "Escape";
        list.Add(escapeCommand);
        var menuCommand = new SystemData.CommandData();
        menuCommand.Id = 2;
        menuCommand.Name = DataSystem.GetTextData(703).Text;
        menuCommand.Key = "Help";
        list.Add(menuCommand);
        return MakeListData(list);
    }

    public SystemData.CommandData BattleAutoButton()
    {
        var menuCommand = new SystemData.CommandData();
        menuCommand.Id = 1;
        menuCommand.Name = DataSystem.GetTextData(706).Text;
        menuCommand.Key = "BATTLE_AUTO";
        return menuCommand;
    }

    public void ChangeBattleAuto()
    {
        ChangeBattleAuto(!GameSystem.ConfigData.BattleAuto);
    }

    public List<ListData> SelectCharacterConditions()
    {
        return MakeListData(_currentBattler.StateInfos);
    }
    
#if UNITY_EDITOR
    public List<TestActionData> testActionDates = new ();
    public int testActionIndex = 0;
    public void MakeTestBattleAction()
    {
        testActionDates.Clear();
        testActionDates = Resources.Load<TestBattleData>("Data/TestBattle").TestActionDates;
    }
    public BattlerInfo TestBattler()
    {
        if (testActionDates.Count > testActionIndex)
        {
            return GetBattlerInfo(testActionDates[testActionIndex].BattlerIndex);
        }
        return null;
    }
    
    public int TestSkillId()
    {
        if (testActionDates.Count > testActionIndex)
        {
            return testActionDates[testActionIndex].SkillId;
        }
        return 0;
    }

    public void SeekActionIndex()
    {
        testActionIndex++;
    }
#endif
}