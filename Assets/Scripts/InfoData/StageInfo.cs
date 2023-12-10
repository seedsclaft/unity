using System;
using System.Collections.Generic;

[Serializable]
public class StageInfo
{
    public StageData Master {get {return DataSystem.Stages.Find(a => a.Id == _id);}}
    private int _id;
    public int Id => _id;
    private int _turns;
    public int Turns => _turns;
    private int _currentTurn;
    public int CurrentTurn => _currentTurn;
    private int _clearCount;
    public int ClearCount => _clearCount;
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
	private List<int> _selectActorIds = new ();
	public List<int> SelectActorIds => _selectActorIds;

    private List<string> _readEventKeys = new ();
    public List<string> ReadEventKeys => _readEventKeys;

    private int _routeSelect;
    public int RouteSelect => _routeSelect;
    private int _defineBossIndex = 0;
    public int DefineBossIndex => _defineBossIndex;

    private int _randomTroopCount = 15;

    private EndingType _endingType = EndingType.D;
    public EndingType EndingType => _endingType;
    public void SetEndingType(EndingType endingType) {_endingType = endingType;}

    private bool _stageClear = false;
    public bool StageClear => _stageClear;
    public void SetStageClear(bool stageClear) {_stageClear = stageClear;}

    private int _rebornActorIndex = -1;
    public int RebornActorIndex => _rebornActorIndex;
    public void SetRebornActorIndex(int rebornActorIndex) {_rebornActorIndex = rebornActorIndex;}
    public StageInfo(StageData stageInfo)
    {
        _id = stageInfo.Id;
        _turns = stageInfo.Turns;
        _currentTurn = 1;
        _IsSubordinate = false;
        _subordinateValue = 50;
        _troopClearCount = 0;
        _randomTroopCount = stageInfo.RandomTroopCount;
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

    public void GainClearCount()
    {
        _clearCount += 1;
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

    public int DefineTroopId(bool containTutorial)
    {
        var defineTroopId = 0;
        if (_defineBossIndex > 0 && _defineBossIndex < 5)
        {
            defineTroopId = _selectActorIds[_defineBossIndex] * 10 + 2000;
            if (containTutorial)
            {
                var tutorialId = _selectActorIds[_defineBossIndex] * 10 + 1000;
                if (!_clearTroopIds.Contains(tutorialId))
                {
                    defineTroopId = tutorialId;
                }
            }
        } else if (_defineBossIndex == 5)
        {
            defineTroopId = _selectActorIds[0] * 10 + 2000;
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
            troopInfo.MakeEnemyTroopDates(_troopClearCount + 1,_currentTurn);
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
            var troopInfo = new TroopInfo(defineTroopId,true);
            troopInfo.MakeEnemyTroopDates(_troopClearCount + 1,_currentTurn);
            _currentTroopInfos.Add(troopInfo);
        }
    }

    public List<TroopInfo> MakeTutorialTroopData(int selectIndex)
    {
        _currentTroopInfos.Clear();
        var troopInfo = new TroopInfo(selectIndex * 10 + 1000,false);
        troopInfo.MakeEnemyTroopDates(_troopClearCount);
        _currentTroopInfos.Add(troopInfo);
        return _currentTroopInfos;
    }

    public List<TroopInfo> MakeRouteSelectTroopData(int routeSelect)
    {
        _currentTroopInfos.Clear();
        var troopIds = new List<int>();
        foreach (var actorId in _selectActorIds)
        {
            troopIds.Add(actorId * 10 + 3000);
        }
        if (!troopIds.Contains(3140) && !troopIds.Contains(3040))
        {
            troopIds.Add(3040);
        }
        var enemyIds = new List<int>();
        if (routeSelect == 0)
        {
            enemyIds.Add(4010);
        } else
        if (routeSelect >= 1)
        {
            if (routeSelect == 1)
            {
                var idx = 0;
                foreach (var actorId in _selectActorIds)
                {
                    if (idx < 3)
                    {
                        enemyIds.Add(actorId * 10 + 3000);
                    }
                    idx++;
                }
            } else
            if (routeSelect == 2)
            {
                foreach (var troopId in troopIds)
                {
                    if (troopId % 1000 == 40)
                    {
                        enemyIds.Add(troopId);
                    }
                }
                var notEncountId = routeSelect == 1 ? 40 : 0;
                while (enemyIds.Count <= 2)
                {
                    int rand = new Random().Next(1, 14);
                    rand *= 10;
                    rand += 3000;
                    if (rand % 1000 != notEncountId)
                    {
                        if (DataSystem.Troops.Find(a => a.TroopId == rand) != null)
                        {
                            if (!enemyIds.Contains(rand) && troopIds.Contains(rand))
                            {
                                enemyIds.Add(rand);
                            }
                        }
                    }
                }
            }
        }
        
        int lv = 15;
        for (int i = 0;i < enemyIds.Count;i++)
        {
            if (_clearTroopIds.Contains(enemyIds[i])) continue;
            var troopInfo = new TroopInfo(enemyIds[i],false);
            troopInfo.MakeEnemyTroopDates(lv);
            troopInfo.MakeGetItemInfos();
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
        
        var troopInfo = new TroopInfo(troopDates[0].TroopId,true);
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

    public void ChangeSubordinate(bool isSubordinate)
    {
        _IsSubordinate = isSubordinate;
    }

    public void ChangeSubordinate(int value)
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

    public void SetRouteSelect(int routeSelect)
    {
        _routeSelect = routeSelect;
    }

    public void SetMoveStageData(StageInfo stageInfo)
    {
        _selectActorIds = stageInfo.SelectActorIds;
        _clearCount = stageInfo.ClearCount;
        _troopClearCount = stageInfo._troopClearCount;
        _routeSelect = stageInfo.RouteSelect;
        _defineBossIndex = stageInfo.DefineBossIndex;
        _troopDates = stageInfo._troopDates;
        _currentTroopInfos = stageInfo._currentTroopInfos;
    }

    public bool IsBendGameClear()
    {
        var BendClear = true;
        var idx = 0;
        foreach (var actorId in _selectActorIds)
        {
            if (idx < 3)
            {
                if (!_clearTroopIds.Contains(actorId * 100 + 2000))
                {
                    BendClear = false;
                }
            }
            idx++;
        }
        return BendClear;
    }

    public int SelectActorIdsClassId(int selectIndex)
    {
        var actorId = SelectActorIds[selectIndex];
        return DataSystem.Actors.Find(a => a.Id == actorId).ClassId;
    }
};