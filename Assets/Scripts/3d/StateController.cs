using System.Collections;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;
using DG.Tweening;

namespace Ryneus
{
    public class StateController : MonoBehaviour
    {
        [SerializeField] private Animator animator = null;

        private AnimationState _lastState = AnimationState.None;

        public void Initialize(bool isActor) 
        {
            var rect = gameObject.GetComponent<RectTransform>();
            rect.localScale = new Vector3(200,200,200);
            ChangePlayerSide(isActor);    
        }

        public void ChangePlayerSide(bool isActor)
        {
            var rotationY = isActor ? 65 : -115;
            var rect = gameObject.GetComponent<RectTransform>();
            rect.localRotation = Quaternion.Euler(new Vector3(0,rotationY,0));
        }

        public void RunForward()
        {
            var duration = 0.8f;
            gameObject.transform.DOLocalMoveX(0, 0);
            var sequence = DOTween.Sequence()
                .SetDelay(1)
                .Append(gameObject.transform.DOLocalMoveX(560, duration));
        }

        public void StartAnimation(AnimationState animationState)
        {
            if (_lastState != animationState)
            {
                if (_lastState == AnimationState.Death)
                {
                    return;
                }
                _lastState = animationState;
                animator.SetInteger("State",(int)animationState);
            }
        }
    }
}

public enum AnimationState
{
    None = -1,
    Idle = 0,
    RunForward = 1,
    Attack = 2,
    Jump = 3,
    Damaged = 4,
    Death = 5

}