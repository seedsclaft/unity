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
        public void Initialize(GameObject parent,float x,float y,float scale)
        {
            _isInit = true;
            _sequence = new List<Sequence>();
            _sequence2 = new List<Sequence>();
        }

        public void SetData(ActorInfo actorInfo)
        {
            actorInfoComponent.UpdateInfo(actorInfo,null);
            _actorInfo = actorInfo;
            HideCursor();
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
                if (idx < 2)
                {
                    var sequence2 = DOTween.Sequence()
                        .Append(rect.DOScaleX(startScale, 0.0f))
                        .Join(selectCursorImage.DOFade(0f, 0.0f))
                        .Append(rect.DOScaleX(1f, startDuration))
                        .Join(selectCursorImage.DOFade(maxFade, startDuration));
                        _sequence2.Add(sequence2);
                } else
                {
                    var sequence2 = DOTween.Sequence()
                        .Append(rect.DOScaleY(startScale, 0.0f))
                        .Join(selectCursorImage.DOFade(0f, 0.0f))
                        .Append(rect.DOScaleY(1f, startDuration))
                        .Join(selectCursorImage.DOFade(maxFade, startDuration));
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

        private void EndSelectCursor()
        {
            foreach (var selectCursorRect in selectCursorImages)
            {
                var sequence = DOTween.Sequence()
                    .Append(selectCursorRect.DOFade(0f, 0.0f));
                _sequence.Add(sequence);
            }
        }

        private void KillSequence()
        {
            _sequence.ForEach(a => a.Kill());
            _sequence2.ForEach(a => a.Kill());
        }
    }
}