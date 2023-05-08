using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using TMPro;

public class ActorInfoComponent : MonoBehaviour
{
    [SerializeField] private Image mainThumb;
    [SerializeField] private Image awakenThumb;
    [SerializeField] private Material grayscale;
    [SerializeField] private Image faceThumb;
    public Image FaceThumb {get {return faceThumb;}}
    [SerializeField] private Image awakenFaceThumb;
    public Image AwakenFaceThumb {get {return awakenFaceThumb;}}
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI element;
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
    public void UpdateInfo(ActorInfo actorInfo,List<ActorInfo> actorInfos)
    {
        if (actorInfo == null){
            return;
        }
        var actorData = DataSystem.Actors.Find(actor => actor.Id == actorInfo.ActorId);
        
        UpdateData(actorData);
        if (mainThumb != null){
            if (actorInfo.CurrentHp == 0 && actorInfo.InBattle || actorInfo.Lost)
            {
                UpdateLostMainThumb();
            }
        }
        if (demigod != null){
            demigod.text = actorInfo.DemigodParam.ToString();
        }
        if (lv != null){
            lv.text = actorInfo.Level.ToString();
        }
        if (sp != null){
            sp.text = actorInfo.Sp.ToString();
        }
        if (statusInfoComponent != null){
            statusInfoComponent.UpdateInfo(actorInfo.CurrentStatus);
            statusInfoComponent.UpdateHp(actorInfo.CurrentHp,actorInfo.MaxHp);
            statusInfoComponent.UpdateMp(actorInfo.CurrentMp,actorInfo.MaxMp);
        }
        if (needStatusInfoComponent != null){
            StatusInfo statusInfo = new StatusInfo();
            statusInfo.SetParameter(
                actorInfo.UsePoint.GetParameter(StatusParamType.Hp),
                actorInfo.UsePoint.GetParameter(StatusParamType.Mp),
                actorInfo.UsePoint.GetParameter(StatusParamType.Atk),
                actorInfo.UsePoint.GetParameter(StatusParamType.Def),
                actorInfo.UsePoint.GetParameter(StatusParamType.Spd)
            );
            var _currentAlcana = GameSystem.CurrentData.CurrentAlcana;
            if (_currentAlcana != null)
            {
                if (_currentAlcana.IsStatusCostDown(StatusParamType.Hp))
                {
                    statusInfo.AddParameter(StatusParamType.Hp,-1);
                } else
                if (_currentAlcana.IsStatusCostDown(StatusParamType.Mp))
                {
                    statusInfo.AddParameter(StatusParamType.Mp,-1);
                } else
                if (_currentAlcana.IsStatusCostDown(StatusParamType.Atk))
                {
                    statusInfo.AddParameter(StatusParamType.Atk,-1);
                } else
                if (_currentAlcana.IsStatusCostDown(StatusParamType.Def))
                {
                    statusInfo.AddParameter(StatusParamType.Def,-1);
                } else
                if (_currentAlcana.IsStatusCostDown(StatusParamType.Spd))
                {
                    statusInfo.AddParameter(StatusParamType.Spd,-1);
                }
            }
            needStatusInfoComponent.UpdateInfo(statusInfo);
        }
        if (element1 != null){
            UpdateAttributeParam(element1,actorInfo.Attribute[0]);
        }
        if (element2 != null){
            UpdateAttributeParam(element2,actorInfo.Attribute[1]);
        }
        if (element3 != null){
            UpdateAttributeParam(element3,actorInfo.Attribute[2]);
        }
        if (element4 != null){
            UpdateAttributeParam(element4,actorInfo.Attribute[3]);
        }
        if (element5 != null){
            UpdateAttributeParam(element5,actorInfo.Attribute[4]);
        }
        
    }


    public void UpdateData(ActorsData.ActorData actorData)
    {

        if (mainThumb != null){
            UpdateMainThumb(actorData.ImagePath,actorData.X,actorData.Y,actorData.Scale);

        }
        if (awakenThumb != null){
            UpdateAwakenThumb(actorData.ImagePath,actorData.AwakenX,actorData.AwakenY,actorData.AwakenScale);
        }
        if (faceThumb != null){
            UpdateMainFaceThumb(actorData.ImagePath);
        }
        if (awakenFaceThumb != null){
            UpdateAwakenFaceThumb(actorData.ImagePath);
        }
        if (nameText != null){
            nameText.text = actorData.Name;
        }
        if (element != null){
            //element.text = actorData.Name;
        }
    }
    private void UpdateMainThumb(string imagePath,int x,int y,float scale)
    {
        //var handle = await ResourceSystem.LoadAsset<Sprite>("Texture/Character/Actors/" + imagePath + "/Main.png");
        var handle = Resources.Load<Sprite>("Texture/Character/Actors/" + imagePath + "/Main");
        if (mainThumb != null)
        {
            RectTransform rect = mainThumb.GetComponent < RectTransform > ();
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
            RectTransform rect = awakenThumb.GetComponent < RectTransform > ();
            rect.localPosition = new Vector3(x, y, 0);
            rect.localScale = new Vector3(scale, scale, 1);
            awakenThumb.sprite = handle;
        }
    }

    private void UpdateMainFaceThumb(string imagePath)
    {
        //var handle = await ResourceSystem.LoadAsset<Sprite>("Texture/Character/Actors/" + imagePath + "/MainFace.png");
        var handle = Resources.Load<Sprite>("Texture/Character/Actors/" + imagePath + "/MainFace");
        if (faceThumb != null) faceThumb.sprite = handle;
    }

    private void UpdateAwakenFaceThumb(string imagePath)
    {
        //var handle = await ResourceSystem.LoadAsset<Sprite>("Texture/Character/Actors/" + imagePath + "/AwakenFace.png");
        var handle = Resources.Load<Sprite>("Texture/Character/Actors/" + imagePath + "/AwakenFace");
        if (awakenFaceThumb != null)
        {
            awakenFaceThumb.sprite = handle;
        }
    }

    private void UpdateAttributeParam(TextMeshProUGUI textMeshProUGUI,int param){
        string attributeParam = "G";
        if (param > 100){
            attributeParam = "S";
        } else
        if (param > 80){
            attributeParam = "A";
        } else
        if (param > 60){
            attributeParam = "B";
        } else
        if (param > 40){
            attributeParam = "C";
        } else
        if (param > 20){
            attributeParam = "D";
        } else
        if (param > 10){
            attributeParam = "E";
        } else
        if (param > 0){
            attributeParam = "F";
        }
        textMeshProUGUI.text = attributeParam;
    }

    public void ChangeHp(int value,int maxHp)
    {
        if (statusInfoComponent == null) return;
        statusInfoComponent.UpdateHp(value,maxHp);
    }

    public void ChangeMp(int value,int maxMp)
    {
        if (statusInfoComponent == null) return;
        statusInfoComponent.UpdateMp(value,maxMp);
    }

    public void ShowUI()
    {
        if (statusInfoComponent == null) return;
        statusInfoComponent.gameObject.SetActive(true);
    }

    public void HideUI()
    {
        if (statusInfoComponent == null) return;
        statusInfoComponent.gameObject.SetActive(false);
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
        if (mainThumb != null){
            mainThumb.sprite = null;
        }
        if (awakenThumb != null){
            awakenThumb.sprite = null;
        }
    }
}
