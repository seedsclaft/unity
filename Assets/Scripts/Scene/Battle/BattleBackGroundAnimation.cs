using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class BattleBackGroundAnimation : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer1 = null;
    [SerializeField] private SpriteRenderer spriteRenderer2 = null;
    [SerializeField] private List<Sprite> sprites = null;
    private int _lastSpriteIndex = -1;
    
    public void StartAnimation()
    {
        return;
        var duration = 0.8f;
        var sequence = DOTween.Sequence()
            .SetDelay(duration)
            .OnStepComplete(() => {
                var spriteChange = false;
                while (spriteChange == false)
                {
                    var spriteIndex = Random.Range(0,sprites.Count);
                    if (spriteIndex != _lastSpriteIndex)
                    {
                        spriteChange = true;
                        _lastSpriteIndex = spriteIndex;
                    }
                }
                var sprite = sprites[_lastSpriteIndex];
                spriteRenderer1.sprite = sprite;
                spriteRenderer1.transform.DOScale(50,0);
                spriteRenderer1.transform.DOScale(52,duration).SetEase(Ease.OutExpo);
                spriteRenderer2.sprite = sprite;
                spriteRenderer1.DOFade(0.5f,0);
            })
            .SetLoops(-1);
    }
}
