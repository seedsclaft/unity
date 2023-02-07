using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Threading;
using System.Threading.Tasks;

public class TitleModel : DataSystem
{
    public new List<SystemData.MenuCommandData> TitleCommand
    {
        get { return DataSystem.TitleCommand;}
    }

    public async Task<List<AudioClip>> BgmData(){
        BGMData bGMData = DataSystem.Data.GetBGM("TITLE");
        List<string> data = new List<string>();
        data.Add("BGM/" + bGMData.FileName + "_intro.ogg");
        data.Add("BGM/" + bGMData.FileName + "_loop.ogg");
        
        var result1 = await ResourceSystem.LoadAsset<AudioClip>(data[0]);
        var result2 = await ResourceSystem.LoadAsset<AudioClip>(data[1]);
        return new List<AudioClip>(){
            result1,result2
        };
    }
}
