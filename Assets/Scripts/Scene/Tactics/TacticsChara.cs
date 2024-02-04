using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TacticsChara : MonoBehaviour
{
    [SerializeField] private ActorInfoComponent actorInfoComponent;
    [SerializeField] private GameObject status;

    private bool _isInit = false;
    public bool IsInit => _isInit;
    public void Initialize(GameObject parent,float x,float y,float scale)
    {
        status.transform.SetParent(parent.transform,false);
        status.GetComponent<RectTransform>().localPosition = new Vector3(x, y - (300 * scale), 0);
        _isInit = true;
    }

    public void SetData(ActorInfo actorInfo)
    {
        actorInfoComponent.UpdateInfo(actorInfo,null);
    }
}
