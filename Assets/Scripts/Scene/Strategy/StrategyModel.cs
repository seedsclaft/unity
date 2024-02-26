using System.Collections.Generic;

public class StrategyModel : BaseModel
{
    private StrategySceneInfo _sceneParam;
    public StrategySceneInfo SceneParam => _sceneParam;
    public StrategyModel()
    {
        _sceneParam = (StrategySceneInfo)GameSystem.SceneStackManager.LastSceneParam;
    }
    public void ClearSceneParam()
    {
        _sceneParam = null;
    }
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
        if (SceneParam != null)
        {
            return SceneParam.ActorInfos.FindAll(a => a.BattleIndex == -1);
        }
        return null;
    }

    public void SetLvUp()
    {
        if (_levelUpData.Count > 0) return;
        if (CurrentStage.RecordStage) return;
        //var lvUpActorInfos = TacticsActors().FindAll(a => a.TacticsCommandType == TacticsCommandType.Train);
        var lvUpList = new List<ActorInfo>();
        // 結果出力
        foreach (var lvUpActorInfo in BattleMembers())
        {
            var statusInfo = lvUpActorInfo.LevelUp(0);
            lvUpActorInfo.TempStatus.SetParameter(
                statusInfo.Hp,
                statusInfo.Mp,
                statusInfo.Atk,
                statusInfo.Def,
                statusInfo.Spd
            );
            lvUpList.Add(lvUpActorInfo);
        }
        _levelUpData = lvUpList;
    }

    public void MakeResult()
    {
        var getItemInfos = SceneParam.GetItemInfos;
        var record = CurrentStage.SymbolRecordList.Find(a => a.IsSameSymbol(CurrentStage.Id,CurrentStage.CurrentTurn,CurrentSaveData.CurrentStage.CurrentSeekIndex));
        var beforeRecord = PartyInfo.SymbolRecordList.Find(a => a.IsSameSymbol(CurrentStage.Id,CurrentStage.CurrentTurn,CurrentSaveData.CurrentStage.CurrentSeekIndex));
        
        foreach (var getItemInfo in getItemInfos)
        {
            switch (getItemInfo.GetItemType)
            {
                case GetItemType.Numinous:
                    if (beforeRecord == null || (beforeRecord != null && beforeRecord.Cleared == false))
                    {
                        PartyInfo.ChangeCurrency(Currency + getItemInfo.Param1);
                    }
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
                    
                    var rate = PartyInfo.BattleResultScore * 0.01f;
                    rate *= getItemInfo.Param1;
                    getItemInfo.SetParam2((int)rate);
                    getItemInfo.MakeTextData();
                    record.SetBattleScore((int)rate);
                    break;
            }
        }
        // クリアフラグを立てる
        record.SetCleared(true); 
        
        if (CurrentStage.RecordStage)
        {
            foreach (var actorInfo in TempInfo.TempRecordActors)
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
        return MakeListData(SceneParam.GetItemInfos);
    }

    public List<ActorInfo> BattleResultActors()
    {
        return SceneParam.ActorInfos.FindAll(a => a.BattleIndex >= 0);
    }

    public void ClearBattleData(List<ActorInfo> actorInfos)
    {
        foreach (var actorInfo in actorInfos)
        {
            if (actorInfo.BattleIndex >= 0)
            {
                actorInfo.SetBattleIndex(-1);
            }
            actorInfo.ClearTacticsCommand();
        }
    }

    public List<ActorInfo> LostMembers()
    {
        return BattleResultActors().FindAll(a => a.BattleIndex >= 0 && a.CurrentHp == 0);
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
        if (isSeek)
        {
            CurrentStage.SeekStage();
            //CurrentStage.SetSymbolInfos(CurrentTurnSymbolInfos(CurrentStage.CurrentTurn));
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

        // データ復元
        records.Sort((a,b)=> a.SeekIndex > b.SeekIndex ? 1 : -1);
        foreach (var symbolResultInfo in records)
        {
            var beforeRecord = beforeRecords.Find(a => a.IsSameSymbol(symbolResultInfo));
            if (beforeRecord != null)
            {
                if (beforeRecord.Cleared)
                {
                    symbolResultInfo.SetCleared(true);
                }
            }
            PartyInfo.SetSymbolResultInfo(symbolResultInfo);
        }

        PartyInfo.InitActorInfos();
        foreach (var actorInfo in TempInfo.TempRecordActors)
        {
			PartyInfo.UpdateActorInfo(actorInfo);
        }
        foreach (var actorId in TempInfo.TempRecordActorIdList)
        {
            PartyInfo.AddActorId(actorId);
        }
        
        PartyInfo.ClearAlchemy();
        foreach (var alchemyId in TempInfo.TempRecordAlchemyList)
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

        TempInfo.ClearRecordActors();
        TempInfo.ClearRecordActorIdList();
        TempInfo.ClearRecordAlchemyList();
        SavePlayerData();
        SavePlayerStageData(false);
    }

    public void CommitCurrentParallelResult()
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
                            //removeAlchemyIdList.Add(getItemInfo.Param1);
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
                            //removeActorIdList.Add(getItemInfo.Param1);
                        }
                    }
                }
            }
        }

        // データ復元
        records.Sort((a,b)=> a.SeekIndex > b.SeekIndex ? 1 : -1);
        foreach (var symbolResultInfo in records)
        {
            var beforeRecord = beforeRecords.Find(a => a.IsSameSymbol(symbolResultInfo));
            if (beforeRecord != null)
            {
                if (beforeRecord.Cleared)
                {
                    symbolResultInfo.SetCleared(true);
                }
                if (beforeRecord.Selected)
                {
                    symbolResultInfo.SetSelected(true);
                }
            }
            PartyInfo.SetSymbolResultInfo(symbolResultInfo);
        }

        PartyInfo.InitActorInfos();
        foreach (var actorInfo in TempInfo.TempRecordActors)
        {
			PartyInfo.UpdateActorInfo(actorInfo);
        }
        foreach (var actorId in TempInfo.TempRecordActorIdList)
        {
            PartyInfo.AddActorId(actorId);
        }
        
        PartyInfo.ClearAlchemy();
        foreach (var alchemyId in TempInfo.TempRecordAlchemyList)
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

        TempInfo.ClearRecordActors();
        TempInfo.ClearRecordActorIdList();
        TempInfo.ClearRecordAlchemyList();
        SavePlayerData();
        SavePlayerStageData(false);
    }

    public bool ChainParallelMode()
    {
        var chain = false;
        var beforeRecords = PartyInfo.SymbolRecordList.FindAll(a => a.StageId == CurrentStage.Id && a.Seek == CurrentStage.CurrentTurn);
        foreach (var record in beforeRecords)
        {
            if (record.Selected == false)
            {
                chain = true;
            }
        }
        return chain;
    }

    public bool EnableBattleSkip()
    {
        // スキップ廃止
        return false;
        //return CurrentData.PlayerInfo.EnableBattleSkip(CurrentTroopInfo().TroopId);
    }



    public void ReturnTempBattleMembers()
    {
        foreach (var tempActorInfo in TempInfo.TempActorInfos)
        {
            tempActorInfo.SetBattleIndex(-1);
            PartyInfo.UpdateActorInfo(tempActorInfo);
        }
        TempInfo.ClearBattleActors();
    }
}

public class StrategySceneInfo
{
    public List<GetItemInfo> GetItemInfos;
    public List<ActorInfo> ActorInfos;
}