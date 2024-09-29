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
        [SerializeField] private StatusInfoComponent needStatusInfoComponent;
        [SerializeField] private TextMeshProUGUI gridKey;
        [SerializeField] private List<GameObject> actorOnlyGameObjects;

        public void UpdateInfo(BattlerInfo battlerInfo)
        {
            if (battlerInfo == null)
            {
                Clear();
                return;
            }
            var enemyData = battlerInfo.EnemyData;
            UpdateData(enemyData);
            lv?.SetText(battlerInfo.Level.ToString());
            if (statusInfoComponent != null)
            {
                HideActorOnly();
                statusInfoComponent.UpdateInfo(battlerInfo.Status);
                statusInfoComponent.UpdateHp(battlerInfo.Hp,battlerInfo.MaxHp);
                statusInfoComponent.UpdateMp(battlerInfo.Mp,battlerInfo.MaxMp);
            }
            if (needStatusInfoComponent != null)
            {
                UpdateNeedStatus(battlerInfo);
            }
            if (gridKey != null)
            {
                UpdateGridKey(battlerInfo.EnemyIndex);
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
            if (mainThumb != null)
            {
                UpdateMainThumb(enemyData.ImagePath,0,0,1.0f);
            }
            nameText?.SetText(enemyData.Name);
        }

        public void UpdateGridKey(int index)
        {
            var textId = 16800 + index;
            gridKey.text = DataSystem.GetText(textId);
        }

        public void Clear()
        {
            if (mainThumb != null)
            {
                mainThumb.gameObject.SetActive(false);
            }
            //nameText?.SetText("");
        }

        private void HideActorOnly()
        {
            foreach (var actorOnlyGameObject in actorOnlyGameObjects)
            {
                actorOnlyGameObject.SetActive(false);
            }
        }

        private void UpdateNeedStatus(BattlerInfo battlerInfo)
        {
            if (needStatusInfoComponent != null)
            {
                var NeedStatus = new StatusInfo();
                NeedStatus.SetParameter(
                    battlerInfo.EnemyData.HpGrowth,
                    battlerInfo.EnemyData.MpGrowth,
                    battlerInfo.EnemyData.AtkGrowth,
                    battlerInfo.EnemyData.DefGrowth,
                    battlerInfo.EnemyData.SpdGrowth
                    );
                needStatusInfoComponent.UpdateInfo(NeedStatus);
            }
        }
    }
}
