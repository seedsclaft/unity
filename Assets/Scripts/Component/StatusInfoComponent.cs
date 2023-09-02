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
    [SerializeField] private Image hpGaugeBg;
    [SerializeField] private Image hpGauge;
    [SerializeField] private Image mpGaugeBg;
    [SerializeField] private Image mpGauge;
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
            //RectTransform bgRect = hpGaugeBg.gameObject.GetComponent < RectTransform > ();
            RectTransform rect = hpGauge.gameObject.GetComponent < RectTransform > ();
            //bgRect.sizeDelta = new Vector2(maxhp,bgRect.sizeDelta.y);
            rect.sizeDelta = new Vector2(80 * (currentHp * 100 / maxStatusHp) * 0.01f - 3,rect.sizeDelta.y);
            //hpGauge.fillAmount = currentHp / maxhp;
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
            RectTransform bgRect = mpGaugeBg.gameObject.GetComponent < RectTransform > ();
            RectTransform rect = mpGauge.gameObject.GetComponent < RectTransform > ();
            bgRect.sizeDelta = new Vector2(maxStatusMp * 1.5f,bgRect.sizeDelta.y);
            rect.sizeDelta = new Vector2(maxStatusMp * 1.5f * (currentMp*100 / maxStatusMp) * 0.01f - 3,rect.sizeDelta.y);
            //mpGauge.fillAmount = currentMp / maxmp;
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
