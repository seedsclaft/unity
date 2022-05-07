using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Threading;
using System.Threading.Tasks;

public class MainMenuModel : DataSystem
{
    private MenuComandType _commandType = MenuComandType.None;
    public async Task<List<SystemData.MenuCommandData>> MenuCommand(){
        var asset = await Addressables.LoadAssetAsync<SystemData>("Assets/Data/System.asset").Task;
        
        return asset.MenuCommandDataList;
    }
    

    public List<Sprite> ActorsImage(List<ActorsData.ActorData> actors){
        var sprites = new List<Sprite>();
        for (var i = 0;i < actors.Count;i++)
        {
            var asset = Addressables.LoadAssetAsync<Sprite>(
            "Assets/Images/Actors/000" + actors[i].ImagePath + "/main.png"
            );
            asset.WaitForCompletion();
            sprites.Add(asset.Result);
            Addressables.Release(asset);
        }
        return sprites;
    }
    
    public async Task<AudioClip> BgmData(){
        return await Addressables.LoadAssetAsync<AudioClip>("Assets/Audios/limitant.mp3").Task;
        
    }
    public async Task<AudioClip> BgmData2(){
        return await Addressables.LoadAssetAsync<AudioClip>("Assets/Audios/field001.mp3").Task;
        
    }

    public void SetSelectedMenuCommand(MenuComandType commandType)
    {
        _commandType = commandType;
    }
}

namespace MainMenuModelData{
    public class CommandData{
        public string key = "";
        public string name;
    }
}
