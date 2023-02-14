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
    }

    private void DestroyChild(GameObject gameObject)
    {
        foreach(Transform child in gameObject.transform){
            Destroy(child.gameObject);
        }
    }

    public void StartDamage(DamageType damageType,int value)
    {
        UpdateAllHide();
        _busy = true;
        string result = value.ToString();
        List<GameObject> _damageList = new List<GameObject>();
        for (int i = 0; i < result.Count(); i++)
        {   
            GameObject prefab = Instantiate(GetPrefabType(damageType));
            prefab.transform.SetParent(GetRootType(damageType).transform, false);
            TextMeshProUGUI textMeshProUGUI = prefab.GetComponent<TextMeshProUGUI>();
            textMeshProUGUI.text = result[i].ToString();
            _damageList.Add(prefab);
        }

        for (int i = _damageList.Count-1; i >= 0; i--)
        {   
            TextMeshProUGUI textMeshProUGUI = _damageList[i].GetComponent<TextMeshProUGUI>();
            textMeshProUGUI.alpha = 0;
            textMeshProUGUI.DOFade(0.0f, 0.0f)
                .SetDelay(1.8f)
                .SetEase(Ease.InOutQuad)
                .OnComplete(() => {
                    _busy = false;
                    textMeshProUGUI.DOFade(0.0f, 0.2f);
                });
            Sequence sequence = DOTween.Sequence()
                .SetDelay(i * 0.05f)
                .Append(textMeshProUGUI.DOFade(1.0f, 0.1f))
                .SetEase(Ease.InOutQuad)
                .Append(textMeshProUGUI.gameObject.transform.DOLocalMoveY(16, 0.1f))
                .SetDelay(0.1f)
                .SetEase(Ease.InOutQuad)
                .Append(textMeshProUGUI.gameObject.transform.DOLocalMoveY(0, 0.2f))
                .SetDelay(0.25f)
                .SetEase(Ease.InOutQuad)
                .Append(textMeshProUGUI.gameObject.transform.DOLocalMoveY(4, 0.05f))
                .SetDelay(0.05f)
                .SetEase(Ease.InOutQuad)
                .Append(textMeshProUGUI.gameObject.transform.DOLocalMoveY(0, 0.05f))
                .OnComplete(() => Debug.Log("Completed"));
        }
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
        }
        return null;
    }
}
