using System;
using System.Collections.Generic;

[Serializable]
public class PlayerInfo
{
    public PlayerInfo()
    {
        InitSlotInfo();
		ClearStageClearCount();
		InitStageClearCount();
    }

    private int _userId = -1;
    public int UserId => _userId;
    public void SetUserId()
    {
        if (_userId == -1)
        {
            int strong = 100;
		    int sec = (int)DateTime.Now.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
            _userId = sec + (strong*UnityEngine.Random.Range(0,strong));
        }
    }

    private string _playerName = "";
    public string PlayerName => _playerName;
    public void SetPlayerName(string name)
    {
        _playerName = name;
    }

    private Dictionary<int,int> _bestScore = new ();
    public int GetBestScore(int stageId)
    {
        if (!_bestScore.ContainsKey(stageId))
        {
            return 0;
        }
        return _bestScore[stageId];
    }
    public void SetBestScore(int stageId,int score)
    {
        if (!_bestScore.ContainsKey(stageId))
        {
            _bestScore[stageId] = 0;
        }
        if (score > _bestScore[stageId])
        {
            _bestScore[stageId] = score;
        }
    }

    // クリア情報
    private Dictionary<int,int> _stageClearDict = new ();
    public Dictionary<int,int> StageClearDict => _stageClearDict;
    public int ClearCount(int stageId)
    {
        if (_stageClearDict.ContainsKey(stageId))
        {
            return _stageClearDict[stageId];
        }
        return 0;
    }

    private void ClearStageClearCount()
    {
        _stageClearDict.Clear();
    }

	private void InitStageClearCount()
	{
		for (int i = 0;i < DataSystem.Stages.Count;i++)
		{
			if (!DataSystem.Stages[i].Selectable) continue;
            if (!_stageClearDict.ContainsKey(DataSystem.Stages[i].Id))
            {
			    _stageClearDict[DataSystem.Stages[i].Id] = 0;
            }
		}
	}

	public void StageClear(int stageId)
	{
        if (!_stageClearDict.ContainsKey(stageId))
        {
            _stageClearDict[stageId] = 0;
        }
        _stageClearDict[stageId]++;
	}

    private List<ActorInfo> _saveActorList = new ();
    public List<ActorInfo> SaveActorList => _saveActorList;

    private List<SlotInfo> _slotSaveList = new ();
    public List<SlotInfo> SlotSaveList => _slotSaveList;
    readonly int _slotSaveCount = 3;

    public void AddActorInfo(ActorInfo actorInfo)
    {
        _saveActorList.Add(actorInfo);
    }

    public void EraseReborn(int index)
    {
        _saveActorList.RemoveAt(index);
    }
    
    private void InitSlotInfo()
    {
        _slotSaveList.Clear();
        for (int i = 0;i < _slotSaveCount;i++)
        {
            var slotInfo = new SlotInfo(new List<ActorInfo>(){});
            _slotSaveList.Add(slotInfo);
        }
    }
    
    public void SaveSlotData(int index,SlotInfo slotInfo)
    {
        var saveSlot = new SlotInfo(slotInfo.ActorInfos);
        saveSlot.SetTimeRecord();
        UpdateSlotInfo(index,saveSlot);
    }

    private void UpdateSlotInfo(int slotId,SlotInfo slotInfo)
    {
        _slotSaveList[slotId] = slotInfo;
    }

    // 倒したTroopID
    private List<int> _clearedTroopIds = new ();
    public bool EnableBattleSkip(int troopId)
    {
        return _clearedTroopIds != null && _clearedTroopIds.Contains(troopId);
    }

    public void AddClearedTroopId(int troopId)
    {
        if (!EnableBattleSkip(troopId))
        {
            _clearedTroopIds.Add(troopId);
        }
    }
}
