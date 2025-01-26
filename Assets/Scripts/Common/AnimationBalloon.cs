using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Utage;
using System.Collections.Generic;
using UnityEngine.U2D;
using System.Linq;

namespace Ryneus
{
    public class AnimationBalloon : MonoBehaviour
    {
        [SerializeField] private Image balloon;
        [SerializeField] private Image balloon2;
        private List<Sequence> _playingSequence = new();
        private bool _preserved = false;
        private float _initPositionY;
        private AdvGraphicLayer _targetLayer;

        public void Play(AdvGraphicLayer layer,AnimationBalloonType animationBalloonType)
        {
            _preserved = false;
            Debug.Log("Play");
            switch(animationBalloonType)
            {
                case AnimationBalloonType.Surprise:
                    PlayAnimation(layer);
                    Surprise();
                    break;
                case AnimationBalloonType.Question:
                    Question();
                    break;
                case AnimationBalloonType.Note:
                    PlayStepAnimation(layer);
                    Note();
                    break;
                case AnimationBalloonType.Heart:
                    Heart();
                    break;
                case AnimationBalloonType.Angry:
                    PlayAnimation(layer);
                    Angry();
                    break;
                case AnimationBalloonType.Sweat:
                    PlayAnimation(layer);
                    Sweat();
                    break;
                case AnimationBalloonType.Hmm:
                    Hmm();
                    break;
                case AnimationBalloonType.Silent:
                    Silent();
                    break;
                case AnimationBalloonType.Light:
                    PlayAnimation(layer);
                    Light();
                    break;
                default:
                    Surprise();
                    break;
            }
        }

        private void InitAlpha(Image image,float opacity)
        {
            image.color = new Color(balloon.color.r,balloon.color.g,balloon.color.b,opacity);
        }
        
        private void InitPosition(Image image,float offsetX,float offsetY)
        {
            image.transform.localPosition = new Vector2(offsetX,offsetY);
        }

        private void PlayAnimation(AdvGraphicLayer layer)
        {
            _targetLayer = layer;
            _initPositionY = layer.transform.localPosition.y;
            var sequence = DOTween.Sequence()
                .SetEase(Ease.InOutQuad)
                .Append(layer.transform.DOLocalMoveY(0.25f+_initPositionY, 0.1f))
                .Append(layer.transform.DOLocalMoveY(_initPositionY, 0.3f))
                .AppendInterval(1.6f)
                .OnComplete(() => 
                {
                    layer.transform.DOLocalMoveY(_initPositionY, 0f);
                });
            sequence.SetLoops(-1);
            _playingSequence.Add(sequence);
        }

        private void PlayStepAnimation(AdvGraphicLayer layer)
        {
            _targetLayer = layer;
            _initPositionY = layer.transform.localPosition.y;
            var sequence = DOTween.Sequence()
                .SetEase(Ease.InOutQuad)
                .Append(layer.transform.DOLocalMoveY(0.25f+_initPositionY, 0.3f))
                .Append(layer.transform.DOLocalMoveY(_initPositionY, 0.3f))
                .OnComplete(() => 
                {
                    layer.transform.DOLocalMoveY(_initPositionY, 0f);
                });
            sequence.SetLoops(-1);
            _playingSequence.Add(sequence);
        }

        public bool StopAnimation()
        {
            if (_preserved == false)
            {
                _preserved = true;
                return false;
            }
            foreach (var playingSequence in _playingSequence)
            {
                playingSequence.Complete();
            }
            foreach (var playingSequence in _playingSequence)
            {
                playingSequence.Kill();
            }
            InitAlpha(balloon,0);
            InitAlpha(balloon2,0);
            _playingSequence.Clear();
            if (_targetLayer != null)
            {
                _targetLayer.transform.localPosition = new Vector3(_targetLayer.transform.localPosition.x,_initPositionY);
                _targetLayer = null;
            }
            return true;
        }

        private void Surprise()
        {
            var sprite = ResourceSystem.LoadResource<Sprite>(ResourceSystem.SystemTexturePath + "Surprise");
            balloon.sprite = sprite;
            balloon.SetNativeSize();

            var opacity = 0;
            var offsetX = 80f;
            var offsetY = 176f;
            InitAlpha(balloon,opacity);
            InitPosition(balloon,offsetX,offsetY);

            // 10フレーム右上
            var duration = 0.3f;
            var afterWait = 1.4f;
            var sequence = DOTween.Sequence()
                .SetEase(Ease.InOutQuad)
                .Append(balloon.transform.DOLocalMoveX(24+offsetX, duration))
                .Join(balloon.transform.DOLocalMoveY(48+offsetY, duration))
                .Join(balloon.DOFade(1, duration))
                .AppendInterval(afterWait)
                .Append(balloon.DOFade(0, duration))
                .OnComplete(() => 
                {
                    InitAlpha(balloon,opacity);
                    InitPosition(balloon,offsetX,offsetY);
                });
            sequence.SetLoops(-1);
            _playingSequence.Add(sequence);
        }

        private void Question()
        {
            var sprite = ResourceSystem.LoadResource<Sprite>(ResourceSystem.SystemTexturePath + "Question");
            balloon.sprite = sprite;
            balloon.SetNativeSize();

            var opacity = 1;
            var offsetX = 0;
            var offsetY = 176;
            InitAlpha(balloon,opacity);
            InitPosition(balloon,offsetX,offsetY);

            // 1ずつ上昇、左右移動
            var duration = 0.4f;
            var afterWait = 1.2f;
            balloon.transform.DOLocalMoveY(40+offsetY, duration+afterWait);
            var sequence = DOTween.Sequence()
                .SetEase(Ease.InOutQuad)
                .Append(balloon.transform.DOLocalMoveX(4+offsetX, 0.2f))
                .Append(balloon.transform.DOLocalMoveX(-4+offsetX, 0.2f));
            sequence.SetLoops(8);
            var sequence2 = DOTween.Sequence()
                .SetEase(Ease.InOutQuad)
                .SetDelay(duration)
                .Append(balloon.DOFade(0, afterWait))
                .OnComplete(() => 
                {
                    //InitAlpha(opacity);
                    //InitPosition(balloon,offsetX,offsetY);
                });
            _playingSequence.Add(sequence);
        }

        private void Note()
        {
         	Sprite[] sprites = ResourceSystem.LoadResources<Sprite>(ResourceSystem.SystemTexturePath + "Note");
            balloon.sprite = sprites.First(a => a.name == "Note_0");
            balloon.SetNativeSize();

            var opacity = 1;
            var offsetX = 80;
            var offsetY = 216;
            InitAlpha(balloon,opacity);
            InitPosition(balloon,offsetX,offsetY);

            // 上下移動とスプライト切替
            var duration = 0.3f;
            var spriteFlag = true;
            var sequence = DOTween.Sequence()
                .SetEase(Ease.InOutQuad)
                .Append(balloon.transform.DOLocalMoveY(offsetY, 0));
                //.Append(balloon.transform.DOLocalMoveY(offsetY, duration));
            //sequence.SetLoops(-1);
            _playingSequence.Add(sequence);
            var sequence2 = DOTween.Sequence()
                .SetEase(Ease.InOutQuad)
                .SetDelay(duration)
                .OnStepComplete(() => 
                {
                    spriteFlag = !spriteFlag;
                    var spriteName = spriteFlag ? "Note_0" : "Note_1";
                    balloon.sprite = sprites.First(a => a.name == spriteName);
                    balloon.SetNativeSize();
                });
            sequence2.SetLoops(-1);
            _playingSequence.Add(sequence2);
        }

        private void Heart()
        {
            var sprite = ResourceSystem.LoadResource<Sprite>(ResourceSystem.SystemTexturePath + "Heart");
            balloon.sprite = sprite;
            balloon.SetNativeSize();

            var opacity = 1;
            var offsetX = 0;
            var offsetY = 176;
            InitAlpha(balloon,opacity);
            InitPosition(balloon,offsetX,offsetY);

            // 位置と透過が上がって下がる
            var duration = 0.8f;
            var sequence = DOTween.Sequence()
                .SetEase(Ease.InOutQuad)
                .Append(balloon.transform.DOLocalMoveY(40+offsetY, duration))
                .Append(balloon.transform.DOLocalMoveY(offsetY, duration));
            sequence.SetLoops(-1);
            var sequence2 = DOTween.Sequence()
                .SetEase(Ease.InOutQuad)
                .Append(balloon.DOFade(1, duration))
                .Append(balloon.DOFade(0, duration))
                .OnComplete(() => 
                {
                    InitAlpha(balloon,opacity);
                    InitPosition(balloon,offsetX,offsetY);
                });
            sequence2.SetLoops(-1);
        }        
        
        private void Angry()
        {
            var sprite = ResourceSystem.LoadResource<Sprite>(ResourceSystem.SystemTexturePath + "Note_0");
            balloon.sprite = sprite;
            balloon.SetNativeSize();

            var opacity = 0;
            var offsetX = 80;
            var offsetY = 176;
            InitAlpha(balloon,opacity);
            InitPosition(balloon,offsetX,offsetY);

            // Offsetを左右移動し進行方向へ移動
            var duration = 0.4f;
            var afterWait = 0.8f;
            var sequence = DOTween.Sequence()
                .SetEase(Ease.InOutQuad)
                .Append(balloon.transform.DOLocalMoveX(offsetX, duration))
                .Join(balloon.transform.DOLocalMoveY(offsetY, duration))
                .AppendInterval(afterWait)
                .OnComplete(() => 
                {
                    balloon.transform.DOFlip();
                });
            sequence.SetLoops(-1);
            var sequence2 = DOTween.Sequence()
                .SetDelay(afterWait)
                .SetEase(Ease.InOutQuad)
                .Append(balloon.transform.DOLocalMoveX(offsetX*-1, duration))
                .Join(balloon.transform.DOLocalMoveY(offsetY, duration));
            sequence2.SetLoops(-1);
        }

        private void Sweat()
        {
            var sprite = ResourceSystem.LoadResource<Sprite>(ResourceSystem.SystemTexturePath + "Sweat");
            balloon.sprite = sprite;
            balloon.SetNativeSize();

            var opacity = 1;
            var offsetX = 80;
            var offsetY = 176;
            InitAlpha(balloon,opacity);
            InitPosition(balloon,offsetX,offsetY);

            // 位置が上がり透過が下がって一時停止
            var duration = 0.3f;
            var afterWait = 1.4f;
            var sequence = DOTween.Sequence()
                .SetEase(Ease.InOutQuad)
                .Append(balloon.transform.DOLocalMoveY(-40+offsetY, duration))
                .Append(balloon.DOFade(0, duration))
                .AppendInterval(afterWait)
                .OnComplete(() => 
                {
                    InitAlpha(balloon,opacity);
                    InitPosition(balloon,offsetX,offsetY);
                });
            sequence.SetLoops(-1);
            _playingSequence.Add(sequence);
        }
        
        private void Hmm()
        {
            var sprite = ResourceSystem.LoadResource<Sprite>(ResourceSystem.SystemTexturePath + "Hmm_0");
            balloon.sprite = sprite;
            balloon.SetNativeSize();
            var sprite2 = ResourceSystem.LoadResource<Sprite>(ResourceSystem.SystemTexturePath + "Hmm_1");
            balloon2.sprite = sprite2;
            balloon2.SetNativeSize();

            var opacity = 1;
            var offsetX = 0;
            var offsetY = 176;
            InitAlpha(balloon,opacity);
            InitPosition(balloon,offsetX,offsetY);

            // 左右移動し上昇して消える
            var duration = 1.6f;
            var sequence = DOTween.Sequence()
                .SetEase(Ease.InOutQuad)
                .Append(balloon.DOFade(0, duration))
                .Join(balloon.transform.DOLocalMoveY(40+offsetY, duration))
                .OnComplete(() => 
                {
                    InitAlpha(balloon,opacity);
                    InitPosition(balloon,offsetX,offsetY);
                });
            sequence.SetLoops(-1);
            _playingSequence.Add(sequence);
            var sequence2 = DOTween.Sequence()
                .SetEase(Ease.InOutQuad)
                .Append(balloon.transform.DOLocalMoveX(4+offsetX, 0.2f))
                .Append(balloon.transform.DOLocalMoveX(-4+offsetX, 0.2f));
            sequence2.SetLoops(-1);
            _playingSequence.Add(sequence2);
            // 浮き出てから消える
            var sequence3 = DOTween.Sequence()
                .SetEase(Ease.InOutQuad)
                .Append(balloon2.transform.DOLocalMoveY(40+offsetY, 0))
                .Join(balloon2.DOFade(1, duration/2))
                .Append(balloon2.DOFade(0, duration/2));
            sequence3.SetLoops(-1);
            _playingSequence.Add(sequence3);
        }
        
        private void Silent()
        {
            var sprite = ResourceSystem.LoadResource<Sprite>(ResourceSystem.SystemTexturePath + "Silent_0");
            balloon.sprite = sprite;
            balloon.SetNativeSize();

            var opacity = 1;
            var offsetX = 80;
            var offsetY = 176;
            InitAlpha(balloon,opacity);
            InitPosition(balloon,offsetX,offsetY);

            // 位置固定でスプライト切替
            var duration = 0.4f;
            var spriteFlag = 0;
            var sequence2 = DOTween.Sequence()
                .SetEase(Ease.InOutQuad)
                .SetDelay(duration)
                .OnComplete(() => 
                {
                    spriteFlag++;
                    if (spriteFlag > 2)
                    {
                        spriteFlag = 0;
                    }
                    var spriteName = "Silent" + spriteFlag.ToString();
                    var sprite = ResourceSystem.LoadResource<Sprite>(ResourceSystem.SystemTexturePath + spriteName);
                    balloon.sprite = sprite;
                });
            sequence2.SetLoops(-1);
        }

        private void Light()
        {
            var sprite = ResourceSystem.LoadResource<Sprite>(ResourceSystem.SystemTexturePath + "Light_0");
            
            balloon.DOFade(0,0);
            balloon.sprite = sprite;
            balloon.SetNativeSize();

            var opacity = 1;
            var offsetX = 0;
            var offsetY = 216;
            InitAlpha(balloon,opacity);
            InitPosition(balloon,offsetX,offsetY);

            // 上下移動し発光して消える
            var duration = 0.3f;
            var afterWait = 1.4f;
            var sequence = DOTween.Sequence()
                .SetEase(Ease.InOutQuad)
                .Append(balloon.transform.DOLocalMoveY(64+offsetY, duration))
                .Join(balloon.DOFade(1f,duration))
                .AppendInterval(duration)
                .Append(balloon.transform.DOLocalMoveY(128+offsetY, afterWait))
                .Join(balloon.DOFade(0f,afterWait));
            sequence.SetLoops(-1);
            _playingSequence.Add(sequence);
            /*
            var sequence2 = DOTween.Sequence()
                .SetEase(Ease.InOutQuad)
                .SetDelay(duration)
                .Append(balloon2.transform.DOLocalMoveY(64+offsetY, 0))
                .Append(balloon2.DOFade(0.5f,0.1f))
                .Append(balloon2.DOFade(0,0.1f))
                .Append(balloon.DOFade(0,afterWait-0.2f))
                .OnComplete(() => 
                {
                    InitAlpha(opacity);
                    InitPosition(balloon,offsetX,offsetY);
                });
            sequence2.SetLoops(-1);
            _playingSequence.Add(sequence2);
            */
        }
    }

    public enum AnimationBalloonType
    {
        None = 0,
        Surprise = 1,
        Question = 2,
        Note = 3,
        Heart = 4,
        Angry = 5,
        Sweat = 6,
        Hmm = 7,
        Silent = 8,
        Light = 9,
    }
}
