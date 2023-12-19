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
    [SerializeField] private Color normalColor;
    [SerializeField] private Color upperColor;
    [SerializeField] private Color downColor;
    [SerializeField] private CanvasGroup canvasGroup;
    public void UpdateInfo(StatusInfo statusInfo,StatusInfo baseStatus = null)
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
            if (baseStatus != null)
            {
                ChangeTextColor(atk,statusInfo.Atk,baseStatus.Atk);
            }
        }
        if (def != null)
        {
            def.text = statusInfo.Def.ToString();
            if (baseStatus != null)
            {
                ChangeTextColor(def,statusInfo.Def,baseStatus.Def);
            }
        }
        if (spd != null)
        {
            spd.text = statusInfo.Spd.ToString();
            if (baseStatus != null)
            {
                ChangeTextColor(spd,statusInfo.Spd,baseStatus.Spd);
            }
        }
    }

    private void ChangeTextColor(TextMeshProUGUI text,int currentStatus,int baseStatus)
    {
        if (currentStatus > baseStatus)
        {
            text.color = upperColor;        
        } else
        if (currentStatus < baseStatus)
        {
            text.color = downColor;
        } else
        {
            text.color = normalColor;
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
            var rate = 0f;
            if (currentHp > 0)
            {
                rate = (float)currentHp / (float)maxStatusHp;
            }
            hpGaugeAnimation.UpdateGauge(80,3,rate);
        }
    }
    public void UpdateMp(int currentMp,int maxStatusMp)
    {
        if (currentMp < 0)
        {
            currentMp = 0;
        }
        if (currentMp > maxStatusMp)
        {
            currentMp = maxStatusMp;
        }
        if (mp != null){
            mp.text = currentMp.ToString();
        }
        if (maxMp != null){
            maxMp.text = maxStatusMp.ToString();
        }
        if (mpGaugeAnimation != null)
        {
            var rate = 0f;
            if (currentMp > 0)
            {
                rate = (float)currentMp / (float)maxStatusMp;
            }
            mpGaugeAnimation.SetGaugeAnimation(maxStatusMp * 1.5f,3,rate);
            mpGaugeAnimation.UpdateGauge(maxStatusMp * 1.5f,3,rate);
        }
    }

    public void UpdateHpAnimation(int fromHp,int currentHp,int maxStatusHp)
    {
        if (hpGaugeAnimation != null)
        {
            var fromRate = 0f;
            if (fromHp > 0)
            {
                fromRate = (float)fromHp / (float)maxStatusHp;
            }
            hpGaugeAnimation.SetGaugeAnimation(80,3,fromRate);
            var rate = 0f;
            if (currentHp > 0)
            {
                rate = (float)currentHp / (float)maxStatusHp;
            }
            hpGaugeAnimation.UpdateGaugeAnimation(rate);
        }
    }

    public void UpdateMpAnimation(int fromMp,int currentMp,int maxStatusMp)
    {
        if (mpGaugeAnimation != null)
        {
            var fromRate = 0f;
            if (fromMp > 0)
            {
                fromRate = (float)fromMp / (float)maxStatusMp;
            }
            mpGaugeAnimation.SetGaugeAnimation(maxStatusMp * 1.5f,3,fromRate);
            var rate = 0f;
            if (currentMp > 0)
            {
                rate = (float)currentMp / (float)maxStatusMp;
            }
            mpGaugeAnimation.UpdateGaugeAnimation(rate);
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
