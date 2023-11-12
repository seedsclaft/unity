using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using NCMB;
using System.Threading;
using UnityEngine;

public class RankingModel : BaseModel
{
    private List<RankingInfo> _rakingInfos;
    public async void RankingInfos(System.Action<List<ListData>> endEvent)
    {
        var isEnd = false;
        if (_rakingInfos == null)
        {
            _rakingInfos = new List<RankingInfo>();
            var list = new List<RankingInfo>();

            NCMBQuery<NCMBObject> query = new NCMBQuery<NCMBObject> ("ranking");

            //Scoreフィールドの降順でデータを取得
            query.OrderByDescending ("Score");

            //検索件数を5件に設定
            query.Limit = 100;

            //データストアでの検索を行う
            query.FindAsync ((List<NCMBObject> objList ,NCMBException e) => {
                if (e != null) {
                    //検索失敗時の処理
                    isEnd = true;
                } else {
                    //検索成功時の処理
                    foreach (NCMBObject obj in objList) {
                        var rankingInfo = new RankingInfo();
                        if (obj.ContainsKey("Score"))
                        {
                            rankingInfo.Score = int.Parse(obj["Score"].ToString());
                        }
                        if (obj.ContainsKey("Name"))
                        {
                            rankingInfo.Name = (string)obj["Name"];
                        }
                        if (obj.ContainsKey("SelectIdx"))
                        {
                            ArrayList selectIdx = obj["SelectIdx"] as ArrayList;
                            long[] numbers = new long[selectIdx.Count];
                            for (var i = 0; i < selectIdx.Count; i++) {
                                numbers[i] = (long) selectIdx[i];
                                rankingInfo.SelectIdx.Add((int)numbers[i]);
                            }
                        }
                        if (obj.ContainsKey("SelectRank"))
                        {
                            ArrayList selectRank = obj["SelectRank"] as ArrayList;
                            long[] numbers2 = new long[selectRank.Count];
                            for (var i = 0; i < selectRank.Count; i++) {
                                numbers2[i] = (long) selectRank[i];
                                rankingInfo.SelectRank.Add((int)numbers2[i]);
                            }
                        }
                        list.Add(rankingInfo);
                    }
                        
                    _rakingInfos = list;
                    isEnd = true;
                }
            });
        }
        _cancellationTokenSource = new CancellationTokenSource();
        
        try {
            await UniTask.WaitUntil(() => isEnd == true,PlayerLoopTiming.Update,_cancellationTokenSource.Token);
            if (endEvent != null) 
            {
                var list = new List<ListData>();
                foreach (var rakingInfo in _rakingInfos)
                {
                    var listData = new ListData(rakingInfo);
                    list.Add(listData);
                }
                endEvent(list);
            }
        } catch (OperationCanceledException e)
        {
            Debug.Log(e);
        }
    }
}
