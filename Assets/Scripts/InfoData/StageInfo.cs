using System;
using System.Collections.Generic;

[Serializable]
public class StageInfo
{
    public StageData Master {get {return DataSystem.Stages.Find(a => a.Id == _id);}}
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
	private List<TroopInfo> _currentTroopInfos = new();
	
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

    private int _randomTroopCount = 15;

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
        _randomTroopCount = stageData.RandomTroopCount;
        _clearTroopIds.Clear();
		MakeTroopData();
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
		for (int i = 1;i < _randomTroopCount;i++)
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

    public List<TroopInfo> TacticsTroops(int stageTurn)
    {
        if (_currentTroopInfos.Count > 0) return _currentTroopInfos; 
        _currentTroopInfos.Clear();
        var troopDates = _troopDates.FindAll(a => !_clearTroopIds.Contains(a.Id) && a.Line == LineType.Back);
        int max = 2;
        if (troopDates.Count < 2)
        {
            max = troopDates.Count - 1;
        }
        var troopsData = new List<TroopData>();
        while (troopsData.Count <= max)
        {
            int rand = new Random().Next(0, troopDates.Count);
            if (!troopsData.Contains(troopDates[rand]))
            {
                troopDates[rand].Lv = _troopClearCount + 1;
                troopsData.Add(troopDates[rand]);
            }
        }
        int enemyCount = 2;
        for (int i = 0;i < troopsData.Count;i++)
        {
            var troopInfo = new TroopInfo(troopsData[i].TroopId,true);
			for (int j = 0;j < enemyCount;j++)
			{
        		int rand = new System.Random().Next(1, _randomTroopCount);
                var enemyData = DataSystem.Enemies.Find(a => a.Id == rand);
                var enemy = new BattlerInfo(enemyData,_troopClearCount + 1,j,0,false);
                troopInfo.AddEnemy(enemy);
            }
            var bossData = DataSystem.Enemies.Find(a => a.Id == troopsData[i].EnemyId);
            var boss = new BattlerInfo(bossData,_troopClearCount + 1,troopInfo.BattlerInfos.Count,troopDates[i].Line,troopDates[i].BossFlag);
            troopInfo.AddEnemy(boss);
            troopInfo.MakeGetItemInfos();
            _currentTroopInfos.Add(troopInfo);
        }

        // 確定中ボス情報
        MakeDefineTroop(true);

        return _currentTroopInfos;
    }

    public List<TroopInfo> SurvivalTroops(int turns)
    {
        if (_currentTroopInfos.Count > 0) return _currentTroopInfos; 
        _currentTroopInfos.Clear();
        var troopDates = new List<TroopData>();
        troopDates.AddRange(DataSystem.Troops.FindAll(a => a.TroopId >= _defineBossEnemyKey && a.TroopId < (_lastBossEnemyKey + 999)));
        var bossDates = troopDates.FindAll(a => a.BossFlag == true);
        var enemyTroopDates = troopDates.FindAll(a => a.BossFlag == false);
        
        int randIndex = new Random().Next(0, bossDates.Count);
        var troopInfo = new TroopInfo(bossDates[randIndex].TroopId,false);

        var bossEnemy = DataSystem.Enemies.Find(a => a.Id == bossDates[randIndex].EnemyId);
        var boss = new BattlerInfo(bossEnemy,bossDates[randIndex].Lv + 30 + turns * 2,0,bossDates[randIndex].Line,true);
        troopInfo.AddEnemy(boss);

        var enemyCount = 0;
        while (enemyCount < 2)
        {  
            int enemyRandIndex = new Random().Next(0, enemyTroopDates.Count); 
            var enemyData = DataSystem.Enemies.Find(a => a.Id == enemyTroopDates[enemyRandIndex].EnemyId);
            var enemy = new BattlerInfo(enemyData,enemyTroopDates[enemyRandIndex].Lv + 30 + turns * 2,enemyCount + 1,enemyTroopDates[enemyRandIndex].Line,false);
            troopInfo.AddEnemy(enemy);
            enemyCount++;
        }
        var getItemData = new GetItemData();
        getItemData.Type = GetItemType.AllRegeneration;
        var getItemInfo = new GetItemInfo(getItemData);
        getItemInfo.SetTitleData(DataSystem.System.GetTextData(3240).Text);
        troopInfo.AddGetItemInfo(getItemInfo);
        _currentTroopInfos.Add(troopInfo);
        return _currentTroopInfos;
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

    private void MakeDefineTroop(bool containTutorial)
    {
        var defineTroopId = DefineTroopId(containTutorial);
        if (!ClearedTroopId(defineTroopId))
        {
            var troopDates = DataSystem.Troops.FindAll(a => a.TroopId == defineTroopId);
            var troopInfo = new TroopInfo(defineTroopId,true);
            troopInfo.MakeEnemyTroopDates(_troopClearCount + 1,_displayTurns);
            _currentTroopInfos[_currentTroopInfos.Count-1] = troopInfo;
        }
    }

    private void MakeBossOnlyTroop()
    {
        _currentTroopInfos.Clear();
        var defineTroopId = DefineTroopId(false);
        if (!ClearedTroopId(defineTroopId))
        {
            var troopDates = DataSystem.Troops.FindAll(a => a.TroopId == defineTroopId);
            var troopInfo = new TroopInfo(defineTroopId,false);
            troopInfo.MakeEnemyTroopDates(_troopClearCount + 1,_displayTurns);
            _currentTroopInfos.Add(troopInfo);
        }
    }

    public void MakeLastBossOnlyTroop(int routeSelect)
    {
        _currentTroopInfos.Clear();
        var bossId = Master.BossId;
        int lv = 20;
        if (routeSelect > 0)
        {
            // アクターに呼応する敵 + 光属性
            var troopIds = new List<int>();
            foreach (var actorId in _selectActorIds)
            {
                var troopId = actorId * 10 + bossId;
                if (!_clearTroopIds.Contains(troopId))
                {
                    troopIds.Add(troopId);
                }
            }
            if (troopIds.FindIndex(a => a % 1000 == 40) == -1)
            {
                var troopId = bossId + 40;
                if (!_clearTroopIds.Contains(troopId))
                {
                    troopIds.Add(troopId);
                }
            }
            var enemyIds = new List<int>();
            foreach (var troopId in troopIds)
            {
                if (troopId % 1000 == 40)
                {
                    // 先頭は光属性
                    enemyIds.Add(troopId);
                }
            }
            bossId = enemyIds[0];
        } else
        {
            lv = 30;
            bossId = bossId + 40;
        }
        var troopInfo = new TroopInfo(bossId,false);
        troopInfo.MakeEnemyTroopDates(lv);
        _currentTroopInfos.Add(troopInfo);
    }

    public List<TroopInfo> MakeTutorialTroopData(int selectIndex)
    {
        _currentTroopInfos.Clear();
        var troopInfo = new TroopInfo(selectIndex * 10 + _tutorialEnemyKey,false);
        troopInfo.MakeEnemyTroopDates(_troopClearCount);
        _currentTroopInfos.Add(troopInfo);
        return _currentTroopInfos;
    }

    public List<TroopInfo> MakeRouteSelectTroopData(int routeSelect)
    {
        _currentTroopInfos.Clear();
        var bossId = Master.BossId;
        int lv = 20;
        if (routeSelect > 0)
        {
            // アクターに呼応する敵 + 光属性
            var troopIds = new List<int>();
            foreach (var actorId in _selectActorIds)
            {
                var troopId = actorId * 10 + bossId;
                if (!_clearTroopIds.Contains(troopId))
                {
                    troopIds.Add(troopId);
                }
            }
            if (troopIds.FindIndex(a => a % 1000 == 40) == -1)
            {
                var troopId = bossId + 40;
                if (!_clearTroopIds.Contains(troopId))
                {
                    troopIds.Add(troopId);
                }
            }
            var enemyIds = new List<int>();
            foreach (var troopId in troopIds)
            {
                if (troopId % 1000 == 40)
                {
                    // 先頭は光属性
                    enemyIds.Add(troopId);
                }
            }

            while (enemyIds.Count < troopIds.Count)
            {
                int randIndex = new Random().Next(0, troopIds.Count);
                if (!enemyIds.Contains(troopIds[randIndex]))
                {
                    enemyIds.Add(troopIds[randIndex]);
                }
            }
            
            for (int i = 0;i < enemyIds.Count;i++)
            {
                if (_currentTroopInfos.Count > 2) continue;
                var troopInfo = new TroopInfo(enemyIds[i],false);
                troopInfo.MakeEnemyTroopDates(lv);
                _currentTroopInfos.Add(troopInfo);
            }
        } else
        {
            lv = 30;
            var troopInfo = new TroopInfo(bossId + 40,false);
            troopInfo.MakeEnemyTroopDates(lv);
            _currentTroopInfos.Add(troopInfo);
        }
        return _currentTroopInfos;
    } 

    public void SetBattleIndex(int battleIndex)
    {
        _currentBattleIndex = battleIndex;
    }

    public TroopInfo CurrentTroopInfo()
    {
        return _currentTroopInfos[_currentBattleIndex];
    }

    public List<BattlerInfo> CurrentBattleInfos()
    {
        return _currentTroopInfos[_currentBattleIndex].BattlerInfos;
    }

    public void ClearTacticsEnemies()
    {
        _currentTroopInfos.Clear();
    }

    public void ChangeCurrentTroopLineZeroErase()
    {
        if (_currentTroopInfos.Count == 0) return;
        
        for (int i = 0;i < _currentTroopInfos.Count;i++)
        {
            for (int j = _currentTroopInfos[i].BattlerInfos.Count-1;j >= 0;j--)
            {
                if (_currentTroopInfos[i].BattlerInfos[j].LineIndex == 0)
                {
                    _currentTroopInfos[i].RemoveAtEnemyIndex(_currentTroopInfos[i].BattlerInfos[j].Index);
                }
            }
        }
    }

    public void ChangeCurrentTroopAddState(StateInfo stateInfo)
    {
        if (_currentTroopInfos.Count == 0) return;
        
        for (int i = 0;i < _currentTroopInfos.Count;i++)
        {
            for (int j = _currentTroopInfos[i].BattlerInfos.Count-1;j >= 0;j--)
            {
                _currentTroopInfos[i].BattlerInfos[j].AddState(stateInfo,true);
            }
        }
    }
    
    public void TestTroops(int troopId,int troopLv)
    {
        _currentTroopInfos.Clear();
        var troopDates = DataSystem.Troops.FindAll(a => a.TroopId == troopId);
        
        var troopInfo = new TroopInfo(troopDates[0].TroopId,false);
        for (int i = 0;i < troopDates.Count;i++)
        {
            var enemyData = DataSystem.Enemies.Find(a => a.Id == troopDates[i].EnemyId);
            bool isBoss = troopDates[i].BossFlag;
            var enemy = new BattlerInfo(enemyData,troopDates[i].Lv + troopLv,i,troopDates[i].Line,isBoss);
            troopInfo.AddEnemy(enemy);
        }
        _currentTroopInfos.Add(troopInfo);
        _currentBattleIndex = 0;
    }

    public void SeekStage()
    {
        _currentTurn++;
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

    public void SetDefineBossOnly(int index)
    {
        _defineBossIndex = index;
        MakeBossOnlyTroop();
    }

    public void SetLastBossOnly()
    {
        MakeLastBossOnlyTroop(_routeSelect);
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
        _currentTroopInfos = stageInfo._currentTroopInfos;
        _displayTurns = stageInfo._displayTurns - stageInfo.CurrentTurn + 1;
        _savedCount = stageInfo._savedCount;
        _subordinateValue = stageInfo.SubordinateValue;
        _IsSubordinate = stageInfo.IsSubordinate;
    }

    public int SelectActorIdsClassId(int selectIndex)
    {
        if (SelectActorIds.Count > selectIndex)
        {
            var actorId = SelectActorIds[selectIndex];
            return DataSystem.Actors.Find(a => a.Id == actorId).ClassId;
        }
        return 0;
    }

    public void SetSurvivalMode()
    {
        _survivalMode = true;
    }
};