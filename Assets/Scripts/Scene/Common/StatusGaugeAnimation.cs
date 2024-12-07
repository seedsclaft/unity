using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace Ryneus
{
    public class StatusGaugeAnimation : MonoBehaviour
    {
        [SerializeField] private Image gaugeBg;
        [SerializeField] private Image gauge;
        [SerializeField] private Image gaugeAnimation;
        [SerializeField] private int fillWidth = 100;
        [SerializeField] private int fillMargin = 2;

        private float _waitDuration = 0.8f;
        private float _delayDuration = 0.25f;

        private Sequence _animation = null;

        public void UpdateGauge(float gaugeAmount)
        {
            if (gauge != null)
            {
                var bgRect = gaugeBg.gameObject.GetComponent<RectTransform>();
                bgRect.sizeDelta = new Vector2(fillWidth,bgRect.sizeDelta.y);
                gaugeBg.fillAmount = 1.0f;

                var rect = gauge.gameObject.GetComponent<RectTransform>();
                rect.sizeDelta = new Vector2(fillWidth - fillMargin,rect.sizeDelta.y);
                gauge.fillAmount = gaugeAmount;
            }
        }

        public void SetGaugeAnimation(float gaugeAmount)
        {
            if (gaugeAnimation != null)
            {
                var gaugeRect = gaugeAnimation.gameObject.GetComponent<RectTransform>();
                gaugeRect.sizeDelta = new Vector2(fillWidth - fillMargin,gaugeRect.sizeDelta.y);
                gaugeAnimation.fillAmount = gaugeAmount;
            }
        }

        public void UpdateGaugeAnimation(float gaugeAmount)
        {
            if (gaugeAnimation != null)
            {
                _animation?.Kill(true);
                var sequence = DOTween.Sequence()
                    .Append(gaugeAnimation.DOFillAmount(gaugeAmount,_waitDuration)
                    .SetDelay(_delayDuration)
                    .OnComplete(() => 
                        {
                            _animation = null;
                            gaugeAnimation.fillAmount = gauge.fillAmount;
                        })
                    );
                _animation = sequence;
            }
        }
    }
}