
using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;
#if UNITY_WEBGL
using System.Runtime.InteropServices;
#elif UNITY_ANDROID
using Firebase.Firestore;
using Firebase.Extensions;
#endif

public class FireBaseController : SingletonMonoBehaviour<FireBaseController>
{
    private bool _isInit = false;
    public static bool IsBusy = false;
    public static List<RankingInfo> RankingInfos = new ();
    public static int CurrentScore = 0;
    #if UNITY_WEBGL 
    [DllImport("__Internal")]
    private static extern void FirebaseInit();

    [DllImport("__Internal")]
    private static extern void FirebaseReadRankingData(int instanceId,Action<int,string> result);
    
    [DllImport("__Internal")]
    private static extern void FirebaseCurrentRankingData(int instanceId,string userId,Action<int,string> result);
   
    [DllImport("__Internal")]
    private static extern void FirebaseWriteRankingData(int instanceId,string userId,int score,string name,int[] selectIndex,int selectIndexSize,int[] selectRank,int selectRankSize,Action<int,string> result);
   
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
            var array = jsonString.Split(";.:");
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
        FireBaseController.CurrentScore = 0;
        IsBusy = true;
        FirebaseCurrentRankingData(gameObject.GetInstanceID(),userId,OnCurrentFirestore);
    }

    [AOT.MonoPInvokeCallback(typeof(Action<int,string>))]
    private static void OnCurrentFirestore(int instanceId,string jsonString)
    {
        Debug.Log("OnCurrentFirestore End");
        if (jsonString != "-1")
        {
            var ranking = JsonUtility.FromJson<RankingInfo>(jsonString);
            FireBaseController.CurrentScore = ranking.Score;
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
        FirebaseWriteRankingData(gameObject.GetInstanceID(),userId,score,name,selectIdx.ToArray(),selectIdx.Length,selectRank.ToArray(),selectRank.Length,OnWriteFirestore);
    }

    
    [AOT.MonoPInvokeCallback(typeof(Action<int,string>))]
    private static void OnWriteFirestore(int instanceId,string jsonString)
    {
        Debug.Log("OnWriteFirestore End");
        IsBusy = false;
    }
    #endif

    #if UNITY_ANDROID
    private Firebase.FirebaseApp _app;
    public void Initialize()
    {
        if (_isInit)
        {
            return;
        }
        _isInit = true;
        Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task => {
            var dependencyStatus = task.Result;
            if (dependencyStatus == Firebase.DependencyStatus.Available) {
                // Create and hold a reference to your FirebaseApp,
                // where app is a Firebase.FirebaseApp property of your application class.
                _app = Firebase.FirebaseApp.DefaultInstance;
                // Set a flag here to indicate whether Firebase is ready to use by your app.
            } else {
                UnityEngine.Debug.LogError(System.String.Format(
                "Could not resolve all Firebase dependencies: {0}", dependencyStatus));
                // Firebase Unity SDK is not safe to use here.
            }
        });
    }

    private RankingInfo MakeRankingInfo(Dictionary<string, object> docDictionary)
    {
        var selectIdxData = (List<object>)Convert.ChangeType(docDictionary["SelectIdx"], typeof(List<object>));
        var selectRankData = (List<object>)Convert.ChangeType(docDictionary["SelectRank"], typeof(List<object>));
        
        var selectIdxList = new List<int>();
        foreach (var selectIdx in selectIdxData)
        {
            selectIdxList.Add((int) Convert.ChangeType(selectIdx, typeof(int)));
        }
        var selectRankList = new List<int>();
        foreach (var selectRank in selectRankData)
        {
            selectRankList.Add((int) Convert.ChangeType(selectRank, typeof(int)));
        }
        return new RankingInfo(){
            Name = docDictionary["Name"].ToString(),
            Score = (int) Convert.ChangeType(docDictionary["Score"], typeof(int)),
            SelectIdx = selectIdxList,
            SelectRank = selectRankList
        };
    }

    public void ReadRankingData()
    {
        if (!_isInit)
        {
            return;
        }
        FireBaseController.RankingInfos.Clear();
        IsBusy = true;
        FirebaseFirestore db = FirebaseFirestore.DefaultInstance;

        var cnf = db.Collection("ranking").OrderByDescending("Score").Limit(100);
        
        cnf.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            var data = new List<RankingInfo>(); 
            var rank = 1;
            QuerySnapshot querySnapshot = task.Result;
            
            if (task.IsCompletedSuccessfully)
            {
                foreach (var document in querySnapshot.Documents)
                {
                    var ranking = MakeRankingInfo(document.ToDictionary());
                    ranking.Rank = rank;
                    data.Add(ranking);
                    rank++;
                }
                Debug.Log("OnReadFirestore End");
                FireBaseController.RankingInfos = data;
                IsBusy = false;
            } else
            {
                IsBusy = false;
            }
        });
    }

    public void CurrentRankingData(string userId)
    {
        if (!_isInit)
        {
            return;
        }
        FireBaseController.CurrentScore = 0;
        IsBusy = true;
        FirebaseFirestore db = FirebaseFirestore.DefaultInstance;
        var cnf = db.Collection("ranking").Document(userId);
        cnf.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompletedSuccessfully)
            {
                DocumentSnapshot doc = task.Result;
                var ranking = MakeRankingInfo(doc.ToDictionary());
                FireBaseController.CurrentScore = ranking.Score;
                IsBusy = false;
            } else
            {
                IsBusy = false;
            }
        });
    }

    public void WriteRankingData(string userId,int score,string name, List<int> selectIdx,List<int> selectRank)
    {
        if (!_isInit)
        {
            return;
        }
        IsBusy = true;
        FirebaseFirestore db = FirebaseFirestore.DefaultInstance;
        DocumentReference docRef = db.Collection("ranking").Document(userId);
        RankingInfo rankingData = new RankingInfo
        {
            Score = score,
            Name = name,
            SelectIdx = selectIdx,
            SelectRank = selectRank,
        };
        docRef.SetAsync(rankingData).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompletedSuccessfully)
            {
                IsBusy = false;
            } else
            {
                IsBusy = false;
            }
        });
    }
    #endif
}