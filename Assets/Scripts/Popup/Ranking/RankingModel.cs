using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
public class RankingModel : BaseModel
{
    private List<RankingInfo> _rakingInfos;
    public async UniTask<List<RankingInfo>> RankingInfos()
    {
        if ( _rakingInfos!=null) return _rakingInfos;
        var list = new List<RankingInfo>();
        string ranking = "ranking";
        var countRef = GameSystem.db.Collection(ranking);
        var snapshot = await countRef.GetSnapshotAsync();
        var all = snapshot.Count;
        var rankAll = await countRef.OrderBy("Score").Limit(100).GetSnapshotAsync();
        
        foreach (var document in rankAll.Documents)
        {
            var rankingInfo = new RankingInfo();
            Dictionary<string, object> docDictionary = document.ToDictionary();
            if (docDictionary.ContainsKey("Score"))
            {
                rankingInfo.Score = int.Parse(docDictionary["Score"].ToString());
            }
            if (docDictionary.ContainsKey("Name"))
            {
                rankingInfo.Name = (string)docDictionary["Name"];
            }
            if (docDictionary.ContainsKey("SelectIdx"))
            {
                var selectIdx = docDictionary["SelectIdx"];
                foreach (var item in (List<object>)selectIdx)
                {
                    rankingInfo.SelectIdx.Add(int.Parse(item.ToString()));
                }
            }
            if (docDictionary.ContainsKey("SelectRank"))
            {
                var selectIdx = docDictionary["SelectRank"];
                foreach (var item in (List<object>)selectIdx)
                {
                    rankingInfo.SelectRank.Add(int.Parse(item.ToString()));
                }
            }
            list.Add(rankingInfo);
        }
        _rakingInfos = list;
        return list;
    }
}
