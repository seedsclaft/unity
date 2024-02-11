using System.Collections.Generic;
using UnityEngine;

public class SymbolRecordModel : BaseModel
{
    public List<ListData> StageSymbolInfos(int seek)
    {
        var symbolInfos = CurrentTurnSymbolInfos(seek+1);
        var symbolRecords = PartyInfo.SymbolRecordList.FindAll(a => a.StageId == CurrentStage.Id && a.Selected == true);
        for (int i = 0;i < symbolInfos.Count;i++)
        {
            var symbolInfo = symbolInfos[i];
            var saveRecord = symbolRecords.Find(a => a.IsSameSymbol(CurrentStage.Id,seek+1,i));
            symbolInfo.SetSelected(saveRecord != null);
            MakePrizeData(saveRecord,symbolInfo.GetItemInfos);
        }
        return MakeListData(symbolInfos);
    }

    public List<ListData> SymbolRecords()
    {
        var symbolRecords = PartyInfo.SymbolRecordList.FindAll(a => a.StageId == CurrentStage.Id && a.Selected == true);
        var symbolInfos = new List<SymbolInfo>();
        
        var stageData = DataSystem.FindStage(CurrentStage.Id);
        foreach (var symbolRecord in symbolRecords)
        {
            var currentSymbolInfos = CurrentTurnSymbolInfos(symbolRecord.Seek);
            var symbolInfo = currentSymbolInfos[symbolRecord.SeekIndex];
            MakePrizeData(symbolRecord,symbolInfo.GetItemInfos);
            symbolInfos.Add(symbolInfo);
        }
        return MakeListData(symbolInfos);
    }

    public List<ActorInfo> SymbolActors(int seek)
    {
        var symbolRecord = PartyInfo.SymbolRecordList.Find(a => a.StageId == CurrentStage.Id && a.Selected == true && a.Seek == seek+1);
        return symbolRecord.ActorInfos;
    }

    public List<int> SymbolAlchemyList(int seek)
    {
        var symbolRecord = PartyInfo.SymbolRecordList.Find(a => a.StageId == CurrentStage.Id && a.Selected == true && a.Seek == seek+1);
        return symbolRecord.AlchemyIdList;
    }
    
    public TroopInfo BattleTroops(int troopId,int enemyCount)
    {
        var troopInfo = new TroopInfo(troopId,false);
        troopInfo.MakeEnemyTroopDates(CurrentStage.ClearCount);
        for (int j = 0;j < enemyCount;j++)
        {
            int rand = new System.Random().Next(1, CurrentStage.Master.RandomTroopCount);
            var enemyData = DataSystem.Enemies.Find(a => a.Id == rand);
            var enemy = new BattlerInfo(enemyData,CurrentStage.ClearCount + 1,j,0,false);
            troopInfo.AddEnemy(enemy);
        }
        troopInfo.MakeGetItemInfos();
        return troopInfo;
    }

    private void MakePrizeData(SymbolResultInfo saveRecord,List<GetItemInfo> getItemInfos)
    {
        foreach (var getItemInfo in getItemInfos)
        {
            if (saveRecord != null && getItemInfo.GetItemType == GetItemType.SaveHuman)
            {
                var rate = (float)saveRecord.BattleScore * 0.01f;
                rate *= getItemInfo.Param1;
                getItemInfo.SetParam2((int)rate);
                getItemInfo.MakeTextData();
            }
        }
    }

    public void MakeSymbolRecordStage(int seek)
    {
        CurrentSaveData.MakeStageData(CurrentStage.Id);
        CurrentStage.SetRecordStage(true);
        TempData.SetRecordActors(PartyInfo.ActorInfos);
        TempData.SetRecordAlchemyList(PartyInfo.AlchemyIdList);
        
        PartyInfo.InitActorInfos();
        foreach (var symbolActor in SymbolActors(seek))
        {
            PartyInfo.AddActorId(symbolActor.ActorId);
            PartyInfo.UpdateActorInfo(symbolActor);
        }

        PartyInfo.ClearAlchemy();
        foreach (var alchemyId in SymbolAlchemyList(seek))
        {
            PartyInfo.AddAlchemy(alchemyId);
        }

        for (int i = 0;i < seek;i++)
        {
            CurrentStage.SeekStage();
        }
        CurrentStage.SetSymbolInfos(CurrentTurnSymbolInfos(CurrentStage.CurrentTurn));
        MakeSymbolResultInfos();
    }
}
