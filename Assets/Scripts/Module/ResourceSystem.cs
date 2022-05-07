using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Threading;
using System.Threading.Tasks;

public class ResourceSystem : MonoBehaviour
{
    readonly string BootScene = "Assets/Prefabs/Boot/BootScene.prefab";

    public static async Task<T> LoadScene<T>(Scene scene){
        string address = GetScenePrefab(scene);
        var asset = await Addressables.LoadAssetAsync<T>(address).Task;
        var result = asset;
        Addressables.Release(asset);
        return result;
    }
    /*
    public static List<T> LoadScene<T>(Scene scene){
        string address = GetScenePrefab(scene);
        var resource = new List<T>();
        for (var i = 0;i < resource.Count;i++)
        {
            var asset = Addressables.LoadAssetAsync<T>(
            address
            );
            asset.WaitForCompletion();
            resource.Add(asset.Result);
            Addressables.Release(asset);
        }
        return resource;
    }
    */

    private static string GetScenePrefab(Scene scene)
    {
        switch (scene)
        {
            case Scene.Boot:
            return "Assets/Prefabs/Boot/BootScene.prefab";
            case Scene.Title:
            return "Assets/Prefabs/Title/TitleScene.prefab";
            case Scene.MainMenu:
            return "Assets/Prefabs/MainMenu/MainMenuScene.prefab";

            
        }
        return "";
    }
}

public enum Scene
{
    None,

    Boot,
    Title,
    MainMenu
}