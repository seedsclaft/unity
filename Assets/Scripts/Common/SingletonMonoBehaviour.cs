using System;
using UnityEngine;
using System.Linq;
using System.Collections;
using UnityEngine.Networking;
using System.IO;
using UnityEditor;

public class SingletonMonoBehaviour<T> : MonoBehaviour where T : SingletonMonoBehaviour<T>
{
    protected static T _instance;
    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                Type t = typeof(T);
                _instance = (T)FindObjectOfType(t);

                if(_instance != null) return _instance;
                
                var obj = new GameObject(typeof(T).Name);
                _instance = obj.AddComponent<T>();
                DontDestroyOnLoad(obj);
            }

            return _instance;
        }
    }
}
