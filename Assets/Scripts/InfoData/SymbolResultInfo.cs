using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SymbolResultInfo
{
    private int _stageId;
    public int StageId => _stageId;
    private int _seek;
    public int Seek => _seek;
    private int _seekIndex;
    public int SeekIndex => _seekIndex;

    public int _currency;
    public int Currency => _currency;
    public bool _selected;
    public bool Selected => _selected;
    public void SetSelected(bool isSelected)
    {
        _selected = isSelected;
    }
    public int _battleScore;
    public int BattleScore => _battleScore;
    public void SetBattleScore(int battleScore)
    {
        _battleScore = battleScore;
    }

    public List<ActorInfo> _actorsData = new ();
    public List<ActorInfo> ActorsData => _actorsData;

    public SymbolResultInfo(int stageId,int seek,int seekIndex,int currency)
    {
        _stageId = stageId;
        _seek = seek;
        _seekIndex = seekIndex;
        _currency = currency;
        _selected = false;
    }

    public void SetStartActorInfos(List<ActorInfo> actorInfos)
    {
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
            _actorsData.Add(recordActorInfo);
        }
    }

    public bool IsSameSymbol(SymbolResultInfo symbolResultInfo)
    {
        return symbolResultInfo._stageId == _stageId && symbolResultInfo._seek == _seek && symbolResultInfo._seekIndex == _seekIndex;
    }

    public bool IsSameSymbol(int stageId,int seek,int seekIndex)
    {
        return _stageId == stageId && _seek == seek && _seekIndex == seekIndex;
    }
}
