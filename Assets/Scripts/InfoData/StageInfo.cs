using System;
using System.Collections.Generic;

[Serializable]
public class StageInfo
{
    public StageData Master => DataSystem.FindStage(_id);
    private int _id;
    public int Id => _id;
    private int _savedCount = 0;
    public int SavedCount => _savedCount;
    public void GainSaveCount()
    {
        _savedCount++;
    }
    private int _continueCount = 0;
    public int ContinueCount => _continueCount;
    public void GainContinueCount()
    {
        _continueCount++;
    }
    private int _currentTurn;
    public int CurrentTurn => _currentTurn;
    private int _clearCount;
    public int ClearCount => _clearCount;
    public void SetClearCount(int count)
    {
        _clearCount = count;
    }
    private int _troopClearCount;
    public int TroopClearCount => _troopClearCount;

	private List<TroopData> _troopDates = new();
	private List<SymbolInfo> _currentSymbolInfos = new();
	public List<SymbolInfo> CurrentSymbolInfos => _currentSymbolInfos;
	
    private int _currentSeekIndex = -1;
    public int CurrentSeekIndex => _currentSeekIndex;


	private List<int> _clearTroopIds = new ();
    public List<int> ClearTroopIds => _clearTroopIds;
	private List<int> _selectActorIds = new ();
	public List<int> SelectActorIds => _selectActorIds;

    private List<string> _readEventKeys = new ();
    public List<string> ReadEventKeys => _readEventKeys;

    private int _routeSelect = 0;
    public int RouteSelect => _routeSelect;


    private EndingType _endingType = EndingType.C;
    public EndingType EndingType => _endingType;
    public void SetEndingType(EndingType endingType) {_endingType = endingType;}

    private bool _stageClear = false;
    public bool StageClear => _stageClear;
    public void SetStageClear(bool stageClear) {_stageClear = stageClear;}

    private bool _recordStage = false;
    public bool RecordStage => _recordStage;
    public void SetRecordStage(bool recordStage) {_recordStage = recordStage;}

    private int _rebornActorIndex = -1;
    public int RebornActorIndex => _rebornActorIndex;
    public void SetRebornActorIndex(int rebornActorIndex) {_rebornActorIndex = rebornActorIndex;}
    
    private bool _survivalMode = false;
    public bool SurvivalMode => _survivalMode;

    private List<SymbolResultInfo> _symbolRecordList = new ();
    public List<SymbolResultInfo> SymbolRecordList => _symbolRecordList;
    public void SetSymbolResultInfo(SymbolResultInfo symbolResultInfo)
    {
        var findIndex = _symbolRecordList.FindIndex(a => a.IsSameSymbol(symbolResultInfo));
        if (findIndex < 0)
        {
        } else{
            _symbolRecordList.RemoveAt(findIndex);
        }
        _symbolRecordList.Add(symbolResultInfo);
        _symbolRecordList.Sort((a,b) => a.Seek - b.Seek > 0 ? 1 : -1);
    }

    public StageInfo(StageData stageData)
    {
        _id = stageData.Id;
        _currentTurn = 1;
        _troopClearCount = 0;
        _savedCount = 0;
        _clearTroopIds.Clear();
        _symbolRecordList.Clear();
    }

    public void MakeTurnSymbol()
    {
        _currentSymbolInfos.Clear();
        var symbols = Master.StageSymbols.FindAll(a => a.Seek == _currentTurn);
        foreach (var symbol in symbols)
        {
            var symbolInfo = new SymbolInfo(symbol);
            var getItemInfos = new List<GetItemInfo>();
            if (symbol.BattleSymbol == 1){
                if (symbol.Param1 > 0)
                {
                    symbolInfo.SetTroopInfo(BattleTroops(symbol.Param1,symbol.Param2));
                }
                
                var prizeSets = DataSystem.PrizeSets.FindAll(a => a.Id == symbolInfo.TroopInfo.Master.PrizeSetId);
                foreach (var prizeSet in prizeSets)
                {
                    var getItemInfo = new GetItemInfo(prizeSet.GetItem);
                    getItemInfos.Add(getItemInfo);
                }
            } else
            if (symbol.BossSymbol == 1){
                if (symbol.Param1 > 0)
                {
                    symbolInfo.SetTroopInfo(BattleTroops(symbol.Param1,symbol.Param2));
                }
                var prizeSets = DataSystem.PrizeSets.FindAll(a => a.Id == symbolInfo.TroopInfo.Master.PrizeSetId);
                foreach (var prizeSet in prizeSets)
                {
                    var getItemInfo = new GetItemInfo(prizeSet.GetItem);
                    getItemInfos.Add(getItemInfo);
                }
            }
            if (symbol.PrizeSetId > 0)
            {
                var prizeSets = DataSystem.PrizeSets.FindAll(a => a.Id == symbol.PrizeSetId);
                foreach (var prizeSet in prizeSets)
                {
                    var getItemInfo = new GetItemInfo(prizeSet.GetItem);
                    getItemInfos.Add(getItemInfo);
                }
            }
            symbolInfo.MakeGetItemInfos(getItemInfos);
            _currentSymbolInfos.Add(symbolInfo);
        }
        // レコード作成
        for (int i = 0;i < _currentSymbolInfos.Count;i++)
        {
            var record = new SymbolResultInfo(_id,_currentTurn,i,GameSystem.CurrentStageData.Party.Currency);
            var actorInfos = new List<ActorInfo>();
            foreach (var selectActorId in _selectActorIds)
            {
                actorInfos.Add(GameSystem.CurrentStageData.Actors.Find(a => a.ActorId == selectActorId));
            }
            record.SetStartActorInfos(actorInfos);
            record.SetAlchemyIdList(GameSystem.CurrentStageData.Party.AlchemyIdList);
            SetSymbolResultInfo(record);
        }
    }

    public void AddSelectActorId(int actorId)
    {
        _selectActorIds.Add(actorId);
    }

    public void ClearSelectActorId()
    {
        _selectActorIds.Clear();
    }

    public TroopInfo BattleTroops(int troopId,int enemyCount)
    {
        var troopInfo = new TroopInfo(troopId,false);
        troopInfo.MakeEnemyTroopDates(_troopClearCount);
        for (int j = 0;j < enemyCount;j++)
        {
            int rand = new System.Random().Next(1, Master.RandomTroopCount);
            var enemyData = DataSystem.Enemies.Find(a => a.Id == rand);
            var enemy = new BattlerInfo(enemyData,_troopClearCount + 1,j,0,false);
            troopInfo.AddEnemy(enemy);
        }
        troopInfo.MakeGetItemInfos();
        return troopInfo;
    }

    public void SetSeekIndex(int battleIndex)
    {
        _currentSeekIndex = battleIndex;
    }

    public SymbolInfo CurrentSelectSymbol()
    {
        return _currentSymbolInfos[_currentSeekIndex];
    }

    public TroopInfo CurrentTroopInfo()
    {
        return _currentSymbolInfos[_currentSeekIndex].TroopInfo;
    }

    public List<BattlerInfo> CurrentBattleInfos()
    {
        return _currentSymbolInfos[_currentSeekIndex].BattlerInfos();
    }

    
    public void TestTroops(int troopId,int troopLv)
    {
        _currentSymbolInfos.Clear();
        var troopDate = DataSystem.Troops.Find(a => a.TroopId == troopId);
        
        var troopInfo = new TroopInfo(troopDate.TroopId,false);
        for (int i = 0;i < troopDate.TroopEnemies.Count;i++)
        {
            var enemyData = DataSystem.Enemies.Find(a => a.Id == troopDate.TroopEnemies[i].EnemyId);
            bool isBoss = troopDate.TroopEnemies[i].BossFlag;
            var enemy = new BattlerInfo(enemyData,troopDate.TroopEnemies[i].Lv + troopLv - 1,i,troopDate.TroopEnemies[i].Line,isBoss);
            troopInfo.AddEnemy(enemy);
        }
        _currentSeekIndex = 0;
        var symbolInfo = new SymbolInfo();
        symbolInfo.SetTroopInfo(troopInfo);
        _currentSymbolInfos.Add(symbolInfo);
    }

    public void SeekStage()
    {
        _currentTurn++;
        MakeTurnSymbol();
    }

    public void DeSeekStage()
    {
        _currentTurn--;
    }

    public void AddEventReadFlag(string key)
    {
        _readEventKeys.Add(key);
    }

    public void GainTroopClearCount(int value)
    {
        _troopClearCount += value;
    }

    public void AddClearTroopId(int troopId)
    {
        _clearTroopIds.Add(troopId);
    }

    public bool ClearedTroopId(int troopId)
    {
        return _clearTroopIds.Contains(troopId);
    }


    public void SetRouteSelect(int routeSelect)
    {
        _routeSelect = 0;
        if (routeSelect > 0)
        {
            _routeSelect = routeSelect;
        }
    }

    public void SetMoveStageData(StageInfo stageInfo)
    {
        _selectActorIds = stageInfo.SelectActorIds;
        _clearCount = stageInfo.ClearCount;
        _troopClearCount = stageInfo._troopClearCount;
        _routeSelect = stageInfo.RouteSelect;
        _troopDates = stageInfo._troopDates;
        _savedCount = stageInfo._savedCount;
        _continueCount = stageInfo._continueCount;
        _clearTroopIds = stageInfo._clearTroopIds;
        //_readEventKeys = stageInfo._readEventKeys;
        _endingType = stageInfo._endingType;
        _stageClear = stageInfo._stageClear;
        _rebornActorIndex = stageInfo._rebornActorIndex;
        _survivalMode = stageInfo._survivalMode;
    }

    public int SelectActorIdsClassId(int selectIndex)
    {
        if (SelectActorIds.Count > selectIndex)
        {
            var actorId = SelectActorIds[selectIndex];
            return DataSystem.FindActor(actorId).ClassId;
        }
        return 0;
    }

    public void SetSurvivalMode()
    {
        _survivalMode = true;
    }
};