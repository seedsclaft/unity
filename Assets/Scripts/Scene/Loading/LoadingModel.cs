using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class LoadingModel : BaseModel
{
    private TipsData.TipData _currentTips = null; 
    public LoadingModel()
    {
    }

    public void RefreshTips()
    {
        _currentTips = DataSystem.Tips[0];
    }

    public string TipsText()
    {
        return _currentTips.Name;
    }
    
    public Sprite TipsImage()
    {
        string path = "Texture/BG/" + _currentTips.ImagePath;
        //var result = await ResourceSystem.LoadAsset<Sprite>(path);
        var result = Resources.Load<Sprite>(path);
        return result;
    }
}
