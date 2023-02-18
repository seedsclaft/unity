using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System;
using System.Threading;
using System.Threading.Tasks;
using Effekseer;

public class BattleModel : BaseModel
{
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
            BattlerInfo battlerInfo = new BattlerInfo(Actors()[i],i);
            _battlers.Add(battlerInfo);
        }
        int troopId = 1;
        var enemies = DataSystem.Troops.FindAll(a => a.TroopId == troopId);
        
        for (int i = 0;i < enemies.Count;i++)
        {
            EnemiesData.EnemyData enemyData = DataSystem.Enemies.Find(a => a.Id == enemies[i].EnemyId);
            BattlerInfo battlerInfo = new BattlerInfo(enemyData,enemies[i].Lv,i,enemies[i].Line);
            _battlers.Add(battlerInfo);
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
        MackActionBattler();
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
                BattlerInfo target = _battlers.Find(a => a.Index == stateInfo.TargetIndex);
                if (target != null)
                {
                    target.ChangeHp(stateInfo.Effect * -1);
                    ActionResultInfo actionResultInfo = new ActionResultInfo(_battlers[i].Index,stateInfo.TargetIndex,null);
                    actionResultInfo.HpDamage = stateInfo.Effect;
                    actionResultInfos.Add(actionResultInfo);
                }
            }
        }
        return actionResultInfos;
    }

    public void MackActionBattler()
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
        List<BattlerInfo> battlerInfos = _battlers.FindAll(a => a.IsState(StateType.Chain));
        for (int i = 0; i < _battlers.Count;i++)
        {
            List<StateInfo> stateInfos = _battlers[i].GetStateInfoAll(StateType.Chain);
            for (int j = stateInfos.Count-1; 0 <= j;j--)
            {
                if (stateInfos[j].BattlerId == CurrentBattler.Index)
                {
                    targetIndexs.Add(stateInfos[j].TargetIndex);
                    _battlers[i].RemoveState(stateInfos[j]);
                }
            }
        }
        if (targetIndexs.Count > 0)
        {
            CurrentBattler.GainChainCount(1);
        }
        return targetIndexs;
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
    
    public List<ActorInfo> Actors(){
        return GameSystem.CurrentData.Actors;
    }

    public List<SkillInfo> SkillActionList(AttributeType attributeType)
    {
        if (attributeType != AttributeType.None)
        {
            _currentAttributeType = attributeType;
        }
        List<SkillInfo> skillInfos = CurrentBattler.Skills.FindAll(a => a.Attribute == _currentAttributeType);
        for (int i = 0; i < skillInfos.Count;i++)
        {
            skillInfos[i].SetEnable();
            if (skillInfos[i].Master.MpCost > CurrentBattler.Mp)
            {
                skillInfos[i].SetDisable();
            }
            if (skillInfos[i].Master.SkillType == SkillType.Passive)
            {
                skillInfos[i].SetDisable();
            }
            if (skillInfos[i].Master.SkillType == SkillType.Demigod)
            {
                skillInfos[i].SetDisable();
            }
            if (skillInfos[i].Master.SkillType == SkillType.Awaken)
            {
                if (!CurrentBattler.IsState(StateType.Demigod))
                {
                    skillInfos[i].SetDisable();
                }
            }
        }
        return skillInfos;
    }

    public void ClearActionInfo()
    {
        _actionInfos.Clear();
    }

    public ActionInfo MakeActionInfo(int skillId)
    {
        var skill = DataSystem.Skills.Find(a => a.Id == skillId);
        int LastTargetIndex = -1;
        if (_currentBattler.isActor)
        {
            LastTargetIndex = _currentBattler.LastTargetIndex();
            if (skill.TargetType == TargetType.Opponent)
            {
                if (BattlerEnemies()[LastTargetIndex] == null || BattlerEnemies()[LastTargetIndex].IsAlive() == false)
                {
                    LastTargetIndex = BattlerEnemies().FindIndex(a => a.IsAlive());
                }
            } else
            {
                LastTargetIndex = _currentBattler.Index;
            }
        }
        ActionInfo actionInfo = new ActionInfo(skillId,_currentBattler.Index,LastTargetIndex);
        _actionInfos.Add(actionInfo);
        return actionInfo;
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
        List<BattlerInfo> targetInfos = new List<BattlerInfo>();
        if (actionInfo.Master.TargetType == TargetType.Self)
        {
            if (actionInfo.SubjectIndex < 100)
            {
                targetInfos = BattlerActors();
            } else{
                targetInfos = BattlerEnemies();
            }
        } else
        if (actionInfo.SubjectIndex < 100)
        {
            if (actionInfo.Master.TargetType == TargetType.Opponent)
            {
                targetInfos = BattlerEnemies();
                if (indexList.Count > 0)
                {
                    CurrentBattler.SetLastTargetIndex(indexList[0] - 100);
                }
            } else
            if (actionInfo.Master.TargetType == TargetType.Friend)
            {
                targetInfos = BattlerActors();
            }
        } else
        {
            if (actionInfo.Master.TargetType == TargetType.Opponent)
            {
                targetInfos = BattlerActors();
            } else
            if (actionInfo.Master.TargetType == TargetType.Friend)
            {
                targetInfos = BattlerEnemies();
            }
        }
        int MpCost = actionInfo.Master.MpCost;
        actionInfo.SetMpCost(MpCost);
        List<ActionResultInfo> actionResultInfos = new List<ActionResultInfo>();
        for (int i = 0; i < indexList.Count;i++)
        {
            BattlerInfo Target = targetInfos.Find(a => a.Index == indexList[i]);
            int targetIndex = Target.Index;
            ActionResultInfo actionResultInfo = new ActionResultInfo(CurrentBattler.Index,targetIndex,CurrentActionInfo());
            actionResultInfo.MakeResultData(CurrentBattler,Target);
            actionResultInfos.Add(actionResultInfo);
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
            CurrentBattler.ChangeMp(actionInfo.Master.MpCost * -1);
            List<ActionResultInfo> actionResultInfos = actionInfo.actionResults;
            for (int i = 0; i < actionResultInfos.Count; i++)
            {
                BattlerInfo battlerInfo = _battlers.Find(a => a.Index == actionResultInfos[i].TargetIndex);
                if (actionResultInfos[i].HpDamage != 0)
                {
                    battlerInfo.ChangeHp(-1 * actionResultInfos[i].HpDamage);
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
                BattlerInfo battlerInfo = _battlers.Find(a => a.Index == actionResultInfos[i].TargetIndex);
                if (actionResultInfos[i].IsDead == true)
                {
                    deathBattlerIndex.Add(battlerInfo.Index);
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

    public void CheckTriggerSkillInfos(TriggerTiming triggerTiming)
    {
        List<ActionInfo> actionInfos = new List<ActionInfo>();
        List <SkillInfo> triggeredSkills = CurrentBattler.TriggerdSkillInfos(triggerTiming,CurrentActionInfo());
        if (triggeredSkills.Count > 0)
        {
            for (var i = 0;i < triggeredSkills.Count;i++){
                ActionInfo makeActionInfo = MakeActionInfo(triggeredSkills[i].Id);
                if (triggeredSkills[i].Master.SkillType == SkillType.Demigod){
                    CurrentBattler.SetAwaken();
                }
            }
        }
    }

    public List<int> MakeAutoSelectIndex(ActionInfo actionInfo)
    {
        List<int> indexList = new List<int>();
        List<BattlerInfo> targetInfos = new List<BattlerInfo>();
        if (actionInfo.Master.TargetType == TargetType.Self)
        {
            indexList.Add(CurrentBattler.Index);
        } else
        if (actionInfo.SubjectIndex < 100)
        {
            if (actionInfo.Master.TargetType == TargetType.Opponent)
            {
                targetInfos = BattlerEnemies();
                if (targetInfos.Find(targetInfo => targetInfo.Index == CurrentBattler.LastTargetIndex()) != null)
                {
                    indexList.Add(CurrentBattler.LastTargetIndex());
                }
            } else
            if (actionInfo.Master.TargetType == TargetType.Friend)
            {
                targetInfos = BattlerActors();
            }
        } else
        {
            if (actionInfo.Master.TargetType == TargetType.Opponent)
            {
                targetInfos = BattlerActors();
            } else
            if (actionInfo.Master.TargetType == TargetType.Friend)
            {
                targetInfos = BattlerEnemies();
            }
        }
        if (indexList.Count == 0)
        {
            indexList.Add (targetInfos [UnityEngine.Random.Range (0, targetInfos.Count)].Index);
        }
        return indexList;
    }

    public List<AttributeType> AttributeTypes()
    {
        List<AttributeType> attributeTypes = new List<AttributeType>();
        foreach(var attribute in Enum.GetValues(typeof(AttributeType)))
        {
            if ((int)attribute != 0)
            {
                attributeTypes.Add((AttributeType)attribute);
            }
        } 
        return attributeTypes;
    }


    public List<Sprite> ActorsImage(List<ActorInfo> actors){
        var sprites = new List<Sprite>();
        for (var i = 0;i < actors.Count;i++)
        {
            var actorData = DataSystem.Actors.Find(actor => actor.Id == actors[i].ActorId);
            var asset = Addressables.LoadAssetAsync<Sprite>(
                "Assets/Images/Actors/" + actorData.ImagePath + "/main.png"
            );
            asset.WaitForCompletion();
            sprites.Add(asset.Result);
            Addressables.Release(asset);
        }
        return sprites;
    }
    
    public async Task<List<AudioClip>> BgmData(){
        BGMData bGMData = DataSystem.Data.GetBGM("BATTLE1");
        List<string> data = new List<string>();
        data.Add("BGM/" + bGMData.FileName + "_intro.ogg");
        data.Add("BGM/" + bGMData.FileName + "_loop.ogg");
        
        var result1 = await ResourceSystem.LoadAsset<AudioClip>(data[0]);
        var result2 = await ResourceSystem.LoadAsset<AudioClip>(data[1]);
        return new List<AudioClip>(){
            result1,result2
        };    
    }
}

namespace BattleModelData{
    public class CommandData{
        public string key = "";
        public string name;
    }
}
