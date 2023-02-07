//#define USING_UniRx

using System.Collections;
using UnityEngine;

public class CoroutineAccessor : MonoBehaviour
{
#if !USING_UniRx
    /// <summary>
    /// GameObject アタッチなしで StartCoroutine を使うための instance
    /// </summary>
    static CoroutineAccessor Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject obj = new GameObject(nameof(CoroutineAccessor));
                DontDestroyOnLoad(obj);
                instance = obj.AddComponent<CoroutineAccessor>();
            }
            return instance;
        }
    }
    static CoroutineAccessor instance;

    /// <summary>
    /// OnDisable
    /// </summary>
    void OnDisable()
    {
        if (instance != null)
        {
            Destroy(instance.gameObject);
            instance = null;
        }
    }
#endif
    
    /// <summary>
    /// StartCoroutine
    /// </summary>
    public static Coroutine Start(IEnumerator routine)
    {
#if USING_UniRx
        return UniRx.MainThreadDispatcher.StartCoroutine(routine);
#else
        return Instance.StartCoroutine(routine);
#endif
    }

    /// <summary>
    /// StopCoroutine
    /// </summary>
    public static void Stop(Coroutine coroutine)
    {
        Instance.StopCoroutine(coroutine);
    }

    /// <summary>
    /// StopCoroutine Reference is cleard by null.
    /// </summary>
    public static void StopNull(ref Coroutine coroutine)
    {
        Stop(coroutine);
        coroutine = null;
    }
}