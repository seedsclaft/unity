using System;
using System.Collections.Generic;

[Serializable]
public class SaveInfo
{
	private PlayerInfo _playerInfo = null;
    public PlayerInfo PlayerInfo => _playerInfo;

    public SaveInfo()
    {
		_playerInfo = new PlayerInfo();
		_playerInfo.ClearStageClearCount();
		_playerInfo.InitStageClearCount();
	}

	public void SetPlayerName(string name)
	{
		_playerInfo.SetPlayerName(name);
		_playerInfo.SetUserId();
	}
}