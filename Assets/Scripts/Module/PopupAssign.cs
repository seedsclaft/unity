using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopupAssign : MonoBehaviour
{
    [SerializeField] private GameObject confirmRoot = null;
    [SerializeField] private GameObject confirmPrefab = null;
    [SerializeField] private GameObject skillDetailPrefab = null;
    [SerializeField] private GameObject rulingPrefab = null;
    [SerializeField] private GameObject optionPrefab = null;
    [SerializeField] private GameObject rankingPrefab = null;
    [SerializeField] private GameObject creditPrefab = null;
    
    public GameObject CreatePopup(PopupType popupType)
    {
        var prefab = Instantiate(GetPopupObject(popupType));
        prefab.transform.SetParent(confirmRoot.transform, false);
        confirmRoot.gameObject.SetActive(false);
        return prefab;
    }

    private GameObject GetPopupObject(PopupType popupType)
    {
        switch (popupType)
        {
            case PopupType.Confirm:
            return confirmPrefab;
            case PopupType.SkillDetail:
            return skillDetailPrefab;
            case PopupType.Ruling:
            return rulingPrefab;
            case PopupType.Option:
            return optionPrefab;
            case PopupType.Ranking:
            return rankingPrefab;
            case PopupType.Credit:
            return creditPrefab;
        }
        return null;
    }
}

public enum PopupType{
    Confirm,
    SkillDetail,
    Ruling,
    Option,
    Ranking,
    Credit
}