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
            var levelBonus = 0;
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
        if (CurrentStage.RecordStage)
        {
            // 魔法習熟度進行
            /*
            foreach (var actorInfo in StageMembers())
            {
                var learningSkills = actorInfo.SeekAlchemy();
                foreach (var learningSkill in learningSkills)
                {
                    var getItemData = new GetItemData();
                    getItemData.Type = GetItemType.LearnSkill;
                    getItemData.Param1 = actorInfo.ActorId;
                    getItemData.Param2 = learningSkill.Id;
                    getItemInfos.Add(new GetItemInfo(getItemData));
                }
            }
            */
            foreach (var getItemInfo in getItemInfos)
            {
                switch (getItemInfo.GetItemType)
                {
                    case GetItemType.Numinous:
                        //PartyInfo.ChangeCurrency(Currency + getItemInfo.Param1);
                        break;
                    case GetItemType.Skill:
                        PartyInfo.AddAlchemy(getItemInfo.Param1);
                        break;
                    case GetItemType.Regeneration:
                        foreach (var stageMember in StageMembers())
                        {
                            if (stageMember.Lost == false)
                            {
                                stageMember.ChangeHp(stageMember.CurrentHp + getItemInfo.Param1);
                                stageMember.ChangeMp(stageMember.CurrentMp + getItemInfo.Param1);
                            }
                        }
                        break;
                    case GetItemType.Demigod:
                        foreach (var stageMember in StageMembers())
                        {
                            if (stageMember.Lost == false)
                            {
                                stageMember.GainDemigod(getItemInfo.Param1);
                            }
                        }
                        break;
                    case GetItemType.StatusUp:
                        foreach (var stageMember in StageMembers())
                        {
                            if (stageMember.Lost == false)
                            {
                                stageMember.TempStatus.AddParameterAll(getItemInfo.Param1);
                                stageMember.DecideStrength(0);
                            }
                        }
                        break;
                    case GetItemType.AddActor:
                        PartyInfo.AddActorId(getItemInfo.Param1);
                        break;
                    case GetItemType.SaveHuman:
                        var record = CurrentStage.SymbolRecordList.Find(a => a.IsSameSymbol(CurrentStage.Id,CurrentStage.CurrentTurn,CurrentSaveData.CurrentStage.CurrentSeekIndex));
                        record.SetBattleScore(PartyInfo.BattleResultScore);
                        break;
                }
            }

            foreach (var actorInfo in TempData.TempRecordActors)
            {
                // 強さ差分を適用
                var recordActor = Actors().Find(a => a.ActorId == actorInfo.ActorId);
                if (recordActor != null)
                {
                    var levelUpCost = recordActor.LevelUpCost;
                    actorInfo.GainLevelUpCost(levelUpCost);
                    var levelUpByCost = actorInfo.LevelUpByCost();
                    if (levelUpByCost > 0)
                    {
                        var statusInfo = actorInfo.LevelUp(levelUpByCost-1);
                        actorInfo.TempStatus.SetParameter(
                            statusInfo.Hp,
                            statusInfo.Mp,
                            statusInfo.Atk,
                            statusInfo.Def,
                            statusInfo.Spd
                        );
                        actorInfo.DecideStrength(0);
                        var getItemData = new GetItemData();
                        var getItemInfo = new GetItemInfo(getItemData);
                        getItemInfo.MakeActorLvUpResult(actorInfo.Master.Name,actorInfo.Level);
                        getItemInfos.Add(getItemInfo);
                    }
                }
            }

            _resultItemInfos = ListData.MakeListData(getItemInfos);
        } else
        {
            // 魔法習熟度進行
            foreach (var actorInfo in StageMembers())
            {
                var learningSkills = actorInfo.SeekAlchemy();
                foreach (var learningSkill in learningSkills)
                {
                    var getItemData = new GetItemData();
                    getItemData.Type = GetItemType.LearnSkill;
                    getItemData.Param1 = actorInfo.ActorId;
                    getItemData.Param2 = learningSkill.Id;
                    getItemInfos.Add(new GetItemInfo(getItemData));
                }
            }
            foreach (var getItemInfo in getItemInfos)
            {
                switch (getItemInfo.GetItemType)
                {
                    case GetItemType.Numinous:
                        PartyInfo.ChangeCurrency(Currency + getItemInfo.Param1);
                        break;
                    case GetItemType.Skill:
                        PartyInfo.AddAlchemy(getItemInfo.Param1);
                        break;
                    case GetItemType.Regeneration:
                        foreach (var stageMember in StageMembers())
                        {
                            if (stageMember.Lost == false)
                            {
                                stageMember.ChangeHp(stageMember.CurrentHp + getItemInfo.Param1);
                                stageMember.ChangeMp(stageMember.CurrentMp + getItemInfo.Param1);
                            }
                        }
                        break;
                    case GetItemType.Demigod:
                        foreach (var stageMember in StageMembers())
                        {
                            if (stageMember.Lost == false)
                            {
                                stageMember.GainDemigod(getItemInfo.Param1);
                            }
                        }
                        break;
                    case GetItemType.StatusUp:
                        foreach (var stageMember in StageMembers())
                        {
                            if (stageMember.Lost == false)
                            {
                                stageMember.TempStatus.AddParameterAll(getItemInfo.Param1);
                                stageMember.DecideStrength(0);
                            }
                        }
                        break;
                    case GetItemType.AddActor:
                        PartyInfo.AddActorId(getItemInfo.Param1);
                        break;
                    case GetItemType.SaveHuman:
                        var record = CurrentStage.SymbolRecordList.Find(a => a.IsSameSymbol(CurrentStage.Id,CurrentStage.CurrentTurn,CurrentSaveData.CurrentStage.CurrentSeekIndex));
                        record.SetBattleScore(PartyInfo.BattleResultScore);
                        break;
                }
            }
            _resultItemInfos = ListData.MakeListData(getItemInfos);
        }
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
    
    public void EndStrategy(bool isSeek)
    {
        // レコード作成
        var record = CurrentStage.SymbolRecordList.Find(a => a.IsSameSymbol(CurrentStage.Id,CurrentStage.CurrentTurn,CurrentSaveData.CurrentStage.CurrentSeekIndex));
        record.SetSelected(true);
        CurrentStage.SetSymbolResultInfo(record);

        foreach (var actorInfo in StageMembers())
        {
            actorInfo.ChangeTacticsCostRate(1);
            actorInfo.ClearTacticsCommand();
        }
        //CurrentStage.ChangeSubordinateValue(-5);
        StageAlcana.SetAlcanaStateInfo(null);
        StageAlcana.ClearCurrentTurnAlcanaList();
        if (isSeek)
        {
            CurrentStage.SeekStage();
            CurrentStage.SetSymbolInfos(CurrentTurnSymbolInfos(CurrentStage.CurrentTurn));
            MakeSymbolResultInfos();
        }
    }

    public void CommitResult()
    {
        CurrentData.PlayerInfo.StageClear(CurrentStage.Id);
        foreach (var symbolResultInfo in CurrentStage.SymbolRecordList)
        {
            PartyInfo.SetSymbolResultInfo(symbolResultInfo);
        }
        SavePlayerData();
        SavePlayerStageData(false);
    }

    public void CommitCurrentResult()
    {
        var beforeRecords = PartyInfo.SymbolRecordList.FindAll(a => a.StageId == CurrentStage.Id && a.Seek == CurrentStage.CurrentTurn);
        var records = CurrentStage.SymbolRecordList.FindAll(a => a.StageId == CurrentStage.Id && a.Seek == CurrentStage.CurrentTurn);
        
        var addAlchemyIdList = new List<int>();
        var removeAlchemyIdList = new List<int>();

        var addActorIdList = new List<int>();
        var removeActorIdList = new List<int>();
        // 差分確認
        foreach (var beforeRecord in beforeRecords)
        {
            var currentRecord = records.Find(a => a.IsSameSymbol(beforeRecord));
            if (beforeRecord.Selected != currentRecord.Selected)
            {
                var symbol = CurrentStage.CurrentSymbolInfos[currentRecord.SeekIndex];
                if (symbol.SymbolType == SymbolType.Alcana)
                {
                    foreach (var getItemInfo in symbol.GetItemInfos)
                    {
                        if (currentRecord.Selected)
                        {
                            // 増える
                            addAlchemyIdList.Add(getItemInfo.Param1);
                        } else
                        {
                            removeAlchemyIdList.Add(getItemInfo.Param1);
                        }
                    }
                }
                if (symbol.SymbolType == SymbolType.Actor)
                {
                    foreach (var getItemInfo in symbol.GetItemInfos)
                    {
                        if (currentRecord.Selected)
                        {
                            // 増える
                            addActorIdList.Add(getItemInfo.Param1);
                        } else
                        {
                            removeActorIdList.Add(getItemInfo.Param1);
                        }
                    }
                }
            }
        }
        foreach (var symbolResultInfo in records)
        {
            PartyInfo.SetSymbolResultInfo(symbolResultInfo);
        }

        PartyInfo.InitActorInfos();
        foreach (var actorInfo in TempData.TempRecordActors)
        {
			PartyInfo.UpdateActorInfo(actorInfo);
        }
        foreach (var actorId in TempData.TempRecordActorIdList)
        {
            PartyInfo.AddActorId(actorId);
        }
        
        PartyInfo.ClearAlchemy();
        foreach (var alchemyId in TempData.TempRecordAlchemyList)
        {
            PartyInfo.AddAlchemy(alchemyId);
        }
        foreach (var alchemyId in addAlchemyIdList)
        {
            PartyInfo.AddAlchemy(alchemyId);
        }
        foreach (var alchemyId in removeAlchemyIdList)
        {
            PartyInfo.RemoveAlchemy(alchemyId);
        }
        foreach (var actorId in addActorIdList)
        {
            PartyInfo.AddActorId(actorId);
        }
        foreach (var actorId in removeActorIdList)
        {
            PartyInfo.RemoveActor(actorId);
        }
        
        // 後のレコードを書き換え
        var afterRecords = PartyInfo.SymbolRecordList.FindAll(a => a.StageId >= CurrentStage.Id && a.Seek > CurrentStage.CurrentTurn);
        foreach (var symbolResultInfo in afterRecords)
        {
            foreach (var alchemyId in addAlchemyIdList)
            {
                symbolResultInfo.AddAlchemyId(alchemyId);
            }
            foreach (var alchemyId in removeAlchemyIdList)
            {
                symbolResultInfo.RemoveAlchemyId(alchemyId);
            }
            foreach (var actorId in addActorIdList)
            {
                symbolResultInfo.AddActorId(actorId);
            }
            foreach (var actorId in removeActorIdList)
            {
                symbolResultInfo.RemoveActorId(actorId);
            }
            PartyInfo.SetSymbolResultInfo(symbolResultInfo);
        }

        TempData.ClearRecordActors();
        TempData.ClearRecordActorIdList();
        TempData.ClearRecordAlchemyList();
        SavePlayerData();
        SavePlayerStageData(false);
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
            PartyInfo.UpdateActorInfo(tempActorInfo);
        }
        TempData.ClearBattleActors();
    }
}
