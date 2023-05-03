using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Threading;
using System.Threading.Tasks;

public class ResourceSystem : MonoBehaviour
{
    private static GameObject _lastScene = null;
    private static List<Object> _lastLoadAssets = new List<Object>();
    public static async Task<GameObject> CreateScene<T>(Scene scene)
    {
        string address = GetScenePrefab(scene);
        var handle = await Addressables.InstantiateAsync(address).Task;
        _lastScene = handle;
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
        switch (scene)
        {
            case Scene.Boot:
            return "BootScene";
            case Scene.Title:
            return "TitleScene";
            case Scene.NameEntry:
            return "NameEntryScene";
            case Scene.MainMenu:
            return "MainMenuScene";
            case Scene.Battle:
            return "BattleScene";
            case Scene.Status:
            return "StatusScene";
            case Scene.Tactics:
            return "TacticsScene";
            case Scene.Strategy:
            return "StrategyScene";
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
            data.Add("BGM/" + bGMData.FileName + "_intro.ogg");
            data.Add("BGM/" + bGMData.FileName + "_loop.ogg");
        } else{
            data.Add("BGM/" + bGMData.FileName + ".ogg");
        }
        AudioClip result1 = null;
        AudioClip result2 = null;
        result1 = await LoadAsset<AudioClip>(data[0]);
        if (bGMData.Loop)
        {
            result2 = await LoadAsset<AudioClip>(data[1]);
        }
        return new List<AudioClip>(){
            result1,result2
        };
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