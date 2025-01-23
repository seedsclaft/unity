using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace Ryneus
{
    public class AnimationBalloon : MonoBehaviour
    {
        
        [SerializeField] private Image balloon;
        [SerializeField] private Image balloon2;

        public void Play(AnimationBalloonType animationBalloonType)
        {
            switch(animationBalloonType)
            {
                case AnimationBalloonType.Surprise:
                    Surprise();
                    break;
                case AnimationBalloonType.Question:
                    Question();
                    break;
                case AnimationBalloonType.Note:
                    Note();
                    break;
                case AnimationBalloonType.Heart:
                    Heart();
                    break;
                case AnimationBalloonType.Angry:
                    Angry();
                    break;
                case AnimationBalloonType.Sweat:
                    Sweat();
                    break;
                case AnimationBalloonType.Hmm:
                    Hmm();
                    break;
                case AnimationBalloonType.Silent:
                    Silent();
                    break;
                case AnimationBalloonType.Light:
                    Light();
                    break;
                default:
                    Surprise();
                    break;
            }
        }

        private void InitAlpha(float opacity)
        {
            balloon.color = new Color(balloon.color.r,balloon.color.g,balloon.color.b,opacity);
        }
        
        private void InitPosition(int offsetX,int offsetY)
        {
            transform.localPosition = new Vector2(offsetX,offsetY);
        }

        private void Surprise()
        {
            var sprite = ResourceSystem.LoadResource<Sprite>(ResourceSystem.SystemTexturePath + "Surprise");
            balloon.sprite = sprite;

            var opacity = 0;
            var offsetX = 80;
            var offsetY = 176;
            InitAlpha(opacity);
            InitPosition(offsetX,offsetY);

            // 10フレーム右上
            var duration = 0.4f;
            var afterWait = 1.2f;
            var sequence = DOTween.Sequence()
                .SetEase(Ease.InOutQuad)
                .Append(transform.DOLocalMoveX(60+offsetX, duration))
                .Join(transform.DOLocalMoveY(90+offsetY, duration))
                .Join(balloon.DOFade(1, duration))
                .AppendInterval(afterWait)
                .Append(balloon.DOFade(0, 0.4f))
                .OnComplete(() => 
                {
                    InitAlpha(opacity);
                    InitPosition(offsetX,offsetY);
                });
            sequence.SetLoops(-1);
        }

        private void Question()
        {
            var sprite = ResourceSystem.LoadResource<Sprite>(ResourceSystem.SystemTexturePath + "Question");
            balloon.sprite = sprite;

            var opacity = 1;
            var offsetX = 0;
            var offsetY = 176;
            InitAlpha(opacity);
            InitPosition(offsetX,offsetY);

            // 1ずつ上昇、左右移動
            var duration = 0.4f;
            var afterWait = 1.2f;
            transform.DOLocalMoveY(90+offsetY, duration+afterWait);
            var sequence = DOTween.Sequence()
                .SetEase(Ease.InOutQuad)
                .Append(transform.DOLocalMoveX(4+offsetX, 0.2f))
                .Append(transform.DOLocalMoveX(-4+offsetX, 0.2f));
            sequence.SetLoops(8);
            var sequence2 = DOTween.Sequence()
                .SetEase(Ease.InOutQuad)
                .SetDelay(duration)
                .Append(balloon.DOFade(0, afterWait))
                .OnComplete(() => 
                {
                    InitAlpha(opacity);
                    InitPosition(offsetX,offsetY);
                });
        }

        private void Note()
        {
            var sprite = ResourceSystem.LoadResource<Sprite>(ResourceSystem.SystemTexturePath + "Note_0");
            balloon.sprite = sprite;

            var opacity = 1;
            var offsetX = 80;
            var offsetY = 176;
            InitAlpha(opacity);
            InitPosition(offsetX,offsetY);

            // 上下移動とスプライト切替
            var duration = 0.4f;
            var spriteFlag = true;
            var sequence = DOTween.Sequence()
                .SetEase(Ease.InOutQuad)
                .Append(transform.DOLocalMoveY(24+offsetY, duration))
                .Append(transform.DOLocalMoveY(offsetY, duration));
            sequence.SetLoops(-1);
            var sequence2 = DOTween.Sequence()
                .SetEase(Ease.InOutQuad)
                .SetDelay(duration)
                .OnComplete(() => 
                {
                    spriteFlag = !spriteFlag;
                    var spriteName = spriteFlag ? "Note_0" : "Note_1";
                    var sprite = ResourceSystem.LoadResource<Sprite>(ResourceSystem.SystemTexturePath + spriteName);
                    balloon.sprite = sprite;
                });
            sequence2.SetLoops(-1);
        }

        private void Heart()
        {
            var sprite = ResourceSystem.LoadResource<Sprite>(ResourceSystem.SystemTexturePath + "Heart");
            balloon.sprite = sprite;

            var opacity = 1;
            var offsetX = 0;
            var offsetY = 176;
            InitAlpha(opacity);
            InitPosition(offsetX,offsetY);

            // 位置と透過が上がって下がる
            var duration = 0.8f;
            var sequence = DOTween.Sequence()
                .SetEase(Ease.InOutQuad)
                .Append(transform.DOLocalMoveY(40+offsetY, duration))
                .Append(transform.DOLocalMoveY(offsetY, duration));
            sequence.SetLoops(-1);
            var sequence2 = DOTween.Sequence()
                .SetEase(Ease.InOutQuad)
                .Append(balloon.DOFade(1, duration))
                .Append(balloon.DOFade(0, duration))
                .OnComplete(() => 
                {
                    InitAlpha(opacity);
                    InitPosition(offsetX,offsetY);
                });
            sequence2.SetLoops(-1);
        }        
        
        private void Angry()
        {
            var sprite = ResourceSystem.LoadResource<Sprite>(ResourceSystem.SystemTexturePath + "Note_0");
            balloon.sprite = sprite;

            var opacity = 0;
            var offsetX = 80;
            var offsetY = 176;
            InitAlpha(opacity);
            InitPosition(offsetX,offsetY);

            // Offsetを左右移動し進行方向へ移動
            var duration = 0.4f;
            var afterWait = 0.8f;
            var sequence = DOTween.Sequence()
                .SetEase(Ease.InOutQuad)
                .Append(transform.DOLocalMoveX(offsetX, duration))
                .Join(transform.DOLocalMoveY(offsetY, duration))
                .AppendInterval(afterWait)
                .OnComplete(() => 
                {
                    balloon.transform.DOFlip();
                });
            sequence.SetLoops(-1);
            var sequence2 = DOTween.Sequence()
                .SetDelay(afterWait)
                .SetEase(Ease.InOutQuad)
                .Append(transform.DOLocalMoveX(offsetX*-1, duration))
                .Join(transform.DOLocalMoveY(offsetY, duration));
            sequence2.SetLoops(-1);
        }

        private void Sweat()
        {
            var sprite = ResourceSystem.LoadResource<Sprite>(ResourceSystem.SystemTexturePath + "Sweat");
            balloon.sprite = sprite;

            var opacity = 1;
            var offsetX = 80;
            var offsetY = 176;
            InitAlpha(opacity);
            InitPosition(offsetX,offsetY);

            // 位置が上がり透過が下がって一時停止
            var duration = 0.8f;
            var afterWait = 0.8f;
            var sequence = DOTween.Sequence()
                .SetEase(Ease.InOutQuad)
                .Append(transform.DOLocalMoveY(40+offsetY, duration))
                .Append(balloon.DOFade(0, duration))
                .AppendInterval(afterWait)
                .OnComplete(() => 
                {
                    InitAlpha(opacity);
                    InitPosition(offsetX,offsetY);
                });
            sequence.SetLoops(-1);
        }
        
        private void Hmm()
        {
            var sprite = ResourceSystem.LoadResource<Sprite>(ResourceSystem.SystemTexturePath + "Hmm_0");
            balloon.sprite = sprite;
            var sprite2 = ResourceSystem.LoadResource<Sprite>(ResourceSystem.SystemTexturePath + "Hmm_1");
            balloon2.sprite = sprite2;

            var opacity = 1;
            var offsetX = 0;
            var offsetY = 176;
            InitAlpha(opacity);
            InitPosition(offsetX,offsetY);

            // 左右移動し上昇して消える
            var duration = 1.6f;
            var sequence = DOTween.Sequence()
                .SetEase(Ease.InOutQuad)
                .Append(balloon.DOFade(0, duration))
                .Join(balloon.transform.DOLocalMoveY(offsetY, duration))
                .OnComplete(() => 
                {
                    InitAlpha(opacity);
                    InitPosition(offsetX,offsetY);
                });
            sequence.SetLoops(-1);
            var sequence2 = DOTween.Sequence()
                .SetEase(Ease.InOutQuad)
                .Append(transform.DOLocalMoveX(24+offsetX, duration/2))
                .Append(transform.DOLocalMoveX(-24+offsetX, duration/2));
            sequence2.SetLoops(-1);
            // 浮き出てから消える
            var sequence3 = DOTween.Sequence()
                .SetEase(Ease.InOutQuad)
                .Append(balloon2.DOFade(1, duration/2))
                .Append(balloon2.DOFade(0, duration/2));
            sequence3.SetLoops(-1);
        }
        
        private void Silent()
        {
            var sprite = ResourceSystem.LoadResource<Sprite>(ResourceSystem.SystemTexturePath + "Silent_0");
            balloon.sprite = sprite;

            var opacity = 1;
            var offsetX = 80;
            var offsetY = 176;
            InitAlpha(opacity);
            InitPosition(offsetX,offsetY);

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
            balloon.sprite = sprite;
            var sprite2 = ResourceSystem.LoadResource<Sprite>(ResourceSystem.SystemTexturePath + "Light_1");
            balloon2.sprite = sprite2;

            var opacity = 1;
            var offsetX = 0;
            var offsetY = 176;
            InitAlpha(opacity);
            InitPosition(offsetX,offsetY);
            balloon2.DOFade(0,0);

            // 上下移動し発光して消える
            var duration = 0.8f;
            var afterWait = 0.8f;
            var sequence = DOTween.Sequence()
                .SetEase(Ease.InOutQuad)
                .Append(transform.DOLocalMoveY(64+offsetY, duration))
                .Append(transform.DOLocalMoveY(offsetY, afterWait));
            sequence.SetLoops(-1);
            var sequence2 = DOTween.Sequence()
                .SetEase(Ease.InOutQuad)
                .SetDelay(duration)
                .Append(balloon2.DOFade(1,0.1f))
                .Append(balloon2.DOFade(0,0.1f))
                .Append(balloon.DOFade(0,afterWait-0.2f))
                .OnComplete(() => 
                {
                    InitAlpha(opacity);
                    InitPosition(offsetX,offsetY);
                });
            sequence2.SetLoops(-1);
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
