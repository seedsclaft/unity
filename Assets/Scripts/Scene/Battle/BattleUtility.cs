using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleUtility
{
    public static List<BattlerInfo> CalcNextBattler(List<BattlerInfo> battlers)
    {
        int ap = battlers[0].Ap;
        for (int i = 0;i < battlers.Count;i++){
            battlers[i].GainAp(-1 * ap);
        }
        return battlers;
    }
}
