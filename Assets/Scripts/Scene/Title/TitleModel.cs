using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Threading;
using System.Threading.Tasks;

public class TitleModel : BaseModel
{
    public List<SystemData.MenuCommandData> TitleCommand
    {
        get { return DataSystem.TitleCommand;}
    }

    public bool ExistsLoadFile()
    {
        return SaveSystem.ExistsLoadFile();
    }
}
