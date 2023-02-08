﻿using System.Collections;
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
    [SerializeField] private Image awakenFaceThumb;
    [SerializeField] private TextMeshProUGUI name;
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
            //UpdateMainThumb(actorData.ImagePath);
        }
        if (awakenThumb != null){
            //UpdateAwakenThumb(actorData.ImagePath);
        }
        if (faceThumb != null){
            //UpdateMainFaceThumb(actorData.ImagePath);
        }
        if (awakenFaceThumb != null){
            //UpdateAwakenFaceThumb(actorData.ImagePath);
        }
        if (name != null){
            name.text = actorData.Name;
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
            //sp.text = actorData.Name;
        }
        if (statusInfoComponent != null){
            statusInfoComponent.UpdateInfo(actorInfo.Status);
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

    private void UpdateMainThumb(string imagePath)
    {
        Addressables.LoadAssetAsync<Sprite>(
            "Assets/Images/Actors/" + imagePath + "/Main.png"
        ).Completed += op => {
            mainThumb.sprite = op.Result;
        };
    }

    private void UpdateAwakenThumb(string imagePath)
    {
        Addressables.LoadAssetAsync<Sprite>(
            "Assets/Images/Actors/" + imagePath + "/Awaken.png"
        ).Completed += op => {
            mainThumb.sprite = op.Result;
        };
    }

    private void UpdateMainFaceThumb(string imagePath)
    {
        Addressables.LoadAssetAsync<Sprite>(
            "Assets/Images/Actors/" + imagePath + "/MainFace.png"
        ).Completed += op => {
            mainThumb.sprite = op.Result;
        };
    }
    private void UpdateAwakenFaceThumb(string imagePath)
    {
        Addressables.LoadAssetAsync<Sprite>(
            "Assets/Images/Actors/" + imagePath + "/AwakenFace.png"
        ).Completed += op => {
            mainThumb.sprite = op.Result;
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
}
