using System;
using System.Collections.Generic;
using Effekseer;
using Cysharp.Threading.Tasks;

public class BattleModel : BaseModel
{
    public BattleModel()
    {
    }

    private List<BattlerInfo> _battlers = new List<BattlerInfo>();
    public List<BattlerInfo> Battlers => _battlers;
    
    private int _currentIndex = 0; 
    public int CurrentIndex => _currentIndex;

    private List<BattleRecord> _battleRecords = new ();
    public List<BattleRecord> BattleRecords => _battleRecords;

    private AttributeType _currentAttributeType = AttributeType.Fire;    
    public AttributeType CurrentAttributeType => _currentAttributeType;

    public ActorInfo CurrentActor
    {
        get {return PartyMembers()[_currentIndex];}
    }

    private BattlerInfo _currentBattler = null;
    public BattlerInfo CurrentBattler => _currentBattler;

    public ActorsData.ActorData CurrentBattlerActorData()
    {
        return DataSystem.Actors.Find(a => a.Id == _currentBattler.CharaId);
    }

    private List<ActionInfo> _actionInfos = new ();
    private Dictionary<int,List<SkillInfo>> _passiveSkillInfos = new Dictionary<int,List<SkillInfo>>();
    private Dictionary<int,List<SkillInfo>> _usedPassiveSkillInfos = new Dictionary<int,List<SkillInfo>>();

    public UniTask<List<UnityEngine.AudioClip>> GetBattleBgm()
    {
        if (CurrentStage != null)
        {
            var troops = CurrentStage.CurrentTroopInfo();
            if (troops.TroopId >= 100 && troops.TroopId <= 500 && troops.TroopId % 100 == 0)
            {
                return GetBgmData("BOSS1");
            }
            if (troops.TroopId >= 1100 && troops.TroopId <= 1500)
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
    }


    public bool UpdateAp()
    {
        var isRemoveState = false;
        _battlers.ForEach(battler => {
            battler.UpdateAp();
            var removeStates = battler.UpdateState(RemovalTiming.UpdateAp);
            if (removeStates != null)
            {
                isRemoveState = true;
            }
        });
        MakeActionBattler();
        return isRemoveState;
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
                    SkillsData.FeatureData featureData = new SkillsData.FeatureData();
                    featureData.FeatureType = FeatureType.HpDefineDamage;
                    featureData.Param1 = chainDamage;
                    
                    ActionResultInfo actionResultInfo = new ActionResultInfo(subject,target,new List<SkillsData.FeatureData>(){featureData},-1);
                        
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
                        SkillsData.FeatureData featureData = new SkillsData.FeatureData();
                        featureData.FeatureType = FeatureType.HpHeal;
                        featureData.Param1 = stateInfo.Effect;
                        foreach (var target in targets)
                        {
                            ActionResultInfo actionResultInfo = new ActionResultInfo(_battlers[i],target,new List<SkillsData.FeatureData>(){featureData},-1);
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
                if (battlerInfo.LastSelectSkillId > 0)
                {
                    _currentAttributeType = battlerInfo.GetSkillAttribute();
                }
                break;
            }
        }
    }

    public void SetActionBattler(int targetIndex)
    {
        BattlerInfo battlerInfo = GetBattlerInfo(targetIndex);
        if (battlerInfo != null)
        {
            _currentBattler = battlerInfo;
        }
    }

    public List<int> CheckChainBattler()
    {
        List<int> targetIndexs = new ();
        for (int i = 0; i < _battlers.Count;i++)
        {
            List<StateInfo> stateInfos = _battlers[i].GetStateInfoAll(StateType.Chain);
            for (int j = stateInfos.Count-1; 0 <= j;j--)
            {
                if (stateInfos[j].BattlerId == CurrentBattler.Index)
                {
                    if (_battlers[i].IsAlive())
                    {
                        targetIndexs.Add(stateInfos[j].TargetIndex);
                    } else
                    {
                        CurrentBattler.RemoveState(stateInfos[j],true);
                    }
                }
            }
        }
        return targetIndexs;
    }

    private List<StateInfo> CheckChainedBattler(int targetIndex)
    {
        List<StateInfo> stateInfos = new ();
        for (int i = 0; i < _battlers.Count;i++)
        {
            List<StateInfo> chainStateInfos = _battlers[i].GetStateInfoAll(StateType.Chain);
            for (int j = chainStateInfos.Count-1; 0 <= j;j--)
            {
                if (chainStateInfos[j].BattlerId == targetIndex)
                {
                    stateInfos.Add(chainStateInfos[j]);
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

    public void ChangeActorIndex(int value){
        _currentIndex += value;
        if (_currentIndex > PartyMembers().Count-1){
            _currentIndex = 0;
        } else
        if (_currentIndex < 0){
            _currentIndex = PartyMembers().Count-1;
        }
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

    public List<SkillInfo> SkillActionList(AttributeType attributeType)
    {
        _currentAttributeType = attributeType;
        List<SkillInfo> skillInfos = CurrentBattler.Skills.FindAll(a => a.Master.SkillType != SkillType.None && a.Master.Id > 100);
        if (attributeType != AttributeType.None)
        {
            skillInfos = skillInfos.FindAll(a => a.Attribute == _currentAttributeType);
        }
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
        return skillInfos;
    }

    public int SelectSkillIndex(List<SkillInfo> skillInfos)
    {
        int selectIndex = 0;
        if (CurrentBattler != null && CurrentBattler.isActor == true)
        {
            var skillIndex = skillInfos.FindIndex(a => a.Id == CurrentBattler.LastSelectSkillId);
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
        List<int> targetIndexList = MakeActionTarget(skillInfo.Master.Id,battlerInfo.Index,true);
        if (targetIndexList.Count == 0)
        {
            return false;
        }
        return true;
    }

    public void ClearActionInfo()
    {
        _actionInfos.Clear();
    }

    public bool EnableCurrentBattler()
    {
        if (CurrentBattler.CanMove() == false)
        {
            return false;
        }
        return true;
    }

    public ActionInfo MakeActionInfo(BattlerInfo subject, int skillId,bool IsInterrupt,bool IsTriggerd)
    {
        SkillsData.SkillData skill = DataSystem.Skills.Find(a => a.Id == skillId);
        List<int> targetIndexList = MakeActionTarget(skillId,subject.Index,true);
        if (subject.IsState(StateType.Substitute))
        {
            int substituteId = subject.GetStateInfo(StateType.Substitute).BattlerId;
            if (targetIndexList.Contains(substituteId))
            {
                targetIndexList.Clear();
                targetIndexList.Add(substituteId);
            } else{
                List<int> tempIndexList = MakeActionTarget(skillId,subject.Index,false);
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
            if (skill.TargetType == TargetType.Opponent)
            {
                BattlerInfo targetBattler = BattlerEnemies().Find(a => a.Index == LastTargetIndex && a.IsAlive() && targetIndexList.Contains(LastTargetIndex));
                if (targetBattler == null || targetBattler.IsAlive() == false)
                {
                    if (BattlerEnemies().Count > 0 && BattlerEnemies().Find(a => a.IsAlive() && targetIndexList.Contains(a.Index)) != null)
                    {
                        LastTargetIndex = BattlerEnemies().Find(a => a.IsAlive() && targetIndexList.Contains(a.Index)).Index;
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
        ActionInfo actionInfo = new ActionInfo(skillId,subject.Index,LastTargetIndex,targetIndexList);
        if (subject.IsState(StateType.Extension))
        {
            actionInfo.SetRangeType(RangeType.L);
        }
        if (IsTriggerd)
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
        _battleRecords.Add(new BattleRecord( actionInfo ));
        return actionInfo;
    }

    public List<int> MakeActionTarget(int skillId,int subjectIndex,bool checkCondition)
    {
        SkillsData.SkillData skill = DataSystem.Skills.Find(a => a.Id == skillId);
        BattlerInfo subject = GetBattlerInfo(subjectIndex);
        
        RangeType rangeType = skill.Range;
        if (subject.IsState(StateType.Extension))
        {
            rangeType = RangeType.L;
        }

        List<int> targetIndexList = new List<int>();
        if (skill.TargetType == TargetType.All)
        {
            targetIndexList = TargetIndexAll(subject.isActor,targetIndexList,rangeType);
        } else
        if (skill.TargetType == TargetType.Opponent)
        {
            targetIndexList = TargetIndexOpponent(subject.isActor,targetIndexList,rangeType,subject.LineIndex);
        } else
        if (skill.TargetType == TargetType.Friend)
        {
            targetIndexList = TargetIndexFriend(subject.isActor,targetIndexList);
        } else 
        if (skill.TargetType == TargetType.Self)
        {
            targetIndexList.Add(subject.Index);
        }

        if (skill.AliveOnly)
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

    private List<int> TargetIndexAll(bool isActor,List<int> targetIndexList,RangeType rangeType)
    {
        if (isActor)
        {
            List<BattlerInfo> battlerInfos = BattlerActors();
            for (int i = 0;i < battlerInfos.Count;i++)
            {
                targetIndexList.Add(battlerInfos[i].Index);
            }
            if (rangeType == RangeType.S)
            {
                // 最前列は
                bool IsFrontAlive = BattlerEnemies().Find(a => a.IsAlive() && a.LineIndex == LineType.Front) != null;
                if (IsFrontAlive)
                {
                    List<BattlerInfo> frontBattlerInfos = BattlerEnemies().FindAll(a => a.IsAlive() && a.LineIndex == LineType.Front);
                    for (int i = 0;i < frontBattlerInfos.Count;i++)
                    {
                        if (frontBattlerInfos[i].IsAlive())
                        {
                            targetIndexList.Add(frontBattlerInfos[i].Index);
                        }
                    }
                } else
                {
                    List<BattlerInfo> backBattlerInfos = BattlerEnemies().FindAll(a => a.IsAlive() && a.LineIndex == LineType.Back);
                    for (int i = 0;i < backBattlerInfos.Count;i++)
                    {
                        targetIndexList.Add(backBattlerInfos[i].Index);
                    }
                }
            } else
            {
                List<BattlerInfo> backBattlerInfos = BattlerEnemies().FindAll(a => a.IsAlive() && a.LineIndex == LineType.Back);
                for (int i = 0;i < backBattlerInfos.Count;i++)
                {
                    targetIndexList.Add(backBattlerInfos[i].Index);
                }
            }
        } else
        {
            List<BattlerInfo> battlerInfos = _battlers;
            for (int i = 0;i < battlerInfos.Count;i++)
            {
                targetIndexList.Add(battlerInfos[i].Index);
            }
        }
        return targetIndexList;
    }

    private List<int> TargetIndexOpponent(bool isActor,List<int> targetIndexList,RangeType rangeType,LineType lineType)
    {   
        if (isActor)
        {
            if (rangeType == RangeType.S)
            {
                // 最前列は
                bool IsFrontAlive = BattlerEnemies().Find(a => a.IsAlive() && a.LineIndex == 0) != null;
                if (IsFrontAlive)
                {
                    List<BattlerInfo> frontBattlerInfos = BattlerEnemies().FindAll(a => a.IsAlive() && a.LineIndex == 0);
                    for (int i = 0;i < frontBattlerInfos.Count;i++)
                    {
                        if (frontBattlerInfos[i].IsAlive())
                        {
                            targetIndexList.Add(frontBattlerInfos[i].Index);
                        }
                    }
                } else
                {
                    List<BattlerInfo> backBattlerInfos = BattlerEnemies().FindAll(a => a.IsAlive() && a.LineIndex == LineType.Back);
                    for (int i = 0;i < backBattlerInfos.Count;i++)
                    {
                        targetIndexList.Add(backBattlerInfos[i].Index);
                    }
                }
            } else
            {
                List<BattlerInfo> battlerInfos = BattlerEnemies().FindAll(a => a.IsAlive());
                for (int i = 0;i < battlerInfos.Count;i++)
                {
                    targetIndexList.Add(battlerInfos[i].Index);
                }
            }
        } else{
            if (rangeType == RangeType.S)
            {
                // 最前列は
                bool IsFrontLine = lineType == LineType.Front;
                if (IsFrontLine)
                {
                    List<BattlerInfo> battlerInfos = BattlerActors();
                    for (int i = 0;i < battlerInfos.Count;i++)
                    {
                        targetIndexList.Add(battlerInfos[i].Index);
                    }
                } else{
                    if (BattlerEnemies().Find(a => a.IsAlive() && a.LineIndex == LineType.Front) == null)
                    {
                        List<BattlerInfo> battlerInfos = BattlerActors();
                        for (int i = 0;i < battlerInfos.Count;i++)
                        {
                            targetIndexList.Add(battlerInfos[i].Index);
                        }
                    }
                }
            } else
            { 
                List<BattlerInfo> battlerInfos = BattlerActors();
                for (int i = 0;i < battlerInfos.Count;i++)
                {
                    targetIndexList.Add(battlerInfos[i].Index);
                }
            }
        }
        return targetIndexList;
    }

    private List<int> TargetIndexFriend(bool isActor,List<int> targetIndexList)
    {
        List<BattlerInfo> battlerInfos = isActor ? BattlerActors() : BattlerEnemies();
        for (int i = 0;i < battlerInfos.Count;i++)
        {
            targetIndexList.Add(battlerInfos[i].Index);
        }
        return targetIndexList;
    }

    public bool CanUseCondition(int skillId,int targetIndex)
    {
        bool IsEnable = false;
        SkillsData.SkillData skill = DataSystem.Skills.Find(a => a.Id == skillId);
        BattlerInfo target = GetBattlerInfo(targetIndex);
        foreach (var featureData in skill.FeatureDatas)
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
                case FeatureType.AddState:
                if ((StateType)featureData.Param1 == StateType.RemoveBuff && target.GetRemovalBuffStates().Count > 0)
                {
                    IsEnable = true;
                } else
                if (!target.IsState((StateType)featureData.Param1) && !target.IsState(StateType.Barrier))
                {
                    IsEnable = true;
                } else
                if (CurrentBattler.isActor || (StateType)featureData.Param1 == StateType.DamageUp || (StateType)featureData.Param1 == StateType.Prizm)
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

    public void MakeActionResultInfo(ActionInfo actionInfo,List<int> indexList)
    {   
        if (actionInfo.SubjectIndex < 100)
        {
            if (actionInfo.Master.TargetType == TargetType.Opponent)
            {
                if (indexList.Count > 0)
                {
                    CurrentBattler.SetLastTargetIndex(indexList[0]);
                }
            }
        }
        if (actionInfo.Master.TargetType == TargetType.All)
        {
            if (indexList.Count > 0)
            {
                if (indexList[0] > 100)
                {
                    CurrentBattler.SetLastTargetIndex(indexList[0]);
                }
            }
        }
        int MpCost = actionInfo.Master.MpCost;
        actionInfo.SetMpCost(MpCost);
        var isPrizm = false;
        var repeatTime = actionInfo.Master.RepeatTime;
        if (actionInfo.Master.Attribute == AttributeType.Shine && CurrentBattler.IsState(StateType.Prizm))
        {
            var damageFeatures = actionInfo.Master.FeatureDatas.FindAll(a => a.FeatureType == FeatureType.HpDamage || a.FeatureType == FeatureType.HpDefineDamage);
            if (damageFeatures.Count > 0)
            {
                repeatTime *= (CurrentBattler.GetStateInfoAll(StateType.Prizm).Count + 1);
                isPrizm = true;
            }
        }

        Dictionary<int,int> nodamageDict = new ();
        for (int i = 0; i < indexList.Count;i++)
        {
            nodamageDict[indexList[i]] = GetBattlerInfo(indexList[i]).StateTurn(StateType.NoDamage);
        }
        List<ActionResultInfo> actionResultInfos = new List<ActionResultInfo>();

        for (int i = 0; i < repeatTime;i++)
        {
            for (int j = 0; j < indexList.Count;j++)
            {
                BattlerInfo Target = GetBattlerInfo(indexList[j]);
                var featureDatas = new List<SkillsData.FeatureData>();
                foreach (var featureData in actionInfo.Master.FeatureDatas)
                {
                    if (isPrizm)
                    {
                        if (featureData.FeatureType == FeatureType.HpDamage || featureData.FeatureType == FeatureType.HpDefineDamage)
                        {
                            var prizmCount = (CurrentBattler.GetStateInfoAll(StateType.Prizm).Count + 1);
                            var prizmTime = (i+1) % prizmCount;
                            if (prizmTime == 0)
                            {
                                prizmTime = prizmCount;
                            }
                            var damageFeature = featureData.Copy();
                            var ratio = 1.0f / (prizmTime);
                            //UnityEngine.Debug.Log(ratio);
                            damageFeature.Param1 = (int)(damageFeature.Param1 * ratio);
                            featureDatas.Add(damageFeature);
                            continue;
                        }
                    }
                    featureDatas.Add(featureData);
                }

                ActionResultInfo actionResultInfo = new ActionResultInfo(CurrentBattler,Target,featureDatas,actionInfo.Master.Id);
                if (actionResultInfo.HpDamage > 0 
                || actionResultInfo.AddedStates.Find(a => a.Master.Id == (int)StateType.Stun) != null
                || actionResultInfo.AddedStates.Find(a => a.Master.Id == (int)StateType.Chain) != null
                || actionResultInfo.AddedStates.Find(a => a.Master.Id == (int)StateType.Death) != null
                || actionResultInfo.DeadIndexList.Contains(actionResultInfo.TargetIndex))            
                {
                    List<StateInfo> chainStateInfos = CheckChainedBattler(actionResultInfo.TargetIndex);
                    if (chainStateInfos.Count > 0)
                    {
                        for (int k = 0; k < chainStateInfos.Count;k++)
                        {
                            BattlerInfo chainedBattlerInfo = GetBattlerInfo(chainStateInfos[k].TargetIndex);
                            chainedBattlerInfo.RemoveState(chainStateInfos[k],true);
                            actionResultInfo.AddRemoveState(chainStateInfos[k]);
                        }
                    }
                    if (Target.IsState(StateType.Benediction))
                    {
                        List<StateInfo> benedictStateInfos = Target.GetStateInfoAll(StateType.Benediction);
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
                    
                    if ((removeCount+displayCount+1) >= nodamageDict[indexList[j]])
                    {
                        var noDamageState = Target.GetStateInfo(StateType.NoDamage);
                        if (noDamageState != null)
                        {
                            Target.RemoveState(noDamageState,true);
                            actionResultInfo.AddRemoveState(noDamageState);
                            var displayedResults = actionResultInfos.FindAll(a => a.DisplayStates.Find(b => b.Master.Id == (int)StateType.NoDamage) != null);

                            foreach (var displayedResult in displayedResults)
                            {
                                for (int k = displayedResult.DisplayStates.Count-1; 0 >= k;k--)
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
            List<StateInfo> curseStateInfos = CheckCursedBattler(indexList[i]);
            if (curseStateInfos.Count > 0)
            {
                for (int j = 0; j < curseStateInfos.Count;j++)
                {
                    var hpDamage = 0;
                    for (int k = 0;k < actionResultInfos.Count;k++)
                    {
                        if (actionResultInfos[k].CursedDamage) continue;
                        if (k >= beforeCurseIndex && actionResultInfos[k].HpDamage > 0 && curseStateInfos[j].BattlerId == actionResultInfos[k].TargetIndex)
                        {
                            hpDamage += actionResultInfos[k].HpDamage;
                        }
                    }
                    if (hpDamage > 0)
                    {
                        SkillsData.FeatureData featureData = new SkillsData.FeatureData();
                        featureData.FeatureType = FeatureType.HpCursedDamage;
                        featureData.Param1 = (int)MathF.Floor(hpDamage * curseStateInfos[j].Effect * 0.01f);

                        BattlerInfo curseBattlerInfo = GetBattlerInfo(curseStateInfos[j].TargetIndex);
                        ActionResultInfo curseActionResultInfo = new ActionResultInfo(GetBattlerInfo(curseStateInfos[j].BattlerId),curseBattlerInfo,new List<SkillsData.FeatureData>(){featureData},-1);
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

    public void ExecCurrentActionResult()
    {
        ActionInfo actionInfo = CurrentActionInfo();
        if (actionInfo != null)
        {
            // Mpの支払い
            CurrentBattler.GainMp(actionInfo.MpCost * -1);
            CurrentBattler.GainPaybattleMp(actionInfo.MpCost);
            List<ActionResultInfo> actionResultInfos = CalcDeathIndexList(actionInfo.ActionResults);
            for (int i = 0; i < actionResultInfos.Count; i++)
            {
                ExecActionResultInfo(actionResultInfos[i]);
            }
        }
    }

    public void PopupActionResultInfo(List<ActionResultInfo> actionResultInfos)
    {
        // ドレイン回復をまとめる
        List<ActionResultInfo> reHealResults = actionResultInfos.FindAll(a => a.ReHeal > 0);
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
        List<ActionResultInfo> counterResults = actionResultInfos.FindAll(a => a.ReDamage > 0);
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
        if (actionResultInfo.HpDamage > 0 || actionResultInfo.ExecStateInfos[target.Index].Contains(StateType.CounterOura))
        {
            if (target.IsState(StateType.CounterOura))
            {
                List<StateInfo> counterOuraStateInfos = target.GetStateInfoAll(StateType.CounterOura);
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
        if (CurrentActionInfo().TriggeredSkill == false)
        {
            CurrentBattler.ResetAp(false);
        }
        _actionInfos.RemoveAt(0);
        _currentBattler = null;
    }

    public void CheckPlusSkill()
    {
        ActionInfo actionInfo = CurrentActionInfo();
        if (actionInfo != null)
        {
            List<ActionInfo> actionInfos = actionInfo.CheckPlusSkill();
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
        var afterSkillInfo = CurrentBattler.Skills.Find(a => a.Master.FeatureDatas.Find(b => b.FeatureType == FeatureType.AddState && (StateType)b.Param1 == StateType.AfterHeal) != null);
        if (CurrentBattler.IsState(StateType.AfterHeal) && afterSkillInfo != null)
        {
            var stateInfo = CurrentBattler.GetStateInfo(StateType.AfterHeal);
            var actionInfo = MakeActionInfo(CurrentBattler,afterSkillInfo.Id,false,false);
            
            if (actionInfo != null)
            {
                _actionInfos.Remove(actionInfo);
                var party = CurrentBattler.isActor ? BattlerActors() : BattlerEnemies();
                var targetIndexs = new List<int>();
                foreach (var member in party)
                {
                    if (CurrentBattler.Index != member.Index)
                    {
                        targetIndexs.Add(member.Index);
                    }
                    
                }
                foreach (var targetIndex in targetIndexs)
                {
                    SkillsData.FeatureData featureData = new SkillsData.FeatureData();
                    featureData.FeatureType = FeatureType.HpHeal;
                    featureData.Param1 = stateInfo.Effect;

                    ActionResultInfo actionResultInfo = new ActionResultInfo(GetBattlerInfo(targetIndex),GetBattlerInfo(targetIndex),new List<SkillsData.FeatureData>(){featureData},-1);
                    afterHealResults.Add(actionResultInfo);
                }
            }
        }
        return afterHealResults;
    }

    private List<ActionResultInfo> AssistHealActionResults()
    {
        var assistHealResults = new List<ActionResultInfo>();
        var afterSkillInfo = CurrentBattler.Skills.Find(a => a.Master.FeatureDatas.Find(b => b.FeatureType == FeatureType.AddState && (StateType)b.Param1 == StateType.AssistHeal) != null);
        if (CurrentBattler.IsState(StateType.AssistHeal) && afterSkillInfo != null)
        {
            var stateInfo = CurrentBattler.GetStateInfo(StateType.AssistHeal);
            var actionInfo = MakeActionInfo(CurrentBattler,afterSkillInfo.Id,false,false);
            
            if (actionInfo != null)
            {
                _actionInfos.Remove(actionInfo);
                var party = CurrentBattler.isActor ? BattlerActors() : BattlerEnemies();
                party = party.FindAll(a => a.IsAlive());
                var targetIndexs = new List<int>();
                foreach (var member in party)
                {
                    //if (CurrentBattler.Index != member.Index)
                    //{
                        targetIndexs.Add(member.Index);
                    //}
                    
                }
                var healValue = CurrentActionInfo().ActionResults.FindAll(a => a.HpDamage > 0).Count;
                foreach (var targetIndex in targetIndexs)
                {
                    SkillsData.FeatureData featureData = new SkillsData.FeatureData();
                    featureData.FeatureType = FeatureType.HpHeal;
                    featureData.Param1 = healValue * stateInfo.Effect;

                    ActionResultInfo actionResultInfo = new ActionResultInfo(GetBattlerInfo(targetIndex),GetBattlerInfo(targetIndex),new List<SkillsData.FeatureData>(){featureData},-1);
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
        var targetIndexs = new List<int>();
        foreach (var result in results)
        {
            if (!targetIndexs.Contains(result.TargetIndex))
            {
                targetIndexs.Add(result.TargetIndex);
            }
        }
        foreach (var targetIndex in targetIndexs)
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
        
        SkillsData.FeatureData featureData = new SkillsData.FeatureData();
        featureData.FeatureType = featureType;

        for (int i = 0;i < stateInfos.Count;i++)
        {
            featureData.Param1 = stateInfos[i].Effect;
            BattlerInfo target = GetBattlerInfo(stateInfos[i].BattlerId);
            if (target.IsAlive())
            {
                ActionResultInfo actionResultInfo = new ActionResultInfo(GetBattlerInfo(stateInfos[i].BattlerId),GetBattlerInfo(stateInfos[i].TargetIndex),new List<SkillsData.FeatureData>(){featureData},-1);
                actionResultInfos.Add(actionResultInfo);
            }
        }
        return actionResultInfos;
    }

    public void GainHpTargetIndex(int index,int value)
    {
        GetBattlerInfo(index).GainHp(value);
    }

    public bool CheckTriggerSkillInfos(TriggerTiming triggerTiming,List<ActionResultInfo> actionResultInfos,bool checkCurrent = true)
    {
        bool IsTriggered = false;
        if (CurrentBattler == null && checkCurrent == true)
        {
            return false;
        }
        List<ActionInfo> actionInfos = new List<ActionInfo>();
        List<SkillInfo> triggeredSkills = new List<SkillInfo>();
        List<BattlerInfo> TargetBattlerInfos = new List<BattlerInfo>();
        if (checkCurrent)
        {
            TargetBattlerInfos.Add(CurrentBattler);
        }
        for (var i = 0;i < actionResultInfos.Count;i++)
        {
            BattlerInfo target = GetBattlerInfo(actionResultInfos[i].TargetIndex);
            if (!TargetBattlerInfos.Contains(target))
            {
                TargetBattlerInfos.Add(target);
            }
        }
        for (var i = 0;i < _battlers.Count;i++)
        {
            if (!TargetBattlerInfos.Contains(_battlers[i]))
            {
                TargetBattlerInfos.Add(_battlers[i]);
            }
        }
        
        for (var i = 0;i < TargetBattlerInfos.Count;i++)
        {
            BattlerInfo target = TargetBattlerInfos[i];
            triggeredSkills.Clear();
            foreach (var skillInfo in target.ActiveSkills())
            {
                if (checkCurrent == false || CurrentActionInfo().Master.Id != skillInfo.Id)
                {
                    var triggerDatas = skillInfo.Master.TriggerDatas.FindAll(a => a.TriggerTiming == triggerTiming);
                    if (IsTriggerdSkillInfo(target,triggerDatas,triggerTiming,actionResultInfos))
                    {
                        triggeredSkills.Add(skillInfo);
                    }
                }
            }
            if (triggeredSkills.Count > 0)
            {
                IsTriggered = true;
                for (var j = 0;j < triggeredSkills.Count;j++)
                {
                    if (triggeredSkills[j].Master.SkillType == SkillType.Demigod){
                        if (target.IsAwaken == false)
                        {
                            target.SetAwaken();
                            ActionInfo makeActionInfo = MakeActionInfo(target,triggeredSkills[j].Id,triggerTiming == TriggerTiming.Interrupt,true);
                        } else{

                        }
                    } else{
                        ActionInfo makeActionInfo = MakeActionInfo(target,triggeredSkills[j].Id,triggerTiming == TriggerTiming.Interrupt,true);
                    }
                }
            }
        }

        return IsTriggered;
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
                var triggerDatas = passiveInfo.Master.TriggerDatas.FindAll(a => a.TriggerTiming == triggerTiming);
                if (IsTriggerdSkillInfo(battlerInfo,triggerDatas,triggerTiming,new List<ActionResultInfo>()))
                {                
                    bool usable = true;
                    // トリガーのParam2を使用回数制限にする
                    if (triggerDatas.Find(a => a.Param2 == 1) != null)
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
                            ActionResultInfo actionResultInfo = new ActionResultInfo(battlerInfo,battlerInfo,passiveInfo.Master.FeatureDatas,passiveInfo.Id);
                            actionResultInfos.Add(actionResultInfo);
                        } else
                        if (passiveInfo.Master.Scope == ScopeType.All)
                        {
                            var partyMember = battlerInfo.isActor ? BattlerActors() : BattlerEnemies();
                            foreach (var member in partyMember)
                            {
                                if ((passiveInfo.Master.AliveOnly && member.IsAlive()) || (!passiveInfo.Master.AliveOnly && !member.IsAlive()) )
                                {
                                    ActionResultInfo actionResultInfo = new ActionResultInfo(battlerInfo,member,passiveInfo.Master.FeatureDatas,passiveInfo.Id);
                                    actionResultInfos.Add(actionResultInfo);
                                }
                            }
                        } else
                        {
                            if (passiveInfo.Master.TargetType == TargetType.AttackTarget)
                            {
                                if (CurrentActionInfo() != null && CurrentActionInfo().ActionResults.Count > 0)
                                {
                                    ActionResultInfo actionResultInfo = new ActionResultInfo(battlerInfo,GetBattlerInfo(CurrentActionInfo().ActionResults[0].TargetIndex),passiveInfo.Master.FeatureDatas,passiveInfo.Id);
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
                
                foreach (var feature in passiveInfo.Master.FeatureDatas)
                {
                    if (feature.FeatureType == FeatureType.AddState)
                    {
                        var triggerDatas = passiveInfo.Master.TriggerDatas.FindAll(a => a.TriggerTiming == TriggerTiming.After || a.TriggerTiming == TriggerTiming.StartBattle);
                        if (IsRemove == false && !IsTriggerdSkillInfo(battlerInfo,triggerDatas,TriggerTiming.After,new List<ActionResultInfo>()))
                        {
                            IsRemove = true;
                            
                            SkillsData.FeatureData featureData = new SkillsData.FeatureData();
                            featureData.FeatureType = FeatureType.RemoveStatePassive;
                            featureData.Param1 = feature.Param1;
                            if (passiveInfo.Master.Scope == ScopeType.Self)
                            {
                                ActionResultInfo actionResultInfo = new ActionResultInfo(battlerInfo,battlerInfo,new List<SkillsData.FeatureData>(){featureData},passiveInfo.Id);
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

                                        ActionResultInfo actionResultInfo = new ActionResultInfo(battlerInfo,member,new List<SkillsData.FeatureData>(){featureData},passiveInfo.Id);
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

    private bool IsTriggerdSkillInfo(BattlerInfo battlerInfo,List<SkillsData.TriggerData> triggerDatas,TriggerTiming triggerTiming,List<ActionResultInfo> actionResultInfos)
    {
        bool IsTriggered = false;
        //var triggerDatas = skillInfo.Master.TriggerDatas.FindAll(a => a.TriggerTiming == triggerTiming);
        if (triggerDatas.Count > 0)
        {
            for (var j = 0;j < triggerDatas.Count;j++)
            {
                if (triggerTiming == TriggerTiming.Use)
                {
                    if (CurrentActionInfo() == null)
                    {
                        IsTriggered = false;
                        break;
                    }
                    if (CurrentActionInfo().SubjectIndex != battlerInfo.Index)
                    {
                        IsTriggered = false;
                        break;
                    }
                }
                if (triggerDatas[j].IsTriggerdSkillInfo(battlerInfo,BattlerActors(),BattlerEnemies()))
                {
                    IsTriggered = true;
                }
                if (triggerDatas[j].TriggerType == TriggerType.AttackState)
                {
                    if (battlerInfo.IsAlive() && CurrentActionInfo() != null && CurrentActionInfo().SubjectIndex == battlerInfo.Index && CurrentActionInfo().ActionResults.Find(a => a.HpDamage > 0) != null)
                    {
                        if (triggerDatas[j].Param1 > UnityEngine.Random.Range(0,100))
                        {
                            IsTriggered = true;
                        }
                    }
                }
                if (triggerTiming == TriggerTiming.After && triggerDatas[j].TriggerType == TriggerType.PayBattleMp)
                {
                    if (battlerInfo.IsAlive() && battlerInfo.PayBattleMp >= triggerDatas[j].Param1)
                    {
                        IsTriggered = true;
                    }
                }
                if (battlerInfo.IsAwaken == false && triggerTiming == TriggerTiming.Interrupt && triggerDatas[j].TriggerType == TriggerType.ActionResultDeath)
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
                if (battlerInfo.IsAwaken == false && triggerDatas[j].TriggerType == TriggerType.DeadWithoutSelf)
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
                if (battlerInfo.IsAwaken == false && triggerDatas[j].TriggerType == TriggerType.SelfDead)
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
                if (battlerInfo.IsAwaken == false && triggerTiming == TriggerTiming.Interrupt && triggerDatas[j].TriggerType == TriggerType.ActionResultAddState)
                {
                    if (battlerInfo.IsAlive())
                    {
                        if (CurrentActionInfo() != null && battlerInfo.isActor != CurrentBattler.isActor)
                        {
                            var states = CurrentActionInfo().Master.FeatureDatas.FindAll(a => a.FeatureType == FeatureType.AddState);
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
                if (battlerInfo.IsAwaken == false && triggerTiming == TriggerTiming.After && triggerDatas[j].TriggerType == TriggerType.DefeatEnemyByAttack)
                {
                    if (battlerInfo.IsAlive() && CurrentBattler != null && battlerInfo.Index == CurrentBattler.Index)
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
                if (triggerDatas[j].TriggerType == TriggerType.LessTroopMembers)
                {
                    var troops = _battlers.FindAll(a => !a.isActor);
                    var party = _battlers.FindAll(a => a.isActor);
                    if ( troops.Count >= party.Count )
                    {
                        IsTriggered = true;
                    }
                }
                if (triggerDatas[j].TriggerType == TriggerType.MoreTroopMembers)
                {
                    var troops = _battlers.FindAll(a => !a.isActor);
                    var party = _battlers.FindAll(a => a.isActor);
                    if ( troops.Count <= party.Count )
                    {
                        IsTriggered = true;
                    }
                }
                // Param3をAnd条件フラグにする
                if (triggerDatas[j].Param3 == 1)
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
        if (skillInfo.TriggerDatas.Count > 0)
        {
            CanUse = false;
            CanUse = IsTriggerdSkillInfo(battlerInfo,skillInfo.TriggerDatas,TriggerTiming.None,new List<ActionResultInfo>());
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
        List<int> targetIndexList = MakeActionTarget(actionInfo.Master.Id,actionInfo.SubjectIndex,true);
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
                List<int> tempIndexList = MakeActionTarget(actionInfo.Master.Id,actionInfo.SubjectIndex,false);
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
        var deathIndexs = new List<int>();
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
                        if (!deathIndexs.Contains(battlerInfo.Index) && !actionResultInfo.DeadIndexList.Contains(battlerInfo.Index))
                        {
                            deathIndexs.Add(battlerInfo.Index);
                            actionResultInfo.DeadIndexList.Add(battlerInfo.Index);
                        }
                    }
                }
                if (actionResultInfo.SubjectIndex == battlerInfo.Index)
                {
                    if (damageData[actionResultInfo.SubjectIndex] >= battlerInfo.Hp)
                    {
                        if (!deathIndexs.Contains(battlerInfo.Index) && !actionResultInfo.DeadIndexList.Contains(battlerInfo.Index))
                        {
                            deathIndexs.Add(battlerInfo.Index);
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

    public bool EnableEspape()
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
}