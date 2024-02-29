using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;

namespace Ryneus
{
    public class AnimationUtility
    {
        public static void LocalMoveToTransform(GameObject target,Vector3 from,Vector3 to,float duration)
        {
            target.GetComponent<RectTransform>().localPosition = from;
            DOTween.Sequence()
                .Append(target.transform.DOLocalMove(to,duration))
                .SetEase(Ease.OutQuart);
        }

        public static void LocalMoveToLoopTransform(GameObject target,Vector3 from,Vector3 to,float duration)
        {
            target.GetComponent<RectTransform>().localPosition = from;
            DOTween.Sequence()
                .Append(target.transform.DOLocalMove(to,duration))
                .SetEase(Ease.InOutQuad)
                .SetLoops(-1,LoopType.Yoyo);
        }

        public static void AlphaToTransform(CanvasGroup canvasGroup,float from,float to,float duration,float delay = 0f)
        {
            canvasGroup.alpha = from;
            if (delay > 0)
            {
                DOTween.Sequence()
                    .SetDelay(delay)
                    .Append(canvasGroup.DOFade(to,duration))
                    .SetEase(Ease.OutQuart);
            } else
            {
                DOTween.Sequence()
                    .Append(canvasGroup.DOFade(to,duration))
                    .SetEase(Ease.OutQuart);
            }
        }

        public static void CountUpText(TextMeshProUGUI text,int from,int to)
        {
            int nowNumber = from;
            int updateNumber = to;
            // 指定したupdateNumberまでカウントアップ・カウントダウンする
            DOTween.To(() => nowNumber, (n) => nowNumber = n, updateNumber, 0.5f)
                .OnUpdate(() => text.text = nowNumber.ToString("#,0"));
        }
    }
}