using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using TMPro;
using DG.Tweening;

public class StatusInfoComponent : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI maxhp;
    [SerializeField] private TextMeshProUGUI hp;
    [SerializeField] private TextMeshProUGUI maxmp;
    [SerializeField] private TextMeshProUGUI mp;
    [SerializeField] private TextMeshProUGUI atk;
    [SerializeField] private TextMeshProUGUI def;
    [SerializeField] private TextMeshProUGUI spd;
    [SerializeField] private Image hpGaugeBg;
    [SerializeField] private Image hpGauge;
    [SerializeField] private Image hpGaugeAnimation;
    [SerializeField] private Image mpGaugeBg;
    [SerializeField] private Image mpGauge;
    [SerializeField] private Image mpGaugeAnimation;
    [SerializeField] private CanvasGroup canvasGroup;
    public void UpdateInfo(StatusInfo statusInfo)
    {
        if (statusInfo == null){
            return;
        }
        if (maxhp != null)
        {
            maxhp.text = statusInfo.Hp.ToString();
        }
        if (maxmp != null)
        {
            maxmp.text = statusInfo.Mp.ToString();
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
        if (maxhp != null){
            maxhp.text = maxStatusHp.ToString();
        }
        if (hpGauge != null)
        {
            var bgRect = hpGaugeBg.gameObject.GetComponent<RectTransform>();
            var rect = hpGauge.gameObject.GetComponent<RectTransform>();
            bgRect.sizeDelta = new Vector2(80,bgRect.sizeDelta.y);
            rect.sizeDelta = new Vector2(80 - 3,rect.sizeDelta.y);
            hpGaugeBg.fillAmount = 1.0f;
            hpGauge.fillAmount = (float)currentHp / (float)maxStatusHp;
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
        if (maxmp != null){
            maxmp.text = maxStatusMp.ToString();
        }
        if (mpGauge != null)
        {
            var bgRect = mpGaugeBg.gameObject.GetComponent<RectTransform>();
            var rect = mpGauge.gameObject.GetComponent<RectTransform>();
            bgRect.sizeDelta = new Vector2(maxStatusMp * 1.5f,bgRect.sizeDelta.y);
            rect.sizeDelta = new Vector2(maxStatusMp * 1.5f - 3,rect.sizeDelta.y);
            mpGauge.fillAmount = (float)currentMp / (float)maxStatusMp;
            mpGaugeBg.fillAmount = 1.0f;
        }
    }

    public void ChangeHpAnimation(int currentHp,int maxStatusHp)
    {
        if (hpGaugeAnimation != null)
        {
            var waitDuration = 1.0f;
            var sequence = DOTween.Sequence()
                .Append(hpGaugeAnimation.DOFillAmount((float)currentHp / (float)maxStatusHp,waitDuration)
                .SetDelay(0.5f));
        }
    }

    public void ChangeMpAnimation(int currentMp,int maxStatusMp)
    {
        if (mpGaugeAnimation != null)
        {
            var waitDuration = 1.0f;
            var sequence = DOTween.Sequence()
                .Append(mpGaugeAnimation.DOFillAmount((float)currentMp / (float)maxStatusMp,waitDuration)
                .SetDelay(0.5f));
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
