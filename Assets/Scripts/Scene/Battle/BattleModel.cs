using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleModel : BaseModel
{
    private List<BattlerInfo> _battleMembers;
    public List<BattlerInfo> BattleMembers { get {return _battleMembers;}}
    public void InitBattleMembers()
    {
        _battleMembers = new List<BattlerInfo>();
        for (int i = 0;i < CurrentData.Actors.Count ; i++){
            BattlerInfo info = new BattlerInfo(CurrentData.Actors[i],i);
            _battleMembers.Add(info);
        }
        var paramdata = (List<EnemiesData.EnemyData>)CurrentTempData.ParamData;
        for (int i = 0;i < paramdata.Count ; i++){
            BattlerInfo info = new BattlerInfo(paramdata[i],1,CurrentData.Actors.Count+i);
            _battleMembers.Add(info);
        }
        SortBattleMembers();
    }

    public List<BattlerInfo> BattleActors()
    {
        return _battleMembers.FindAll(a => a.isActor == true);
    }

    public List<BattlerInfo> BattleEnemies()
    {
        return _battleMembers.FindAll(a => a.isActor == false);
    }

    private void SortBattleMembers()
    {
        _battleMembers.Sort((a,b) => a.Index - b.Index);
        _battleMembers.Sort((a,b) => a.Ap - b.Ap);
    }
}
