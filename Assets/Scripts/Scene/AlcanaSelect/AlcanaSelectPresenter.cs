
namespace Ryneus
{
    public class AlcanaSelectPresenter : BasePresenter
    {
        private AlcanaSelectModel _model = null;
        private AlcanaSelectView _view = null;
        public AlcanaSelectPresenter(AlcanaSelectView view)
        {
            _view = view;
            SetView(_view);
            _model = new AlcanaSelectModel();
            SetModel(_model);
            Initialize();
        }

        private void Initialize()
        { 
            _view.SetEvent((type) => UpdateCommand(type));
        }

        private void UpdateCommand(AlcanaSelectViewEvent viewEvent)
        {
            if (_view.Busy){
                return;
            }
        }
    }
}