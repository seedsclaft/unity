using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StatusInfoComponent : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI maxHp;
    [SerializeField] private TextMeshProUGUI hp;
    [SerializeField] private TextMeshProUGUI maxMp;
    [SerializeField] private TextMeshProUGUI mp;
    [SerializeField] private TextMeshProUGUI atk;
    [SerializeField] private TextMeshProUGUI def;
    [SerializeField] private TextMeshProUGUI spd;
    [SerializeField] private StatusGaugeAnimation hpGaugeAnimation;
    [SerializeField] private StatusGaugeAnimation mpGaugeAnimation;
    [SerializeField] private CanvasGroup canvasGroup;
    public void UpdateInfo(StatusInfo statusInfo)
    {
        if (statusInfo == null){
            return;
        }
        if (maxHp != null)
        {
            maxHp.text = statusInfo.Hp.ToString();
        }
        if (maxMp != null)
        {
            maxMp.text = statusInfo.Mp.ToString();
        }
        if (atk != null)
        {
            atk.text = statusInfo.Atk.ToString();
        }
        if (def != null)
        {
            def.text = statusInfo.Def.ToString();
        }
        if (spd != null)
        {
            spd.text = statusInfo.Spd.ToString();
        }
        
    }
    public void UpdateHp(int currentHp,int maxStatusHp)
    {
        if (currentHp < 0)
        {
            currentHp = 0;
        }
        if (currentHp > maxStatusHp)
        {
            currentHp = maxStatusHp;
        }
        if (hp != null){
            hp.text = currentHp.ToString();
        }
        if (maxHp != null){
            maxHp.text = maxStatusHp.ToString();
        }
        if (hpGaugeAnimation != null)
        {
            hpGaugeAnimation.UpdateGauge(80,3,(float)currentHp / (float)maxStatusHp);
        }
    }
    public void UpdateMp(int currentMp,int maxStatusMp)
    {
        if (currentMp < 0)
        {
            currentMp = 0;
        }
        if (mp != null){
            mp.text = currentMp.ToString();
        }
        if (maxMp != null){
            maxMp.text = maxStatusMp.ToString();
        }
        if (mpGaugeAnimation != null)
        {
            mpGaugeAnimation.UpdateGauge(maxStatusMp * 1.5f,3,(float)currentMp / (float)maxStatusMp);
        }
    }

    public void UpdateHpAnimation(int currentHp,int maxStatusHp)
    {
        if (hpGaugeAnimation != null)
        {
            hpGaugeAnimation.UpdateGaugeAnimation((float)currentHp / (float)maxStatusHp);
        }
    }

    public void UpdateMpAnimation(int currentMp,int maxStatusMp)
    {
        if (mpGaugeAnimation != null)
        {
            mpGaugeAnimation.UpdateGaugeAnimation((float)currentMp / (float)maxStatusMp);
        }
    }

    public void UpdateAtk(int value)
    {
        if (atk != null)
        {
            atk.text = value.ToString();
        }
    }

    public void UpdateDef(int value)
    {
        if (def != null)
        {
            def.text = value.ToString();
        }
    }

    public void UpdateSpd(int value)
    {
        if (spd != null)
        {
            spd.text = value.ToString();
        }
    }

    public void ShowStatus()
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1.0f;
        }
    }

    public void HideStatus()
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
        }
    }
}
