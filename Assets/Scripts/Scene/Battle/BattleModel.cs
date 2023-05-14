using System.Collections;
using System.Collections.Generic;
using System;
using System.Threading;
using System.Threading.Tasks;
using Effekseer;

public class BattleModel : BaseModel
{
    public BattleModel()
    {
        CurrentScene = Scene.Battle;
    }

    private List<BattlerInfo> _battlers = new List<BattlerInfo>();
    public List<BattlerInfo> Battlers
    {
        get {return _battlers;}
    }
    
    private int _currentIndex = 0; 
    public int CurrentIndex
    {
        get {return _currentIndex;}
    }

    private AttributeType _currentAttributeType = AttributeType.Fire;    
    public AttributeType CurrentAttributeType
    {
        get {return _currentAttributeType;}
    }

    public ActorInfo CurrentActor
    {
        get {return PartyMembers()[_currentIndex];}
    }

    private BattlerInfo _currentBattler = null;
    public BattlerInfo CurrentBattler
    {
        get {return _currentBattler;}
    }
    public ActorsData.ActorData CurrentBattlerActorData()
    {
        return DataSystem.Actors.Find(a => a.Id == _currentBattler.CharaId);
    }

    private List<ActionInfo> _actionInfos = new List<ActionInfo>();
    private Dictionary<int,List<SkillInfo>> _passiveSkillInfos = new Dictionary<int,List<SkillInfo>>();

    public Task<List<UnityEngine.AudioClip>> GetBattleBgm()
    {
        var battleMembers = PartyMembers();
        return GetBgmData("BATTLE" + (battleMembers[0].ActorId).ToString());
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
                    battlerInfo.AddState(stateInfo);
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
            _passiveSkillInfos[battlerInfo1.Index] = new List<SkillInfo>();
        }
    }

    public void UpdateAp()
    {
        for (int i = 0;i < _battlers.Count;i++)
        {
            BattlerInfo battlerInfo = _battlers[i];
            _battlers[i].UpdateAp();
            _battlers[i].UpdateState(RemovalTiming.UpdateAp);
        }
        MakeActionBattler();
    }
    
    public List<ActionResultInfo> UpdateChainState()
    {
        List<ActionResultInfo> actionResultInfos = new List<ActionResultInfo>();
        for (int i = 0;i < _battlers.Count;i++)
        {
            List<StateInfo> chainDamageStateInfos = _battlers[i].UpdateChainState();
            for (int j = 0;j < chainDamageStateInfos.Count;j++)
            {
                StateInfo stateInfo = chainDamageStateInfos[j];
                BattlerInfo subject = GetBattlerInfo(stateInfo.BattlerId);
                BattlerInfo target = GetBattlerInfo(stateInfo.TargetIndex);
                if (target != null)
                {
                    int chainDamage = stateInfo.Effect;
                    if (subject.IsState(StateType.ChainDamageUp))
                    {
                        chainDamage += subject.ChainSuccessCount;
                    }
                    SkillsData.FeatureData featureData = new SkillsData.FeatureData();
                    featureData.FeatureType = FeatureType.HpDefineDamage;
                    featureData.Param1 = chainDamage;
                    
                    ActionResultInfo actionResultInfo = new ActionResultInfo(subject,target,new List<SkillsData.FeatureData>(){featureData});
                        
                    if ((target.Hp - chainDamage) <= 0)
                    {
                        _battlers[i].RemoveState(stateInfo);
                    }
                    actionResultInfos.Add(actionResultInfo);
                }
            }
        }
        return actionResultInfos;
    }

    public List<ActionResultInfo> UpdateBenedictionState()
    {
        List<ActionResultInfo> actionResultInfos = new List<ActionResultInfo>();
        for (int i = 0;i < _battlers.Count;i++)
        {
            List<StateInfo> benedictionStateInfos = _battlers[i].GetStateInfoAll(StateType.Benediction);
            for (int j = 0;j < benedictionStateInfos.Count;j++)
            {
                StateInfo stateInfo = benedictionStateInfos[j];
                if (stateInfo.Turns % stateInfo.BaseTurns == 0)
                {
                    BattlerInfo subject = _battlers.Find(a => a.Index == stateInfo.BattlerId);
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
                        ActionResultInfo actionResultInfo = new ActionResultInfo(_battlers[i],target,new List<SkillsData.FeatureData>(){featureData});
                        actionResultInfos.Add(actionResultInfo);
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
        List<int> targetIndexs = new List<int>();
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
                        CurrentBattler.RemoveState(stateInfos[j]);
                    }
                }
            }
        }
        return targetIndexs;
    }

    private List<StateInfo> CheckChainedBattler(int targetIndex)
    {
        List<StateInfo> stateInfos = new List<StateInfo>();
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
        List<StateInfo> stateInfos = new List<StateInfo>();
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
        if (attributeType != AttributeType.None)
        {
            _currentAttributeType = attributeType;
        }
        List<SkillInfo> skillInfos = CurrentBattler.Skills.FindAll(a => a.Attribute == _currentAttributeType && a.Master.SkillType != SkillType.None);
        for (int i = 0; i < skillInfos.Count;i++)
        {
            skillInfos[i].SetEnable(CheckCanUse(skillInfos[i],CurrentBattler));
        }
        return skillInfos;
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
        if (battlerInfo.IsState(StateType.Banish))
        {
            if (skillInfo.Master.MpCost > 0)
            {
                return false;
            }
        }
        if (CanUseTrigger(skillInfo,battlerInfo) == false)
        {
            return false;
        }
        List<int> targetIndexList = MakeActionTarget(skillInfo.Master.Id,battlerInfo.Index);
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
        if (CurrentBattler.IsState(StateType.Stun))
        {
            return false;
        }
        return true;
    }

    public ActionInfo MakeActionInfo(BattlerInfo subject, int skillId,bool IsInterrupt)
    {
        SkillsData.SkillData skill = DataSystem.Skills.Find(a => a.Id == skillId);
        int LastTargetIndex = -1;
        if (subject.isActor)
        {
            LastTargetIndex = subject.LastTargetIndex();
            if (skill.TargetType == TargetType.Opponent)
            {
                BattlerInfo targetBattler = BattlerEnemies().Find(a => a.Index == LastTargetIndex && a.IsAlive());
                if (targetBattler == null || targetBattler.IsAlive() == false)
                {
                    if (BattlerEnemies().Count > 0 && BattlerEnemies().Find(a => a.IsAlive()) != null)
                    {
                        LastTargetIndex = BattlerEnemies().Find(a => a.IsAlive()).Index;
                    }
                }
            } else
            {
                LastTargetIndex = subject.Index;
            }
        }
        List<int> targetIndexList = MakeActionTarget(skillId,subject.Index);
        if (subject.IsState(StateType.Substitute))
        {
            int substituteId = subject.GetStateInfoAll(StateType.Substitute)[0].BattlerId;
            if (targetIndexList.Contains(substituteId))
            {
                targetIndexList.Clear();
                targetIndexList.Add(substituteId);
            }
        }
        ActionInfo actionInfo = new ActionInfo(skillId,subject.Index,LastTargetIndex,targetIndexList);
        if (subject.IsState(StateType.Extension))
        {
            actionInfo.SetRangeType(RangeType.L);
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

    public List<int> MakeActionTarget(int skillId,int subjectIndex)
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
        for (int i = targetIndexList.Count-1;i >= 0;i--)
        {
            if (CanUseCondition(skillId,targetIndexList[i]) == false)
            {
                targetIndexList.Remove(targetIndexList[i]);
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
        if (isActor)
        {
            List<BattlerInfo> battlerInfos = BattlerActors();
            for (int i = 0;i < battlerInfos.Count;i++)
            {
                targetIndexList.Add(battlerInfos[i].Index);
            }
        } else
        {
            List<BattlerInfo> battlerInfos = BattlerEnemies();
            for (int i = 0;i < battlerInfos.Count;i++)
            {
                targetIndexList.Add(battlerInfos[i].Index);
            }
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
                if (CurrentBattler.isActor)
                {
                    {
                        IsEnable = true;
                    }
                } else 
                if (target.Hp < target.MaxHp)
                {
                    if (!target.isActor)
                    {
                        IsEnable = true;
                    }
                }
                break;
                case FeatureType.MpHeal:
                if (target.Mp < target.MaxMp)
                {
                    IsEnable = true;
                }
                break;
                case FeatureType.AddState:
                if (!target.IsState((StateType)featureData.Param1))
                {
                    IsEnable = true;
                }
                break;
                case FeatureType.RemoveState:
                if (target.IsState((StateType)featureData.Param1))
                {
                    IsEnable = true;
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
                    CurrentBattler.SetLastTargetIndex(indexList[0] - 100);
                }
            }
        }
        if (actionInfo.Master.TargetType == TargetType.All)
        {
            if (indexList.Count > 0)
            {
                if (indexList[0] > 100)
                {
                    CurrentBattler.SetLastTargetIndex(indexList[0] - 100);
                }
            }
        }
        int MpCost = actionInfo.Master.MpCost;
        actionInfo.SetMpCost(MpCost);
        List<ActionResultInfo> actionResultInfos = new List<ActionResultInfo>();
        for (int i = 0; i < indexList.Count;i++)
        {
            BattlerInfo Target = GetBattlerInfo(indexList[i]);
            ActionResultInfo actionResultInfo = new ActionResultInfo(CurrentBattler,Target,actionInfo.Master.FeatureDatas);
            if (actionResultInfo.HpDamage > 0 
             || actionResultInfo.AddedStates.Find(a => a.Master.Id == (int)StateType.Stun) != null
             || actionResultInfo.DeadIndexList.Contains(actionResultInfo.TargetIndex))            
            {
                List<StateInfo> chainStateInfos = CheckChainedBattler(actionResultInfo.TargetIndex);
                if (chainStateInfos.Count > 0)
                {
                    for (int j = 0; j < chainStateInfos.Count;j++)
                    {
                        BattlerInfo chainedBattlerInfo = GetBattlerInfo(chainStateInfos[j].TargetIndex);
                        chainedBattlerInfo.RemoveState(chainStateInfos[j]);
                        actionResultInfo.AddRemoveState(chainStateInfos[j]);
                    }
                }
                if (Target.IsState(StateType.Benediction))
                {
                    List<StateInfo> benedictStateInfos = Target.GetStateInfoAll(StateType.Benediction);
                    for (int j = 0; j < benedictStateInfos.Count;j++)
                    {
                        Target.RemoveState(benedictStateInfos[j]);
                        Target.ResetAp(false);
                        actionResultInfo.AddRemoveState(benedictStateInfos[j]);
                    }
                }
                if (Target.IsState(StateType.CounterOura))
                {
                    List<StateInfo> counterOuraStateInfos = Target.GetStateInfoAll(StateType.CounterOura);
                    for (int j = 0; j < counterOuraStateInfos.Count;j++)
                    {
                        Target.RemoveState(counterOuraStateInfos[j]);
                        actionResultInfo.AddRemoveState(counterOuraStateInfos[j]);
                    }
                }
            }
            actionResultInfos.Add(actionResultInfo);
            // 呪い
            List<StateInfo> curseStateInfos = CheckCursedBattler(actionResultInfo.TargetIndex);
            if (actionResultInfo.HpDamage > 0 && curseStateInfos.Count > 0)
            {
                SkillsData.FeatureData featureData = new SkillsData.FeatureData();
                featureData.FeatureType = FeatureType.HpDamage;
                featureData.Param1 = actionResultInfo.HpDamage;
                for (int j = 0; j < curseStateInfos.Count;j++)
                {
                    BattlerInfo curseBattlerInfo = GetBattlerInfo(curseStateInfos[j].TargetIndex);
                    ActionResultInfo curseActionResultInfo = new ActionResultInfo(GetBattlerInfo(curseStateInfos[j].BattlerId),curseBattlerInfo,new List<SkillsData.FeatureData>(){featureData});
                    actionResultInfos.Add(curseActionResultInfo);
                }
            }
        }
        actionInfo.SetActionResult(actionResultInfos);
    }

    public EffekseerEffectAsset SkillActionAnimation(string animationName)
    {
        //string path = "Assets/Animations/" + animationName + ".asset";
        //var result = await ResourceSystem.LoadAsset<EffekseerEffectAsset>(path);
        string path = "Animations/" + animationName;
        var result = UnityEngine.Resources.Load<EffekseerEffectAsset>(path);
        return result;
    }

    public void ExecActionResult()
    {
        ActionInfo actionInfo = CurrentActionInfo();
        if (actionInfo != null)
        {
            // Mpの支払い
            CurrentBattler.GainMp(actionInfo.MpCost * -1);
            CurrentBattler.GainPaybattleMp(actionInfo.MpCost);
            List<ActionResultInfo> actionResultInfos = actionInfo.ActionResults;
            for (int i = 0; i < actionResultInfos.Count; i++)
            {
                ExecActionResultInfo(actionResultInfos[i]);
            }
        }
    }

    public void ExecActionResultInfo(ActionResultInfo actionResultInfo)
    {
        BattlerInfo subject = GetBattlerInfo(actionResultInfo.SubjectIndex);
        BattlerInfo target = GetBattlerInfo(actionResultInfo.TargetIndex);
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
            subject.ChangeAp(actionResultInfo.ApHeal * -1);
        }
        if (actionResultInfo.ReDamage != 0)
        {
            subject.GainHp(-1 * actionResultInfo.ReDamage);
        }
        if (actionResultInfo.ReHeal != 0)
        {
            subject.GainHp(actionResultInfo.ReHeal);
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
    }
    
    public List<int> DeathBattlerIndex(List<ActionResultInfo> actionResultInfos)
    {
        List<int> deathBattlerIndex = new List<int>();
        for (int i = 0; i < actionResultInfos.Count; i++)
        {
            for (int j = 0; j < actionResultInfos[i].DeadIndexList.Count; j++)
            {
                deathBattlerIndex.Add(actionResultInfos[i].DeadIndexList[j]);
            }
        }
        return deathBattlerIndex;
    }

    public void UpdateTurn()
    {
        CurrentBattler.UpdateState(RemovalTiming.UpdateTurn);
        CurrentBattler.TurnEnd();
    }

    public void TurnEnd()
    {
        _actionInfos.RemoveAt(0);
        CurrentBattler.ResetAp(false);
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

    public bool CheckRegene()
    {
        return CurrentBattler != null && CurrentBattler.IsState(StateType.Regene);
    }

    public List<ActionResultInfo> UpdateRegeneState()
    {
        return MakeStateActionResult(CurrentBattler,StateType.Regene,FeatureType.HpHeal);
    }

    public bool CheckSlipDamage()
    {
        return CurrentBattler != null && CurrentBattler.IsState(StateType.SlipDamage);
    }

    public List<ActionResultInfo> UpdateSlipDamageState()
    {
        return MakeStateActionResult(CurrentBattler,StateType.SlipDamage,FeatureType.HpDefineDamage);
    }

    public List<ActionResultInfo> MakeStateActionResult(BattlerInfo battlerInfo,StateType stateType,FeatureType featureType)
    {
        List<ActionResultInfo> actionResultInfos = new List<ActionResultInfo>();
        List<StateInfo> stateInfos = battlerInfo.GetStateInfoAll(stateType);
        
        SkillsData.FeatureData featureData = new SkillsData.FeatureData();
        featureData.FeatureType = featureType;

        for (int i = 0;i < stateInfos.Count;i++)
        {
            featureData.Param1 = stateInfos[i].Effect;
            ActionResultInfo actionResultInfo = new ActionResultInfo(GetBattlerInfo(stateInfos[i].BattlerId),GetBattlerInfo(stateInfos[i].TargetIndex),new List<SkillsData.FeatureData>(){featureData});
            actionResultInfos.Add(actionResultInfo);
        }
        return actionResultInfos;
    }

    public void gainHpTargetIndex(int index,int value)
    {
        GetBattlerInfo(index).GainHp(value);
    }

    public bool CheckTriggerSkillInfos(TriggerTiming triggerTiming)
    {
        bool IsTriggered = false;
        if (CurrentBattler == null)
        {
            return false;
        }
        List<ActionInfo> actionInfos = new List<ActionInfo>();
        List<SkillInfo> triggeredSkills = new List<SkillInfo>();
        List<BattlerInfo> TargetBattlerInfos = new List<BattlerInfo>();
        TargetBattlerInfos.Add(CurrentBattler);
        List<ActionResultInfo> actionResultInfos = CurrentActionInfo().ActionResults;
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
                if (CurrentActionInfo().Master.Id != skillInfo.Id)
                {
                    if (IsTriggerdSkillInfo(target,skillInfo,triggerTiming))
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
                            ActionInfo makeActionInfo = MakeActionInfo(target,triggeredSkills[j].Id,triggeredSkills[j].Interrupt);
                        } else{

                        }
                    } else{
                        ActionInfo makeActionInfo = MakeActionInfo(target,triggeredSkills[j].Id,triggeredSkills[j].Interrupt);
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
            for (int j = 0;i < passiveSkills.Count;i++)
            {
                SkillInfo passiveInfo = passiveSkills[j];
                if (IsTriggerdSkillInfo(battlerInfo,passiveInfo,triggerTiming))
                {                
                    ActionResultInfo actionResultInfo = new ActionResultInfo(battlerInfo,battlerInfo,passiveInfo.Master.FeatureDatas);
                    actionResultInfos.Add(actionResultInfo);
                    _passiveSkillInfos[battlerInfo.Index].Add(passiveInfo);
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
            for (int j = 0;i < passiveSkills.Count;i++)
            {
                SkillInfo passiveInfo = passiveSkills[j];
                bool IsRemove = false;
                foreach (var feature in passiveInfo.Master.FeatureDatas)
                {
                    if (feature.FeatureType == FeatureType.AddState)
                    {
                        foreach (var triggerData in passiveInfo.Master.TriggerDatas)
                        {
                            if (IsRemove == false && !triggerData.IsTriggerdSkillInfo(battlerInfo,BattlerActors(),BattlerEnemies()))
                            {
                                IsRemove = true;
                                SkillsData.FeatureData featureData = new SkillsData.FeatureData();
                                featureData.FeatureType = FeatureType.RemoveState;
                                featureData.Param1 = feature.Param1;
                                ActionResultInfo actionResultInfo = new ActionResultInfo(battlerInfo,battlerInfo,new List<SkillsData.FeatureData>(){featureData});
                                actionResultInfos.Add(actionResultInfo);
                            }
                        }
                    }
                }
            }
        }
        return actionResultInfos;
    }

    private bool IsTriggerdSkillInfo(BattlerInfo battlerInfo,SkillInfo skillInfo,TriggerTiming triggerTiming)
    {
        bool IsTriggered = false;
        var triggerDatas = skillInfo.Master.TriggerDatas.FindAll(a => a.TriggerTiming == triggerTiming);
        if (triggerDatas.Count > 0)
        {
            for (var j = 0;j < triggerDatas.Count;j++)
            {
                if (triggerDatas[j].IsTriggerdSkillInfo(battlerInfo,BattlerActors(),BattlerEnemies()))
                {
                    IsTriggered = true;
                }
                if (triggerDatas[j].TriggerType == TriggerType.MpUnder)
                {
                    if (CurrentActionInfo() != null)
                    {
                        if ((battlerInfo.Mp + CurrentActionInfo().MpCost) > triggerDatas[j].Param1)
                        {
                            IsTriggered = false;
                        }
                    }
                }
                if (triggerTiming == TriggerTiming.After && triggerDatas[j].TriggerType == TriggerType.PayBattleMp)
                {
                    if ( battlerInfo.PayBattleMp >= triggerDatas[j].Param1)
                    {
                        IsTriggered = true;
                    }
                }
                if (triggerTiming == TriggerTiming.After && triggerDatas[j].TriggerType == TriggerType.ActionResultDeath)
                {
                    List<ActionResultInfo> actionResultInfos = CurrentActionInfo().ActionResults;
                    if (actionResultInfos.Find(a => BattlerActors().Find(b => a.DeadIndexList.Contains(b.Index)) != null) != null)
                    {
                        IsTriggered = true;
                    }
                }
                if (triggerDatas[j].TriggerType == TriggerType.DeadWithoutSelf)
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
                if (triggerDatas[j].TriggerType == TriggerType.SelfDead)
                {
                    if (CurrentActionInfo().ActionResults.Find(a => a.DeadIndexList.Contains(battlerInfo.Index) == true) != null)
                    {
                        IsTriggered = true;
                        List<StateInfo> stateInfos = battlerInfo.GetStateInfoAll(StateType.Death);
                        for (var i = 0;i < stateInfos.Count;i++){
                            battlerInfo.RemoveState(stateInfos[i]);
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
        return CanUse;
    }

    public List<int> MakeAutoSelectIndex(ActionInfo actionInfo)
    {
        List<int> targetIndexList = MakeActionTarget(actionInfo.Master.Id,actionInfo.SubjectIndex);
        if (targetIndexList.Count == 0)
        {
            return targetIndexList;
        }
        List<int> indexList = new List<int>();
        
        int targetRand = 0;
        for (int i = 0;i < targetIndexList.Count;i++)
        {
            BattlerInfo battlerInfo = GetBattlerInfo(targetIndexList[i]);
            targetRand += battlerInfo.TargetRate();
        }
        targetRand = UnityEngine.Random.Range (0,targetRand);
        int targetIndex = -1;
        for (int i = 0;i < targetIndexList.Count;i++)
        {
            BattlerInfo battlerInfo = GetBattlerInfo(targetIndexList[i]);
            targetRand -= battlerInfo.TargetRate();
            if (targetRand <= 0 && targetIndex == -1)
            {
                targetIndex = targetIndexList[i];
            }
        }
        // 挑発
        if (CurrentBattler.IsState(StateType.Substitute))
        {
            int substituteId = CurrentBattler.GetStateInfoAll(StateType.Substitute)[0].BattlerId;
            if (targetIndexList.Contains(substituteId))
            {
                targetIndex = substituteId;
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
            indexList.Add (targetIndex);
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
        return indexList;
    }

    public int MakeAutoSkillId(BattlerInfo battlerInfo)
    {
        List<SkillInfo> skillInfos = battlerInfo.Skills.FindAll(a => CheckCanUse(a,battlerInfo) && a.Master.SkillType != SkillType.None);
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
        int troopId = CurrentTroopInfo().TroopId;
        return troopId >= 100 && troopId < 2000;
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
    }

}