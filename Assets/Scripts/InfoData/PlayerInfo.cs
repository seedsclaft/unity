using System;
using System.Collections;
using System.Collections.Generic;

[Serializable]
public class PlayerInfo
{
    private int _playerId = -1;
    public int PlayerId => _playerId;
    private string _playerName = "";
    public string PlayerName => _playerName;
    private int _bestScore = -1;
    public int BestScore => _bestScore;
    private List<ActorInfo> _saveActorList = new ();
    public List<ActorInfo> SaveActorList => _saveActorList;

    public void SetPlayerName(string name)
    {
        _playerName = name;
    }
    
    public void SetPlayerId()
    {
        if (_playerId == -1)
        {
            int strong = 1000;
		    int sec = (int)DateTime.Now.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
            _playerId = sec + (strong*UnityEngine.Random.Range(0,strong));
        }
    }

    public void SetBestScore(int score)
    {
        if (score > _bestScore)
        {
            _bestScore = score;
        }
    }

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
}
