using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class StatusGaugeAnimation : MonoBehaviour
{
    [SerializeField] private Image gaugeBg;
    [SerializeField] private Image gauge;
    [SerializeField] private Image gaugeAnimation;

    private float _waitDuration = 1.0f;
    private float _delayDuration = 0.5f;

    public void UpdateGauge(float width,float margin,float gaugeAmount)
    {
        if (gauge != null)
        {
            var bgRect = gaugeBg.gameObject.GetComponent<RectTransform>();
            bgRect.sizeDelta = new Vector2(width,bgRect.sizeDelta.y);
            gaugeBg.fillAmount = 1.0f;

            var rect = gauge.gameObject.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(width - margin,rect.sizeDelta.y);
            gauge.fillAmount = gaugeAmount;
        }
        if (gaugeAnimation != null)
        {
            var gaugeRect = gaugeAnimation.gameObject.GetComponent<RectTransform>();
            gaugeRect.sizeDelta = new Vector2(width - margin,gaugeRect.sizeDelta.y);
            gaugeAnimation.fillAmount = gaugeAmount;
        }
    }

    public void UpdateGaugeAnimation(float gaugeAmount)
    {
        if (gaugeAnimation != null)
        {
            var sequence = DOTween.Sequence()
                .Append(gaugeAnimation.DOFillAmount(gaugeAmount,_waitDuration)
                .SetDelay(_delayDuration));
        }
    }
}
