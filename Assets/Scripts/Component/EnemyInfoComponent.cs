using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using TMPro;

namespace Ryneus
{
    public class EnemyInfoComponent : MonoBehaviour
    {
        [SerializeField] private Image mainThumb;
        public Image MainThumb => mainThumb;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI lv;
        [SerializeField] private StatusInfoComponent statusInfoComponent;
        [SerializeField] private TextMeshProUGUI gridKey;
        

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
                statusInfoComponent.UpdateHp(battlerInfo.Hp,battlerInfo.MaxHp);
                statusInfoComponent.UpdateMp(battlerInfo.Mp,battlerInfo.MaxMp);
            }
            
        }

        private void UpdateMainThumb(string imagePath,int x,int y,float scale)
        {
            //var handle = await ResourceSystem.LoadAsset<Sprite>("Enemies/" + imagePath);
            var handle = ResourceSystem.LoadEnemySprite(imagePath);
            if (mainThumb != null)
            {
                mainThumb.gameObject.SetActive(true);
                var rect = mainThumb.GetComponent<RectTransform>();
                rect.localPosition = new Vector3(x, y, 0);
                rect.localScale = new Vector3(scale, scale, 1);
                mainThumb.sprite = handle;
            }
        }
        
        public void UpdateData(EnemyData enemyData)
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

        public void SetGridKey(int index)
        {
            var textId = 360 + index;
            gridKey.text = DataSystem.GetTextData(textId).Text;
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
}
