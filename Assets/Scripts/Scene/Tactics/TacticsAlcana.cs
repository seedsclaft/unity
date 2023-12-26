using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Effekseer;

public class TacticsAlcana : MonoBehaviour
{
    [SerializeField] private Image cardImage;
    [SerializeField] private EffekseerEmitter emitter;
    [SerializeField] private _2dxFX_Shiny_Reflect refrect;
    private bool _busy = false;
    public bool IsBusy{ get { return _busy;}}

    private void Awake() {
        Reset();
    }

    public void Reset()
    {
        cardImage.gameObject.transform.DOLocalMoveX(0, 0);
        cardImage.gameObject.transform.DOLocalMoveY(0, 0);
        cardImage.gameObject.GetComponent<RectTransform>().localScale = new Vector3(1,1,1);
        cardImage.DOFade(0, 0);
        refrect.enabled = false;
        emitter.enabled = false;
    }

    public void StartAlcanaAnimation(System.Action endEvent)
    {
        _busy = true;
        Reset();
        emitter.enabled = true;
        emitter.Play();
        var start = DOTween.Sequence()
            .SetDelay(0.3f)
            .Append(cardImage.DOFade(1f, 0.4f))
            .Join(cardImage.gameObject.transform.DOLocalMoveY(40,0.4f))
            .AppendInterval(0.2f)
            .SetEase(Ease.InOutQuad)
            .OnComplete(() => {
                refrect.enabled = true;
            });

        var card = DOTween.Sequence()
            .SetDelay(2f)
            .Append(cardImage.gameObject.transform.DOScale(0.25f, 0.4f))
            //.Join(cardImage.gameObject.transform.DOLocalMoveX(-340,0.4f))
            //.Join(cardImage.gameObject.transform.DOLocalMoveY(262,0.4f))
            .Join(cardImage.DOFade(0f, 0.4f))
            .OnComplete(() => {
                Reset();
                if (endEvent != null) endEvent();
                _busy = false;
            });
    }
    
    public void UseAnim(System.Action endEvent)
    {
        _busy = true;
        Reset();
        emitter.enabled = true;
        emitter.Play();
        var start = DOTween.Sequence()
            .SetDelay(0.3f)
            .Append(cardImage.DOFade(1f, 0.4f))
            .Join(cardImage.gameObject.transform.DOLocalMoveY(40,0.4f))
            .AppendInterval(0.2f)
            .SetEase(Ease.InOutQuad)
            .OnComplete(() => {
                refrect.enabled = true;
                Reset();
                //if (endEvent != null) endEvent();
                _busy = false;
            });
    }
}
