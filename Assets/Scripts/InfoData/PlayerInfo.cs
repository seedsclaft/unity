using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PlayerInfo
{
    private string _playerName = "";
    public string PlayerName {get {return _playerName;}}
    public void SetPlayerName(string name)
    {
        _playerName = name;
    }
}
