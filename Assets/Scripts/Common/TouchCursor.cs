using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace Ryneus
{
    public class TouchCursor : MonoBehaviour
    {
        [SerializeField] private RectTransform baseRect = null;
        [SerializeField] private RectTransform touchCursorRect = null;
        [SerializeField] private ParticleSystem particle = null;
        [SerializeField] private Image circle = null;

        private void Update() 
        {
            if (Input.GetMouseButtonDown(0))
            {
                StartTouchAnimation();
            }
        }

        private void StartTouchAnimation()
        {
            var pos = Input.mousePosition;
            pos.x -= baseRect.sizeDelta.x / 2;
            pos.y -= baseRect.sizeDelta.y /2;
            pos.z = 10f; 
                
            touchCursorRect.localPosition = pos;
            gameObject.SetActive(false);
            gameObject.SetActive(true);
            particle.Play();

            circle.DOFade(1,0);
            circle.transform.DOScale(0,0);

            var duration = 0.4f;
            circle.DOFade(0,duration);
            circle.transform.DOScale(1,duration);
        }
    }
}
