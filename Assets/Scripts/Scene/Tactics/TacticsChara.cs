using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace Ryneus
{
    public class TacticsChara : OnOffButton
    {
        [SerializeField] private ActorInfoComponent actorInfoComponent;
        [SerializeField] private GameObject selectCursor;
        [SerializeField] private List<Image> selectCursorImages;
        public ActorInfo ActorInfo => _actorInfo;
        private ActorInfo _actorInfo;
        private bool _isInit = false;
        public bool IsInit => _isInit;

        private List<Sequence> _sequence;
        private List<Sequence> _sequence2;

        private float _backLocalX;
        public float BackLocalX => _backLocalX;
        private float _backLocalY;
        public float BackLocalY => _backLocalY;
        private float _backScale;
        public float BackScale => _backScale;
        public void Initialize(GameObject parent,float x,float y,float scale)
        {
            _sequence = new List<Sequence>();
            _sequence2 = new List<Sequence>();
        }

        public void SetData(ActorInfo actorInfo)
        {
            actorInfoComponent.UpdateInfo(actorInfo,null);
            _actorInfo = actorInfo;
            HideCursor();
            if (_isInit == false)
            {
                var parent = gameObject.transform.parent;
                var parentRect = parent.GetComponent<RectTransform>();
                _backLocalX = parentRect.localPosition.x;
                _backLocalY = parentRect.localPosition.y;
                _backScale = parentRect.localScale.x;
                _isInit = true;
            }
        }

        public void HideCursor()
        {
            SetActiveCursor(false);
        }

        public void Show()
        {
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public void SetSelectCursor(bool isSelected)
        {
            selectCursor?.SetActive(isSelected);
            KillSequence();
            if (isSelected)
            {
                StartSelectCursor();
            } else
            {
                EndSelectCursor();
            }
        }

        private void StartSelectCursor()
        {
            var startDuration = 0.4f;
            var startScale = 40;
            var blinkDuration = 0.2f;
            var maxFade = 0.8f;
            var idx = 0;
            foreach (var selectCursorImage in selectCursorImages)
            {
                var rect = selectCursorImage.GetComponent<RectTransform>();
                if (idx == 0)
                {
                    var sequence2 = DOTween.Sequence()
                        .Append(rect.DOScaleX(startScale, 0.0f))
                        .Join(selectCursorImage.DOFade(0f, 0.0f))
                        .Join(rect.DOLocalMoveY(480, 0.0f))
                        .Append(rect.DOScaleX(1f, startDuration))
                        .Join(selectCursorImage.DOFade(maxFade, startDuration))
                        .Join(rect.DOLocalMoveY(120, startDuration));
                        _sequence2.Add(sequence2);
                } else
                if (idx == 1)
                {
                    var sequence2 = DOTween.Sequence()
                        .Append(rect.DOScaleX(startScale, 0.0f))
                        .Join(selectCursorImage.DOFade(0f, 0.0f))
                        .Join(rect.DOLocalMoveY(-480, 0.0f))
                        .Append(rect.DOScaleX(1f, startDuration))
                        .Join(selectCursorImage.DOFade(maxFade, startDuration))
                        .Join(rect.DOLocalMoveY(-120, startDuration));
                        _sequence2.Add(sequence2);
                } else
                if (idx == 2)
                {
                    var sequence2 = DOTween.Sequence()
                        .Append(rect.DOScaleY(startScale, 0.0f))
                        .Join(selectCursorImage.DOFade(0f, 0.0f))
                        .Join(rect.DOLocalMoveX(-480, 0.0f))
                        .Append(rect.DOScaleY(1f, startDuration))
                        .Join(selectCursorImage.DOFade(maxFade, startDuration))
                        .Join(rect.DOLocalMoveX(-120, startDuration));
                        _sequence2.Add(sequence2);
                } else
                {
                    var sequence2 = DOTween.Sequence()
                        .Append(rect.DOScaleY(startScale, 0.0f))
                        .Join(selectCursorImage.DOFade(0f, 0.0f))
                        .Join(rect.DOLocalMoveX(480, 0.0f))
                        .Append(rect.DOScaleY(1f, startDuration))
                        .Join(selectCursorImage.DOFade(maxFade, startDuration))
                        .Join(rect.DOLocalMoveX(120, startDuration));
                        _sequence2.Add(sequence2);
                }
                var sequence = DOTween.Sequence()
                    .SetDelay(0.5f)
                    .Append(selectCursorImage.DOFade(0f, blinkDuration))
                    .Append(selectCursorImage.DOFade(maxFade, blinkDuration))
                    .SetLoops(-1);
                _sequence.Add(sequence);
                idx++;
            }
        }

        public void EndSelectCursor()
        {
            foreach (var selectCursorRect in selectCursorImages)
            {
                var sequence = DOTween.Sequence()
                    .Append(selectCursorRect.DOFade(0f, 0.0f));
                _sequence.Add(sequence);
            }
        }

        public void KillSequence()
        {
            _sequence.ForEach(a => a.Kill());
            _sequence2.ForEach(a => a.Kill());
        }

        public void ZoomActor()
        {
            KillSequence();
            EndSelectCursor();
            ShowActor();
            var duration = 0.25f;
            var parent = gameObject.transform.parent;
            var scale = 0.9f;
            var x = -80;
            var y = 320;
            var sequence = DOTween.Sequence()
                .Append(parent.DOScale(scale,duration))
                .Join(parent.DOLocalMoveX(x,duration))
                .Join(parent.DOLocalMoveY(y,duration));
            _sequence.Add(sequence);
        }

        public void EndZoomActor()
        {
            KillSequence();
            var duration = 0.25f;
            var parent = gameObject.transform.parent;
            var sequence = DOTween.Sequence()
                .Append(parent.DOScale(_backScale,duration))
                .Join(parent.DOLocalMoveX(_backLocalX,duration))
                .Join(parent.DOLocalMoveY(_backLocalY,duration));
            ShowActor();
            _sequence.Add(sequence);
        }

        public void ShowActor()
        {
            var sequence = DOTween.Sequence()
                .Append(actorInfoComponent.MainThumb.DOFade(1f, 0.25f));
            _sequence.Add(sequence);
        }

        public void HideActor()
        {
            var sequence = DOTween.Sequence()
                .Append(actorInfoComponent.MainThumb.DOFade(0f, 0.25f));
            _sequence.Add(sequence);
        }
    }
}