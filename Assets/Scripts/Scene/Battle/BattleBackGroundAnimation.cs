using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace Ryneus
{
    public class BattleBackGroundAnimation : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer = null;
        [SerializeField] private _2dxFX_DestroyedFX destroy = null;
        
        private bool _seekAnimation = false;
        private int _seekFrame = 60;

        public void SeekAnimation()
        {
            _seekAnimation = true;
            _seekFrame = 24;
        }

        private void Update() 
        {
            if (destroy != null)
            {
                if (_seekAnimation)
                {
                    destroy.Seed += 0.0004f;
                    if (destroy.Seed > 1)
                    {
                        destroy.Seed = 0;
                    }
                    _seekFrame--;
                    if (_seekFrame == 0)
                    {
                        _seekAnimation = false;
                    }
                } else
                {
                    destroy.Seed += 0.00001f;
                }
            }
        }
    }
}