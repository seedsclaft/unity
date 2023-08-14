using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using NCMB;

public class ResultModel : BaseModel
{
    public List<string> GetStageResult()
    {
        return new List<string>();
    }

    public List<SystemData.MenuCommandData> ResultCommand()
    {
        List<SystemData.MenuCommandData> menuCommandDatas = new List<SystemData.MenuCommandData>();
        SystemData.MenuCommandData yesCommand = new SystemData.MenuCommandData();
        yesCommand.Key = "Yes";
        yesCommand.Name = DataSystem.System.GetTextData(6).Text;
        yesCommand.Id = 0;
        menuCommandDatas.Add(yesCommand);
        SystemData.MenuCommandData noCommand = new SystemData.MenuCommandData();
        noCommand.Key = "No";
        noCommand.Name = DataSystem.System.GetTextData(3040).Text;
        noCommand.Id = 1;
        menuCommandDatas.Add(noCommand);
        return menuCommandDatas;
    }

    public string EndingType()
    {
        string endType = "END-";
        endType += CurrentStage.EndingType;
        return endType;
    }

    public int TotalEvaluate()
    {        
        int evaluate = 0;
        foreach (var actorInfo in EvaluateMembers())
        {
            evaluate += actorInfo.Evaluate();
        }
        if (CurrentStage.EndingType == global::EndingType.A)
        {
            evaluate += 1000;
        }
        if (CurrentStage.EndingType == global::EndingType.B)
        {
            evaluate += 500;
        }
        return evaluate;
    }

    public List<int> SelectIdxList()
    {
        var selectIdx = new List<int>();
        foreach (var actorInfo in EvaluateMembers())
        {
            selectIdx.Add(actorInfo.ActorId);
        }
        return selectIdx;
    }

    public List<int> SelectRankList()
    {
        var selectIdrank = new List<int>();
        foreach (var actorInfo in EvaluateMembers())
        {
            selectIdrank.Add(actorInfo.Evaluate());
        }
        return selectIdrank;
    }

    private List<ActorInfo> EvaluateMembers()
    {
        List<int> SelectActorIds = CurrentData.CurrentStage.SelectActorIds;
        var members = new List<ActorInfo>();
        for (int i = 0;i < SelectActorIds.Count ;i++)
        {
            var temp = CurrentData.Actors.Find(a => a.ActorId == SelectActorIds[i]);
            if (temp != null)
            {
                members.Add(temp);
            }
        }
        return members;
    }

    public bool IsNewRecord()
    {
        return TotalEvaluate() > CurrentData.PlayerInfo.BestScore;
    }

    public void ApllyScore()
    {
        CurrentData.PlayerInfo.SetBestScore(TotalEvaluate());
    }

    public List<SystemData.MenuCommandData> StageEndCommand()
    {
        List<SystemData.MenuCommandData> menuCommandDatas = new List<SystemData.MenuCommandData>();
        SystemData.MenuCommandData yesCommand = new SystemData.MenuCommandData();
        yesCommand.Key = "Yes";
        yesCommand.Name = DataSystem.System.GetTextData(6).Text;
        yesCommand.Id = 0;
        menuCommandDatas.Add(yesCommand);
        SystemData.MenuCommandData noCommand = new SystemData.MenuCommandData();
        noCommand.Key = "No";
        noCommand.Name = DataSystem.System.GetTextData(16020).Text;
        noCommand.Id = 1;
        menuCommandDatas.Add(noCommand);
        return menuCommandDatas;
    }

    public void SetActors()
    {
        // Party初期化
        PartyInfo.InitActors();
        for (int i = 0;i < ResultMembers().Count;i++)
        {
            PartyInfo.AddActor(ResultMembers()[i].ActorId);
        }
    }

    public void SendRankingData(System.Action endEvent)
    {
        NCMBObject obj = new NCMBObject("ranking");
            obj["UserId"]  = CurrentData.PlayerInfo.PlayerId.ToString();
            obj["Name"]  = GameSystem.CurrentData.PlayerInfo.PlayerName;
            obj["Score"] = TotalEvaluate();
            obj["SelectIdx"]  = SelectIdxList();
            obj["SelectRank"] = SelectRankList();
            obj.SaveAsync((res) => {
                if (endEvent != null) endEvent();
            });
    }

    public void UpdateRankingData(string objectId,System.Action endEvent)
    {
        NCMBObject obj = new NCMBObject("ranking");
            obj["UserId"]  = CurrentData.PlayerInfo.PlayerId.ToString();
            obj["Name"]  = GameSystem.CurrentData.PlayerInfo.PlayerName;
            obj["Score"] = TotalEvaluate();
            obj["SelectIdx"]  = SelectIdxList();
            obj["SelectRank"] = SelectRankList();
            obj.ObjectId = objectId;
            obj.SaveAsync((res) => {
                if (endEvent != null) endEvent();
            });
    }
    
    public async void GetSelfRankingData(System.Action<string> endEvent)
    {
        int evaluate = TotalEvaluate();

        NCMBQuery<NCMBObject> query = new NCMBQuery<NCMBObject> ("ranking");

        //Scoreフィールドの降順でデータを取得
        query.OrderByDescending ("Score");

        //検索件数を5件に設定
        query.Limit = 100;

        var rank = 1;
        var isEnd = false;
        var count = 0;
        long selfScore = -1;
        long lineScore = -1;
        string objectId = "";
        //データストアでの検索を行う
        query.FindAsync ((List<NCMBObject> objList ,NCMBException e) => {
            if (e != null) {
                //検索失敗時の処理
                isEnd = true;
            } else {
                count = objList.Count;
                //検索成功時の処理
                foreach (NCMBObject obj in objList) {
                    if (isEnd == false)
                    {
                        if ((string)obj["UserId"] == GameSystem.CurrentData.PlayerInfo.PlayerId.ToString())
                        {
                            selfScore = (long)obj["Score"];
                            objectId = obj.ObjectId;
                            isEnd = true;
                        } else
                        {
                            var otherScore = (long)obj["Score"];
                            if (otherScore > evaluate)
                            {
                                rank++;
                            }
                        }
                    }
                    if (obj.ContainsKey("Score"))
                    {
                        lineScore = (long)obj["Score"];
                    }
                }
                isEnd = true;
            }
        });
        await UniTask.WaitUntil(() => isEnd == true);
        if (selfScore > evaluate)
        {
            if (endEvent != null) endEvent("記録更新なし");
        }
        if (evaluate < lineScore && count >= 100)
        {
            if (endEvent != null) endEvent("圏外");
        }

        if (selfScore > -1)
        {
            UpdateRankingData(objectId,() => {
                if (endEvent != null) endEvent(rank.ToString() + "位" + " / " + count.ToString());
            });
        } else
        {
            count += 1;
            SendRankingData(() => {
                if (endEvent != null) endEvent(rank.ToString() + "位" + " / " + count.ToString());
            });
        }

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
        var commandReborn = DataSystem.Skills.FindAll(a => a.FeatureDatas.Find(b => b.FeatureType == FeatureType.RebornCommandLvUp) != null);
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
        skill.SetParam(param2.ToString(),param2,(commandRand+1));
        return skill;
    }    
    
    private SkillInfo AddStatusRebornSkill()
    {
        var statusReborn = DataSystem.Skills.FindAll(a => a.FeatureDatas.Find(b => b.FeatureType == FeatureType.RebornStatusUp) != null);
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
        skill.SetParam(param2.ToString(),param2,statusRand);
        return skill;
    }

    
    private List<SkillInfo> AddMagicRebornSkill()
    {
        var list = new List<SkillInfo>();
        var magicReborn = DataSystem.Skills.Find(a => a.FeatureDatas.Find(b => b.FeatureType == FeatureType.RebornAddSkill) != null);
        
        var actorInfo = EvaluateMembers()[0];
        var skills = actorInfo.Skills.FindAll(a => a.Master.Rank == 1 && actorInfo.Master.LearningSkills.Find(b => b.SkillId == a.Master.Id) == null);
        foreach (var skill in skills)
        {
            var rate = 10;
            if (actorInfo.Attribute[(int)(skill.Attribute-1)] > 80)
            {
                rate = 20;
            }
            var skillRand = UnityEngine.Random.Range(0,100);
            if (rate >= skillRand)
            {
                var rebornSkillInfo = new SkillInfo(magicReborn.Id);
                rebornSkillInfo.SetParam(skill.Master.Name,0,skill.Master.Id);
                list.Add(rebornSkillInfo);
            }
        }
        return list;
    }

    private SkillInfo AddQuestRebornSkill()
    {
        var quedtReborn = DataSystem.Skills.Find(a => a.FeatureDatas.Find(b => b.FeatureType == FeatureType.RebornQuest) != null);
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
        var skill = new SkillInfo(quedtReborn.Id);
        skill.SetParam(param2.ToString(),param2,0);
        return skill;
    }

    private int _rebornActorIndex = 0;
    
    public List<ActorInfo> ActorInfos(){
        return CurrentData.PlayerInfo.SaveActorList;
    }

    public List<int> DisableActorIndexs()
    {
        var list = new List<int>();
        if (ActorInfos().Count > 10)
        {

        }
        var actorListIndexs = new List<int>();
        foreach (var actorInfo in ActorInfos())
        {
            if (!actorListIndexs.Contains(actorInfo.ActorId))
            {
                actorListIndexs.Add(actorInfo.ActorId);
            }
        }
        if (actorListIndexs.Count == 2)
        {
            var minSize = ActorInfos().FindAll(a => a.ActorId == actorListIndexs[0]);
            var maxSize = ActorInfos().FindAll(a => a.ActorId == actorListIndexs[1]);
            if (minSize.Count == 1 || maxSize.Count == 1)
            {
                if (minSize.Count == 1)
                {
                    var idx = 0;
                    foreach (var actorInfo in ActorInfos())
                    {
                        if (actorInfo.ActorId == actorListIndexs[0])
                        {
                            list.Add(idx);
                        }
                        idx++;
                    }
                } else
                if (maxSize.Count == 1)
                {
                    var idx = 0;
                    foreach (var actorInfo in ActorInfos())
                    {
                        if (actorInfo.ActorId == actorListIndexs[1])
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
        List<ActorInfo> actorInfos = ActorInfos();
        if (actorInfos.Count > _rebornActorIndex)
        {
            return actorInfos[_rebornActorIndex];
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
