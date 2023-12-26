using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// セーブデータに保存しないデータ類を管理
public class TempInfo
{
    private List<ActorInfo> _tempActorInfos = new ();
    public List<ActorInfo> TempActorInfos => _tempActorInfos;
    private List<SkillInfo> _tempAlcanaSkillInfos = new ();
    public List<SkillInfo> TempAlcanaSkillInfos => _tempAlcanaSkillInfos;
    public void CashBattleActors(List<ActorInfo> actorInfos)
    {
        ClearBattleActors();
        foreach (var actorInfo in actorInfos)
        {
            var tempInfo = new ActorInfo(actorInfo.Master);
            tempInfo.CopyData(actorInfo);
            _tempActorInfos.Add(tempInfo);
        }
    }

    public void ClearBattleActors()
    {
        _tempActorInfos.Clear();
    }

    public void SetAlcanaSkillInfo(List<SkillInfo> skillInfos)
    {
        _tempAlcanaSkillInfos = skillInfos;
    }

    public void ClearAlcana()
    {
        _tempAlcanaSkillInfos.Clear();
    }
}
