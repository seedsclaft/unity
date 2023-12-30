using System;
using System.Collections.Generic;

[Serializable]
public class SaveInfo
{
	private PlayerInfo _playerInfo = null;
    public PlayerInfo PlayerInfo => _playerInfo;

	private AlcanaInfo _alcanaInfo = null;
    public AlcanaInfo AlcanaInfo => _alcanaInfo;
    public SaveInfo()
    {
		_playerInfo = new PlayerInfo();
		_alcanaInfo = new AlcanaInfo();
	}

	public void SetPlayerName(string name)
	{
		_playerInfo.SetPlayerName(name);
		_playerInfo.SetUserId();
	}
}