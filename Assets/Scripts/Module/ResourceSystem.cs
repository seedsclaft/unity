using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Threading;
using System.Threading.Tasks;

public class ResourceSystem : MonoBehaviour
{
    public static async Task<T> LoadScene<T>(Scene scene){
        string address = GetScenePrefab(scene);
        var asset = await Addressables.LoadAssetAsync<T>(address).Task;
        //var asset = await Addressables.InstantiateAsync<T>(address).Task;
        var result = asset;
        //Debug.Log(address);
        //Addressables.Release(asset);
        return result;
    }

    public static async Task<T> LoadAsset<T>(string address){
        var asset = await Addressables.LoadAssetAsync<T>(address).Task;
        var result = asset;
        //Addressables.Release(asset);
        return result;
    }

    private static string GetScenePrefab(Scene scene)
    {
        switch (scene)
        {
            case Scene.Boot:
            return "BootScene";
            case Scene.Title:
            return "Assets/Prefabs/Title/TitleScene.prefab";
            case Scene.NameEntry:
            return "Assets/Prefabs/NameEntry/NameEntryScene.prefab";
            case Scene.MainMenu:
            return "Assets/Prefabs/MainMenu/MainMenuScene.prefab";
            case Scene.Battle:
            return "Assets/Prefabs/Battle/BattleScene.prefab";
            case Scene.Status:
            return "Assets/Prefabs/Status/StatusScene.prefab";
            case Scene.Tactics:
            return "Assets/Prefabs/Tactics/TacticsScene.prefab";
            case Scene.Strategy:
            return "Assets/Prefabs/Strategy/StrategyScene.prefab";

            
        }
        return "";
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