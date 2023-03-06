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
	private List<TroopsData.TroopData> _currentEnemyData = new List<TroopsData.TroopData>();
	private List<int> _clearTroopIds = new List<int>();
    
	private List<int> _selectActorIds = new List<int>();
	public List<int> SelectActorIds { get {return _selectActorIds;}}

    public StageInfo(StagesData.StageData stageInfo)
    {
        _id = stageInfo.Id;
        _turns = stageInfo.Turns;
        _stageEvents = stageInfo.StageEvents;
        _currentTurn = 1;
		MakeTroopData();
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
		}
	}

    public List<TroopsData.TroopData> TacticsEnemies()
    {
        if (_currentEnemyData.Count > 0) return _currentEnemyData;
        List<TroopsData.TroopData> troopDatas = _troopDatas.FindAll(a => !_clearTroopIds.Contains(a.Id) && a.Line == 1);
        int max = 2;
        if (troopDatas.Count < 2)
        {
            max = troopDatas.Count;
        }
        while (_currentEnemyData.Count <= max)
        {
            int rand = new Random().Next(0, troopDatas.Count);
            if (!_currentEnemyData.Contains(troopDatas[rand]))
            {
                troopDatas[rand].Lv = _currentTurn - 4;
                _currentEnemyData.Add(troopDatas[rand]);
            }
        }     
        return _currentEnemyData;
    }
    
    public void ClearTacticsEnemies()
    {
        _currentEnemyData.Clear();
    }

    public void SeekStage()
    {
        _currentTurn++;
    }
};