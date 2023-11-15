using System;
using System.Collections.Generic;
using Effekseer;
using Cysharp.Threading.Tasks;

public class BattleModel : BaseModel
{
    private int _actionIndex = 0;

    private List<BattlerInfo> _battlers = new List<BattlerInfo>();
    public List<BattlerInfo> Battlers => _battlers;

    private UnitInfo _party;
    private UnitInfo _troop;

    private List<BattleRecord> _battleRecords = new ();
    public List<BattleRecord> BattleRecords => _battleRecords;

    private BattlerInfo _currentBattler = null;
    public BattlerInfo CurrentBattler => _currentBattler;

    private List<ActionInfo> _actionInfos = new ();

    private List<ActionInfo> _turnActionInfos = new ();
    private Dictionary<int,List<SkillInfo>> _passiveSkillInfos = new Dictionary<int,List<SkillInfo>>();
    private Dictionary<int,List<SkillInfo>> _usedPassiveSkillInfos = new Dictionary<int,List<SkillInfo>>();

    public UniTask<List<UnityEngine.AudioClip>> GetBattleBgm()
    {
        if (CurrentStage != null)
        {
            var troops = CurrentStage.CurrentTroopInfo();
            if (troops.TroopId >= 100 && troops.TroopId <= 600 && troops.TroopId % 100 == 0)
            {
                return GetBgmData("BOSS1");
            }
            if (troops.TroopId >= 1100 && troops.TroopId <= 1600)
            {
                return GetBgmData("BOSS1");
            }
            if (troops.TroopId >= 2000)
            {
                return GetBgmData("LAST_BOSS");
            }
        }
        var battleMembers = PartyMembers();
        return GetBgmData("BATTLE" + (battleMembers[0].Master.ClassId).ToString());
    }

    public void CreateBattleData()
    {
        _actionIndex = 0;
        _battlers.Clear();
        var battleMembers = PartyMembers();
        for (int i = 0;i < battleMembers.Count;i++)
        {
            if (battleMembers[i].InBattle == true)
            {
                BattlerInfo battlerInfo = new BattlerInfo(battleMembers[i],i);
                if (CurrentAlcana.AlcanaState != null)
                {
                    StateInfo stateInfo = CurrentAlcana.AlcanaState;
                    battlerInfo.AddState(stateInfo,true);
                }
                _battlers.Add(battlerInfo);
            }
        }
        var enemies = CurrentStage.CurrentBattleInfos();
        
        for (int i = 0;i < enemies.Count;i++)
        {
            _battlers.Add(enemies[i]);
        }
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


    public List<StateInfo> UpdateAp()
    {
        var removeStateList = new List<StateInfo>();
        _battlers.ForEach(battler => {
            battler.UpdateAp();
            var removeStates = battler.UpdateState(RemovalTiming.UpdateAp);
            if (removeStates.Count > 0)
            {
                removeStateList.AddRange(removeStates);
            }
        });
        MakeActionBattler();
        return removeStateList;
    }
    
    public List<ActionResultInfo> UpdateChainState()
    {
        List<ActionResultInfo> actionResultInfos = new ();
        for (int i = 0;i < _battlers.Count;i++)
        {
            List<StateInfo> chainDamageStateInfos = _battlers[i].UpdateChainState();
            for (int j = 0;j < chainDamageStateInfos.Count;j++)
            {
                StateInfo stateInfo = chainDamageStateInfos[j];
                BattlerInfo subject = GetBattlerInfo(stateInfo.BattlerId);
                BattlerInfo target = GetBattlerInfo(stateInfo.TargetIndex);
                if (subject.IsAlive() && target != null)
                {
                    int chainDamage = stateInfo.Effect;
                    if (subject.IsState(StateType.ChainDamageUp))
                    {
                        chainDamage += subject.ChainSuccessCount;
                    }
                    SkillData.FeatureData featureData = new SkillData.FeatureData();
                    featureData.FeatureType = FeatureType.HpDefineDamage;
                    featureData.Param1 = chainDamage;
                    
                    ActionResultInfo actionResultInfo = new ActionResultInfo(subject,target,new List<SkillData.FeatureData>(){featureData},-1);
                        
                    if ((target.Hp - chainDamage) <= 0)
                    {
                        _battlers[i].RemoveState(stateInfo,true);
                    }
                    actionResultInfos.Add(actionResultInfo);
                }
            }
        }
        return actionResultInfos;
    }

    public List<ActionResultInfo> UpdateBenedictionState()
    {
        List<ActionResultInfo> actionResultInfos = new ();
        for (int i = 0;i < _battlers.Count;i++)
        {
            List<StateInfo> benedictionStateInfos = _battlers[i].GetStateInfoAll(StateType.Benediction);
            for (int j = 0;j < benedictionStateInfos.Count;j++)
            {
                StateInfo stateInfo = benedictionStateInfos[j];
                if (stateInfo.Turns % stateInfo.BaseTurns == 0)
                {
                    BattlerInfo subject = _battlers.Find(a => a.Index == stateInfo.BattlerId);
                    if (subject.IsAlive())
                    {
                        List<BattlerInfo> targets = new List<BattlerInfo>();
                        if (subject.isActor)
                        {
                            targets = BattlerActors().FindAll(a => a.Index != subject.Index && a.IsAlive());
                        } else{
                            targets = BattlerEnemies().FindAll(a => a.Index != subject.Index && a.IsAlive());
                        }
                        SkillData.FeatureData featureData = new SkillData.FeatureData();
                        featureData.FeatureType = FeatureType.HpHeal;
                        featureData.Param1 = stateInfo.Effect;
                        foreach (var target in targets)
                        {
                            ActionResultInfo actionResultInfo = new ActionResultInfo(_battlers[i],target,new List<SkillData.FeatureData>(){featureData},-1);
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
        CurrentBattler.SetLastSelectSkillId(skillId);
    }

    public void MakeActionBattler()
    {
        _battlers.Sort((a,b) => a.Ap - b.Ap);
        for (int i = 0;i < _battlers.Count;i++)
        {
            BattlerInfo battlerInfo = _battlers[i];
            if (battlerInfo.Ap <= 0)
            {
                _currentBattler = battlerInfo;
                break;
            }
        }
    }

    public void SetActionBattler(int targetIndex)
    {
        var battlerInfo = GetBattlerInfo(targetIndex);
        if (battlerInfo != null)
        {
            _currentBattler = battlerInfo;
        }
    }

    public List<int> CheckChainBattler()
    {
        List<int> targetIndexes = new ();
        for (int i = 0; i < _battlers.Count;i++)
        {
            var stateInfos = _battlers[i].GetStateInfoAll(StateType.Chain);
            for (int j = stateInfos.Count-1; 0 <= j;j--)
            {
                if (stateInfos[j].BattlerId == CurrentBattler.Index)
                {
                    if (_battlers[i].IsAlive())
                    {
                        targetIndexes.Add(stateInfos[j].TargetIndex);
                    } else
                    {
                        CurrentBattler.RemoveState(stateInfos[j],true);
                    }
                }
            }
        }
        return targetIndexes;
    }

    // 攻撃を受けた対象が付与した拘束ステートを取得
    private List<StateInfo> CheckChainedBattler(int targetIndex)
    {
        var stateInfos = new List<StateInfo>();
        foreach (var battler in _battlers)
        {
            var chainStateInfos = battler.GetStateInfoAll(StateType.Chain);
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
    
    public List<StateInfo> CheckCursedBattler(int targetIndex)
    {
        List<StateInfo> stateInfos = new ();
        for (int i = 0; i < _battlers.Count;i++)
        {
            List<StateInfo> curseStateInfos = _battlers[i].GetStateInfoAll(StateType.Curse);
            for (int j = curseStateInfos.Count-1; 0 <= j;j--)
            {
                if (curseStateInfos[j].BattlerId == targetIndex)
                {
                    stateInfos.Add(curseStateInfos[j]);
                }
            }
        }
        return stateInfos;
    }

    public List<BattlerInfo> BattlerActors(){
        return _battlers.FindAll(a => a.isActor == true);
    }

    public List<BattlerInfo> BattlerEnemies(){
        return _battlers.FindAll(a => a.isActor == false);
    }

    public BattlerInfo GetBattlerInfo(int index)
    {
        return _battlers.Find(a => a.Index == index);
    }

    public List<ListData> SkillActionList()
    {
        var skillInfos = CurrentBattler.Skills.FindAll(a => a.Master.SkillType != SkillType.None && a.Master.Id > 100);
        for (int i = 0; i < skillInfos.Count;i++)
        {
            skillInfos[i].SetEnable(CheckCanUse(skillInfos[i],CurrentBattler));
        }
        var sortList1 = new List<SkillInfo>();
        var sortList2 = new List<SkillInfo>();
        var sortList3 = new List<SkillInfo>();
        skillInfos.Sort((a,b) => {return a.Master.Id > b.Master.Id ? 1 : -1;});
        foreach (var skillInfo in skillInfos)
        {
            if (skillInfo.Master.IconIndex <= MagicIconType.Psionics)
            {
                sortList1.Add(skillInfo);
            } else
            if (skillInfo.Master.IconIndex >= MagicIconType.Demigod)
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
        var list = new List<ListData>();
        var idx = 0;
        foreach (var skillInfo in skillInfos)
        {
            var listData = new ListData(skillInfo,idx);
            list.Add(listData);
        }
        return list;
    }

    public int SelectSkillIndex(List<ListData> skillInfos)
    {
        int selectIndex = 0;
        if (CurrentBattler != null && CurrentBattler.isActor == true)
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
        _turnActionInfos.Clear();
    }

    public bool EnableCurrentBattler()
    {
        if (CurrentBattler.CanMove() == false)
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
                List<int> tempIndexList = GetSkillTargetIndexes(skillInfo.Id,subject.Index,false);
                if (tempIndexList.Contains(substituteId))
                {
                    targetIndexList.Clear();
                    targetIndexList.Add(substituteId);
                }
            }
        }
        int LastTargetIndex = -1;
        if (subject.isActor)
        {
            LastTargetIndex = subject.LastTargetIndex();
            if (skillData.TargetType == TargetType.Opponent)
            {
                BattlerInfo targetBattler = _troop.AliveBattlerInfos.Find(a => a.Index == LastTargetIndex && targetIndexList.Contains(LastTargetIndex));
                if (targetBattler == null && _troop.BattlerInfos.Count > 0)
                {
                    var containsOpponent = _troop.AliveBattlerInfos.Find(a => targetIndexList.Contains(a.Index));
                    if (containsOpponent != null)
                    {
                        LastTargetIndex = containsOpponent.Index;
                    }
                }
            } else
            {
                LastTargetIndex = subject.Index;
                if (targetIndexList.Count > 0)
                {
                    LastTargetIndex = targetIndexList[0];
                }
            }
        }
        var actionInfo = new ActionInfo(_actionIndex,skillInfo.Id,subject.Index,LastTargetIndex,targetIndexList);
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
            _turnActionInfos.Insert(0,actionInfo);
        } else
        {
            _actionInfos.Add(actionInfo);
            _turnActionInfos.Add(actionInfo);
        }
        _battleRecords.Add(new BattleRecord(actionInfo));
        return actionInfo;
    }

    // 選択可能な対象のインデックスを取得
    public List<int> GetSkillTargetIndexes(int skillId,int subjectIndex,bool checkCondition)
    {
        SkillData skillData = DataSystem.Skills.Find(a => a.Id == skillId);
        BattlerInfo subject = GetBattlerInfo(subjectIndex);
        
        RangeType rangeType = skillData.Range;
        if (subject.IsState(StateType.Extension))
        {
            rangeType = RangeType.L;
        }

        List<int> targetIndexList = new List<int>();
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
        } else 
        if (skillData.TargetType == TargetType.Self)
        {
            targetIndexList.Add(subject.Index);
        }

        if (skillData.AliveOnly)
        {
            targetIndexList = targetIndexList.FindAll(a => _battlers.Find(b => a == b.Index).IsAlive());
        } else
        {
            targetIndexList = targetIndexList.FindAll(a => !_battlers.Find(b => a == b.Index).IsAlive());
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
        if (isActor)
        {
            foreach (var battlerInfo in _party.BattlerInfos)
            {
                targetIndexList.Add(battlerInfo.Index);
            }
            if (rangeType == RangeType.S)
            {
                // 前列のみ
                foreach (var battlerInfo in _troop.FrontBattlers())
                {
                    targetIndexList.Add(battlerInfo.Index);
                }
            } else
            {
                foreach (var battlerInfo in _troop.BattlerInfos)
                {
                    targetIndexList.Add(battlerInfo.Index);
                }
            }
        } else
        {
            foreach (var battlerInfo in _battlers)
            {
                targetIndexList.Add(battlerInfo.Index);
            }
        }
        return targetIndexList;
    }

    // 選択範囲が相手
    private List<int> TargetIndexOpponent(bool isActor,List<int> targetIndexList,RangeType rangeType,LineType lineType)
    {   
        if (isActor)
        {
            if (rangeType == RangeType.S)
            {
                // 前列のみ
                foreach (var battlerInfo in _troop.FrontBattlers())
                {
                    targetIndexList.Add(battlerInfo.Index);
                }
            } else
            {
                foreach (var battlerInfo in _troop.BattlerInfos)
                {
                    targetIndexList.Add(battlerInfo.Index);
                }
            }
        } else{
            if (rangeType == RangeType.S)
            {
                if ((lineType == LineType.Back && !_troop.IsFrontAlive()) || lineType == LineType.Front)
                {
                    foreach (var battlerInfo in _party.BattlerInfos)
                    {
                        targetIndexList.Add(battlerInfo.Index);
                    }
                }
            } else
            {
                foreach (var battlerInfo in _party.BattlerInfos)
                {
                    targetIndexList.Add(battlerInfo.Index);
                }
            }
        }
        return targetIndexList;
    }

    private List<int> TargetIndexFriend(bool isActor,List<int> targetIndexList)
    {
        List<BattlerInfo> battlerInfos = isActor ? _party.BattlerInfos : _troop.BattlerInfos;
        foreach (var battlerInfo in battlerInfos)
        {
            targetIndexList.Add(battlerInfo.Index);
        }
        return targetIndexList;
    }

    public bool CanUseCondition(int skillId,int targetIndex)
    {
        bool IsEnable = false;
        SkillData skill = DataSystem.Skills.Find(a => a.Id == skillId);
        BattlerInfo target = GetBattlerInfo(targetIndex);
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
                if (CurrentBattler != null && CurrentBattler.isActor)
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
                if (CurrentBattler != null)
                {
                    if (CurrentBattler.isActor)
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
                    if (CurrentBattler.isActor || (StateType)featureData.Param1 == StateType.DamageUp || (StateType)featureData.Param1 == StateType.Prism)
                    {
                        IsEnable = true;
                    } else
                    if (!CurrentBattler.isActor && !target.IsState((StateType)featureData.Param1) && target.IsState(StateType.Barrier))
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
                case FeatureType.BreakUndead:
                if (CurrentBattler.isActor)
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

    public ActionInfo CurrentActionInfo()
    {
        if (_actionInfos.Count < 1){
            return null;
        }
        return _actionInfos[0];
    }

    public void RemoveActionInfo(int index)
    {
        _actionInfos.RemoveAt(index);
    }

    // indexListにActionを使ったときのリザルトを生成
    public void MakeActionResultInfo(ActionInfo actionInfo,List<int> indexList)
    {
        var subject = GetBattlerInfo(actionInfo.SubjectIndex);
        if (subject.isActor)
        {
            if (actionInfo.Master.TargetType == TargetType.Opponent)
            {
                if (indexList.Count > 0)
                {
                    subject.SetLastTargetIndex(indexList[0]);
                }
            }
        }
        if (actionInfo.Master.TargetType == TargetType.All)
        {
            if (indexList.Count > 0)
            {
                if (indexList[0] > 100)
                {
                    subject.SetLastTargetIndex(indexList[0]);
                }
            }
        }
        int MpCost = actionInfo.Master.MpCost;
        actionInfo.SetMpCost(MpCost);

        var isPrism = false;
        var repeatTime = actionInfo.Master.RepeatTime;
        if (actionInfo.Master.Attribute == AttributeType.Shine && subject.IsState(StateType.Prism))
        {
            var damageFeatures = actionInfo.Master.FeatureDates.FindAll(a => a.FeatureType == FeatureType.HpDamage || a.FeatureType == FeatureType.HpDefineDamage);
            if (damageFeatures.Count > 0)
            {
                repeatTime *= (subject.GetStateInfoAll(StateType.Prism).Count + 1);
                isPrism = true;
            }
        }

        // 攻撃前の攻撃無効回数を管理
        var noDamageDict = new Dictionary<int,int>();
        for (int i = 0; i < indexList.Count;i++)
        {
            noDamageDict[indexList[i]] = GetBattlerInfo(indexList[i]).StateTurn(StateType.NoDamage);
        }
        var actionResultInfos = new List<ActionResultInfo>();

        for (int i = 0; i < repeatTime;i++)
        {
            for (int j = 0; j < indexList.Count;j++)
            {
                var Target = GetBattlerInfo(indexList[j]);
                var featureDates = new List<SkillData.FeatureData>();
                foreach (var featureData in actionInfo.Master.FeatureDates)
                {
                    if (isPrism)
                    {
                        if (featureData.FeatureType == FeatureType.HpDamage || featureData.FeatureType == FeatureType.HpDefineDamage)
                        {
                            var prismCount = (subject.GetStateInfoAll(StateType.Prism).Count + 1);
                            var prismTime = (i+1) % prismCount;
                            if (prismTime == 0)
                            {
                                prismTime = prismCount;
                            }
                            var damageFeature = featureData.Copy();
                            var ratio = 1.0f / (prismTime);
                            damageFeature.Param1 = (int)(damageFeature.Param1 * ratio);
                            featureDates.Add(damageFeature);
                            continue;
                        }
                    }
                    featureDates.Add(featureData);
                }

                var actionResultInfo = new ActionResultInfo(subject,Target,featureDates,actionInfo.Master.Id,actionInfo.Master.Scope == ScopeType.One);
                
                if (actionResultInfo.HpDamage > 0 
                || actionResultInfo.AddedStates.Find(a => a.Master.Id == (int)StateType.Stun) != null
                || actionResultInfo.AddedStates.Find(a => a.Master.Id == (int)StateType.Chain) != null
                || actionResultInfo.AddedStates.Find(a => a.Master.Id == (int)StateType.Death) != null
                || actionResultInfo.DeadIndexList.Contains(actionResultInfo.TargetIndex))            
                {
                    var chainStateInfos = CheckChainedBattler(actionResultInfo.TargetIndex);
                    if (chainStateInfos.Count > 0)
                    {
                        for (int k = 0; k < chainStateInfos.Count;k++)
                        {
                            var chainedBattlerInfo = GetBattlerInfo(chainStateInfos[k].TargetIndex);
                            chainedBattlerInfo.RemoveState(chainStateInfos[k],true);
                            actionResultInfo.AddRemoveState(chainStateInfos[k]);
                        }
                    }
                    if (Target.IsState(StateType.Benediction))
                    {
                        var benedictStateInfos = Target.GetStateInfoAll(StateType.Benediction);
                        for (int k = 0; k < benedictStateInfos.Count;k++)
                        {
                            Target.RemoveState(benedictStateInfos[k],true);
                            Target.ResetAp(false);
                            actionResultInfo.AddRemoveState(benedictStateInfos[k]);
                        }
                    }
                }

                int noDamageCount = Target.StateTurn(StateType.NoDamage);
                int currentRemoveCount = actionResultInfo.RemovedStates.FindAll(a => a.Master.Id == (int)StateType.NoDamage).Count;
                int currentDisplayCount = actionResultInfo.DisplayStates.FindAll(a => a.Master.Id == (int)StateType.NoDamage).Count;
                if ((currentRemoveCount+currentDisplayCount) > 0)
                {
                    var removeCount = actionResultInfos.FindAll(a => a.RemovedStates.Find(b => b.Master.Id == (int)StateType.NoDamage) != null).Count;
                    var displayCount = actionResultInfos.FindAll(a => a.DisplayStates.Find(b => b.Master.Id == (int)StateType.NoDamage) != null).Count;
                    
                    if ((removeCount+displayCount+1) >= noDamageDict[indexList[j]])
                    {
                        var noDamageState = Target.GetStateInfo(StateType.NoDamage);
                        if (noDamageState != null)
                        {
                            Target.RemoveState(noDamageState,true);
                            actionResultInfo.AddRemoveState(noDamageState);
                            var displayedResults = actionResultInfos.FindAll(a => a.DisplayStates.Find(b => b.Master.Id == (int)StateType.NoDamage) != null);

                            foreach (var displayedResult in displayedResults)
                            {
                                for (int k = displayedResult.DisplayStates.Count-1; k >= 0;k--)
                                {
                                    if (displayedResult.DisplayStates[k] == noDamageState)
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

        actionInfo.SetActionResult(actionResultInfos);
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
            var curseStateInfos = CheckCursedBattler(indexList[i]);
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
        //string path = "Assets/Animations/" + animationName + ".asset";
        //var result = await ResourceSystem.LoadAsset<EffekseerEffectAsset>(path);
        string path = "Animations/" + animationName;
        var result = UnityEngine.Resources.Load<EffekseerEffectAsset>(path);
        return result;
    }

    public List<MakerEffectData.SoundTimings> SkillActionSoundTimings(string animationName)
    {
        string makerEffectPath = animationName.Replace("MakerEffect/","");
        string path = "MakerEffectDatas/" + makerEffectPath;
        var result = UnityEngine.Resources.Load<MakerEffectAssetData>(path);
        if (result != null)
        {
            return result.AssetData.soundTimings;
        }
        return null;
    }

    public void ExecCurrentActionResult()
    {
        ActionInfo actionInfo = CurrentActionInfo();
        if (actionInfo != null)
        {
            // Mpの支払い
            CurrentBattler.GainMp(actionInfo.MpCost * -1);
            CurrentBattler.GainPaybattleMp(actionInfo.MpCost);
            // カード破棄
            //CurrentBattler.RemoveDeck(actionInfo.SkillInfo.DeckIndex);
            List<ActionResultInfo> actionResultInfos = CalcDeathIndexList(actionInfo.ActionResults);
            for (int i = 0; i < actionResultInfos.Count; i++)
            {
                ExecActionResultInfo(actionResultInfos[i]);
            }
        }
    }

    // 複数のActionResultのreDamageとreHealを1つにまとめる
    public void AdjustReactionActionResultInfo(List<ActionResultInfo> actionResultInfos)
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
    }

    public void ExecActionResultInfo(ActionResultInfo actionResultInfo)
    {
        BattlerInfo subject = GetBattlerInfo(actionResultInfo.SubjectIndex);
        BattlerInfo target = GetBattlerInfo(actionResultInfo.TargetIndex);
        foreach (var addState in actionResultInfo.AddedStates)
        {
            BattlerInfo addTarget = GetBattlerInfo(addState.TargetIndex);
            addTarget.AddState(addState,true);
        }
        foreach (var removeState in actionResultInfo.RemovedStates)
        {
            BattlerInfo removeTarget = GetBattlerInfo(removeState.TargetIndex);
            removeTarget.RemoveState(removeState,true);
        }
        if (actionResultInfo.HpDamage != 0)
        {
            target.GainHp(-1 * actionResultInfo.HpDamage);
        }
        if (actionResultInfo.HpHeal != 0)
        {
            target.GainHp(actionResultInfo.HpHeal);
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
        if (actionResultInfo.ReHeal != 0)
        {
            subject.GainHp(actionResultInfo.ReHeal);
        }
        if (actionResultInfo.ReDamage != 0)
        {
            subject.GainHp(-1 * actionResultInfo.ReDamage);
        }
        foreach (var targetIndex in actionResultInfo.ExecStateInfos)
        {
            BattlerInfo execTarget = GetBattlerInfo(targetIndex.Key);
            if (execTarget != null)
            {
                foreach (var stateId in targetIndex.Value)
                {
                    execTarget.UpdateStateCount(RemovalTiming.UpdateCount,(int)stateId);
                }
            }
        }
        //if (actionResultInfo.AddedStates.Find(a => a.Master.Id == (int)StateType.Stun) != null
        //    || actionResultInfo.DeadIndexList.Contains(actionResultInfo.TargetIndex))  
        if (actionResultInfo.HpDamage > 0 || actionResultInfo.ExecStateInfos[target.Index].Contains(StateType.CounterAura))
        {
            if (target.IsState(StateType.CounterAura))
            {
                List<StateInfo> counterOuraStateInfos = target.GetStateInfoAll(StateType.CounterAura);
                for (int j = 0; j < counterOuraStateInfos.Count;j++)
                {
                    target.RemoveState(counterOuraStateInfos[j],true);
                    actionResultInfo.AddRemoveState(counterOuraStateInfos[j]);
                }
            }
        }
        _battleRecords.Add(new BattleRecord(actionResultInfo));
    }
    
    public List<int> DeathBattlerIndex(List<ActionResultInfo> actionResultInfos)
    {
        List<int> deathBattlerIndex = new ();
        for (int i = 0; i < actionResultInfos.Count; i++)
        {
            for (int j = 0; j < actionResultInfos[i].DeadIndexList.Count; j++)
            {
                // 例外
                if (!GetBattlerInfo(actionResultInfos[i].DeadIndexList[j]).IsState(StateType.Death))
                {

                } else
                {
                    deathBattlerIndex.Add(actionResultInfos[i].DeadIndexList[j]);
                }
            }
        }
        return deathBattlerIndex;
    }

    public List<int> AliveBattlerIndex(List<ActionResultInfo> actionResultInfos)
    {
        List<int> aliveBattlerIndex = new ();
        for (int i = 0; i < actionResultInfos.Count; i++)
        {
            for (int j = 0; j < actionResultInfos[i].AliveIndexList.Count; j++)
            {
                aliveBattlerIndex.Add(actionResultInfos[i].AliveIndexList[j]);
            }
        }
        return aliveBattlerIndex;
    }

    public List<StateInfo> UpdateTurn()
    {
        var result = CurrentBattler.UpdateState(RemovalTiming.UpdateTurn);
        CurrentBattler.TurnEnd();
        return result;
    }

    public void TurnEnd()
    {
        var actionInfo = CurrentActionInfo();
        if (actionInfo.TriggeredSkill == false)
        {    
            var noResetAp = actionInfo.Master.FeatureDates.Find(a => a.FeatureType == FeatureType.NoResetAp);
            if (noResetAp == null)
            {
                _currentBattler.ResetAp(false);
            }
            var afterAp = actionInfo.Master.FeatureDates.Find(a => a.FeatureType == FeatureType.SetAfterAp);
            if (afterAp != null)
            {
                _currentBattler.SetAp(afterAp.Param1);
            }
        }
        _actionInfos.RemoveAt(0);
        _currentBattler = null;
    }

    public void CheckPlusSkill()
    {
        var actionInfo = CurrentActionInfo();
        if (actionInfo != null)
        {
            var actionInfos = actionInfo.CheckPlusSkill();
            if (GetBattlerInfo(actionInfo.SubjectIndex).IsState(StateType.Extension))
            {
                foreach (var item in actionInfos)
                {
                    item.SetRangeType(RangeType.L); 
                }
            }
            _actionInfos.AddRange(actionInfos);
        }
    }

    public List<ActionResultInfo> CheckRegene()
    {
        var results = RegeneActionResults();
        results.AddRange(AfterHealActionResults());
        if (CurrentActionInfo() != null && CurrentActionInfo().ActionResults.Find(a => a.HpDamage > 0) != null)
        {
            results.AddRange(AssistHealActionResults());
        }
        return results;
    }

    private List<ActionResultInfo> RegeneActionResults()
    {
        var regeneResults = MakeStateActionResult(CurrentBattler,StateType.Regene,FeatureType.HpHeal);
        return regeneResults;
    }

    private List<ActionResultInfo> AfterHealActionResults()
    {
        var afterHealResults = new List<ActionResultInfo>();
        var afterSkillInfo = CurrentBattler.Skills.Find(a => a.Master.FeatureDates.Find(b => b.FeatureType == FeatureType.AddState && (StateType)b.Param1 == StateType.AfterHeal) != null);
        if (CurrentBattler.IsState(StateType.AfterHeal) && afterSkillInfo != null)
        {
            var stateInfo = CurrentBattler.GetStateInfo(StateType.AfterHeal);
            var skillInfo = new SkillInfo(afterSkillInfo.Id);
            var actionInfo = MakeActionInfo(CurrentBattler,skillInfo,false,false);
            
            if (actionInfo != null)
            {
                _actionInfos.Remove(actionInfo);
                var party = CurrentBattler.isActor ? BattlerActors() : BattlerEnemies();
                var targetIndexes = new List<int>();
                foreach (var member in party)
                {
                    if (CurrentBattler.Index != member.Index)
                    {
                        targetIndexes.Add(member.Index);
                    }
                    
                }
                foreach (var targetIndex in targetIndexes)
                {
                    SkillData.FeatureData featureData = new SkillData.FeatureData();
                    featureData.FeatureType = FeatureType.HpHeal;
                    featureData.Param1 = stateInfo.Effect;

                    ActionResultInfo actionResultInfo = new ActionResultInfo(GetBattlerInfo(targetIndex),GetBattlerInfo(targetIndex),new List<SkillData.FeatureData>(){featureData},-1);
                    afterHealResults.Add(actionResultInfo);
                }
            }
        }
        return afterHealResults;
    }

    private List<ActionResultInfo> AssistHealActionResults()
    {
        var assistHealResults = new List<ActionResultInfo>();
        var afterSkillInfo = CurrentBattler.Skills.Find(a => a.Master.FeatureDates.Find(b => b.FeatureType == FeatureType.AddState && (StateType)b.Param1 == StateType.AssistHeal) != null);
        if (CurrentBattler.IsState(StateType.AssistHeal) && afterSkillInfo != null)
        {
            var stateInfo = CurrentBattler.GetStateInfo(StateType.AssistHeal);
            var skillInfo = new SkillInfo(afterSkillInfo.Id);
            var actionInfo = MakeActionInfo(CurrentBattler,skillInfo,false,false);
            
            if (actionInfo != null)
            {
                _actionInfos.Remove(actionInfo);
                var party = CurrentBattler.isActor ? BattlerActors() : BattlerEnemies();
                party = party.FindAll(a => a.IsAlive());
                var targetIndexes = new List<int>();
                foreach (var member in party)
                {
                    //if (CurrentBattler.Index != member.Index)
                    //{
                        targetIndexes.Add(member.Index);
                    //}
                    
                }
                var healValue = CurrentActionInfo().ActionResults.FindAll(a => a.HpDamage > 0).Count;
                foreach (var targetIndex in targetIndexes)
                {
                    SkillData.FeatureData featureData = new SkillData.FeatureData();
                    featureData.FeatureType = FeatureType.HpHeal;
                    featureData.Param1 = healValue * stateInfo.Effect;

                    ActionResultInfo actionResultInfo = new ActionResultInfo(GetBattlerInfo(targetIndex),GetBattlerInfo(targetIndex),new List<SkillData.FeatureData>(){featureData},-1);
                    assistHealResults.Add(actionResultInfo);
                }
            }
        }
        return assistHealResults;
    }

    public List<ActionResultInfo> CheckSlipDamage()
    {
        var results = MakeStateActionResult(CurrentBattler,StateType.SlipDamage,FeatureType.HpDefineDamage);
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
            List<ActionResultInfo> damageResults = results.FindAll(a => a.HpDamage > 0 && a.TargetIndex == targetIndex);
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

/*
    public List<ActionResultInfo> UpdateSlipDamageState()
    {
        return MakeStateActionResult(CurrentBattler,StateType.SlipDamage,FeatureType.HpDefineDamage);
    }
    */

    public List<ActionResultInfo> MakeStateActionResult(BattlerInfo battlerInfo,StateType stateType,FeatureType featureType)
    {
        List<ActionResultInfo> actionResultInfos = new ();
        List<StateInfo> stateInfos = battlerInfo.GetStateInfoAll(stateType);
        
        SkillData.FeatureData featureData = new SkillData.FeatureData();
        featureData.FeatureType = featureType;

        for (int i = 0;i < stateInfos.Count;i++)
        {
            featureData.Param1 = stateInfos[i].Effect;
            BattlerInfo target = GetBattlerInfo(stateInfos[i].BattlerId);
            if (target.IsAlive())
            {
                ActionResultInfo actionResultInfo = new ActionResultInfo(GetBattlerInfo(stateInfos[i].BattlerId),GetBattlerInfo(stateInfos[i].TargetIndex),new List<SkillData.FeatureData>(){featureData},-1);
                actionResultInfos.Add(actionResultInfo);
            }
        }
        return actionResultInfos;
    }

    public void GainHpTargetIndex(int index,int value)
    {
        GetBattlerInfo(index).GainHp(value);
    }

    // リザルトから発生するトリガースキルを生成
    public List<ActionInfo> CheckTriggerSkillInfos(TriggerTiming triggerTiming,ActionInfo actionInfo,List<ActionResultInfo> actionResultInfos)
    {
        var madeActionInfos = new List<ActionInfo>();
        /*
        if (CurrentBattler == null)
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
                    if (IsTriggeredSkillInfo(checkBattler,triggerDates,triggerTiming,actionInfo,actionResultInfos))
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
                            ActionInfo makeActionInfo = MakeActionInfo(checkBattler,triggeredSkills[j],triggerTiming == TriggerTiming.Interrupt,true);
                            madeActionInfos.Add(makeActionInfo);
                        }
                    } else{
                        ActionInfo makeActionInfo = MakeActionInfo(checkBattler,triggeredSkills[j],triggerTiming == TriggerTiming.Interrupt,true);
                        madeActionInfos.Add(makeActionInfo);
                    }
                }
            }
        }

        return madeActionInfos;
    }

    public List<ActionResultInfo> CheckTriggerPassiveInfos(TriggerTiming triggerTiming)
    {
        List<ActionResultInfo> actionResultInfos = new List<ActionResultInfo>();
        for (int i = 0;i < _battlers.Count;i++)
        {
            BattlerInfo battlerInfo = _battlers[i];
            List<SkillInfo> passiveSkills = battlerInfo.PassiveSkills();
            for (int j = 0;j < passiveSkills.Count;j++)
            {
                if (battlerInfo.IsState(StateType.NoPassive))
                {
                    continue;
                }
                SkillInfo passiveInfo = passiveSkills[j];
                var triggerDates = passiveInfo.Master.TriggerDates.FindAll(a => a.TriggerTiming == triggerTiming);
                if (IsTriggeredSkillInfo(battlerInfo,triggerDates,triggerTiming,null,new List<ActionResultInfo>()))
                {                
                    bool usable = true;
                    // トリガーのParam2を使用回数制限にする
                    if (triggerDates.Find(a => a.Param2 == 1) != null)
                    {
                        if (_usedPassiveSkillInfos[battlerInfo.Index].Find(a => a.Master.Id == passiveInfo.Master.Id) == null)
                        {
                            _usedPassiveSkillInfos[battlerInfo.Index].Add(passiveInfo);
                        } else
                        {
                            usable = false;
                        }
                    }
                    
                    if (usable)
                    {
                        if (passiveInfo.Master.Scope == ScopeType.Self)
                        {
                            ActionResultInfo actionResultInfo = new ActionResultInfo(battlerInfo,battlerInfo,passiveInfo.Master.FeatureDates,passiveInfo.Id);
                            actionResultInfos.Add(actionResultInfo);
                        } else
                        if (passiveInfo.Master.Scope == ScopeType.All)
                        {
                            var partyMember = battlerInfo.isActor ? BattlerActors() : BattlerEnemies();
                            foreach (var member in partyMember)
                            {
                                if ((passiveInfo.Master.AliveOnly && member.IsAlive()) || (!passiveInfo.Master.AliveOnly && !member.IsAlive()) )
                                {
                                    ActionResultInfo actionResultInfo = new ActionResultInfo(battlerInfo,member,passiveInfo.Master.FeatureDates,passiveInfo.Id);
                                    actionResultInfos.Add(actionResultInfo);
                                }
                            }
                        } else
                        {
                            if (passiveInfo.Master.TargetType == TargetType.AttackTarget)
                            {
                                if (CurrentActionInfo() != null && CurrentActionInfo().ActionResults.Count > 0)
                                {
                                    ActionResultInfo actionResultInfo = new ActionResultInfo(battlerInfo,GetBattlerInfo(CurrentActionInfo().ActionResults[0].TargetIndex),passiveInfo.Master.FeatureDates,passiveInfo.Id);
                                    actionResultInfos.Add(actionResultInfo);
                                }
                            }
                        }
                        if (_passiveSkillInfos[battlerInfo.Index].Find(a => a.Master.Id == passiveInfo.Master.Id) == null)
                        {
                            _passiveSkillInfos[battlerInfo.Index].Add(passiveInfo);
                        }
                    }
                }
            }
        }
        return actionResultInfos;
    }
    
    public List<ActionResultInfo> CheckRemovePassiveInfos()
    {
        List<ActionResultInfo> actionResultInfos = new List<ActionResultInfo>();
        for (int i = 0;i < _battlers.Count;i++)
        {
            BattlerInfo battlerInfo = _battlers[i];
            List<SkillInfo> passiveSkills = _passiveSkillInfos[battlerInfo.Index];
            for (int j = 0;j < passiveSkills.Count;j++)
            {
                SkillInfo passiveInfo = passiveSkills[j];
                bool IsRemove = false;
                
                foreach (var feature in passiveInfo.Master.FeatureDates)
                {
                    if (feature.FeatureType == FeatureType.AddState)
                    {
                        var triggerDates = passiveInfo.Master.TriggerDates.FindAll(a => a.TriggerTiming == TriggerTiming.After || a.TriggerTiming == TriggerTiming.StartBattle);
                        if (IsRemove == false && !IsTriggeredSkillInfo(battlerInfo,triggerDates,TriggerTiming.After,null,new List<ActionResultInfo>()))
                        {
                            IsRemove = true;
                            
                            SkillData.FeatureData featureData = new SkillData.FeatureData();
                            featureData.FeatureType = FeatureType.RemoveStatePassive;
                            featureData.Param1 = feature.Param1;
                            if (passiveInfo.Master.Scope == ScopeType.Self)
                            {
                                ActionResultInfo actionResultInfo = new ActionResultInfo(battlerInfo,battlerInfo,new List<SkillData.FeatureData>(){featureData},passiveInfo.Id);
                                if (actionResultInfos.Find(a => a.RemovedStates.Find(b => b.Master.Id == (int)featureData.FeatureType) != null) != null)
                                {
                                    
                                } else{
                                    var stateInfos = battlerInfo.GetStateInfoAll((StateType)feature.Param1);
                                    if (battlerInfo.IsAlive() && stateInfos.Find(a => a.SkillId == passiveInfo.Id) != null)
                                    {
                                        actionResultInfos.Add(actionResultInfo);
                                    }
                                }
                            } else
                            if (passiveInfo.Master.Scope == ScopeType.All)
                            {
                                var partyMember = battlerInfo.isActor ? BattlerActors() : BattlerEnemies();
                                foreach (var member in partyMember)
                                {
                                    if ((passiveInfo.Master.AliveOnly && member.IsAlive()) || (!passiveInfo.Master.AliveOnly && !member.IsAlive()))
                                    {

                                        ActionResultInfo actionResultInfo = new ActionResultInfo(battlerInfo,member,new List<SkillData.FeatureData>(){featureData},passiveInfo.Id);
                                        if (actionResultInfos.Find(a => a.RemovedStates.Find(b => b.Master.Id == (int)featureData.FeatureType) != null) != null)
                                        {
                                            
                                        } else{
                                            var stateInfos = battlerInfo.GetStateInfoAll((StateType)feature.Param1);
                                            if (member.IsAlive() && stateInfos.Find(a => a.SkillId == passiveInfo.Id) != null)
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

    private bool IsTriggeredSkillInfo(BattlerInfo battlerInfo,List<SkillData.TriggerData> triggerDates,TriggerTiming triggerTiming,ActionInfo actionInfo,List<ActionResultInfo> actionResultInfos)
    {
        bool IsTriggered = false;
        if (triggerDates.Count > 0)
        {
            for (var j = 0;j < triggerDates.Count;j++)
            {
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
                if (triggerDates[j].TriggerType == TriggerType.PayBattleMp)
                {
                    if (battlerInfo.IsAlive() && battlerInfo.PayBattleMp >= triggerDates[j].Param1)
                    {
                        IsTriggered = true;
                    }
                }
                if (battlerInfo.IsAwaken == false && triggerTiming == TriggerTiming.Interrupt && triggerDates[j].TriggerType == TriggerType.ActionResultDeath)
                {
                    if (battlerInfo.IsAlive())
                    {
                        if (battlerInfo.isActor)
                        {
                            if (actionResultInfos.Find(a => BattlerActors().Find(b => a.DeadIndexList.Contains(b.Index)) != null) != null)
                            {
                                IsTriggered = true;
                            }
                        } else
                        {
                            if (actionResultInfos.Find(a => BattlerEnemies().Find(b => a.DeadIndexList.Contains(b.Index)) != null) != null)
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
                        List<StateInfo> stateInfos = battlerInfo.GetStateInfoAll(StateType.Death);
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
                    var opponents = battlerInfo.isActor ? BattlerEnemies() : BattlerActors();
                    if (battlerInfo.IsAlive() && opponents.Find(a => a.IsAlive() && !a.IsState(StateType.Curse)) == null && opponents.FindAll(a => a.IsAlive()).Count > 0)
                    {
                        IsTriggered = true;
                    }
                }
                if (battlerInfo.IsAwaken == false && triggerTiming == TriggerTiming.Interrupt && triggerDates[j].TriggerType == TriggerType.ActionResultAddState)
                {
                    if (battlerInfo.IsAlive())
                    {
                        if (actionInfo != null && battlerInfo.isActor != GetBattlerInfo(actionInfo.SubjectIndex).isActor)
                        {
                            var states = actionInfo.Master.FeatureDates.FindAll(a => a.FeatureType == FeatureType.AddState);
                            foreach (var state in states)
                            {
                                if (state.Param1 == (int)StateType.Stun || state.Param1 == (int)StateType.Slow || state.Param1 == (int)StateType.Curse || state.Param1 == (int)StateType.SlipDamage || state.Param1 == (int)StateType.Blind || state.Param1 == (int)StateType.Freeze)
                                {
                                    IsTriggered = true;
                                }
                            }
                        }
                    }
                }
                if (battlerInfo.IsAwaken == false && triggerTiming == TriggerTiming.After && triggerDates[j].TriggerType == TriggerType.DefeatEnemyByAttack)
                {
                    var attackBattler = GetBattlerInfo(actionInfo.SubjectIndex);
                    if (battlerInfo.IsAlive() && attackBattler != null && battlerInfo.Index == attackBattler.Index)
                    {
                        var opponents = battlerInfo.isActor ? BattlerEnemies() : BattlerActors();
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

    private bool CanUseTrigger(SkillInfo skillInfo,BattlerInfo battlerInfo)
    {
        bool CanUse = true;
        if (skillInfo.TriggerDates.Count > 0)
        {
            CanUse = false;
            CanUse = IsTriggeredSkillInfo(battlerInfo,skillInfo.TriggerDates,TriggerTiming.None,null,new List<ActionResultInfo>());
        }
        return CanUse;
    }

    public List<int> MakeAutoSelectIndex(ActionInfo actionInfo,int oneTargetIndex = -1)
    {
        List<int> indexList = new List<int>();
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
        List<int> targetIndexList = GetSkillTargetIndexes(actionInfo.Master.Id,actionInfo.SubjectIndex,true);
        if (targetIndexList.Count == 0)
        {
            return targetIndexList;
        }
        
        int targetRand = 0;
        for (int i = 0;i < targetIndexList.Count;i++)
        {
            BattlerInfo battlerInfo = GetBattlerInfo(targetIndexList[i]);
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
            BattlerInfo battlerInfo = GetBattlerInfo(targetIndexList[i]);
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
        if (CurrentBattler.IsState(StateType.Substitute))
        {
            int substituteId = CurrentBattler.GetStateInfo(StateType.Substitute).BattlerId;
            if (targetIndexList.Contains(substituteId))
            {
                targetIndex = substituteId;
            } else
            {
                List<int> tempIndexList = GetSkillTargetIndexes(actionInfo.Master.Id,actionInfo.SubjectIndex,false);
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
            
        if (actionInfo.Master.Scope == ScopeType.All)
        {
            indexList = targetIndexList;
        }
        if (actionInfo.Master.Scope == ScopeType.Self)
        {
            indexList = targetIndexList;
        }
        if (actionInfo.Master.Scope == ScopeType.One)
        {
            if (!CurrentBattler.IsState(StateType.Substitute) && oneTargetIndex > -1)
            {
                indexList.Add (oneTargetIndex);
            } else
            {
                indexList.Add (targetIndex);
            }
        }
        if (actionInfo.Master.Scope == ScopeType.WithoutSelfOne)
        {
            indexList.Add (targetIndex);
        }
        if (actionInfo.Master.Scope == ScopeType.WithoutSelfAll)
        {
            indexList.Clear();
            for (int i = 0;i < targetIndexList.Count;i++)
            {
                BattlerInfo battlerInfo = GetBattlerInfo(targetIndexList[i]);
                if (battlerInfo.Index == actionInfo.SubjectIndex)
                {
                    continue;
                }
                indexList.Add(targetIndexList[i]);
            }
        }
        if (actionInfo.Master.Scope == ScopeType.Line)
        {
            indexList.Add (targetIndex);
            BattlerInfo battlerInfo = GetBattlerInfo(targetIndex);
            for (int i = 0;i < _battlers.Count;i++)
            {
                if (battlerInfo.isActor && _battlers[i].isActor)
                {
                    if (battlerInfo.LineIndex == _battlers[i].LineIndex)
                    {
                        if (indexList.IndexOf(_battlers[i].Index) == -1)
                        {                        
                            indexList.Add (_battlers[i].Index);
                        }
                    }
                }
                if (!battlerInfo.isActor && !_battlers[i].isActor)
                {
                    if (battlerInfo.LineIndex == _battlers[i].LineIndex)
                    {
                        if (indexList.IndexOf(_battlers[i].Index) == -1)
                        {                        
                            indexList.Add (_battlers[i].Index);
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
        List<SkillInfo> skillInfos = battlerInfo.ActiveSkills().FindAll(a => CheckCanUse(a,battlerInfo));
        if (skillInfos.Count == 0)
        {
            return 0;
        }
        int weight = 0;
        for (int i = 0;i < skillInfos.Count;i++)
        {
            weight += skillInfos[i].Weight;
        }
        weight = UnityEngine.Random.Range (0,weight);
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
        List<SkillInfo> skillInfos = battlerInfo.ActiveSkills().FindAll(a => CheckCanUse(a,battlerInfo));
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
        foreach (var battlerInfo in _battlers)
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
        foreach (var battlerInfo in _battlers)
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

    private List<StateType> RemoveDeathStateTypes()
    {
        return new List<StateType>(){
            StateType.Chain,
            StateType.Benediction,
            StateType.SlipDamage,
            StateType.Regene
        };
    }

    // 生存していない付与者の該当ステートを解除する
    public List<StateInfo> EndRemoveState()
    {
        List<StateInfo> removeStateInfos = new ();
        var StateTypes = RemoveDeathStateTypes();
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
        return removeStateInfos;
    }

    public void GainAttackCount(int targetIndex)
    {
        GetBattlerInfo(targetIndex).GainAttackedCount(1);
    }


    public bool CheckVictory()
    {
        bool isVictory = BattlerEnemies().Find(a => a.IsAlive()) == null;
        if (isVictory)
        {
            PartyInfo.SetBattleResult(true);
        }
        return isVictory;
    }

    public bool CheckDefeat()
    {
        bool isDefeat = BattlerActors().Find(a => a.IsAlive()) == null;
        if (isDefeat)
        {
            PartyInfo.SetBattleResult(false);
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
        PartyInfo.SetBattleResult(false);
    }

    public void EndBattle()
    {
        foreach (var battler in BattlerActors())
        {
            ActorInfo actorInfo = Actors().Find(a => a.ActorId == battler.CharaId);
            actorInfo.ChangeHp(battler.Hp);
            actorInfo.ChangeMp(battler.Mp);
        }
        SaveSystem.SaveConfigStart(GameSystem.ConfigData);
    }

    public List<SystemData.CommandData> SideMenu()
    {
        var list = new List<SystemData.CommandData>();
        var menucommand = new SystemData.CommandData();
        menucommand.Id = 2;
        menucommand.Name = DataSystem.System.GetTextData(703).Text;
        menucommand.Key = "Help";
        list.Add(menucommand);
        return list;
    }

    public SystemData.CommandData BattleAutoButton()
    {
        var menucommand = new SystemData.CommandData();
        menucommand.Id = 1;
        menucommand.Name = DataSystem.System.GetTextData(706).Text;
        menucommand.Key = "BATTLE_AUTO";
        return menucommand;
    }

    public void ChangeBattleAuto()
    {
        GameSystem.ConfigData._battleAuto = !GameSystem.ConfigData._battleAuto;
    }

    public List<ListData> SelectCharacterConditions()
    {
        var list = new List<ListData>();
        foreach (var stateInfo in CurrentBattler.StateInfos)
        {
            var listData = new ListData(stateInfo);
        }
        return list;
    }
}