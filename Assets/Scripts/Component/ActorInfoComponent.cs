using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using TMPro;

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
    private bool _isMainFaceInit = false;
    private bool _isAwakeFaceInit = false;

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
            if (actorInfo.CurrentHp == 0 && actorInfo.InBattle || actorInfo.Lost)
            {
                UpdateLostMainThumb();
            }
        }
        if (demigod != null)
        {
            demigod.text = actorInfo.DemigodParam.ToString();
        }
        if (lv != null)
        {
            lv.text = actorInfo.Level.ToString();
        }
        if (sp != null){
            sp.text = actorInfo.Sp.ToString();
        }
        if (statusInfoComponent != null)
        {
            statusInfoComponent.UpdateInfo(actorInfo.CurrentStatus);
            statusInfoComponent.UpdateHp(actorInfo.CurrentHp,actorInfo.MaxHp);
            statusInfoComponent.UpdateMp(actorInfo.CurrentMp,actorInfo.MaxMp);
        }
        if (needStatusInfoComponent != null)
        {
            var statusInfo = new StatusInfo();
            statusInfo.SetParameter(
                actorInfo.UsePointCost(StatusParamType.Hp),
                actorInfo.UsePointCost(StatusParamType.Mp),
                actorInfo.UsePointCost(StatusParamType.Atk),
                actorInfo.UsePointCost(StatusParamType.Def),
                actorInfo.UsePointCost(StatusParamType.Spd)
            );
            needStatusInfoComponent.UpdateInfo(statusInfo);
        }
        if (element1 != null)
        {
            UpdateAttributeParam(element1,actorInfo.Attribute[0]);
        }
        if (element2 != null)
        {
            UpdateAttributeParam(element2,actorInfo.Attribute[1]);
        }
        if (element3 != null)
        {
            UpdateAttributeParam(element3,actorInfo.Attribute[2]);
        }
        if (element4 != null)
        {
            UpdateAttributeParam(element4,actorInfo.Attribute[3]);
        }
        if (element5 != null)
        {
            UpdateAttributeParam(element5,actorInfo.Attribute[4]);
        }
        if (element1Cost != null)
        {
            element1Cost.text = TacticsUtility.AlchemyCost(actorInfo,AttributeType.Fire,actorInfos).ToString();
        }
        if (element2Cost != null)
        {
            element2Cost.text = TacticsUtility.AlchemyCost(actorInfo,AttributeType.Thunder,actorInfos).ToString();
        }
        if (element3Cost != null)
        {
            element3Cost.text = TacticsUtility.AlchemyCost(actorInfo,AttributeType.Ice,actorInfos).ToString();
        }
        if (element4Cost != null)
        {
            element4Cost.text = TacticsUtility.AlchemyCost(actorInfo,AttributeType.Shine,actorInfos).ToString();
        }
        if (element5Cost != null)
        {
            element5Cost.text = TacticsUtility.AlchemyCost(actorInfo,AttributeType.Dark,actorInfos).ToString();
        }
        if (recoveryCost != null)
        {
            recoveryCost.text = TacticsUtility.RecoveryCost(actorInfo).ToString();
        }
        if (resourceGain != null)
        {
            resourceGain.text = "+" + TacticsUtility.ResourceGain(actorInfo).ToString();
        }
        
        if (evaluate != null)
        {
            evaluate.text = actorInfo.Evaluate().ToString();
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
        //var handle = await ResourceSystem.LoadAsset<Sprite>("Texture/Character/Actors/" + imagePath + "/Main.png");
        var handle = Resources.Load<Sprite>("Texture/Character/Actors/" + imagePath + "/Main");
        if (mainThumb != null)
        {
            var rect = mainThumb.GetComponent<RectTransform>();
            rect.localPosition = new Vector3(x, y, 0);
            rect.localScale = new Vector3(scale, scale, 1);
            mainThumb.sprite = handle;
        }
    }

    private void UpdateAwakenThumb(string imagePath,int x,int y,float scale)
    {
        //var handle = await ResourceSystem.LoadAsset<Sprite>("Texture/Character/Actors/" + imagePath + "/Awaken.png");
        var handle = Resources.Load<Sprite>("Texture/Character/Actors/" + imagePath + "/Awaken");
        if (awakenThumb != null)
        {
            var rect = awakenThumb.GetComponent<RectTransform>();
            rect.localPosition = new Vector3(x, y, 0);
            rect.localScale = new Vector3(scale, scale, 1);
            awakenThumb.sprite = handle;
        }
    }

    private void UpdateClipThumb(string imagePath)
    {
        var handle = Resources.Load<Sprite>("Texture/Character/Actors/" + imagePath + "/Clip");
        if (clipThumb != null) clipThumb.sprite = handle;
    }

    private void UpdateMainFaceThumb(string imagePath)
    {   
        if (_isMainFaceInit == true) return;
        //var handle = await ResourceSystem.LoadAsset<Sprite>("Texture/Character/Actors/" + imagePath + "/MainFace.png");
        var handle = Resources.Load<Sprite>("Texture/Character/Actors/" + imagePath + "/MainFace");
        if (faceThumb != null) 
        {
            faceThumb.sprite = handle;
            faceThumb.gameObject.SetActive(true);
        }
        _isMainFaceInit = true;
    }

    private void UpdateAwakenFaceThumb(string imagePath)
    {
        if (_isAwakeFaceInit == true) return;
        //var handle = await ResourceSystem.LoadAsset<Sprite>("Texture/Character/Actors/" + imagePath + "/AwakenFace.png");
        var handle = Resources.Load<Sprite>("Texture/Character/Actors/" + imagePath + "/AwakenFace");
        if (awakenFaceThumb != null)
        {
            awakenFaceThumb.sprite = handle;
            awakenFaceThumb.gameObject.SetActive(true);
        }
        _isAwakeFaceInit = true;
    }

    private void UpdateAttributeParam(TextMeshProUGUI textMeshProUGUI,AttributeRank param){
        var textId = 321 + (int)param;
        textMeshProUGUI.text = DataSystem.System.GetTextData(textId).Text;
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
        _isAwakeFaceInit = false;
        _isMainFaceInit = false;
    }
}
