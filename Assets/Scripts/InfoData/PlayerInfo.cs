using System;
using System.Collections.Generic;

[Serializable]
public class PlayerInfo
{
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

    public void ClearStageClearCount()
    {
        _stageClearDict.Clear();
    }

	public void InitStageClearCount()
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

    private Dictionary<int, SlotInfo> _saveSlotDict = new ();
    public Dictionary<int, SlotInfo> GetSaveSlotDict()
    {
        InitSlotInfo();
        return _saveSlotDict;
    }
    readonly int _saveSlotCount = 10;
    private List<int> _clearedTroopIds = new ();

    public void AddActorInfo(ActorInfo actorInfo)
    {
        if (_saveActorList == null)
        {
            _saveActorList = new ();
        }
        _saveActorList.Add(actorInfo);
    }

    public void EraseReborn(int index)
    {
        _saveActorList.RemoveAt(index);
    }
    
    private void InitSlotInfo()
    {
        if (_saveSlotDict.Count != 0)
        {
            return;
        }
        _saveSlotDict = new ();
        for (int i = 0;i < _saveSlotCount;i++)
        {
            var slotInfo = new SlotInfo(new List<ActorInfo>(){},0);
            _saveSlotDict[i] = slotInfo;
        }
    }
    
    public void SaveSlotData(List<ActorInfo> actorInfos,int score)
    {
        InitSlotInfo();
        var slotInfo = new SlotInfo(actorInfos,score);
        var index = -1;
        var find = false;
        foreach (var saveSlotDict in _saveSlotDict)
        {
            if (saveSlotDict.Value.ActorInfos.Count == 0)
            {
                find = true;
                index++;
                break;
            }
            index++;
        }
        if (find == false)
        {
            index = -1;
            foreach (var saveSlotDict in _saveSlotDict)
            {
                if (saveSlotDict.Value.IsLocked == false)
                {
                    find = true;
                    index++;
                    break;
                }
                index++;
            }

        }
        slotInfo.SetTimeRecord();
        UpdateSlotInfo(index,slotInfo);
    }

    private void UpdateSlotInfo(int slotId,SlotInfo slotInfo)
    {
        _saveSlotDict[slotId] = slotInfo;
    }

    public bool EnableBattleSkip(int troopId)
    {
        return _clearedTroopIds != null && _clearedTroopIds.Contains(troopId);
    }

    public void AddClearedTroopId(int troopId)
    {
        if (_clearedTroopIds == null)
        {
            _clearedTroopIds = new List<int>();
        }
        if (!EnableBattleSkip(troopId))
        {
            _clearedTroopIds.Add(troopId);
        }
    }
}
