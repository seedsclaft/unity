using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using Cysharp.Threading.Tasks;
using System.Threading;
#if UNITY_STANDALONE_WIN
using Firebase.Extensions;
using Firebase.Storage;
using Firebase.Firestore;
#endif
namespace Ryneus
{
    public partial class FirebaseController : SingletonMonoBehaviour<FirebaseController>
    {
        #if UNITY_STANDALONE_WIN
        public void Initialize()
        {
            if (_isInit)
            {
                return;
            }
            _isInit = true;
            InitializeWindows();
        }
        
        private static void InitializeWindows()
        {
        }

        public static void LoadReplayFile<T>(string user,T data)
        {
            FirebaseStorage storage = FirebaseStorage.DefaultInstance;
            StorageReference storageRef = storage.RootReference;
            StorageReference storageReference = storageRef.Child(user + ".dat");
            storageReference.GetFileAsync("user.dat").ContinueWith(
                (task) => 
                {
                if (task.IsFaulted || task.IsCanceled) 
                {
                    Debug.Log(task.Exception.ToString());
                    // Uh-oh, an error occurred!
                }
                else 
                {
                }
            });
        }

        public static void UploadFile<T>(string user,T data)
        {
            var TempBinaryFormatter = new BinaryFormatter();
            var memoryStream = new MemoryStream();
            TempBinaryFormatter.Serialize (memoryStream,data);
            var saveData = Convert.ToBase64String (memoryStream.GetBuffer());

            FirebaseStorage storage = FirebaseStorage.DefaultInstance;
            StorageReference storageRef = storage.RootReference;
            StorageReference storageReference = storageRef.Child(user + ".dat");
            storageReference.PutBytesAsync(memoryStream.GetBuffer()).ContinueWith(
                (task) => 
                {
                if (task.IsFaulted || task.IsCanceled) 
                {
                    Debug.Log(task.Exception.ToString());
                    // Uh-oh, an error occurred!
                }
                else 
                {
                    // Metadata contains file metadata such as size, content-type, and download URL.
                    StorageMetadata metadata = task.Result;
                    string md5Hash = metadata.Md5Hash;
                    Debug.Log("Finished uploading...");
                    Debug.Log("md5 hash = " + md5Hash);
                }
            });
        }        
        
        public static void UploadReplayFile(string key,string userId,SaveBattleInfo data)
        {
            IsBusy = true;
            var TempBinaryFormatter = new BinaryFormatter();
            var memoryStream = new MemoryStream();
            TempBinaryFormatter.Serialize (memoryStream,data);
            var saveData = Convert.ToBase64String (memoryStream.GetBuffer());

            FirebaseFirestore db = FirebaseFirestore.DefaultInstance;
            DocumentReference docRef = db.Collection(key).Document(userId);
            Dictionary<string, System.Object> replayData = new Dictionary<string, System.Object>
            {
                { "Data", saveData },
            };
            docRef.SetAsync(replayData).ContinueWithOnMainThread(task =>
            {
                if (task.IsCompletedSuccessfully)
                {
                    Debug.Log("IsCompletedSuccessfully");
                    IsBusy = false;
                } else
                {
                    Debug.Log("IsCompleted");
                    IsBusy = false;
                }
            });
        }

        public static List<SaveBattleInfo> ReplayData = new ();
        public static void LoadReplayFile(string key)
        {
            ReplayData.Clear();
            IsBusy = true;
            FirebaseFirestore db = FirebaseFirestore.DefaultInstance;

            //var cnf = db.Collection(key).OrderByDescending("Score").Limit(100);
            var cnf = db.Collection(key).Limit(10);
            
            cnf.GetSnapshotAsync().ContinueWithOnMainThread(task =>
            {
                var data = new List<RankingInfo>(); 
                QuerySnapshot querySnapshot = task.Result;
                
                if (task.IsCompletedSuccessfully)
                {
                    foreach (var document in querySnapshot.Documents)
                    {
                        var replay = document.ToDictionary()["Data"];
                        var replayStr = (string)Convert.ChangeType(replay,typeof(string));
					    var bytes = Convert.FromBase64String(replayStr);
					    var	TempBinaryFormatter = new BinaryFormatter();
					    var memoryStream = new MemoryStream(bytes);
					    var saveData = (SaveBattleInfo)TempBinaryFormatter.Deserialize(memoryStream);

                        ReplayData.Add(saveData);
                    }
                    Debug.Log("OnReadFirestore End");
                    IsBusy = false;
                } else
                {
                    IsBusy = false;
                }
            });
        }
        #endif
    }
}
