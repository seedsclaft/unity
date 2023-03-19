using System;
using System.Collections;
using System.Collections.Generic;

[Serializable]
public class StageInfo
{
    private int _id;
    public int Id {get {return _id;}}
    private int _turns;
    public int Turns {get {return _turns;}}
    private int _currentTurn;
    public int CurrentTurn {get {return _currentTurn;}}
    private List<StagesData.StageEventData> _stageEvents;
    public List<StagesData.StageEventData> StageEvents {get {return _stageEvents;}}
    private int _clearCount;
    public int ClearCount {get {return _clearCount;}}

	private List<TroopsData.TroopData> _troopDatas = new List<TroopsData.TroopData>();
	public List<TroopsData.TroopData> TroopDatas { get {return _troopDatas;}}
	private List<TroopInfo> _currentTroopInfos = new List<TroopInfo>();
	
    private int _currentBattleIndex = -1;
    public int CurrentBattleIndex { get {return _currentBattleIndex;}}

    private bool _IsSubordinate;
    public bool IsSubordinate {get {return _IsSubordinate;}}

    private int _subordinateValue = 0;
    public int SubordinateValue {get {return _subordinateValue;}}


	private List<int> _clearTroopIds = new List<int>();
    
	private List<int> _selectActorIds = new List<int>();
	public List<int> SelectActorIds { get {return _selectActorIds;}}

    private bool _IsAlcana;
    public bool IsAlcana {get {return _IsAlcana;}}
	private List<int> _alcanaIds = new List<int>();
    public List<int> AlcanaIds {get {return _alcanaIds;}}
    private AlcanaType _currentAlcanaId;
    private List<int> _ownAlcanaIds = new List<int>();
    public List<int> OwnAlcanaIds {get {return _ownAlcanaIds;}}
    private bool _usedAlcana = false;
    public bool UsedAlcana {get {return _usedAlcana;}}
    private StateInfo _alcanaState = null;
    public StateInfo AlcanaState {get {return _alcanaState;}}

    public StageInfo(StagesData.StageData stageInfo)
    {
        _id = stageInfo.Id;
        _turns = stageInfo.Turns;
        _stageEvents = stageInfo.StageEvents;
        _currentTurn = 1;
        _IsSubordinate = false;
        _IsAlcana = false;
        _ownAlcanaIds.Clear();
        _subordinateValue = 50;
        _usedAlcana = false;
		MakeTroopData();
		MakeAlcanaData();
    }

    public void AddSelectActorId(int actorId)
    {
        _selectActorIds.Add(actorId);
    }

	private void MakeTroopData()
	{
		for (int i = 0;i < DataSystem.Enemies.Count;i++)
		{
			TroopsData.TroopData BossTroopData = new TroopsData.TroopData();
			BossTroopData.Id = i + 1001;
			BossTroopData.TroopId = i + 1001;
			BossTroopData.EnemyId = i + 1;
			BossTroopData.Lv = 1;
			BossTroopData.Line = 1;
            BossTroopData.GetItemDatas = DataSystem.Troops.Find(a => a.TroopId == BossTroopData.Id).GetItemDatas;
			_troopDatas.Add(BossTroopData);
            /*
			for (int j = 0;j < 2;j++)
			{
				TroopsData.TroopData TroopData = new TroopsData.TroopData();
				TroopData.Id = i + 1001;
				TroopData.TroopId = i + 1001;
        		int rand = new System.Random().Next(1, DataSystem.Enemies.Count);
				TroopData.EnemyId = rand;
				TroopData.Lv = 1;
				TroopData.Line = 0;
				_troopDatas.Add(TroopData);
			}
            */
		}
	}

    public List<TroopInfo> TacticsTroops()
    {
        if (_currentTroopInfos.Count > 0) return _currentTroopInfos;
        List<TroopsData.TroopData> troopDatas = _troopDatas.FindAll(a => !_clearTroopIds.Contains(a.Id) && a.Line == 1);
        int max = 2;
        if (troopDatas.Count < 2)
        {
            max = troopDatas.Count;
        }
        List<TroopsData.TroopData> _currentEnemyData = new List<TroopsData.TroopData>();
        while (_currentEnemyData.Count <= max)
        {
            int rand = new Random().Next(0, troopDatas.Count);
            if (!_currentEnemyData.Contains(troopDatas[rand]))
            {
                troopDatas[rand].Lv = _clearTroopIds.Count + 1;
                _currentEnemyData.Add(troopDatas[rand]);
            }
        }
        _currentTroopInfos.Clear();
        for (int i = 0;i < _currentEnemyData.Count;i++)
        {
            TroopInfo troopInfo = new TroopInfo(_currentEnemyData[i].TroopId);
			for (int j = 0;j < 2;j++)
			{
        		int rand = new System.Random().Next(1, DataSystem.Enemies.Count);
                EnemiesData.EnemyData enemyData = DataSystem.Enemies.Find(a => a.Id == rand);
                BattlerInfo enemy = new BattlerInfo(enemyData,1,j,0);
                troopInfo.AddEnemy(enemy);
            }
            EnemiesData.EnemyData bossEnemyData = DataSystem.Enemies.Find(a => a.Id == _currentEnemyData[i].EnemyId);
            BattlerInfo bossEnemy = new BattlerInfo(bossEnemyData,_currentEnemyData[i].Lv,2,_currentEnemyData[i].Line);
            troopInfo.AddEnemy(bossEnemy);
            
            List<GetItemData> getItemDatas = _currentEnemyData[i].GetItemDatas;
            for (int j = 0;j < getItemDatas.Count;j++)
            {
                GetItemInfo getItemInfo = new GetItemInfo(getItemDatas[j]);
                if (getItemDatas[j].Type == GetItemType.Skill)
                {
                    SkillsData.SkillData skillData = DataSystem.Skills.Find(a => a.Id == getItemDatas[j].Param1);
                    getItemInfo.SetResultData(skillData.Name);
                    getItemInfo.SetSkillElementId((int)skillData.Attribute);
                }
                if (getItemDatas[j].Type == GetItemType.Numinous)
                {
                    getItemInfo.SetResultData("+" + getItemDatas[j].Param1.ToString() + DataSystem.System.GetTextData(1000).Text);
                }
                troopInfo.AddGetItemInfo(getItemInfo);
            }
            _currentTroopInfos.Add(troopInfo);
        }
        return _currentTroopInfos;
    }

    public List<TroopInfo> MakeTutorialTroopData(int selectIndex)
    {
        _currentTroopInfos.Clear();
        List<TroopsData.TroopData> troopDatas = DataSystem.Troops.FindAll(a => a.TroopId == selectIndex * 10);
        
        TroopInfo troopInfo = new TroopInfo(troopDatas[0].TroopId);
        for (int i = 0;i < troopDatas.Count;i++)
        {
            EnemiesData.EnemyData enemyData = DataSystem.Enemies.Find(a => a.Id == troopDatas[i].EnemyId);
            BattlerInfo enemy = new BattlerInfo(enemyData,troopDatas[i].Lv,i,troopDatas[i].Line);
            troopInfo.AddEnemy(enemy);
        }
        
        List<GetItemData> getItemDatas = troopDatas.Find(a => a.Line == 1).GetItemDatas;
        for (int j = 0;j < getItemDatas.Count;j++)
        {
            GetItemInfo getItemInfo = new GetItemInfo(getItemDatas[j]);
            if (getItemDatas[j].Type == GetItemType.Skill)
            {
                SkillsData.SkillData skillData = DataSystem.Skills.Find(a => a.Id == getItemDatas[j].Param1);
                getItemInfo.SetResultData(skillData.Name);
                getItemInfo.SetSkillElementId((int)skillData.Attribute);
            }
            if (getItemDatas[j].Type == GetItemType.Numinous)
            {
                getItemInfo.SetResultData("+" + getItemDatas[j].Param1.ToString() + DataSystem.System.GetTextData(1000).Text);
            }
            troopInfo.AddGetItemInfo(getItemInfo);
        }
        _currentTroopInfos.Add(troopInfo);
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

    public void ChengeCurrentTroopLevel(float rate)
    {
        if (_currentTroopInfos.Count == 0) return;
        
        for (int i = 0;i < _currentTroopInfos.Count;i++)
        {
            for (int j = _currentTroopInfos[i].BattlerInfos.Count-1;j >= 0;j--)
            {
                EnemiesData.EnemyData enemyData = _currentTroopInfos[i].BattlerInfos[j].EnemyData;
                int lv = (int)MathF.Floor(_currentTroopInfos[i].BattlerInfos[j].Level * rate);
                int line = _currentTroopInfos[i].BattlerInfos[j].LineIndex;
                BattlerInfo enemy = new BattlerInfo(enemyData,lv,j,line);
                _currentTroopInfos[i].RemoveAtEnemyIndex(_currentTroopInfos[i].BattlerInfos[j].Index);
                _currentTroopInfos[i].AddEnemy(enemy);
            }

        }
    }

    public void ChengeCurrentTroopLine()
    {
        if (_currentTroopInfos.Count == 0) return;
        
        for (int i = 0;i < _currentTroopInfos.Count;i++)
        {
            for (int j = _currentTroopInfos[i].BattlerInfos.Count-1;j >= 0;j--)
            {
                EnemiesData.EnemyData enemyData = _currentTroopInfos[i].BattlerInfos[j].EnemyData;
                int lv = _currentTroopInfos[i].BattlerInfos[j].Level;
                int line = _currentTroopInfos[i].BattlerInfos[j].LineIndex;
                if (line == 0) 
                {
                     line = 1;
                }
                else
                {
                     line = 0;
                }
                BattlerInfo enemy = new BattlerInfo(enemyData,lv,j,line);
                _currentTroopInfos[i].RemoveAtEnemyIndex(_currentTroopInfos[i].BattlerInfos[j].Index);
                _currentTroopInfos[i].AddEnemy(enemy);
            }

        }
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

    public void ChengeCurrentTroopHp(float rate)
    {
        if (_currentTroopInfos.Count == 0) return;
        
        for (int i = 0;i < _currentTroopInfos.Count;i++)
        {
            for (int j = _currentTroopInfos[i].BattlerInfos.Count-1;j >= 0;j--)
            {
                EnemiesData.EnemyData enemyData = _currentTroopInfos[i].BattlerInfos[j].EnemyData;
                int lv = _currentTroopInfos[i].BattlerInfos[j].Level;
                int line = _currentTroopInfos[i].BattlerInfos[j].LineIndex;
                BattlerInfo enemy = new BattlerInfo(enemyData,lv,j,line);
                enemy.GainHp((int)MathF.Floor(enemy.Hp * rate));
                _currentTroopInfos[i].RemoveAtEnemyIndex(_currentTroopInfos[i].BattlerInfos[j].Index);
                _currentTroopInfos[i].AddEnemy(enemy);
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
                _currentTroopInfos[i].BattlerInfos[j].AddState(stateInfo);
            }
        }
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

    public void SetIsAlcana(bool isAlcana)
    {
        _IsAlcana = isAlcana;
    }

	private void MakeAlcanaData()
    {
        _alcanaIds.Clear();
        List<AlcanaData.Alcana> alcana = DataSystem.Alcana;
        while (_alcanaIds.Count < alcana.Count-1)
        {
            int rand = new Random().Next(0, alcana.Count-1);
            if (_alcanaIds.FindIndex(a => a == rand) == -1)
            {
                _alcanaIds.Add(rand);
            }
        }
    }

    public void SetAlacanaState(StateInfo stateInfo)
    {
        _alcanaState = stateInfo;
    }

    public void AddAlcanaId()
    {
        int alcanaId = _alcanaIds[0];
        _alcanaIds.RemoveAt(0);
        _ownAlcanaIds.Add(alcanaId);
    }

    public void OpenAlcana()
    {
        int id = _ownAlcanaIds[0];
        _ownAlcanaIds.RemoveAt(0);
        _currentAlcanaId = (AlcanaType)id;
    }

    public AlcanaData.Alcana CurrentAlcana()
    {
        return DataSystem.Alcana.Find(a => a.Id == (int)_currentAlcanaId);
    }

    public void UseAlcana(bool isUse)
    {
        _usedAlcana = isUse;
    }

    public void DeleteAlcana()
    {
        _currentAlcanaId = (AlcanaType)(-1);
    }

    public void ChangeNextAlcana()
    {
        List<AlcanaData.Alcana> alcana = DataSystem.Alcana;
        int rand = new Random().Next(0, alcana.Count-1);
        _alcanaIds[0] = rand;
    }
};