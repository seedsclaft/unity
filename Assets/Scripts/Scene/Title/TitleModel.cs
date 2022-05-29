using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Threading;
using System.Threading.Tasks;

public class TitleModel : DataSystem
{
    public new List<SystemData.MenuCommandData> TitleCommand {get{return DataSystem.TitleCommand;}}

    public async Task<AudioClip> BgmData(){
        return await Addressables.LoadAssetAsync<AudioClip>("Assets/Audios/limitant.mp3").Task;
    }
}
