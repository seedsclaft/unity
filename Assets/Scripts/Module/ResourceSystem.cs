using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;

public class ResourceSystem : MonoBehaviour
{
    private static GameObject _lastScene = null;
    private static List<Object> _lastLoadAssets = new List<Object>();
    public static async Task<GameObject> CreateScene<T>(Scene scene)
    {
        string address = GetScenePrefab(scene);
        //var handle = await Addressables.InstantiateAsync(address).Task;
        var handle = Resources.Load<GameObject>(address);
        //_lastScene = handle;
        return handle;
    }

    public static void ReleaseScene()
    {
        if (_lastScene != null)
        {
            Addressables.ReleaseInstance(_lastScene);
            _lastScene = null;
        }
    }

    private static string GetScenePrefab(Scene scene)
    {
        string path = "Prefabs/";
        switch (scene)
        {
            case Scene.Boot:
            return path + "Boot/BootScene";
            case Scene.Title:
            return path + "Title/TitleScene";
            case Scene.NameEntry:
            return path + "NameEntry/NameEntryScene";
            case Scene.MainMenu:
            return path + "MainMenu/MainMenuScene";
            case Scene.Battle:
            return path + "Battle/BattleScene";
            case Scene.Status:
            return path + "Status/StatusScene";
            case Scene.Tactics:
            return path + "Tactics/TacticsScene";
            case Scene.Strategy:
            return path + "Strategy/StrategyScene";
        }
        return "";
    }

    public static async Task<T> LoadAsset<T>(string address){
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

    public static async Task<List<AudioClip>> LoadBGMAsset(string bgmKey)
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
        //result1 = await LoadAsset<AudioClip>(data[0]);
        result1 = await LoadBGMResources(data[0]);
        if (bGMData.Loop)
        {
            //result2 = await LoadAsset<AudioClip>(data[1]);
            result2 = await LoadBGMResources(data[1]);
        }
        return new List<AudioClip>(){
            result1,result2
        };
    }

    public static async Task<AudioClip> LoadBGMResources(string address)
    {
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
    Strategy
}