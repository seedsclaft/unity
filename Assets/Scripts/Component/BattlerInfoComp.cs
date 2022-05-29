using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;

public class BattlerInfoComp : MonoBehaviour
{
    [SerializeField] private Image mainThumb;
    [SerializeField] private Text maxHp;
    [SerializeField] private Text hp;
    private BattlerInfo _battlerInfo;
    public void UpdateInfo(BattlerInfo battlerInfo)
    {
        if (battlerInfo == null){
            return;
        }
        _battlerInfo = battlerInfo;

        if (battlerInfo.IsActor()){
            UpdateActorData();
        } else{
            UpdateEnemyData();
        }
            
        if (hp != null){
            hp.text = battlerInfo.Status.Hp.ToString();
        }
        
    }

    private void UpdateActorData()
    {
        var actorData = DataSystem.Actors.Find(actor => actor.Id == _battlerInfo.CharaId);
        if (mainThumb != null){
            UpdateActorMainThumb(actorData.ImagePath);
        }
        if (maxHp != null){
            maxHp.text = actorData.CurrentParam(StatusParamType.Hp,_battlerInfo.Level).ToString();
        }
    }
    private void UpdateEnemyData()
    {

        var enemyData = DataSystem.Enemies.Find(enemy => enemy.Id == _battlerInfo.CharaId);
        if (mainThumb != null){
            UpdateEnemyMainThumb(enemyData.ImageName);
        }
        if (maxHp != null){
            maxHp.text = enemyData.CurrentParam(StatusParamType.Hp,_battlerInfo.Level).ToString();
        }
    }

    private void UpdateActorMainThumb(string imagePath)
    {
        Addressables.LoadAssetAsync<Sprite>(
            "Assets/Images/Actors/" + imagePath + "/main.png"
        ).Completed += op => {
            mainThumb.sprite = op.Result;
        };
    }
    private void UpdateEnemyMainThumb(string imagePath)
    {
        Addressables.LoadAssetAsync<Sprite>(
            "Assets/Images/Enemies/" + imagePath + ".png"
        ).Completed += op => {
            mainThumb.sprite = op.Result;
        };
    }
}
