using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using Cysharp.Threading.Tasks;
using Effekseer;
using UnityEngine.U2D;

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
        var bGMData = DataSystem.Data.GetBGM(bgmKey);
        var data = new List<string>();
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

    static string ActorTexturePath => "Texture/Character/Actors/";

    private static AudioClip LoadResourceAudioClip(string path)
    {
        return Resources.Load<AudioClip>(path);
    } 

    public static AudioClip LoadSeAudio(string path)
    {
        return LoadResourceAudioClip("Audios/SE/" + path);
    } 

    private static Sprite LoadResourceSprite(string path)
    {
        return Resources.Load<Sprite>(path);
    } 

    public static Sprite LoadActorMainSprite(string path)
    {
        return LoadResourceSprite(ActorTexturePath + path + "/Main");
    }

    public static Sprite LoadActorMainFaceSprite(string path)
    {
        return LoadResourceSprite(ActorTexturePath + path + "/MainFace");
    }

    public static Sprite LoadActorAwakenSprite(string path)
    {
        return LoadResourceSprite(ActorTexturePath + path + "/Awaken");
    }

    public static Sprite LoadActorAwakenFaceSprite(string path)
    {
        return LoadResourceSprite(ActorTexturePath + path + "/AwakenFace");
    }

    public static Sprite LoadActorClipSprite(string path)
    {
        return LoadResourceSprite(ActorTexturePath + path + "/Clip");
    }

    public static Sprite LoadEnemySprite(string enemyImage)
    {
        return LoadResourceSprite("Texture/Character/Enemies/" + enemyImage);
    }
    
    public static EffekseerEffectAsset LoadResourceEffect(string path)
    {
        return Resources.Load<EffekseerEffectAsset>("Animations/" + path);
    } 

    private static SpriteAtlas LoadResourceSpriteAtlas(string path)
    {
        return Resources.Load<SpriteAtlas>(path);
    } 

    public static SpriteAtlas LoadSpellIcons()
    {
        return LoadResourceSpriteAtlas("Texture/SpellIcons");
    }

    public static SpriteAtlas LoadIcons()
    {
        return LoadResourceSpriteAtlas("Texture/Icons");
    } 

    public static SpriteAtlas LoadSystems()
    {
        return LoadResourceSpriteAtlas("Texture/Systems");
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
    Result,
    Reborn,
    RebornResult,
    Slot,
    FastBattle,
}