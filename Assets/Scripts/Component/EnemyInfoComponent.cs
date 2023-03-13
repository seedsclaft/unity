using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.AddressableAssets;
using TMPro;

public class EnemyInfoComponent : MonoBehaviour
{
    [SerializeField] private Image mainThumb;
    public Image MainThumb {get {return mainThumb;}}
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI lv;
    [SerializeField] private StatusInfoComponent statusInfoComponent;
    
    public void UpdateInfo(BattlerInfo battlerInfo)
    {
        if (battlerInfo == null){
            Clear();
            return;
        }
        var enemyData = battlerInfo.EnemyData;
        UpdateData(enemyData);
        if (lv != null){
            lv.text = battlerInfo.Level.ToString();
        }
        if (statusInfoComponent != null){
            statusInfoComponent.UpdateInfo(battlerInfo.Status);
            statusInfoComponent.UpdateHp(battlerInfo.Hp,battlerInfo.Status.Hp);
            statusInfoComponent.UpdateMp(battlerInfo.Mp,battlerInfo.Status.Mp);
        }
        
    }

    private void UpdateMainThumb(string imagePath,int x,int y,float scale)
    {
        Addressables.LoadAssetAsync<Sprite>(
            "Assets/Images/Enemies/" + imagePath + ".png"
        ).Completed += op => {
            if (mainThumb != null)
            {
                mainThumb.gameObject.SetActive(true);
                RectTransform rect = mainThumb.GetComponent < RectTransform > ();
                rect.localPosition = new Vector3(x, y, 0);
                rect.localScale = new Vector3(scale, scale, 1);
                mainThumb.sprite = op.Result;
            }
        };
    }

    public void ChangeHp(int value,int maxHp)
    {
        if (statusInfoComponent == null) return;
        statusInfoComponent.UpdateHp(value,maxHp);
    }

    public void ChangeMp(int value,int maxHp)
    {
        if (statusInfoComponent == null) return;
        statusInfoComponent.UpdateMp(value,maxHp);
    }

    public void HideUI()
    {
        if (statusInfoComponent == null) return;
        statusInfoComponent.gameObject.SetActive(false);
    }

    public void UpdateData(EnemiesData.EnemyData enemyData)
    {
        if (enemyData == null)
        {
            Clear();
            return;
        }
        if (mainThumb != null){
            UpdateMainThumb(enemyData.ImagePath,0,0,1.0f);
        }
        if (nameText != null){
            nameText.text = enemyData.Name;
        }
    }

    public void Clear()
    {
        if (mainThumb != null){
            mainThumb.gameObject.SetActive(false);
        }
        if (nameText != null){
            nameText.text = "";
        }
    }
}
