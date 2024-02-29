using AlcanaResult;

namespace Ryneus
{
    public class AlcanaResultPresenter : BasePresenter
    {
        private AlcanaResultModel _model = null;
        private AlcanaResultView _view = null;
        private bool _busy = false;
        public AlcanaResultPresenter(AlcanaResultView view)
        {
            _view = view;
            SetView(_view);
            _model = new AlcanaResultModel();
            SetModel(_model);
            Initialize();
        }

        private void Initialize()
        { 
            _view.SetHelpWindow();
            _view.SetResultList(_model.AlcanaResultCommand());
            _view.SetActors(_model.AlcanaMembers());
            _view.SetEvent((type) => UpdateCommand(type));
            _busy = true;

            _view.StartAnimation();
            _view.StartResultAnimation(_model.MakeListData(_model.AlcanaMembers()));
            _busy = false;
        }

        private void UpdateCommand(AlcanaResultViewEvent viewEvent)
        {
            if (_view.Busy){
                return;
            }
            switch (viewEvent.commandType)
            {
                case CommandType.EndAnimation:
                    CommandEndAnimation();
                    break;
                case CommandType.ResultClose:
                    CommandResultClose((ConfirmCommandType)viewEvent.template);
                    break;
            }
        }

        private void CommandEndAnimation()
        {
            _view.ShowResultList(_model.ResultGetItemInfos());
        }

        private void CommandResultClose(ConfirmCommandType confirmCommandType)
        {
            if (confirmCommandType == ConfirmCommandType.Yes)
            {
                CommandEndAlcana();
            } else
            {
                ShowStatus();
            }
            Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Decide);
        }

        private void ShowStatus()
        {
            var statusViewInfo = new StatusViewInfo(() => {
                _view.CommandGameSystem(Base.CommandType.CloseStatus);
                _view.SetHelpText(DataSystem.GetTextData(14010).Text);
                _view.ChangeUIActive(true);
            });
            statusViewInfo.SetDisplayDecideButton(false);
            _view.CommandCallStatus(statusViewInfo);
            _view.ChangeUIActive(false);
        }

        private void CommandEndAlcana()
        {
            _model.ReleaseAlcana();
            _view.CommandSceneChange(Scene.Tactics);
        }
    }
}