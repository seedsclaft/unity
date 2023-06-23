using System;
using System.Collections;
using System.Collections.Generic;

[Serializable]
public class StageInfo
{
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

	private List<TroopsData.TroopData> _troopDatas = new ();
	public List<TroopsData.TroopData> TroopDatas => _troopDatas;
	private List<TroopInfo> _currentTroopInfos = new ();
	
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

    private int _randomTroopCount = 14;

    private int _baseTroopId = 1000;
    private EndingType _endingType = EndingType.D;
    public EndingType EndingType => _endingType;
    public void SetEndingType(EndingType endingType) {_endingType = endingType;}

    private bool _stageClear = false;
    public bool StageClaer => _stageClear;
    public void SetStageClaer(bool stageClear) {_stageClear = stageClear;}

    public StageInfo(StagesData.StageData stageInfo)
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

    public void GainClearCount()
    {
        _clearCount += 1;
    }

	private void MakeTroopData()
	{
		for (int i = 0;i < _randomTroopCount;i++)
		{
			TroopsData.TroopData BossTroopData = new TroopsData.TroopData();
			BossTroopData.Id = i + _baseTroopId + 1;
			BossTroopData.TroopId = i + _baseTroopId + 1;
			BossTroopData.EnemyId = i + 1;
			BossTroopData.Lv = 1;
			BossTroopData.Line = LineType.Back;
            BossTroopData.BossFlag = true;
            TroopsData.TroopData troopData = DataSystem.Troops.Find(a => a.TroopId == BossTroopData.Id);
            if (troopData != null && troopData.GetItemDatas != null)
            {
                BossTroopData.GetItemDatas = troopData.GetItemDatas;
            }
            _troopDatas.Add(BossTroopData);
		}
	}

    public List<TroopInfo> TacticsTroops()
    {
        if (_currentTroopInfos.Count > 0) return _currentTroopInfos;
        _currentTroopInfos.Clear();
        List<TroopsData.TroopData> troopDatas = _troopDatas.FindAll(a => !_clearTroopIds.Contains(a.Id) && a.Line == LineType.Back);
        int max = 2;
        if (troopDatas.Count < 2)
        {
            max = troopDatas.Count - 1;
        }
        List<TroopsData.TroopData> troopsData = new List<TroopsData.TroopData>();
        while (troopsData.Count <= max)
        {
            int rand = new Random().Next(0, troopDatas.Count);
            if (!troopsData.Contains(troopDatas[rand]))
            {
                troopDatas[rand].Lv = _troopClearCount + 1;
                troopsData.Add(troopDatas[rand]);
            }
        }
        int enemyCount = 2;
        for (int i = 0;i < troopsData.Count;i++)
        {
            TroopInfo troopInfo = new TroopInfo(troopsData[i].TroopId);
			for (int j = 0;j < enemyCount;j++)
			{
        		int rand = new System.Random().Next(0, _randomTroopCount) + 1;
                EnemiesData.EnemyData enemyData = DataSystem.Enemies.Find(a => a.Id == rand);
                BattlerInfo enemy = new BattlerInfo(enemyData,_troopClearCount + 1,j,0,false);
                troopInfo.AddEnemy(enemy);
            }
            troopInfo.MakeEnemyData(troopsData[i],enemyCount,0);
            _currentTroopInfos.Add(troopInfo);
        }

        // 確定中ボス情報
        MakeDefineBossTroop();

        return _currentTroopInfos;
    }

    private void MakeDefineBossTroop()
    {
        int bossTroopId = 0;
        if (_defineBossIndex > 0 && _defineBossIndex < 5)
        {
            bossTroopId = _selectActorIds[_defineBossIndex] * 100;
            int tutorialId = _selectActorIds[_defineBossIndex] * 10;
            if (!_clearTroopIds.Contains(tutorialId))
            {
                bossTroopId = tutorialId;
            }
        } else if (_defineBossIndex == 5)
        {
            bossTroopId = _selectActorIds[0] * 100;
        }
        if (!_clearTroopIds.Contains(bossTroopId))
        {
            List<TroopsData.TroopData> troopDatas = DataSystem.Troops.FindAll(a => a.TroopId == bossTroopId);
            TroopInfo troopInfo = new TroopInfo(bossTroopId);
            for (int i = 0;i < troopDatas.Count;i++)
            {
                troopInfo.MakeEnemyData(troopDatas[i],i,_troopClearCount + 1);
            }

            _currentTroopInfos[_currentTroopInfos.Count-1] = troopInfo;
        }
    }

    public List<TroopInfo> MakeTutorialTroopData(int selectIndex)
    {
        _currentTroopInfos.Clear();
        List<TroopsData.TroopData> troopDatas = DataSystem.Troops.FindAll(a => a.TroopId == selectIndex * 10);
        
        TroopInfo troopInfo = new TroopInfo(selectIndex * 10);
        for (int i = 0;i < troopDatas.Count;i++)
        {
            troopInfo.MakeEnemyData(troopDatas[i],i,_troopClearCount);
        }
        _currentTroopInfos.Add(troopInfo);
        return _currentTroopInfos;
    }

    public List<TroopInfo> MakeRouteSelectTroopData(int routeSelect)
    {
        _currentTroopInfos.Clear();
        var enemyIds = new List<int>();
        if (routeSelect == 0)
        {
            enemyIds.Add(2600);
        } else
        if (routeSelect == 1)
        {
            //enemyIds.Add(2600);
            while (enemyIds.Count <= 2)
            {
                int rand = new Random().Next(1, 6);
                rand *= 100;
                rand += 2000;
                if (rand != 2400)
                {
                    if (!enemyIds.Contains(rand))
                    {
                        enemyIds.Add(rand);
                    }
                }
            }
        } else
        if (routeSelect == 2)
        {
            enemyIds.Add(2400);
            while (enemyIds.Count <= 2)
            {
                int rand = new Random().Next(1, 6);
                rand *= 100;
                rand += 2000;
                if (rand != 2600)
                {
                    if (!enemyIds.Contains(rand))
                    {
                        enemyIds.Add(rand);
                    }
                }
            }
        }
        
        int lv = 15;
        for (int i = 0;i < enemyIds.Count;i++)
        {
            if (_clearTroopIds.Contains(enemyIds[i])) continue;
            TroopInfo troopInfo = new TroopInfo(enemyIds[i]);
            List<TroopsData.TroopData> troopDatas = DataSystem.Troops.FindAll(a => a.TroopId == enemyIds[i]);
            for (int j = 0;j < troopDatas.Count;j++)
            {
                troopInfo.MakeEnemyData(troopDatas[j],j,lv);
            }
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

    public void ChengeCurrentTroopLineZeroErase()
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

    public void ChengeCurrentTroopAddState(StateInfo stateInfo)
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
    
    public void TestTroops(int troopId)
    {
        _currentTroopInfos.Clear();
        List<TroopsData.TroopData> troopDatas = DataSystem.Troops.FindAll(a => a.TroopId == troopId);
        
        TroopInfo troopInfo = new TroopInfo(troopDatas[0].TroopId);
        for (int i = 0;i < troopDatas.Count;i++)
        {
            EnemiesData.EnemyData enemyData = DataSystem.Enemies.Find(a => a.Id == troopDatas[i].EnemyId);
            bool isBoss = troopDatas[i].BossFlag;
            BattlerInfo enemy = new BattlerInfo(enemyData,troopDatas[i].Lv,i,troopDatas[i].Line,isBoss);
            troopInfo.AddEnemy(enemy);
        }
        _currentTroopInfos.Add(troopInfo);
        _currentBattleIndex = 0;
    }

    public void SeekStage()
    {
        _currentTurn++;
    }

    public void SetIsSubordinate(bool isSubordinate)
    {
        _IsSubordinate = isSubordinate;
    }

    public void ChangeSubordinate(int value)
    {
        if (_IsSubordinate == false) return;
        _subordinateValue += value;
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

    public void SetDefineBossIndex(int index)
    {
        _defineBossIndex = index;
        if (_troopClearCount > 0)
        {
            MakeDefineBossTroop();
        }
    }

    public void SetRouteSelect(int routeSelect)
    {
        _routeSelect = routeSelect;
    }

    public void RouteSelectData(StageInfo stageInfo)
    {
        _selectActorIds = stageInfo.SelectActorIds;
        _clearCount = stageInfo.ClearCount;
        _troopClearCount = stageInfo._troopClearCount;
        _routeSelect = stageInfo.RouteSelect;
    }
};