using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using DG.Tweening;

public class BattleDamage : MonoBehaviour
{
    [SerializeField] private GameObject hpDamageRoot;
    [SerializeField] private GameObject hpDamagePrefab;
    [SerializeField] private GameObject hpCriticalRoot;
    [SerializeField] private GameObject hpCriticalPrefab;
    [SerializeField] private GameObject hpHealRoot;
    [SerializeField] private GameObject hpHealPrefab;
    [SerializeField] private GameObject mpHealRoot;
    [SerializeField] private GameObject mpHealPrefab;
    [SerializeField] private GameObject stateRoot;
    [SerializeField] private GameObject statePrefab;
    [SerializeField] private GameObject mpDamageRoot;
    [SerializeField] private GameObject mpDamagePrefab;
    //[SerializeField] private List<TextMeshProUGUI> mpHealList;

    private bool _busy = false;
    public bool IsBusy{
        get {return _busy;}
    }
    private void Awake() {
        UpdateAllHide();
    }

    public void UpdateAllHide()
    {
        DestroyChild(hpDamageRoot);
        DestroyChild(hpCriticalRoot);
        DestroyChild(hpHealRoot);
        DestroyChild(mpHealRoot);
        DestroyChild(stateRoot);
    }

    private void DestroyChild(GameObject gameObject)
    {
        foreach(Transform child in gameObject.transform){
            Destroy(child.gameObject);
        }
    }

    public void StartDamage(DamageType damageType,int value,System.Action endEvent,int delayCount)
    {
        UpdateAllHide();
        _busy = true;
        string result = value.ToString();
        var _damageList = new List<GameObject>();
        for (int i = 0; i < result.Count(); i++)
        {   
            var prefab = Instantiate(GetPrefabType(damageType));
            prefab.transform.SetParent(GetRootType(damageType).transform, false);
            var textMeshProUGUI = prefab.GetComponent<TextMeshProUGUI>();
            textMeshProUGUI.text = result[i].ToString();
            _damageList.Add(prefab);
        }

        for (int i = _damageList.Count-1; i >= 0; i--)
        {   
            var textMeshProUGUI = _damageList[i].GetComponent<TextMeshProUGUI>();
            textMeshProUGUI.alpha = 0;
            int delay = i + delayCount * 8;
            var sequence = DOTween.Sequence()
                .SetDelay(delay * 0.04f)
                .Append(textMeshProUGUI.DOFade(1.0f, 0.1f))
                .Append(textMeshProUGUI.gameObject.transform.DOLocalMoveY(16, 0.1f))
                .Append(textMeshProUGUI.gameObject.transform.DOLocalMoveY(0, 0.2f))
                .Append(textMeshProUGUI.gameObject.transform.DOLocalMoveY(4, 0.05f))
                .Append(textMeshProUGUI.gameObject.transform.DOLocalMoveY(0, 0.05f))
                .SetEase(Ease.InOutQuad)
                .OnComplete(() => {
                    var sequence = DOTween.Sequence()
                        .Append(textMeshProUGUI.DOFade(0.0f, 0.2f))
                        .OnComplete(() => {
                            _busy = false;
                            if (endEvent != null) endEvent(); 
                        });
                });
        }
    }

    public void StartHeal(DamageType damageType,int value,System.Action endEvent,int delayCount)
    {
        UpdateAllHide();
        _busy = true;
        var result = value.ToString();
        var _damageList = new List<GameObject>();
        for (int i = 0; i < result.Count(); i++)
        {   
            var prefab = Instantiate(GetPrefabType(damageType));
            prefab.transform.SetParent(GetRootType(damageType).transform, false);
            var textMeshProUGUI = prefab.GetComponent<TextMeshProUGUI>();
            textMeshProUGUI.text = result[i].ToString();
            _damageList.Add(prefab);
        }

        for (int i = _damageList.Count-1; i >= 0; i--)
        {   
            var textMeshProUGUI = _damageList[i].GetComponent<TextMeshProUGUI>();
            textMeshProUGUI.alpha = 0;
            int delay = i + delayCount * 8;
            var sequence = DOTween.Sequence()
                .SetDelay(delay * 0.04f)
                .Append(textMeshProUGUI.DOFade(1.0f, 0.1f))
                .Join(textMeshProUGUI.gameObject.transform.DOLocalMoveY(16, 0.1f))
                .Append(textMeshProUGUI.gameObject.transform.DOLocalMoveY(0, 0.2f))
                .SetDelay(0.04f)
                .Append(textMeshProUGUI.gameObject.transform.DOLocalMoveY(4, 0.05f))
                .Append(textMeshProUGUI.gameObject.transform.DOLocalMoveY(0, 0.05f))
                .SetEase(Ease.InOutQuad)
                .OnComplete(() => {
                    var sequence = DOTween.Sequence()
                        .Append(textMeshProUGUI.DOFade(0.0f, 0.2f))
                        .OnComplete(() => {
                            _busy = false;
                            if (endEvent != null) endEvent();
                        });
                });
        }
    }

    public void StartStatePopup(DamageType damageType,string stateName,float delay,System.Action endEvent)
    {
        var prefab = Instantiate(GetPrefabType(damageType));
        prefab.transform.SetParent(GetRootType(damageType).transform, false);
        var textMeshProUGUI = prefab.GetComponent<TextMeshProUGUI>();
        textMeshProUGUI.text = stateName;

        textMeshProUGUI.alpha = 0;
        var sequence = DOTween.Sequence()
            .SetDelay(delay * 0.5f)
            .Append(textMeshProUGUI.DOFade(1.0f, 0.1f))
            .Join(textMeshProUGUI.gameObject.transform.DOLocalMoveY(8, 0.2f))
            .Append(textMeshProUGUI.gameObject.transform.DOLocalMoveY(8, 1.0f))
            .Append(textMeshProUGUI.gameObject.transform.DOLocalMoveY(24, 0.2f))
            .Join(textMeshProUGUI.DOFade(0.0f, 0.1f))
            .SetEase(Ease.InOutQuad)
            .OnComplete(() => {
                var sequence = DOTween.Sequence()
                    .OnComplete(() => {
                        _busy = false;
                        if (endEvent != null) endEvent();
                    });
            });
    }

    private GameObject GetPrefabType(DamageType damageType)
    {
        switch (damageType)
        {
            case DamageType.HpDamage:
            return hpDamagePrefab;
            case DamageType.HpCritical:
            return hpCriticalPrefab;
            case DamageType.HpHeal:
            return hpHealPrefab;
            case DamageType.MpHeal:
            return mpHealPrefab;
            case DamageType.State:
            return statePrefab;
            case DamageType.MpDamage:
            return mpDamagePrefab;
        }
        return null;
    }
    
    private GameObject GetRootType(DamageType damageType)
    {
        switch (damageType)
        {
            case DamageType.HpDamage:
            return hpDamageRoot;
            case DamageType.HpCritical:
            return hpCriticalRoot;
            case DamageType.HpHeal:
            return hpHealRoot;
            case DamageType.MpHeal:
            return mpHealRoot;
            case DamageType.State:
            return stateRoot;
            case DamageType.MpDamage:
            return mpDamageRoot;
        }
        return null;
    }
}
