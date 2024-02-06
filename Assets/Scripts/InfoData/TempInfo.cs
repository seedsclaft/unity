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
    
    private List<ActorInfo> _tempRecordActors = new ();
    public List<ActorInfo> TempRecordActors => _tempRecordActors;
    public void SetRecordActors(List<ActorInfo> actorInfos)
    {
        _tempRecordActors.Clear();
        var tempRecordActors = new List<ActorInfo>();
        foreach (var actorInfo in actorInfos)
        {
            var recordActorInfo = new ActorInfo(actorInfo.Master);			
            for (int i = 0;i < actorInfo.Master.LearningSkills.Count;i++)
			{
				var _learningData = actorInfo.Master.LearningSkills[i];
				if (recordActorInfo.Skills.Find(a =>a.Id == _learningData.SkillId) != null) continue;
				var skillInfo = new SkillInfo(_learningData.SkillId);
				skillInfo.SetLearningState(LearningState.Learned);
				recordActorInfo.Skills.Add(skillInfo);
			}
            if (actorInfo.Level > 1)
            {
                var statusInfo = recordActorInfo.LevelUp(actorInfo.Level-2);
                actorInfo.TempStatus.SetParameter(
                    statusInfo.Hp,
                    statusInfo.Mp,
                    statusInfo.Atk,
                    statusInfo.Def,
                    statusInfo.Spd
                );
                recordActorInfo.DecideStrength(0);
            }
			recordActorInfo.ChangeHp(actorInfo.CurrentHp);
			recordActorInfo.ChangeMp(actorInfo.CurrentMp);
			recordActorInfo.ChangeLost(actorInfo.Lost);
            tempRecordActors.Add(recordActorInfo);
        }
        _tempRecordActors = tempRecordActors;
    }
    public void ClearRecordActors()
    {
        _tempRecordActors.Clear();
    }

    private List<int> _tempRecordAlchemyList = new ();
    public List<int> TempRecordAlchemyList => _tempRecordAlchemyList;
    public void SetRecordAlchemyList(List<int> tempRecordAlchemyList)
    {
        _tempRecordAlchemyList.Clear();
        var alchemyList = new List<int>();
        foreach (var alchemyId in tempRecordAlchemyList)
        {
            alchemyList.Add(alchemyId);
        }
        _tempRecordAlchemyList = alchemyList;
    }
    public void ClearRecordAlchemyList()
    {
        _tempRecordAlchemyList.Clear();
    }
    
    private List<ActorInfo> _tempStatusActorInfos = new ();
    public List<ActorInfo> TempStatusActorInfos => _tempStatusActorInfos;
    public void SetTempStatusActorInfos(List<ActorInfo> tempStatusActorInfos)
    {
        _tempStatusActorInfos = tempStatusActorInfos;
    }
}
