using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using Cysharp.Threading.Tasks;

public class ResourceSystem : MonoBehaviour
{
    private static GameObject _lastScene = null;
    private static List<Object> _lastLoadAssets = new List<Object>();

    public static void ReleaseScene()
    {
        if (_lastScene != null)
        {
            Addressables.ReleaseInstance(_lastScene);
            _lastScene = null;
        }
    }

    public static async UniTask<T> LoadAsset<T>(string address){
        var handle = await Addressables.LoadAssetAsync<T>(address).Task;
        _lastLoadAssets.Add(handle as Object);
        return handle;
    }

    public static void ReleaseAssets()
    {
        foreach (var lastLoadAssets in _lastLoadAssets)
        {
            Addressables.Release(lastLoadAssets);
        }
        _lastLoadAssets.Clear();
    }

    public async static UniTask<List<AudioClip>>LoadBGMAsset(string bgmKey)
    {    
        BGMData bGMData = DataSystem.Data.GetBGM(bgmKey);
        List<string> data = new List<string>();
        if (bGMData.Loop)
        {
            data.Add("Audios/BGM/" + bGMData.FileName + "_intro");
            data.Add("Audios/BGM/" + bGMData.FileName + "_loop");
        } else{
            data.Add("Audios/BGM/" + bGMData.FileName + "");
        }
        AudioClip result1 = null;
        AudioClip result2 = null;
        result1 = await LoadAssetResources<AudioClip>(data[0]);
        //result1 = Resources.Load<AudioClip>(data[0]);
        if (bGMData.Loop)
        {
            result2 = await LoadAssetResources<AudioClip>(data[1]);
            //result2 = Resources.Load<AudioClip>(data[1]);
        }
        return new List<AudioClip>(){
            result1,result2
        };
    }


    public static async UniTask<AudioClip> LoadAssetResources<T>(string address){
        var handle = Resources.LoadAsync<AudioClip>(address);
        await handle;
        return handle.asset as AudioClip;
    }
}

public static class ResourceRequestExtenion
{
    // Resources.LoadAsyncの戻り値であるResourceRequestにGetAwaiter()を追加する
    public static TaskAwaiter<Object> GetAwaiter(this ResourceRequest resourceRequest)
    {
        var tcs = new TaskCompletionSource<Object>();
        resourceRequest.completed += operation =>
        {
            // ロードが終わった時点でTaskCompletionSource.TrySetResult
            tcs.TrySetResult(resourceRequest.asset);
        };

        // TaskCompletionSource.Task.GetAwaiter()を返す
        return tcs.Task.GetAwaiter();
    }
}

public enum Scene
{
    None,
    Base,
    Boot,
    Title,
    NameEntry,
    MainMenu,
    Battle,
    Map,
    Status,
    Tactics,
    Strategy,
    Result
}