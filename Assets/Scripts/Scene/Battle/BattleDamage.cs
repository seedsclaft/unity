using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;

public class BattleDamage : MonoBehaviour
{
    [SerializeField] private List<TextMeshProUGUI> hpDamageList;
    [SerializeField] private List<TextMeshProUGUI> hpCriticalList;
    [SerializeField] private List<TextMeshProUGUI> hpHealList;
    [SerializeField] private List<TextMeshProUGUI> mpHealList;

    private void Awake() {
        UpdateAllHide();
    }

    public void UpdateAllHide()
    {
        hpCriticalList.ForEach(a => a.gameObject.SetActive(false));
        hpDamageList.ForEach(a => a.gameObject.SetActive(false));
        hpHealList.ForEach(a => a.gameObject.SetActive(false));
        mpHealList.ForEach(a => a.gameObject.SetActive(false));
    }

    public void StartDamage(DamageType damageType,int damage)
    {
        UpdateAllHide();
        string result = damage.ToString();
        for (int i = 0; i < result.Count(); i++)
        {   
            TextMeshProUGUI textMeshProUGUI = GetListType(damageType)[i];
            textMeshProUGUI.text = result[i].ToString();
            textMeshProUGUI.gameObject.SetActive(true);
        }
    }

    private List<TextMeshProUGUI> GetListType(DamageType damageType)
    {
        switch (damageType)
        {
            case DamageType.HpDamage:
            return hpDamageList;
            case DamageType.HpCritical:
            return hpCriticalList;
            case DamageType.HpHeal:
            return hpHealList;
            case DamageType.MpHeal:
            return mpHealList;
        }
        return null;
    }
}
