using System.Collections;
using System.Collections.Generic;

namespace Ryneus
{
    public class ScorePrizePresenter 
    {
        ScorePrizeModel _model = null;
        ScorePrizeView _view = null;

        private bool _busy = true;
        public ScorePrizePresenter(ScorePrizeView view)
        {
            _view = view;
            _model = new ScorePrizeModel();

            Initialize();
        }

        private void Initialize()
        {
            _view.SetEvent((type) => UpdateCommand(type));
            _view.SetHelpInputInfo("CHARACTER_LIST");
            _view.SetScorePrize(_model.ScorePrize());
        }

        private void UpdateCommand(ScorePrizeViewEvent viewEvent)
        {
            if (_busy)
            {
                return;
            }
        }
    }
}