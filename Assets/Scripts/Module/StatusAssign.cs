using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatusAssign : MonoBehaviour
{
    [SerializeField] private GameObject statusRoot = null;
    public GameObject StatusRoot => statusRoot;
    [SerializeField] private GameObject statusPrefab = null;
    [SerializeField] private GameObject enemyDetailPrefab = null;
    public GameObject CreatePopup(StatusType popupType)
    {
        var prefab = Instantiate(GetStatusObject(popupType));
        prefab.transform.SetParent(statusRoot.transform, false);
        statusRoot.gameObject.SetActive(true);
        return prefab;
    }

    private GameObject GetStatusObject(StatusType popupType)
    {
        switch (popupType)
        {
            case StatusType.Status:
            return statusPrefab;
            case StatusType.EnemyDetail:
            return enemyDetailPrefab;
        }
        return null;
    }
}

public enum StatusType{
    Status,
    EnemyDetail,
}