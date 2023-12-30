using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;

public class ResultModel : BaseModel
{
    public List<ListData> ResultCommand()
    {
        var listDates = MakeListData(BaseConfirmCommand(3040,6));
        foreach (var listDate in listDates)
        {
            var data = (SystemData.CommandData)listDate.Data;
            /*
            if (data.Id == 1)
            {
                listDate.SetEnable(false);
            }
            */
        }
        return listDates;
    }

    public string EndingTypeText()
    {
        return DataSystem.System.GetTextData(16080).Text + CurrentStage.EndingType;
    }

    public bool IsNewRecord()
    {
        return TotalEvaluate() > CurrentData.PlayerInfo.GetBestScore(CurrentStage.Id);
    }

    public void ApplyScore()
    {
        CurrentData.PlayerInfo.SetBestScore(CurrentStage.Id,TotalEvaluate());
    }

    public List<ListData> StageEndCommand()
    {
        var listDates = MakeListData(BaseConfirmCommand(16020,6));
        foreach (var listDate in listDates)
        {
            var data = (SystemData.CommandData)listDate.Data;
            /*
            if (data.Id == 1)
            {
                listDate.SetEnable(false);
            }
            */
        }
        return listDates;
    }

    public void SetActors()
    {
        // Party初期化
        PartyInfo.InitActors();
        foreach (var resultMember in ResultMembers())
        {
            PartyInfo.AddActor(resultMember.ActorId);
        }
    }

    // アルカナ新規入手
    public bool CheckIsAlcana()
    {
        return CurrentStage.Master.Alcana;
    }

    public List<ListData> GetAlcanaSkillInfos()
    {
        // ヒロイン
        var actorInfo = EvaluateMembers()[0];
        // Aエンドは3枚　Bエンドは2枚　Cエンドは1枚
        var getCount = 1;
        if (CurrentStage.EndingType == EndingType.B)
        {
            getCount = 2;
        } else
        if (CurrentStage.EndingType == EndingType.A)
        {
            getCount = 3;
        }
        // 候補アルカナ
        var alcanaSkillsDates = DataSystem.Skills.FindAll(a => a.SkillType == SkillType.UseAlcana);
        
        var getAlcanaInfos = new List<SkillInfo>();
        while (getCount > 0)
        {
            var rand = UnityEngine.Random.Range(0,alcanaSkillsDates.Count);
            var randSkillData = alcanaSkillsDates[rand];
            // 既に所有している
            if (CurrentData.AlcanaInfo.OwnAlcanaList.Find(a => a.Id == randSkillData.Id) != null)
            {
                // 重複可能かどうか featureが508魔法入手
                if (randSkillData.FeatureDates.Find(a => a.FeatureType == FeatureType.AddSkillOrCurrency) != null)
                {
                    var addSkillInfo = new SkillInfo(randSkillData.Id);
                    var skills = DataSystem.Skills.FindAll(a => a.Rank == 1 && a.Attribute == randSkillData.Attribute);
                    if (skills.Count > 0)
                    {
                        var skillRand = UnityEngine.Random.Range(0,skills.Count);
                        addSkillInfo.SetParam(skills[skillRand].Id,0,0);
                        addSkillInfo.SetEnable(true);
                        getAlcanaInfos.Add(addSkillInfo);
                        getCount--;
                    }
                }
            } else{
                // 重複判定
                if (getAlcanaInfos.Find(a => a.Id == randSkillData.Id) == null)
                {
                    var uniqueSkillInfo = new SkillInfo(randSkillData.Id);
                    uniqueSkillInfo.SetEnable(true);
                    getAlcanaInfos.Add(uniqueSkillInfo);
                    getCount--;
                }
            }
        }
        return MakeListData(getAlcanaInfos);
    }

    // 転生スキル習得
    public void GetRebornSkills()
    {
        var actorInfo = EvaluateMembers()[0];
        var commandReborn = AddCommandRebornSkill();
        if (commandReborn != null)
        {
            actorInfo.AddRebornSkill(commandReborn);
        }
        actorInfo.AddRebornSkill(AddStatusRebornSkill());
        foreach (var rebornSkill in AddMagicRebornSkill())
        {
            actorInfo.AddRebornSkill(rebornSkill);
        }
        if (CurrentStage.EndingType == global::EndingType.A || CurrentStage.EndingType == global::EndingType.B)
        {
            actorInfo.AddRebornSkill(AddQuestRebornSkill());
        }
        CurrentData.PlayerInfo.AddActorInfo(actorInfo);
    }

    private SkillInfo AddCommandRebornSkill()
    {
        // コマンドLvアップ
        var commandReborn = DataSystem.Skills.FindAll(a => a.FeatureDates.Find(b => b.FeatureType == FeatureType.RebornCommandLvUp) != null);
        var commandRand = UnityEngine.Random.Range(0,commandReborn.Count);
        var param2 = 0;
        var rank1 = 0;
        if (CurrentStage.EndingType == global::EndingType.A || CurrentStage.EndingType == global::EndingType.B)
        {
            rank1 = 10;
        }
        var rank2 = 40;
        var rank3 = 60 - rank1;

        var rankRand = UnityEngine.Random.Range(0,rank1 + rank2 + rank3);
        if (rankRand < rank1)
        {
            param2 = 2;
        } else
        if (rankRand < rank2)
        {
            param2 = 1;
        }
        if (param2 == 0)
        {
            return null;
        }
        var skill = new SkillInfo(commandReborn[commandRand].Id);
        skill.SetParam(param2,param2,(commandRand+1));
        return skill;
    }    
    
    private SkillInfo AddStatusRebornSkill()
    {
        var statusReborn = DataSystem.Skills.FindAll(a => a.FeatureDates.Find(b => b.FeatureType == FeatureType.RebornStatusUp) != null);
        var statusRand = UnityEngine.Random.Range(0,statusReborn.Count);
        var param2 = 2;
        var rank1 = 0;
        if (CurrentStage.EndingType == global::EndingType.A || CurrentStage.EndingType == global::EndingType.B)
        {
            rank1 = 10;
        }
        var rank2 = 40;
        var rank3 = 60 - rank1;

        var rankRand = UnityEngine.Random.Range(0,rank1 + rank2 + rank3);
        if (rankRand < rank1)
        {
            param2 = 6;
        } else
        if (rankRand < rank2)
        {
            param2 = 4;
        }
        var skill = new SkillInfo(statusReborn[statusRand].Id);
        skill.SetParam(param2,param2,statusRand);
        return skill;
    }

    private List<SkillInfo> AddMagicRebornSkill()
    {
        var list = new List<SkillInfo>();
        var magicReborn = DataSystem.Skills.Find(a => a.FeatureDates.Find(b => b.FeatureType == FeatureType.RebornAddSkill) != null);
        
        var actorInfo = EvaluateMembers()[0];
        var skills = actorInfo.Skills.FindAll(a => a.Master.Rank == 1 && actorInfo.Master.LearningSkills.Find(b => b.SkillId == a.Master.Id) == null);
        foreach (var skill in skills)
        {
            var rate = 10;
            if ((int)actorInfo.GetAttributeRank()[(int)(skill.Attribute-1)] <= 1)
            {
                rate = 20;
            }
            var skillRand = UnityEngine.Random.Range(0,100);
            if (rate >= skillRand)
            {
                var rebornSkillInfo = new SkillInfo(magicReborn.Id);
                rebornSkillInfo.SetParam(skill.Id,0,skill.Master.Id);
                list.Add(rebornSkillInfo);
            }
        }
        return list;
    }

    private SkillInfo AddQuestRebornSkill()
    {
        var questReborn = DataSystem.Skills.Find(a => a.FeatureDates.Find(b => b.FeatureType == FeatureType.RebornQuest) != null);
        var param2 = 2;
        var rank1 = 0;
        if (CurrentStage.EndingType == global::EndingType.A || CurrentStage.EndingType == global::EndingType.B)
        {
            rank1 = 10;
        }
        var rank2 = 40;
        var rank3 = 60 - rank1;

        var rankRand = UnityEngine.Random.Range(0,rank1 + rank2 + rank3);
        if (rankRand < rank1)
        {
            param2 = 6;
        } else
        if (rankRand < rank2)
        {
            param2 = 4;
        }
        var skill = new SkillInfo(questReborn.Id);
        skill.SetParam(param2,param2,0);
        return skill;
    }

    private int _rebornActorIndex = 0;
    
    public List<ListData> ActorInfos()
    {
        var listDates = MakeListData(CurrentData.PlayerInfo.SaveActorList);
        foreach (var listDate in listDates)
        {
            var actorInfo = (ActorInfo)listDate.Data;
            if (actorInfo.Master.ClassId == DataSystem.Actors.Find(a => a.Id == CurrentStage.SelectActorIds[0]).ClassId)
            {
                listDate.SetEnable(false);
            }
        }
        return listDates;
    }

    public List<int> DisableActorIndexes()
    {
        var list = new List<int>();
        if (ActorInfos().Count > 10)
        {

        }
        var actorListIndexes = new List<int>();
        foreach (var listData in ActorInfos())
        {
            var actorInfo = (ActorInfo)listData.Data;
            if (!actorListIndexes.Contains(actorInfo.ActorId))
            {
                actorListIndexes.Add(actorInfo.ActorId);
            }
        }
        if (actorListIndexes.Count == 2)
        {
            var minSize = ActorInfos().FindAll(a => ((ActorInfo)a.Data).ActorId == actorListIndexes[0]);
            var maxSize = ActorInfos().FindAll(a => ((ActorInfo)a.Data).ActorId == actorListIndexes[1]);
            if (minSize.Count == 1 || maxSize.Count == 1)
            {
                if (minSize.Count == 1)
                {
                    var idx = 0;
                    foreach (var listData in ActorInfos())
                    {
                        var actorInfo = (ActorInfo)listData.Data;
                        if (actorInfo.ActorId == actorListIndexes[0])
                        {
                            list.Add(idx);
                        }
                        idx++;
                    }
                } else
                if (maxSize.Count == 1)
                {
                    var idx = 0;
                    foreach (var listData in ActorInfos())
                    {
                        var actorInfo = (ActorInfo)listData.Data;
                        if (actorInfo.ActorId == actorListIndexes[1])
                        {
                            list.Add(idx);
                        }
                        idx++;
                    }
                }
            }
        }
        return list;
    }

    public ActorInfo RebornActorInfo()
    {
        var actorInfos = ActorInfos();
        if (actorInfos.Count > _rebornActorIndex)
        {
            return (ActorInfo)actorInfos[_rebornActorIndex].Data;
        }
        return null;
    }

    public void SetRebornActorIndex(int index)
    {
        _rebornActorIndex = index;
    }

    public void EraseReborn()
    {
        CurrentData.PlayerInfo.EraseReborn(_rebornActorIndex);
    }
}
