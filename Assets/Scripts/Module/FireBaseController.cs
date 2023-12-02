
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

public class FireBaseController : SingletonMonoBehaviour<FireBaseController>
{
    #if UNITY_WEBGL 
    [DllImport("__Internal")]
    private static extern void FirebaseInit();

    [DllImport("__Internal")]
    private static extern void FirebaseReadRankingData(int instanceId,Action<int,string> result);
    
    [DllImport("__Internal")]
    private static extern void FirebaseCurrentRankingData(int instanceId,string userId,Action<int,string> result);
   

    [DllImport("__Internal")]
    private static extern void FirebaseWriteRankingData(int instanceId,string userId,int score,string name,List<int> selectIndex,List<int> selectRank,Action<int,string> result);
   
    private bool _isInit = false;
    public static bool IsBusy = false;
    public static List<RankingInfo> RankingInfos = new ();
    public static int CurrentScore = 0;
    public void Initialize()
    {
        if (_isInit)
        {
            return;
        }
        _isInit = true;
        FirebaseInit();
    }

    public void ReadRankingData()
    {
        if (!_isInit)
        {
            return;
        }
        FireBaseController.RankingInfos.Clear();
        IsBusy = true;
        FirebaseReadRankingData(gameObject.GetInstanceID(),OnReadFirestore);
    }
    
    [AOT.MonoPInvokeCallback(typeof(Action<int,string>))]
    private static void OnReadFirestore(int instanceId,string jsonString)
    {
        if (jsonString != "")
        {
            var array = jsonString.Split(";");
            var data = new List<RankingInfo>(); 
            var rank = 1;
            foreach (var arrayStr in array)
            {
                var ranking = JsonUtility.FromJson<RankingInfo>(arrayStr);
                ranking.Rank = rank;
                data.Add(ranking);
                rank++;
            }
            Debug.Log("OnReadFirestore End");
            FireBaseController.RankingInfos = data;
        }
        IsBusy = false;
    }

    public void CurrentRankingData(string userId)
    {
        if (!_isInit)
        {
            return;
        }
        IsBusy = true;
        FirebaseCurrentRankingData(gameObject.GetInstanceID(),userId,OnCurrentFirestore);
    }

    [AOT.MonoPInvokeCallback(typeof(Action<int,string>))]
    private static void OnCurrentFirestore(int instanceId,string jsonString)
    {
        if (jsonString == "-1")
        {
            // 新規登録
        } else
        {
            // スコア比較して更新
            CurrentScore = int.Parse(jsonString);
        }
        IsBusy = false;
    }

    public void WriteRankingData(string userId,int score,string name, List<int> selectIdx,List<int> selectRank)
    {
        if (!_isInit)
        {
            return;
        }
        IsBusy = true;
        FirebaseWriteRankingData(gameObject.GetInstanceID(),userId,score,name,selectIdx,selectRank,OnWriteFirestore);
    }

    
    [AOT.MonoPInvokeCallback(typeof(Action<int,string>))]
    private static void OnWriteFirestore(int instanceId,string jsonString)
    {
        IsBusy = false;
    }
    #endif
}