using System;
using System.Collections;
using System.Collections.Generic;

[Serializable]
public class PlayerInfo
{
    private int _playerId = -1;
    public int PlayerId => _playerId;
    public void SetPlayerId()
    {
        if (_playerId == -1)
        {
            int strong = 1000;
		    int sec = (int)DateTime.Now.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
            _playerId = sec + (strong*UnityEngine.Random.Range(0,strong));
        }
    }

    private string _playerName = "";
    public string PlayerName => _playerName;
    public void SetPlayerName(string name)
    {
        _playerName = name;
    }

    private int _bestScore = -1;
    public int BestScore => _bestScore;
    public void SetBestScore(int score)
    {
        if (score > _bestScore)
        {
            _bestScore = score;
        }
    }

    private List<StageInfo> _stages = new ();
    public List<StageInfo> Stages => _stages;
    public void InitStages()
    {
        _stages.Clear();
    }

	public void InitStageInfo()
	{
		for (int i = 0;i < DataSystem.Stages.Count;i++)
		{
			if (!DataSystem.Stages[i].Selectable) continue;
			StageInfo stageInfo = new StageInfo(DataSystem.Stages[i]);
			_stages.Add(stageInfo);
		}
	}

	public void StageClaer(int stageId)
	{
		StageInfo stageInfo = _stages.Find(a => a.Id == stageId);
		stageInfo.GainClearCount();
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
    public List<int> ClearedTroopIds => _clearedTroopIds;


    


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
