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
        foreach (var actorInfo in ResultMembers())
        {
            evaluate += actorInfo.Evaluate();
        }
        return evaluate;
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
        string userName = CurrentData.PlayerInfo.PlayerId.ToString();
        int evaluate = 0;
        List<int> SelectActorIds = CurrentData.CurrentStage.SelectActorIds;
        var members = new List<ActorInfo>();
        var selectIdx = new List<int>();
        var selectIdrank = new List<int>();
        for (int i = 0;i < SelectActorIds.Count ;i++)
        {
            var temp = CurrentData.Actors.Find(a => a.ActorId == SelectActorIds[i]);
            if (temp != null)
            {
                members.Add(temp);
            }
        }
        foreach (var actorInfo in members)
        {
            evaluate += actorInfo.Evaluate();
            selectIdx.Add(actorInfo.ActorId);
            selectIdrank.Add(actorInfo.Evaluate());
        }
        Dictionary<string, object> user = new Dictionary<string, object>
        {
            { "Score", evaluate },
            { "Name", GameSystem.CurrentData.PlayerInfo.PlayerName },
            { "SelectIdx", selectIdx },
            { "SelectRank", selectIdrank },
        };
        NCMBObject obj = new NCMBObject("ranking");
            obj["Name"]  = userName;
            obj["Score"] = evaluate;
            obj["SelectIdx"]  = selectIdx;
            obj["SelectRank"] = selectIdrank;
            obj.SaveAsync((res) => {
                if (endEvent != null) endEvent();
            });
    }

    public async void GetRankingData(System.Action<string> endEvent)
    {
        NCMBQuery<NCMBObject> query = new NCMBQuery<NCMBObject> ("ranking");

        //Scoreフィールドの降順でデータを取得
        query.OrderByDescending ("Score");

        //検索件数を5件に設定
        query.Limit = 100;

        int rank = 0;
        var isEnd = false;
        //データストアでの検索を行う
        query.FindAsync ((List<NCMBObject> objList ,NCMBException e) => {
            if (e != null) {
                //検索失敗時の処理
                isEnd = true;
            } else {
                //検索成功時の処理
                foreach (NCMBObject obj in objList) {
                    if ((string)obj["Name"] == GameSystem.CurrentData.PlayerInfo.PlayerId.ToString())
                    {
                        isEnd = true;
                    } else
                    {
                        rank++;
                    }
                }
                isEnd = true;
            }
        });
        await UniTask.WaitUntil(() => isEnd == true);
        if (rank != 0)
        {
            if (endEvent != null) endEvent(rank.ToString());
        } else
        {
            if (endEvent != null) endEvent("圏外");
        }
    }
}
