
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

public class FirebaseController : SingletonMonoBehaviour<FirebaseController>
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

    public void ReadRankingData(int stageId,string rankingTypeText)
    {
        if (!_isInit)
        {
            return;
        }
        FirebaseController.RankingInfos.Clear();
        FirebaseController.IsBusy = true;
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
            FirebaseController.RankingInfos = data;
        }
        FirebaseController.IsBusy = false;
    }

    public void CurrentRankingData(int stageId,string userId)
    {
        if (!_isInit)
        {
            return;
        }
        FirebaseController.CurrentScore = 0;
        FirebaseController.IsBusy = true;
        FirebaseCurrentRankingData(gameObject.GetInstanceID(),userId,OnCurrentFirestore);
    }

    [AOT.MonoPInvokeCallback(typeof(Action<int,string>))]
    private static void OnCurrentFirestore(int instanceId,string jsonString)
    {
        Debug.Log("OnCurrentFirestore End");
        if (jsonString != "-1")
        {
            var ranking = JsonUtility.FromJson<RankingInfo>(jsonString);
            FirebaseController.CurrentScore = ranking.Score;
        }
        FirebaseController.IsBusy = false;
    }

    public void WriteRankingData(int stageId,string userId,int score,string name, List<int> selectIdx,List<int> selectRank)
    {
        if (!_isInit)
        {
            return;
        }
        FirebaseController.IsBusy = true;
        FirebaseWriteRankingData(gameObject.GetInstanceID(),userId,score,name,selectIdx.ToArray(),selectIdx.Count,selectRank.ToArray(),selectRank.Count,OnWriteFirestore);
    }

    
    [AOT.MonoPInvokeCallback(typeof(Action<int,string>))]
    private static void OnWriteFirestore(int instanceId,string jsonString)
    {
        Debug.Log("OnWriteFirestore End");
        FirebaseController.IsBusy = false;
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
        Debug.Log("MakeRankingInfo");
        var selectIdxData = (List<object>)Convert.ChangeType(docDictionary["SelectIdx"], typeof(List<object>));
        var selectRankData = (List<object>)Convert.ChangeType(docDictionary["SelectRank"], typeof(List<object>));
        
        var selectIdxList = new List<int>();
        foreach (var selectIdx in selectIdxData)
        {
            selectIdxList.Add((int)Convert.ChangeType(selectIdx, typeof(int)));
        }
        var selectRankList = new List<int>();
        foreach (var selectRank in selectRankData)
        {
            selectRankList.Add((int)Convert.ChangeType(selectRank, typeof(int)));
        }

        var actorInfos = new List<ActorInfo>();
        for (var i = 1;i <= 5;i++)
        {
            var actorKey = "Actor" + i;
            if (docDictionary.ContainsKey(actorKey))
            {
                var convertActorData = (Dictionary<string, object>)Convert.ChangeType(docDictionary[actorKey], typeof(Dictionary<string, object>));
                
                var rankingActorData = new RankingActorData();
                foreach (var rankingActor in convertActorData)
                {
                    if (rankingActor.Key == "ActorId") { rankingActorData.ActorId = (int)Convert.ChangeType(rankingActor.Value, typeof(int));}
                    if (rankingActor.Key == "Level") { rankingActorData.Level = (int)Convert.ChangeType(rankingActor.Value, typeof(int));}
                    if (rankingActor.Key == "DemigodParam") { rankingActorData.DemigodParam = (int)Convert.ChangeType(rankingActor.Value, typeof(int));}
                    if (rankingActor.Key == "Hp") { rankingActorData.Hp = (int)Convert.ChangeType(rankingActor.Value, typeof(int));}
                    if (rankingActor.Key == "Mp") { rankingActorData.Mp = (int)Convert.ChangeType(rankingActor.Value, typeof(int));}
                    if (rankingActor.Key == "Atk") { rankingActorData.Atk = (int)Convert.ChangeType(rankingActor.Value, typeof(int));}
                    if (rankingActor.Key == "Def") { rankingActorData.Def = (int)Convert.ChangeType(rankingActor.Value, typeof(int));}
                    if (rankingActor.Key == "Spd") { rankingActorData.Spd = (int)Convert.ChangeType(rankingActor.Value, typeof(int));}
                    if (rankingActor.Key == "Lost") { rankingActorData.Lost = (Boolean)Convert.ChangeType(rankingActor.Value, typeof(Boolean));}
                    if (rankingActor.Key == "SkillIds")
                    {
                        var skillIdDates = (List<object>)Convert.ChangeType(rankingActor.Value, typeof(List<object>));
                        var skillIds = new List<int>();
                        foreach (var skillIdData in skillIdDates)
                        {
                            skillIds.Add((int)Convert.ChangeType(skillIdData, typeof(int)));
                        }
                        rankingActorData.SkillIds = skillIds;
                    }
                    
                }
                var actorInfo = new ActorInfo(rankingActorData);
                actorInfos.Add(actorInfo);
            }
        }

        Debug.Log("RankingInfo");
        return new RankingInfo(){
            Name = docDictionary["Name"].ToString(),
            Score = (int) Convert.ChangeType(docDictionary["Score"], typeof(int)),
            SelectIdx = selectIdxList,
            SelectRank = selectRankList,
            ActorInfos = actorInfos
        };
    }

    public void ReadRankingData(int stageId,string rankingTypeText)
    {
        if (!_isInit)
        {
            return;
        }
        FirebaseController.RankingInfos.Clear();
        FirebaseController.IsBusy = true;
        FirebaseFirestore db = FirebaseFirestore.DefaultInstance;

        var cnf = db.Collection("ranking" + stageId.ToString()).OrderByDescending("Score").Limit(100);
        
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
                    ranking.RankingTypeText = rankingTypeText;
                    data.Add(ranking);
                    rank++;
                }
                Debug.Log("OnReadFirestore End");
                FirebaseController.RankingInfos = data;
                FirebaseController.IsBusy = false;
            } else
            {
                FirebaseController.IsBusy = false;
            }
        });
    }

    public void CurrentRankingData(int stageId,string userId)
    {
        if (!_isInit)
        {
            return;
        }
        FirebaseController.CurrentScore = 0;
        FirebaseController.IsBusy = true;
        FirebaseFirestore db = FirebaseFirestore.DefaultInstance;
        var cnf = db.Collection("ranking" + stageId.ToString()).Document(userId);
        cnf.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompletedSuccessfully)
            {
                DocumentSnapshot doc = task.Result;
                if (doc.Exists)
                {
                    var ranking = MakeRankingInfo(doc.ToDictionary());
                    FirebaseController.CurrentScore = ranking.Score;
                }
                FirebaseController.IsBusy = false;
            } else
            {
                FirebaseController.IsBusy = false;
            }
        });
    }

    public void WriteRankingData(int stageId,string userId,int score,string name, List<int> selectIdx,List<int> selectRank,List<RankingActorData> actorInfos)
    {
        if (!_isInit)
        {
            return;
        }
        FirebaseController.IsBusy = true;
        FirebaseFirestore db = FirebaseFirestore.DefaultInstance;
        DocumentReference docRef = db.Collection("ranking" + stageId.ToString()).Document(userId);
        Dictionary<string, System.Object> rankingData = new Dictionary<string, System.Object>
        {
            { "Name", name },
            { "Score", score },
            { "SelectIdx", selectIdx },
            { "SelectRank", selectRank },
        };
        if (actorInfos.Count > 0) rankingData.Add("Actor1",actorInfos[0]);
        if (actorInfos.Count > 1) rankingData.Add("Actor2",actorInfos[1]);
        if (actorInfos.Count > 2) rankingData.Add("Actor3",actorInfos[2]);
        if (actorInfos.Count > 3) rankingData.Add("Actor4",actorInfos[3]);
        if (actorInfos.Count > 4) rankingData.Add("Actor5",actorInfos[4]);
        docRef.SetAsync(rankingData).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompletedSuccessfully)
            {
                Debug.Log("IsCompletedSuccessfully");
                FirebaseController.IsBusy = false;
            } else
            {
                Debug.Log("IsCompleted");
                FirebaseController.IsBusy = false;
            }
        });
    }
#endif
}

#if UNITY_ANDROID
[FirestoreData]
public class RankingActorData
{
    [FirestoreProperty]
    public int ActorId { get; set; }

    [FirestoreProperty]
    public int Level { get; set; }

    [FirestoreProperty]
    public int Hp { get; set; }

    [FirestoreProperty]
    public int Mp { get; set; }

    [FirestoreProperty]
    public int Atk { get; set; }

    [FirestoreProperty]
    public int Def { get; set; }

    [FirestoreProperty]
    public int Spd { get; set; }

    [FirestoreProperty]
    public List<int> SkillIds { get; set; }

    [FirestoreProperty]
    public int DemigodParam { get; set; }

    [FirestoreProperty]
    public bool Lost { get; set; }
}
#endif