using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

namespace Ryneus
{
    public class BattleStartAnim : MonoBehaviour
    {
        [SerializeField] private Image backBlack;
        [SerializeField] private Image lineWhite;
        [SerializeField] private TextMeshProUGUI mainText;
        [SerializeField] private TextMeshProUGUI subText;

        private bool _busy = false;
        public bool IsBusy => _busy;


        private void OnEnable() 
        {
            Reset();
        }

        public void Reset()
        {
            mainText.color = new Color(255,255,255,0);
            mainText.transform.DOMoveX(0,0); 
            subText.color = new Color(255,255,255,0);
            subText.transform.DOMoveX(0,0); 
            backBlack.color = new Color(0,0,0,0);
            lineWhite.color = new Color(255,255,255,255);
            lineWhite.transform.DOScaleY(0,0);
        }

        public void SetText(string text) 
        {
            mainText.text = text;
            subText.text = text;
        }

        public void StartAnim()
        {
            _busy = true;
            Reset();
            mainText.transform.DOScaleY(0.95f,0);
            var main = DOTween.Sequence()
                .SetDelay(0.1f)
                .Append(mainText.DOFade(1f,0f))
                .Append(mainText.transform.DOScale(0.95f,0.4f))
                .AppendInterval(1.2f)
                .Append(mainText.DOFade(0f, 0.1f))
                .Join(mainText.transform.DOLocalMoveX(-480, 0.1f));
            
        
            subText.transform.DOScaleY(0.95f,0);
            var sub = DOTween.Sequence()
                .SetDelay(0.1f)
                .Append(subText.DOFade(1f,0f))
                .Append(subText.transform.DOScale(1.25f,0.8f))
                .Join(subText.DOFade(0, 0.8f))
                .AppendInterval(0.8f)
                .Append(subText.transform.DOScale(1f,0f))
                .Join(subText.DOFade(1, 0f))
                .Append(subText.DOFade(0f, 0.1f))
                .Join(subText.transform.DOLocalMoveX(480, 0.1f));

            lineWhite.transform.DOScaleY(0.95f,0);
            var white = DOTween.Sequence()
                .Append(lineWhite.transform.DOScaleY(1f,0.02f))
                .Append(lineWhite.transform.DOScaleY(0.08f,0.1f))
                .Join(lineWhite.DOFade(0f, 1.6f))
                .SetEase(Ease.InOutQuad);

            var black = DOTween.Sequence()
                .SetDelay(0.1f)
                .Append(backBlack.DOFade(1f, 0.1f))
                .Join(backBlack.transform.DOScaleY(1f, 0.1f))
                .AppendInterval(1.6f)
                .Append(backBlack.DOFade(0f, 0.1f))
                .Join(backBlack.transform.DOScaleY(0f, 0.1f))
                .OnComplete(() => 
                {
                    _busy = false;
                });
        }
    }
}