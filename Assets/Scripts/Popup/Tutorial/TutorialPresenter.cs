using System.Collections.Generic;

namespace Ryneus
{
    public class TutorialPresenter : BasePresenter
    {
        private TutorialModel _model = null;
        private TutorialView _view = null;
        private bool _busy = false;
        public TutorialPresenter(TutorialView view)
        {
            _view = view;
            SetView(_view);
            _model = new TutorialModel();
            SetModel(_model);
            Initialize();
        }

        private void Initialize()
        { 
            _view.SetUIButton();
            _view.SetEvent((type) => UpdateCommand(type));

            CommandRefresh();
        }

        private void UpdateCommand(TutorialViewEvent viewEvent)
        {
            if (_busy || _view.AnimationBusy)
            {
                return;
            }
            UnityEngine.Debug.Log(viewEvent.commandType);
            switch (viewEvent.commandType)
            {
                case Tutorial.CommandType.Back:
                    CommandBack();
                    return;
                case Tutorial.CommandType.CallTutorialData:
                    CommandCallTutorialData((TutorialData)viewEvent.template);
                    return;
            }
        }

        private void CommandCallTutorialData(TutorialData tutorialData)
        {
        }

        private void CommandBack()
        {
            if (_view.CheckToggle)
            {
                // チュートリアル省略
                ConfigUtility.ChangeTutorialCheck(false);
            }
            _view.CommandBack();
            SoundManager.Instance.PlayStaticSe(SEType.Cancel);
        }

        private void CommandRefresh()
        {
        }

        private void SetBusy(bool busy)
        {
            _busy = busy;
            _view.SetBusy(busy);
        }

        private void CommandCallHelp()
        {
            _busy = true;
            var popupInfo = new PopupInfo
            {
                PopupType = PopupType.Guide,
                template = "Tutorial",
                EndEvent = () =>
                {
                    _busy = false;
                    SoundManager.Instance.PlayStaticSe(SEType.Cancel);
                }
            };
            _view.CommandCallPopup(popupInfo);
        }
    }
}