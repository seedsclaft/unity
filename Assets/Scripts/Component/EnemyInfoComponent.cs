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
        [SerializeField] private Image gridThumb;
        public Image MainThumb => mainThumb;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI lv;
        [SerializeField] private StatusInfoComponent statusInfoComponent;
        [SerializeField] private StatusInfoComponent needStatusInfoComponent;
        [SerializeField] private TextMeshProUGUI gridKey;
        [SerializeField] private List<GameObject> actorOnlyGameObjects;
        [SerializeField] private List<SkillAttributeItem> weakPoints;

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
                UpdateGridKey(battlerInfo.EnemyIndex.Value);
            }
            if (weakPoints != null)
            {
                UpdateWeakPoints(battlerInfo.WeakPoints);
            }
        }

        private void UpdateMainThumb(Image image,string imagePath,int x,int y,float scale,bool nativeSize)
        {
            //var handle = await ResourceSystem.LoadAsset<Sprite>("Enemies/" + imagePath);
            var handle = ResourceSystem.LoadEnemySprite(imagePath);
            if (image != null)
            {
                image.gameObject.SetActive(true);
                var rect = image.GetComponent<RectTransform>();
                rect.localPosition = new Vector3(x, y, 0);
                rect.localScale = new Vector3(scale, scale, 1);
                image.sprite = handle;
                if (nativeSize)
                {
                    image.SetNativeSize();
                }
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
                UpdateMainThumb(mainThumb,enemyData.ImagePath,0,0,1.0f,false);
            }
            if (gridThumb != null)
            {
                UpdateMainThumb(gridThumb,enemyData.ImagePath,0,0,1.0f,true);
            }
            nameText?.SetText(enemyData.Name);
        }

        public void UpdateGridKey(int index)
        {
            var textId = 16800 + index;
            gridKey.text = DataSystem.GetText(textId);
        }

        private void UpdateWeakPoints(List<KindType> kindTypes)
        {
            for (int i = 0;i < weakPoints.Count;i++)
            {
                weakPoints[i].gameObject.SetActive(kindTypes.Count > i);
                if (kindTypes.Count <= i)
                {
                    continue;
                }
                weakPoints[i].SetListData(new ListData(kindTypes[i]),i);
                weakPoints[i].UpdateViewItem();
                weakPoints[i].SetUnSelect();
            }
        }

        public void Clear()
        {
            if (mainThumb != null)
            {
                mainThumb.gameObject.SetActive(false);
            }
            nameText?.SetText("");
            //lv?.SetText("");
            gridKey?.SetText("");
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
