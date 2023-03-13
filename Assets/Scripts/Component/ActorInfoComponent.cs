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
    public void UpdateInfo(ActorInfo actorInfo)
    {
        if (actorInfo == null){
            return;
        }
        var actorData = DataSystem.Actors.Find(actor => actor.Id == actorInfo.ActorId);
        
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
        if (demigod != null){
            //demigod.text = actorData.Name;
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
            needStatusInfoComponent.UpdateInfo(actorInfo.UsePoint);
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

    private void UpdateMainThumb(string imagePath,int x,int y,float scale)
    {
        Addressables.LoadAssetAsync<Sprite>(
            "Assets/Images/Actors/" + imagePath + "/Main.png"
        ).Completed += op => {
            if (mainThumb != null)
            {
                RectTransform rect = mainThumb.GetComponent < RectTransform > ();
                rect.localPosition = new Vector3(x, y, 0);
                rect.localScale = new Vector3(scale, scale, 1);
                mainThumb.sprite = op.Result;
            }
        };
    }

    private void UpdateAwakenThumb(string imagePath,int x,int y,float scale)
    {
        Addressables.LoadAssetAsync<Sprite>(
            "Assets/Images/Actors/" + imagePath + "/Awaken.png"
        ).Completed += op => {
            RectTransform rect = awakenThumb.GetComponent < RectTransform > ();
            rect.localPosition = new Vector3(x, y, 0);
            rect.localScale = new Vector3(scale, scale, 1);
            awakenThumb.sprite = op.Result;
        };
    }

    private void UpdateMainFaceThumb(string imagePath)
    {
        Addressables.LoadAssetAsync<Sprite>(
            "Assets/Images/Actors/" + imagePath + "/MainFace.png"
        ).Completed += op => {
            faceThumb.sprite = op.Result;
        };
    }
    private void UpdateAwakenFaceThumb(string imagePath)
    {
        Addressables.LoadAssetAsync<Sprite>(
            "Assets/Images/Actors/" + imagePath + "/AwakenFace.png"
        ).Completed += op => {
            awakenFaceThumb.sprite = op.Result;
        };
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
