using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleModel : BaseModel
{
    public List<BattlerInfo> BattleActors()
    {
        List<BattlerInfo> battlers = new List<BattlerInfo>();
        for (int i = 0;i < CurrentData.Actors.Count ; i++){
            BattlerInfo info = new BattlerInfo(CurrentData.Actors[i]);
            battlers.Add(info);
        }
        return battlers;
    }

    public List<BattlerInfo> BattleEnemies()
    {
        List<BattlerInfo> battlers = new List<BattlerInfo>();
        var paramdata = (List<EnemiesData.EnemyData>)CurrentTempData.ParamData;
        for (int i = 0;i < paramdata.Count ; i++){
            BattlerInfo info = new BattlerInfo(paramdata[i],1);
            battlers.Add(info);
        }
        return battlers;
    }
}
