using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ryneus
{
	[Serializable]
    public class SaveBattleInfo
    {
        public Dictionary<int,List<ActionResultInfo>> actionResultInfos = new ();

        public void AddData(int index,ActionResultInfo actionResultInfo)
        {
            if (!actionResultInfos.ContainsKey(index))
            {
                actionResultInfos[index].Clear();
                actionResultInfos[index] = new ();
            }
            actionResultInfos[index].Add(actionResultInfo);
        }
    }
}
