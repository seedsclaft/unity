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
            _view.SetEvent((type) => UpdateCommand(type));
        }

        private void UpdateCommand(AlcanaResultViewEvent viewEvent)
        {
            if (_view.Busy){
                return;
            }
            switch (viewEvent.commandType)
            {
            }
        }

    }
}