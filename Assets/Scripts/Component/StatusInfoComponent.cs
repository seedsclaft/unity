using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using TMPro;

public class StatusInfoComponent : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI maxhp;
    [SerializeField] private TextMeshProUGUI hp;
    [SerializeField] private TextMeshProUGUI maxmp;
    [SerializeField] private TextMeshProUGUI mp;
    [SerializeField] private TextMeshProUGUI atk;
    [SerializeField] private TextMeshProUGUI def;
    [SerializeField] private TextMeshProUGUI spd;
    public void UpdateInfo(StatusInfo statusInfo)
    {
        if (statusInfo == null){
            return;
        }
        if (maxhp != null){
            maxhp.text = statusInfo.Hp.ToString();
        }
        if (maxmp != null){
            maxmp.text = statusInfo.Mp.ToString();
        }
        if (atk != null){
            atk.text = statusInfo.Atk.ToString();
        }
        if (def != null){
            def.text = statusInfo.Def.ToString();
        }
        if (spd != null){
            spd.text = statusInfo.Spd.ToString();
        }
        
    }
    public void UpdateHp(int currentHp)
    {
        if (hp != null){
            hp.text = currentHp.ToString();
        }
    }
    public void UpdateMp(int currentMp)
    {
        if (mp != null){
            mp.text = currentMp.ToString();
        }
    }
}
