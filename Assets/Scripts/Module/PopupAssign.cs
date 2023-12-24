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
    [SerializeField] private GameObject characterListPrefab = null;
    
    private BaseView _popupView = null;
    public GameObject CreatePopup(PopupType popupType,HelpWindow helpWindow)
    {
        var prefab = Instantiate(GetPopupObject(popupType));
        prefab.transform.SetParent(confirmRoot.transform, false);
        confirmRoot.gameObject.SetActive(true);
        var view = prefab.GetComponent<BaseView>();
        view?.SetHelpWindow(helpWindow);
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
            case PopupType.CharacterList:
            return characterListPrefab;
        }
        return null;
    }

    public void CloseConfirm()
    {
        foreach(Transform child in confirmRoot.transform){
            Destroy(child.gameObject);
        }
        confirmRoot.gameObject.SetActive(false);
    }
}

public enum PopupType{
    Confirm,
    SkillDetail,
    Ruling,
    Option,
    Ranking,
    Credit,
    CharacterList
}