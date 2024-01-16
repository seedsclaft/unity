using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.U2D;
using Effekseer;

public class BattleStateOverlay : MonoBehaviour
{
    [SerializeField] private Image icon = null;
    [SerializeField] private EffekseerEmitter effekseerEmitter = null;
    private List<StateInfo> _stateInfos = new List<StateInfo>();
    private string _overlayEffectPath = null;

    private Sequence _iconSequence;

    private int _iconAnimIndex = -1;

    public void SetStates(List<StateInfo> stateInfos)
    {
        _stateInfos = stateInfos;
        IconAnimation();
        OverlayAnimation();
    }

    private void IconAnimation()
    {
        if (_stateInfos.Count == 0)
        {
            StopAnimation();
            return;
        }
        var delay = 0.5f;
        var duration = 1.0f;
        if (_iconAnimIndex < 0 || _iconAnimIndex > (_stateInfos.Count-1))
        {
            _iconAnimIndex = 0;
        }
        UpdateStateIcon();
        if (_stateInfos.Count > 1)
        {
            if (_iconSequence == null)
            {
                _iconSequence = DOTween.Sequence()
                    .SetDelay(delay + duration)
                    .OnComplete(() => {
                        _iconAnimIndex += 1;
                        IconAnimation();
                    });
            } else
            {
                _iconSequence.Kill(false);
                _iconSequence = DOTween.Sequence()
                    .SetDelay(delay + duration)
                    .OnComplete(() => {
                        _iconAnimIndex += 1;
                        IconAnimation();
                    });
            }
        } else
        {
            StopAnimation();
            _iconAnimIndex = 0;
        }
    }

    private void StopAnimation()
    {
        _iconAnimIndex = -1;
        if (_iconSequence != null) 
        {
            _iconSequence.Kill(false);
            _iconSequence = null;
        }
        if (_stateInfos.Count == 0)
        {
            icon.gameObject.SetActive(false);
        }
    }

    private void UpdateStateIcon()
    {
        if (_stateInfos.Count < _iconAnimIndex) return;
        icon.gameObject.SetActive(true);
        var stateInfo = _stateInfos[_iconAnimIndex];
        var spriteAtlas = Resources.Load<SpriteAtlas>("Texture/Icons");
        if (icon != null)
        {
            icon.sprite = spriteAtlas.GetSprite(stateInfo.Master.IconPath);
        }
    }

    private void OverlayAnimation()
    {
        var overlayState = _stateInfos.Find(a => a.Master.EffectPath != "" && a.Master.EffectPath != "\"\"");
        if (overlayState == null)
        {
            _overlayEffectPath = null;
            effekseerEmitter.Stop();
            //effekseerEmitter.enabled = false;
            return;
        }
        if (_overlayEffectPath != overlayState.Master.EffectPath)
        {
            _overlayEffectPath = overlayState.Master.EffectPath;
            var asset = UpdateStateOverlay();
            //effekseerEmitter.enabled = true;
            if (asset != null) {
                var rect = effekseerEmitter.gameObject.GetComponent<RectTransform>();
                if (overlayState.Master.EffectPosition == EffectPositionType.Center)
                {
                    rect.localPosition = new Vector2(rect.localPosition.x,40);
                } else
                if (overlayState.Master.EffectPosition == EffectPositionType.Down)
                {
                    rect.localPosition = new Vector2(rect.localPosition.x,-8);
                }
                rect.localScale = new Vector3(overlayState.Master.EffectScale,overlayState.Master.EffectScale,overlayState.Master.EffectScale);
                effekseerEmitter.effectAsset = asset;
                effekseerEmitter.Play();
            }
        }
    }
    
    private EffekseerEffectAsset UpdateStateOverlay()
    {
        string path = "Animations/" + _overlayEffectPath;
        var result = UnityEngine.Resources.Load<EffekseerEffectAsset>(path);
        return result;
    }

    public void ShowStateOverlay()
    {
        effekseerEmitter.gameObject.SetActive(true);
    }

    public void HideStateOverlay()
    {
        effekseerEmitter.gameObject.SetActive(false);
    }
}
