using System.Collections;
using System.Collections.Generic;

namespace Ryneus
{
    public class HelpPresenter 
    {
        HelpModel _model = null;
        HelpView _view = null;

        private bool _busy = true;
        public HelpPresenter(HelpView view)
        {
            _view = view;
            _model = new HelpModel();

            Initialize();
        }

        private void Initialize()
        {
            _view.SetEvent((type) => UpdateCommand(type));
            _view.SetHelpInputInfo("HELP_LIST");
            _view.OpenAnimation();
        }

        private void UpdateCommand(HelpViewEvent viewEvent)
        {
            if (_busy || _view.AnimationBusy)
            {
                return;
            }
        }
    }
}