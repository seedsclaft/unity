using System.Collections;
using System.Collections.Generic;

namespace Ryneus
{
    public class GuidePresenter 
    {
        GuideModel _model = null;
        GuideView _view = null;

        private bool _busy = true;
        public GuidePresenter(GuideView view)
        {
            _view = view;
            _model = new GuideModel();

            Initialize();
        }

        private void Initialize()
        {
            _view.SetEvent((type) => UpdateCommand(type));
            _view.OpenAnimation();
            _busy = false;
        }

        private void UpdateCommand(GuideViewEvent viewEvent)
        {
            if (_busy || _view.AnimationBusy)
            {
                return;
            }
            switch (viewEvent.commandType)
            {
                case Guide.CommandType.PageLeft:
                    CommandPageLeft();
                    break;
                case Guide.CommandType.PageRight:
                    CommandPageRight();
                    break;
                case Guide.CommandType.StartGuide:
                    CommandStartGuide((string)viewEvent.template);
                    break;
                case Guide.CommandType.CallHelp:
                    CommandCallHelp();
                    break;
            }
        }

        private void CommandPageLeft()
        {
            SoundManager.Instance.PlayStaticSe(SEType.Cursor);
            _model.PageLeft();
            CommandRefresh();
        }

        private void CommandPageRight()
        {
            SoundManager.Instance.PlayStaticSe(SEType.Cursor);
            _model.PageRight();
            CommandRefresh();
        }

        private void CommandStartGuide(string guideKey)
        {
            _model.SetGuideDates(guideKey);
            CommandRefresh();
        }

        private void CommandCallHelp()
        {
            var callHelpId = _model.CallHelpId();
            _view.CommandHelpList(DataSystem.HelpText(callHelpId));
        }

        private void CommandRefresh()
        {
            _view.SetGuideImage(_model.GuideSprite());
            _view.SetHelpText(_model.GuideTextList());
            _view.SetLeftRight(_model.NeedLeftPage(),_model.NeedRightPage());
        }
    }
}