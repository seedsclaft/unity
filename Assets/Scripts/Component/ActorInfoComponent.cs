using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;

public class ActorInfoComponent : MonoBehaviour
{
    [SerializeField] private Image mainThumb;
    [SerializeField] private Text maxHp;
    [SerializeField] private Text hp;
    public void UpdateInfo(ActorInfo actorInfo)
    {
        if (actorInfo == null){
            return;
        }
        var actorData = DataSystem.Actors.Find(actor => actor.Id == actorInfo.ActorId);
        
        if (mainThumb != null){
            UpdateMainThumb(actorData.ImagePath);
        }
        if (maxHp != null){
            maxHp.text = actorData.CurrentParam(StatusParamType.Hp,actorInfo.Level).ToString();
        }
        if (hp != null){
            hp.text = actorInfo.Hp.ToString();
        }
        
    }

    private void UpdateMainThumb(string imagePath)
    {
        Addressables.LoadAssetAsync<Sprite>(
            "Assets/Images/Actors/" + imagePath + "/main.png"
        ).Completed += op => {
            mainThumb.sprite = op.Result;
        };
    }
}
