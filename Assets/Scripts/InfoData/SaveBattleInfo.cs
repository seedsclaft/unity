using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ryneus
{
	[Serializable]
    public class SaveBattleInfo
    {
        public SaveBattleInfo()
        {
        }
        private string _userName = "";
        public string UserName => _userName;
        public void SetUserName(string userName)
        {
            _userName = userName;
        }

        private string _version = "";
        public string Version => _version;
        public void SetVersion(string version)
        {
            _version = version;
        }

        private int _countIndex = 0;
        private UnitInfo _party;
        public UnitInfo Party => _party;
        public void SetParty(UnitInfo party)
        {
            _party = party;
        }
        private UnitInfo _troop;
        public UnitInfo Troop => _troop;
        public void SetTroop(UnitInfo troop)
        {
            _troop = troop;
        }
        public Dictionary<int,ActionInfo> actionInfos = new ();
        public Dictionary<int,List<ActionResultInfo>> actionResultInfos = new ();


        public void AddActionData(ActionInfo actionInfo)
        {
            actionInfos[_countIndex] = actionInfo;
            _countIndex++;
        }

        public void AddResultData(ActionResultInfo actionResultInfo)
        {
            if (!actionResultInfos.ContainsKey(_countIndex))
            {
                actionResultInfos[_countIndex] = new ();
            }
            actionResultInfos[_countIndex].Add(actionResultInfo);
            _countIndex++;
        }
    }
}
