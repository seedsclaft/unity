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
                (task) => {
                if (task.IsFaulted || task.IsCanceled) {
                    Debug.Log(task.Exception.ToString());
                    // Uh-oh, an error occurred!
                }
                else {
                    // Metadata contains file metadata such as size, content-type, and download URL.
                    StorageMetadata metadata = task.Result;
                    string md5Hash = metadata.Md5Hash;
                    Debug.Log("Finished uploading...");
                    Debug.Log("md5 hash = " + md5Hash);
                }
            });;
        }
        #endif
    }
}
