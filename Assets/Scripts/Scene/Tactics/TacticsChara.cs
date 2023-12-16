using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TacticsChara : MonoBehaviour
{
    [SerializeField] private ActorInfoComponent actorInfoComponent;
    [SerializeField] private GameObject status;
    [SerializeField] private StatusInfoComponent statusInfoComponent;
    [SerializeField] private Image bgImage;

    public void Initialize(GameObject parent,float x,float y,float scale)
    {
        status.transform.SetParent(parent.transform,false);
        status.GetComponent<RectTransform>().localPosition = new Vector3(x, y - (200 * scale), 0);
        //bgImage.GetComponent<RectTransform>().localPosition = new Vector3(x, y - (200 * scale), 0);
    }

    public void SetData(ActorInfo actorInfo)
    {
        actorInfoComponent.UpdateInfo(actorInfo,null);
        statusInfoComponent.UpdateInfo(actorInfo.CurrentStatus);
        statusInfoComponent.UpdateHp(actorInfo.CurrentHp,actorInfo.MaxHp);
        statusInfoComponent.UpdateMp(actorInfo.CurrentMp,actorInfo.MaxMp);
    }
}
