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
        get {return Actors()[_currentIndex];}
    }

    private BattlerInfo _currentBattler = null;
    public BattlerInfo CurrentBattler
    {
        get {return _currentBattler;}
    }

    private List<ActionInfo> _actionInfos = new List<ActionInfo>();
    public void CreateBattleData()
    {
        _battlers.Clear();
        for (int i = 0;i < Actors().Count;i++)
        {
            if (Actors()[i].InBattle == true)
            {
                BattlerInfo battlerInfo = new BattlerInfo(Actors()[i],i);
                if (CurrentStage.AlcanaState != null)
                {
                    StateInfo stateInfo = CurrentStage.AlcanaState;
                    battlerInfo.AddState(stateInfo);
                    if (stateInfo.Master.Id == (int)StateType.MaxHpUp)
                    {
                        battlerInfo.GainHp(stateInfo.Effect);
                    }
                    if (stateInfo.Master.Id == (int)StateType.MaxMpUp)
                    {
                        battlerInfo.GainMp(stateInfo.Effect);
                    }
                }
                _battlers.Add(battlerInfo);
            }
        }
        var enemies = CurrentStage.CurrentBattleInfos();
        
        for (int i = 0;i < enemies.Count;i++)
        {
            _battlers.Add(enemies[i]);
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
                BattlerInfo subject = _battlers.Find(a => a.Index == stateInfo.BattlerId);
                BattlerInfo target = _battlers.Find(a => a.Index == stateInfo.TargetIndex);
                if (target != null)
                {
                    int chainDamage = stateInfo.Effect;
                    if (subject.IsState(StateType.ChainDamageUp))
                    {
                        chainDamage += subject.ChainSuccessCount;
                    }
                    target.GainHp(chainDamage * -1);
                    ActionResultInfo actionResultInfo = new ActionResultInfo(_battlers[i].Index,stateInfo.TargetIndex,null);
                    actionResultInfo.HpDamage = chainDamage;
                    if (target.Hp <= 0)
                    {
                        actionResultInfo.IsDead = true;
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
                    foreach (var target in targets)
                    {
                        int healValue = stateInfo.Effect;
                        target.GainHp(healValue * -1);
                        ActionResultInfo actionResultInfo = new ActionResultInfo(_battlers[i].Index,stateInfo.TargetIndex,null);
                        actionResultInfo.HpHeal = healValue;
                        actionResultInfos.Add(actionResultInfo);
                    }
                }
            }
        }
        return actionResultInfos;
    }

    public void SetLastSkill(int skillId)
    {
        CurrentBattler.SetLastSkillIndex(skillId);
    }

    public void MakeActionBattler()
    {
        for (int i = 0;i < _battlers.Count;i++)
        {
            BattlerInfo battlerInfo = _battlers[i];
            if (battlerInfo.Ap <= 0)
            {
                _currentBattler = battlerInfo;
                if (battlerInfo.LastSkill != null)
                {
                    _currentAttributeType = battlerInfo.LastSkill.Attribute;
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
        if (targetIndexs.Count > 0)
        {
            CurrentBattler.GainChainCount(1);
        }
        return targetIndexs;
    }

    public List<StateInfo> CheckChainedBattler(int targetIndex)
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
        if (_currentIndex > Actors().Count-1){
            _currentIndex = 0;
        } else
        if (_currentIndex < 0){
            _currentIndex = Actors().Count-1;
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
            skillInfos[i].SetEnable();
            if (!CheckCanUse(skillInfos[i],CurrentBattler))
            {
                skillInfos[i].SetDisable();
            }
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
        if (skillInfo.CanUseTrigger(battlerInfo,BattlerActors(),BattlerEnemies()) == false)
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
        BattlerInfo subject = _battlers.Find(a => a.Index == subjectIndex);
        
        List<int> targetIndexList = new List<int>();
        if (skill.TargetType == TargetType.All)
        {
            List<BattlerInfo> battlerInfos = _battlers;
            for (int i = 0;i < battlerInfos.Count;i++)
            {
                targetIndexList.Add(battlerInfos[i].Index);
            }
        }
        if (skill.TargetType == TargetType.Self)
        {
            targetIndexList.Add(subject.Index);
        }

        if (subject.isActor)
        {
            if (skill.TargetType == TargetType.Opponent)
            {
                if (skill.Range == RangeType.S)
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
                        List<BattlerInfo> backBattlerInfos = BattlerEnemies().FindAll(a => a.IsAlive() && a.LineIndex == 1);
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
            } else
            if (skill.TargetType == TargetType.Friend)
            {
                List<BattlerInfo> battlerInfos = BattlerActors();
                for (int i = 0;i < battlerInfos.Count;i++)
                {
                    targetIndexList.Add(battlerInfos[i].Index);
                }
            }
        } else
        {
            if (skill.TargetType == TargetType.Opponent)
            {
                if (skill.Range == RangeType.S)
                {
                    // 最前列は
                    bool IsFrontAlive = BattlerEnemies().Find(a => a.IsAlive() && a.LineIndex == 0) != null;
                    if (IsFrontAlive)
                    {
                        if (subject.LineIndex == 0)
                        {
                            List<BattlerInfo> battlerInfos = BattlerActors();
                            for (int i = 0;i < battlerInfos.Count;i++)
                            {
                                targetIndexList.Add(battlerInfos[i].Index);
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
                } else
                {
                    List<BattlerInfo> battlerInfos = BattlerActors();
                    for (int i = 0;i < battlerInfos.Count;i++)
                    {
                        targetIndexList.Add(battlerInfos[i].Index);
                    }
                }
            } else
            if (skill.TargetType == TargetType.Friend)
            {
                List<BattlerInfo> battlerInfos = BattlerEnemies();
                for (int i = 0;i < battlerInfos.Count;i++)
                {
                    targetIndexList.Add(battlerInfos[i].Index);
                }
            }
        }
        if (skill.AliveOnly)
        {
            targetIndexList = targetIndexList.FindAll(a => _battlers.Find(b => a == b.Index).IsAlive());
        } else
        {
            targetIndexList = targetIndexList.FindAll(a => !_battlers.Find(b => a == b.Index).IsAlive());
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
            BattlerInfo Target = _battlers.Find(a => a.Index == indexList[i]);
            ActionResultInfo actionResultInfo = new ActionResultInfo(CurrentBattler.Index,Target.Index,CurrentActionInfo());
            actionResultInfo.MakeResultData(CurrentBattler,Target);
            if (actionResultInfo.HpDamage > 0)
            {
                List<StateInfo> chainStateInfos = CheckChainedBattler(actionResultInfo.TargetIndex);
                if (chainStateInfos.Count > 0)
                {
                    for (int j = 0; j < chainStateInfos.Count;j++)
                    {
                        BattlerInfo chainedBattlerInfo = _battlers.Find(a => a.Index == chainStateInfos[j].TargetIndex);
                        chainedBattlerInfo.RemoveState(chainStateInfos[j]);
                        actionResultInfo.AddRemoveState(chainStateInfos[j]);
                    }
                }
            }
            actionResultInfos.Add(actionResultInfo);
            // 呪い
            List<StateInfo> curseStateInfos = CheckCursedBattler(actionResultInfo.TargetIndex);
            if (actionResultInfo.HpDamage > 0 && curseStateInfos.Count > 0)
            {
                for (int j = 0; j < curseStateInfos.Count;j++)
                {
                    BattlerInfo curseBattlerInfo = _battlers.Find(a => a.Index == curseStateInfos[j].TargetIndex);
                    ActionResultInfo curseActionResultInfo = new ActionResultInfo(curseStateInfos[j].BattlerId,curseBattlerInfo.Index,null);
                    curseActionResultInfo.HpDamage = actionResultInfo.HpDamage;
                    actionResultInfos.Add(curseActionResultInfo);
                    
                    if (curseActionResultInfo.HpDamage >= curseBattlerInfo.Hp)
                    {
                        curseActionResultInfo.AddDeathId(curseBattlerInfo.Index);
                    }
                }
            }
        }
        actionInfo.SetActionResult(actionResultInfos);
    }

    public async Task<EffekseerEffectAsset> SkillActionAnimation(string animationName)
    {
        string path = "Assets/Animations/" + animationName + ".asset";
        var result = await ResourceSystem.LoadAsset<EffekseerEffectAsset>(path);
        return result;
    }

    public void ExecActionResult()
    {
        ActionInfo actionInfo = CurrentActionInfo();
        if (actionInfo != null)
        {
            // Mpの支払い
            CurrentBattler.GainMp(actionInfo.Master.MpCost * -1);
            List<ActionResultInfo> actionResultInfos = actionInfo.actionResults;
            for (int i = 0; i < actionResultInfos.Count; i++)
            {
                BattlerInfo subject = _battlers.Find(a => a.Index == actionResultInfos[i].SubjectIndex);
                BattlerInfo target = _battlers.Find(a => a.Index == actionResultInfos[i].TargetIndex);
                if (actionResultInfos[i].HpDamage != 0)
                {
                    target.GainHp(-1 * actionResultInfos[i].HpDamage);
                }
                if (actionResultInfos[i].HpHeal != 0)
                {
                    target.GainHp(actionResultInfos[i].HpHeal);
                }
                if (actionResultInfos[i].MpHeal != 0)
                {
                    target.GainMp(actionResultInfos[i].MpHeal);
                }
                if (actionResultInfos[i].ApHeal != 0)
                {
                    subject.ChangeAp(actionResultInfos[i].ApHeal * -1);
                }
                if (actionResultInfos[i].ReDamage != 0)
                {
                    subject.GainHp(-1 * actionResultInfos[i].ReDamage);
                }
                if (actionResultInfos[i].ReHeal != 0)
                {
                    subject.GainHp(actionResultInfos[i].ReHeal);
                }
                if (actionResultInfos[i].IsDead)
                {

                }
                foreach (var targetIndex in actionResultInfos[i].ExecStateInfos)
                {
                    BattlerInfo execTarget = _battlers.Find(a => a.Index == targetIndex.Key);
                    if (execTarget != null)
                    {
                        execTarget.UpdateState(RemovalTiming.UpdateCount);
                    }
                }
            }
        }
    }
    
    public List<int> DeathBattlerIndex()
    {
        List<int> deathBattlerIndex = new List<int>();
        ActionInfo actionInfo = CurrentActionInfo();
        if (actionInfo != null)
        {
            List<ActionResultInfo> actionResultInfos = actionInfo.actionResults;
            for (int i = 0; i < actionResultInfos.Count; i++)
            {
                for (int j = 0; j < actionResultInfos[i].DeadIndexList.Count; j++)
                {
                    deathBattlerIndex.Add(actionResultInfos[i].DeadIndexList[j]);
                }
            }
        }
        return deathBattlerIndex;
    }

    public void TurnEnd()
    {
        _actionInfos.RemoveAt(0);
        CurrentBattler.ResetAp(false);
        CurrentBattler.UpdateState(RemovalTiming.UpdateTurn);
        _currentBattler = null;
    }

    public void CheckPlusSkill()
    {
        ActionInfo actionInfo = CurrentActionInfo();
        List<ActionInfo> actionInfos = actionInfo.CheckPlusSkill();
        
        _actionInfos.AddRange(actionInfos);
    }

    public List<BattlerInfo> CheckRegeneBattlers()
    {
        return _battlers.FindAll(a => a.IsState(StateType.Regene));
    }

    public List<ActionResultInfo> UpdateRegeneState()
    {
        List<ActionResultInfo> actionResultInfos = new List<ActionResultInfo>();
        List<BattlerInfo> regeneBattlers = CheckRegeneBattlers();
        for (int i = 0;i < regeneBattlers.Count;i++)
        {
            List<StateInfo> stateInfos = regeneBattlers[i].GetStateInfoAll(StateType.Regene);
            
            for (int j = 0;j < stateInfos.Count;j++)
            {
                regeneBattlers[i].GainHp(stateInfos[j].Effect);
                ActionResultInfo actionResultInfo = new ActionResultInfo(stateInfos[j].BattlerId,stateInfos[j].TargetIndex,null);
                actionResultInfo.HpHeal = stateInfos[j].Effect;
                actionResultInfos.Add(actionResultInfo);
            }
        }
        return actionResultInfos;
    }

    public bool CheckTriggerSkillInfos(TriggerTiming triggerTiming)
    {
        bool IsTriggered = false;
        List<ActionInfo> actionInfos = new List<ActionInfo>();
        List <SkillInfo> triggeredSkills = CurrentBattler.TriggerdSkillInfos(triggerTiming,CurrentActionInfo(),_battlers);
        if (triggeredSkills.Count > 0)
        {
            IsTriggered = true;
            for (var i = 0;i < triggeredSkills.Count;i++)
            {
                ActionInfo makeActionInfo = MakeActionInfo(CurrentBattler,triggeredSkills[i].Id,triggeredSkills[i].Interrupt);
                if (triggeredSkills[i].Master.SkillType == SkillType.Demigod){
                    CurrentBattler.SetAwaken();
                }
            }
        }

        List<ActionResultInfo> actionResultInfos = CurrentActionInfo().actionResults;
        for (var i = 0;i < actionResultInfos.Count;i++)
        {
            BattlerInfo target = _battlers.Find(a => a.Index == actionResultInfos[i].TargetIndex);
            triggeredSkills = target.TriggerdSkillInfos(triggerTiming,CurrentActionInfo(),_battlers);
            if (triggeredSkills.Count > 0)
            {
                IsTriggered = true;
                for (var j = 0;j < triggeredSkills.Count;j++)
                {
                    ActionInfo makeActionInfo = MakeActionInfo(target,triggeredSkills[j].Id,triggeredSkills[j].Interrupt);
                    if (triggeredSkills[j].Master.SkillType == SkillType.Demigod){
                        target.SetAwaken();
                    }
                }
            }
        }

        
        for (var i = 0;i < _battlers.Count;i++)
        {
            BattlerInfo target = _battlers[i];
            triggeredSkills = target.TriggerdSkillInfos(triggerTiming,CurrentActionInfo(),_battlers);
            if (triggeredSkills.Count > 0)
            {
                IsTriggered = true;
                for (var j = 0;j < triggeredSkills.Count;j++)
                {
                    ActionInfo makeActionInfo = MakeActionInfo(target,triggeredSkills[j].Id,triggeredSkills[j].Interrupt);
                    if (triggeredSkills[j].Master.SkillType == SkillType.Demigod){
                        target.SetAwaken();
                    }
                }
            }
        }
        return IsTriggered;
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
            BattlerInfo battlerInfo = _battlers.Find(a => a.Index == targetIndex);
            for (int i = 0;i < _battlers.Count;i++)
            {
                if (battlerInfo.isActor && _battlers[i].isActor)
                {
                    if (battlerInfo.LineIndex == _battlers[i].LineIndex)
                    {
                        indexList.Add (_battlers[i].Index);
                    }
                }
                if (!battlerInfo.isActor && !_battlers[i].isActor)
                {
                    if (battlerInfo.LineIndex == _battlers[i].LineIndex)
                    {
                        indexList.Add (_battlers[i].Index);
                    }
                }
            }
        }
        return indexList;
    }

    public int MakeAutoSkillId(BattlerInfo battlerInfo)
    {
        List<SkillInfo> skillInfos = battlerInfo.Skills.FindAll(a => CheckCanUse(a,battlerInfo) && a.Master.SkillType != SkillType.None);
        
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
        PartyInfo.SetBattleResult(true);
        return BattlerEnemies().Find(a => a.IsAlive()) == null;
    }

    public bool CheckDefeat()
    {
        PartyInfo.SetBattleResult(false);
        return BattlerActors().Find(a => a.IsAlive()) == null;
    }

    public void EndBattle()
    {
        foreach (var battler in BattlerActors())
        {
            ActorInfo actorInfo = battler.ActorInfo;
            actorInfo.ChangeHp(battler.Hp);
            actorInfo.ChangeMp(battler.Mp);
        }
    }
}