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
    private Dictionary<int,List<RankingInfo>> _tempRankingData = new ();
    public Dictionary<int,List<RankingInfo>> TempRankingData => _tempRankingData;
    private bool _tempInputType = false;
    public bool TempInputType => _tempInputType;
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
    
    public void SetRankingInfo(int stageId,List<RankingInfo> rankingInfos)
    {
        _tempRankingData[stageId] = rankingInfos;
    }
    
    public void ClearRankingInfo()
    {
        _tempRankingData.Clear();
    }
    
    public void SetInputType(bool inputType)
    {
        _tempInputType = inputType;
    }    
    
    private List<GetItemInfo> _tempGetItemInfos = new ();
    public List<GetItemInfo> TempGetItemInfos => _tempGetItemInfos;
    public void SetTempGetItemInfos(List<GetItemInfo> tempGetItemInfos)
    {
        _tempGetItemInfos = tempGetItemInfos;
    }
    public void ClearTempGetItemInfos()
    {
        _tempGetItemInfos.Clear();
    }
    private List<ActorInfo> _tempResultActorInfos = new ();
    public List<ActorInfo> TempResultActorInfos => _tempResultActorInfos;
    public void SetTempResultActorInfos(List<ActorInfo> tempResultActorInfos)
    {
        _tempResultActorInfos = tempResultActorInfos;
    }
    public void ClearTempResultActorInfos()
    {
        _tempResultActorInfos.Clear();
    }
}
