using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.AddressableAssets;
using TMPro;

public class EnemyInfoComponent : MonoBehaviour
{
    [SerializeField] private Image mainThumb;
    [SerializeField] private TextMeshProUGUI name;
    [SerializeField] private TextMeshProUGUI lv;
    [SerializeField] private StatusInfoComponent statusInfoComponent;
    
    public void UpdateInfo(BattlerInfo battlerInfo)
    {
        if (battlerInfo == null){
            return;
        }
        var enemyData = battlerInfo.EnemyData;
        
        if (mainThumb != null){
            UpdateMainThumb(enemyData.ImagePath,0,0,1.0f);
        }
        if (name != null){
            name.text = enemyData.Name;
        }
        if (lv != null){
            lv.text = battlerInfo.Level.ToString();
        }
        if (statusInfoComponent != null){
            statusInfoComponent.UpdateInfo(battlerInfo.Status);
        }
        
    }

    private void UpdateMainThumb(string imagePath,int x,int y,float scale)
    {
        Addressables.LoadAssetAsync<Sprite>(
            "Assets/Images/Enemies/" + imagePath + ".png"
        ).Completed += op => {
            RectTransform rect = mainThumb.GetComponent < RectTransform > ();
            rect.localPosition = new Vector3(x, y, 0);
            rect.localScale = new Vector3(scale, scale, 1);
            mainThumb.sprite = op.Result;
        };
    }
}
