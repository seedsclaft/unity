using System;
using System.Collections.Generic;

[Serializable]
public class StageInfo
{
    public StageData Master => DataSystem.FindStage(_id);
    private int _baseStageId;
    public int BaseStageId => _baseStageId;
    private int _id;
    public int Id => _id;
    private int _turns;
    public int Turns => _turns;
    private int _displayTurns;
    public int DisplayTurns => _displayTurns;
    public void SetDisplayTurns()
    {
        _displayTurns = Master.Turns;
    }
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
	
    private int _currentBattleIndex = -1;
    public int CurrentBattleIndex => _currentBattleIndex;

    private bool _IsSubordinate;
    public bool IsSubordinate => _IsSubordinate;

    private int _subordinateValue = 0;
    public int SubordinateValue => _subordinateValue;
	private List<int> _clearTroopIds = new ();
    public List<int> ClearTroopIds => _clearTroopIds;
	private List<int> _selectActorIds = new ();
	public List<int> SelectActorIds => _selectActorIds;

    private List<string> _readEventKeys = new ();
    public List<string> ReadEventKeys => _readEventKeys;

    private int _routeSelect = 0;
    public int RouteSelect => _routeSelect;
    private int _defineBossIndex = 0;
    public int DefineBossIndex => _defineBossIndex;


    private EndingType _endingType = EndingType.C;
    public EndingType EndingType => _endingType;
    public void SetEndingType(EndingType endingType) {_endingType = endingType;}

    private bool _stageClear = false;
    public bool StageClear => _stageClear;
    public void SetStageClear(bool stageClear) {_stageClear = stageClear;}

    private int _rebornActorIndex = -1;
    public int RebornActorIndex => _rebornActorIndex;
    public void SetRebornActorIndex(int rebornActorIndex) {_rebornActorIndex = rebornActorIndex;}
    
    readonly int _tutorialEnemyKey = 1000;
    readonly int _defineBossEnemyKey = 2000;
    readonly int _lastBossEnemyKey = 3000;
    private bool _survivalMode = false;
    public bool SurvivalMode => _survivalMode;
    public StageInfo(StageData stageData)
    {
        _id = stageData.Id;
        _baseStageId = stageData.Id;
        _turns = stageData.Turns;
        _displayTurns = stageData.Turns;
        _currentTurn = 1;
        _IsSubordinate = false;
        _subordinateValue = 0;
        _troopClearCount = 0;
        _savedCount = 0;
        _clearTroopIds.Clear();
		MakeTroopData();
        MakeTurnSymbol();
    }

    public void MakeTurnSymbol()
    {
        _currentSymbolInfos.Clear();
        var symbols = Master.StageSymbols.FindAll(a => a.Seek == _currentTurn);
        foreach (var symbol in symbols)
        {
            var symbolInfo = new SymbolInfo();
            if (symbol.BattleSymbol == 1){
                symbolInfo.SetSymbolType(SymbolType.Battle);
                if (symbol.Param1 > 0)
                {
                    symbolInfo.SetTroopInfo(BattleTroops(symbol.Param1,symbol.Param2));
                }
            } else
            if (symbol.BossSymbol == 1){
                symbolInfo.SetSymbolType(SymbolType.Boss);
                if (symbol.Param1 > 0)
                {
                    symbolInfo.SetTroopInfo(BattleTroops(symbol.Param1,symbol.Param2));
                }
            } else
            if (symbol.RecoverSymbol == 1){
                symbolInfo.SetSymbolType(SymbolType.Recover);
            } else
            if (symbol.AlcanaSymbol == 1){
                symbolInfo.SetSymbolType(SymbolType.Alcana);
            } else
            if (symbol.ActorSymbol == 1){
                symbolInfo.SetSymbolType(SymbolType.Actor);
            } else
            if (symbol.ResourceSymbol == 1){
                symbolInfo.SetSymbolType(SymbolType.Resource);
            } else
            if (symbol.RebirthSymbol == 1){
                symbolInfo.SetSymbolType(SymbolType.Rebirth);
            }
            if (symbol.PrizeSetId > 0)
            {
                var getItemInfos = new List<GetItemInfo>();
                var prizeSets = DataSystem.PrizeSets.FindAll(a => a.Id == symbol.PrizeSetId);
                foreach (var prizeSet in prizeSets)
                {
                    var getItemInfo = new GetItemInfo(prizeSet.GetItem);
                    getItemInfos.Add(getItemInfo);
                }
                symbolInfo.MakeGetItemInfos(getItemInfos);
            }
            _currentSymbolInfos.Add(symbolInfo);
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

    // 雑魚敵グループを生成
	private void MakeTroopData()
	{
		for (int i = 1;i < Master.RandomTroopCount;i++)
		{
			var BossTroopData = new TroopData();
			BossTroopData.Id = i;
			BossTroopData.TroopId = i;
			BossTroopData.EnemyId = i;
			BossTroopData.Lv = 1;
			BossTroopData.Line = LineType.Back;
            BossTroopData.BossFlag = true;
            var troopData = DataSystem.Troops.Find(a => a.TroopId == BossTroopData.Id);
            if (troopData != null && troopData.GetItemDates != null)
            {
                BossTroopData.GetItemDates = troopData.GetItemDates;
            }
            _troopDates.Add(BossTroopData);
		}
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


    public int DefineTroopId(bool containTutorial)
    {
        var defineTroopId = 0;
        if (_defineBossIndex > 0 && _defineBossIndex < 5)
        {
            defineTroopId = _selectActorIds[_defineBossIndex] * 10 + _defineBossEnemyKey;
            if (containTutorial)
            {
                var tutorialId = _selectActorIds[_defineBossIndex] * 10 + _tutorialEnemyKey;
                if (!_clearTroopIds.Contains(tutorialId))
                {
                    defineTroopId = tutorialId;
                }
            }
        } else if (_defineBossIndex == 5)
        {
            defineTroopId = _selectActorIds[0] * 10 + _defineBossEnemyKey;
        }
        return defineTroopId;
    }



    public void SetBattleIndex(int battleIndex)
    {
        _currentBattleIndex = battleIndex;
    }

    public SymbolInfo CurrentSelectSymbol()
    {
        return _currentSymbolInfos[_currentBattleIndex];
    }

    public TroopInfo CurrentTroopInfo()
    {
        return _currentSymbolInfos[_currentBattleIndex].TroopInfo;
    }

    public List<BattlerInfo> CurrentBattleInfos()
    {
        return _currentSymbolInfos[_currentBattleIndex].BattlerInfos();
    }

    
    public void TestTroops(int troopId,int troopLv)
    {
        _currentSymbolInfos.Clear();
        var troopDates = DataSystem.Troops.FindAll(a => a.TroopId == troopId);
        
        var troopInfo = new TroopInfo(troopDates[0].TroopId,false);
        for (int i = 0;i < troopDates.Count;i++)
        {
            var enemyData = DataSystem.Enemies.Find(a => a.Id == troopDates[i].EnemyId);
            bool isBoss = troopDates[i].BossFlag;
            var enemy = new BattlerInfo(enemyData,troopDates[i].Lv + troopLv,i,troopDates[i].Line,isBoss);
            troopInfo.AddEnemy(enemy);
        }
        _currentBattleIndex = 0;
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

    public void ChangeSubordinate(bool isSubordinate)
    {
        _IsSubordinate = isSubordinate;
    }

    public void ChangeSubordinateValue(int value)
    {
        if (_IsSubordinate == false) return;
        _subordinateValue += value;
        if (_subordinateValue > 100)
        {
            _subordinateValue = 100;
        }
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

    public void SetDefineBossIndex(int index)
    {
        _defineBossIndex = index;
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
        _baseStageId = stageInfo.BaseStageId;
        _selectActorIds = stageInfo.SelectActorIds;
        _clearCount = stageInfo.ClearCount;
        _troopClearCount = stageInfo._troopClearCount;
        _routeSelect = stageInfo.RouteSelect;
        _defineBossIndex = stageInfo.DefineBossIndex;
        _troopDates = stageInfo._troopDates;
        _displayTurns = stageInfo._displayTurns - stageInfo.CurrentTurn + 1;
        _savedCount = stageInfo._savedCount;
        _continueCount = stageInfo._continueCount;
        _subordinateValue = stageInfo.SubordinateValue;
        _clearTroopIds = stageInfo._clearTroopIds;
        _IsSubordinate = stageInfo.IsSubordinate;
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