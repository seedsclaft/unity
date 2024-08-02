using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Ryneus
{
    public class ActorInfoComponent : MonoBehaviour
    {
        [SerializeField] private Image mainThumb;
        public Image MainThumb => mainThumb;
        [SerializeField] private Image awakenThumb;
        public Image AwakenThumb => awakenThumb;
        [SerializeField] private Material grayscale;
        [SerializeField] private Image faceThumb;
        public Image FaceThumb => faceThumb;
        [SerializeField] private Image awakenFaceThumb;
        public Image AwakenFaceThumb => awakenFaceThumb;
        [SerializeField] private Image clipThumb;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI subNameText;
        [SerializeField] private TextMeshProUGUI evaluate;
        [SerializeField] private TextMeshProUGUI demigod;
        [SerializeField] private TextMeshProUGUI lv;
        [SerializeField] private TextMeshProUGUI sp;
        [SerializeField] private StatusInfoComponent statusInfoComponent;
        [SerializeField] private StatusInfoComponent needStatusInfoComponent;
        
        [SerializeField] private TextMeshProUGUI element1;
        [SerializeField] private TextMeshProUGUI element2;
        [SerializeField] private TextMeshProUGUI element3;
        [SerializeField] private TextMeshProUGUI element4;
        [SerializeField] private TextMeshProUGUI element5;

        [SerializeField] private TextMeshProUGUI element1Cost;
        [SerializeField] private TextMeshProUGUI element2Cost;
        [SerializeField] private TextMeshProUGUI element3Cost;
        [SerializeField] private TextMeshProUGUI element4Cost;
        [SerializeField] private TextMeshProUGUI element5Cost;

        [SerializeField] private TextMeshProUGUI recoveryCost;
        [SerializeField] private TextMeshProUGUI resourceGain;
        [SerializeField] private TextMeshProUGUI addTiming;

        public void UpdateInfo(ActorInfo actorInfo,List<ActorInfo> actorInfos)
        {
            if (actorInfo == null)
            {
                return;
            }
            var actorData = actorInfo.Master;
            
            UpdateData(actorData);
            if (mainThumb != null)
            {
                if (actorInfo.CurrentHp == 0 && actorInfo.BattleIndex >= 0 || actorInfo.Lost)
                {
                    UpdateLostMainThumb();
                }
            }
            demigod?.SetText(actorInfo.DemigodParam.ToString());
            lv?.SetText(actorInfo.Level.ToString());
            if (sp != null){
            }
            if (statusInfoComponent != null)
            {
                statusInfoComponent.UpdateInfo(actorInfo.CurrentStatus);
                statusInfoComponent.UpdateHp(actorInfo.CurrentHp,actorInfo.MaxHp);
                statusInfoComponent.UpdateMp(actorInfo.CurrentMp,actorInfo.MaxMp);
            }
            if (needStatusInfoComponent != null)
            {
                needStatusInfoComponent.UpdateInfo(actorData.NeedStatus);
            }
            if (element1 != null)
            {
                UpdateAttributeRank(element1,actorInfo,AttributeType.Fire,actorInfos);
            }
            if (element2 != null)
            {
                UpdateAttributeRank(element2,actorInfo,AttributeType.Thunder,actorInfos);
            }
            if (element3 != null)
            {
                UpdateAttributeRank(element3,actorInfo,AttributeType.Ice,actorInfos);
            }
            if (element4 != null)
            {
                UpdateAttributeRank(element4,actorInfo,AttributeType.Shine,actorInfos);
            }
            if (element5 != null)
            {
                UpdateAttributeRank(element5,actorInfo,AttributeType.Dark,actorInfos);
            }
            element1Cost?.SetText(TacticsUtility.LearningMagicCost(actorInfo,AttributeType.Fire,actorInfos).ToString());
            element2Cost?.SetText(TacticsUtility.LearningMagicCost(actorInfo,AttributeType.Thunder,actorInfos).ToString());
            element3Cost?.SetText(TacticsUtility.LearningMagicCost(actorInfo,AttributeType.Ice,actorInfos).ToString());
            element4Cost?.SetText(TacticsUtility.LearningMagicCost(actorInfo,AttributeType.Shine,actorInfos).ToString());
            element5Cost?.SetText(TacticsUtility.LearningMagicCost(actorInfo,AttributeType.Dark,actorInfos).ToString());
            
            recoveryCost?.SetText(TacticsUtility.RemainRecoveryCost(actorInfo,true).ToString());
            resourceGain?.SetText(TacticsUtility.ResourceGain(actorInfo).ToString());
            evaluate?.SetText(actorInfo.Evaluate().ToString());
            addTiming?.SetText(actorInfo.AddTiming.ToString());
        }

        private void UpdateAttributeRank(TextMeshProUGUI text,ActorInfo actorInfo,AttributeType attributeType,List<ActorInfo> actorInfos)
        {
            if (actorInfos != null)
            {
                UpdateAttributeParam(text,actorInfo.AttributeRanks(actorInfos)[(int)attributeType-1]);
            } else
            {
                UpdateAttributeParam(text,actorInfo.GetAttributeRank()[(int)attributeType-1]);
            }
        }


        public void UpdateData(ActorData actorData)
        {
            if (mainThumb != null)
            {
                UpdateMainThumb(actorData.ImagePath,actorData.X,actorData.Y,actorData.Scale);
            }
            if (awakenThumb != null)
            {
                UpdateAwakenThumb(actorData.ImagePath,actorData.AwakenX,actorData.AwakenY,actorData.AwakenScale);
            }
            if (clipThumb != null)
            {
                UpdateClipThumb(actorData.ImagePath);
            }
            if (faceThumb != null)
            {
                UpdateMainFaceThumb(actorData.ImagePath);
            }
            if (awakenFaceThumb != null)
            {
                UpdateAwakenFaceThumb(actorData.ImagePath);
            }
            if (nameText != null)
            {
                nameText.text = actorData.Name;
            }
            if (subNameText != null)
            {
                subNameText.text = actorData.SubName;
            }
        }
        private void UpdateMainThumb(string imagePath,int x,int y,float scale)
        {
            var handle = ResourceSystem.LoadActorMainSprite(imagePath);
            if (mainThumb != null)
            {
                var rect = mainThumb.GetComponent<RectTransform>();
                rect.localPosition = new Vector3(x, y, 0);
                rect.localScale = new Vector3(scale, scale, 1);
                mainThumb.sprite = handle;
                rect.sizeDelta = new Vector3(mainThumb.mainTexture.width, mainThumb.mainTexture.height, 1);
            }
        }

        private void UpdateAwakenThumb(string imagePath,int x,int y,float scale)
        {
            var handle = ResourceSystem.LoadActorAwakenSprite(imagePath);
            if (awakenThumb != null)
            {
                var rect = awakenThumb.GetComponent<RectTransform>();
                rect.localPosition = new Vector3(x, y, 0);
                rect.localScale = new Vector3(scale, scale, 1);
                awakenThumb.sprite = handle;
                rect.sizeDelta = new Vector3(mainThumb.mainTexture.width, mainThumb.mainTexture.height, 1);
            }
        }

        private void UpdateClipThumb(string imagePath)
        {
            var handle = ResourceSystem.LoadActorClipSprite(imagePath);
            if (clipThumb != null) clipThumb.sprite = handle;
        }

        private void UpdateMainFaceThumb(string imagePath)
        {   
            var handle = ResourceSystem.LoadActorMainFaceSprite(imagePath);
            if (faceThumb != null) 
            {
                faceThumb.sprite = handle;
                faceThumb.gameObject.SetActive(true);
            }
        }

        private void UpdateAwakenFaceThumb(string imagePath)
        {
            if (awakenFaceThumb != null)
            {
                awakenFaceThumb.sprite = ResourceSystem.LoadActorAwakenFaceSprite(imagePath);
                awakenFaceThumb.gameObject.SetActive(true);
            }
        }

        private void UpdateAttributeParam(TextMeshProUGUI textMeshProUGUI,AttributeRank param){
            var textId = 321 + (int)param;
            textMeshProUGUI.text = DataSystem.GetText(textId);
        }
        
        public void SetAwakeMode(bool IsAwaken)
        {
            if (faceThumb != null && awakenFaceThumb != null)
            {
                faceThumb.gameObject.SetActive(!IsAwaken);
                awakenFaceThumb.gameObject.SetActive(IsAwaken);
            }
        }

        private void UpdateLostMainThumb()
        {
            if (mainThumb != null && grayscale != null)
            {
                mainThumb.material = grayscale;
            }
        }

        public void Clear()
        {
            if (mainThumb != null)
            {
                mainThumb.sprite = null;
                //mainThumb.gameObject.SetActive(false);
            }
            if (awakenThumb != null)
            {
                awakenThumb.sprite = null;
                //awakenThumb.gameObject.SetActive(false);
            }
            if (faceThumb != null)
            {
                faceThumb.sprite = null;
                faceThumb.gameObject.SetActive(false);
            }
            if (awakenFaceThumb != null)
            {
                awakenFaceThumb.sprite = null;
                awakenFaceThumb.gameObject.SetActive(false);
            }
        }
    }
}
