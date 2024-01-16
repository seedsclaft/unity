using System.Collections.Generic;
using UnityEngine;

public class StrategyModel : BaseModel
{
    private List<TacticsResultInfo> _resultInfos = new();
    

    private List<ActorInfo> _levelUpData = new();
    public List<ActorInfo> LevelUpData => _levelUpData;
    private List<int> _levelUpBonusActorIds = new();
    public List<ListData> LevelUpActorStatus(int index)
    {
        var list = new List<ListData>();
        var listData = new ListData(_levelUpData[0]);
        list.Add(listData);
        list.Add(listData);
        list.Add(listData);
        list.Add(listData);
        list.Add(listData);
        return list;
    }

    private List<ListData> _resultItemInfos = new();
    public List<ListData> ResultGetItemInfos => _resultItemInfos;

    public List<ActorInfo> TacticsActors()
    {
        return TempData.TempResultActorInfos.FindAll(a => a.InBattle == false);
    }

    public void SetLvUp()
    {
        if (_levelUpData.Count > 0) return;
        var lvUpActorInfos = TacticsActors().FindAll(a => a.TacticsCommandType == TacticsCommandType.Train);
        var lvUpList = new List<ActorInfo>();
        // 結果出力
        foreach (var lvUpActorInfo in lvUpActorInfos)
        {
            var levelBonus = PartyInfo.GetTrainLevelBonusValue();
            var statusInfo = lvUpActorInfo.LevelUp(levelBonus);
            lvUpActorInfo.TempStatus.SetParameter(
                statusInfo.Hp,
                statusInfo.Mp,
                statusInfo.Atk,
                statusInfo.Def,
                statusInfo.Spd
            );
            lvUpList.Add(lvUpActorInfo);
            if (levelBonus > 0)
            {
                _levelUpBonusActorIds.Add(lvUpActorInfo.ActorId);
            }
        }
        _levelUpData = lvUpList;
    }

    public void MakeResult()
    {
        var getItemInfos = TempData.TempGetItemInfos;
        _resultItemInfos = ListData.MakeListData(getItemInfos);
    }

    public void SetLevelUpStatus()
    {
        var actorInfo = _levelUpData[0];
        actorInfo.DecideStrength(0);
        _levelUpData.RemoveAt(0);
    }

    public bool BattleResultVictory()
    {
        return PartyInfo.BattleResultVictory;
    }

    public List<ListData> BattleResultInfos()
    {
        return MakeListData(TempData.TempGetItemInfos);
    }

    public List<ActorInfo> BattleResultActors()
    {
        return TempData.TempResultActorInfos.FindAll(a => a.InBattle);
    }

    public void ClearBattleData(List<ActorInfo> actorInfos)
    {
        foreach (var actorInfo in actorInfos)
        {
            actorInfo.SetNextBattleEnemyIndex(-1,0);
            if (actorInfo.InBattle == true)
            {
                actorInfo.SetInBattle(false);
            }
            actorInfo.ClearTacticsCommand();
        }
    }

    public List<ActorInfo> LostMembers()
    {
        return BattleResultActors().FindAll(a => a.InBattle && a.CurrentHp == 0);
    }

    public List<ListData> ResultCommand()
    {
        return MakeListData(BaseConfirmCommand(3040,5));
    }

    public bool IsBonusTactics(int actorId)
    {
        var result = _resultInfos.Find(a => a.ActorId == actorId);
        if (result != null)
        {
            return result.IsBonus;
        }
        return false;
    }
    
    public void EndStrategy()
    {
        CurrentStage.SeekStage();
        foreach (var actorInfo in StageMembers())
        {
            actorInfo.ChangeTacticsCostRate(1);
            actorInfo.ClearTacticsCommand();
        }
        //CurrentStage.ChangeSubordinateValue(-5);
        StageAlcana.SetAlcanaStateInfo(null);
        StageAlcana.ClearCurrentTurnAlcanaList();
    }

    public bool EnableBattleSkip()
    {
        // スキップ廃止
        return false;
        //return CurrentData.PlayerInfo.EnableBattleSkip(CurrentTroopInfo().TroopId);
    }



    public void ReturnTempBattleMembers()
    {
        foreach (var tempActorInfo in TempData.TempActorInfos)
        {
            tempActorInfo.SetInBattle(false);
            CurrentStageData.UpdateActorInfo(tempActorInfo);
        }
        TempData.ClearBattleActors();
    }
}
