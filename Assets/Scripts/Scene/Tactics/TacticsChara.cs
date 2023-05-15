using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TacticsChara : MonoBehaviour
{
    [SerializeField] private ActorInfoComponent actorInfoComponent;
    [SerializeField] private GameObject statusRoot;
    [SerializeField] private GameObject statusPrefab;
    private StatusInfoComponent _statusInfoComponent;

    public void Initialize(GameObject root,float x,float y,float scale)
    {
        GameObject prefab = Instantiate(statusPrefab);
        prefab.transform.SetParent(root.transform, false);
        prefab.GetComponent<RectTransform>().localPosition = new Vector3(x, y - (200 * scale), 0);
        _statusInfoComponent = prefab.GetComponent<StatusInfoComponent>();
    }

    public void SetData(ActorInfo actorInfo)
    {
        actorInfoComponent.UpdateInfo(actorInfo,null);
        _statusInfoComponent.UpdateInfo(actorInfo.CurrentStatus);
        _statusInfoComponent.UpdateHp(actorInfo.CurrentHp,actorInfo.MaxHp);
        _statusInfoComponent.UpdateMp(actorInfo.CurrentMp,actorInfo.MaxMp);
    }
}
