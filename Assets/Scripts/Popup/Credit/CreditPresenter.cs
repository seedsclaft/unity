using System.Collections;
using System.Collections.Generic;

namespace Ryneus
{
    public class CreditPresenter 
    {
        private CreditView _view = null;
        private bool _busy = true;
        public CreditPresenter(CreditView view)
        {
            _view = view;

            Initialize();
        }

        private void Initialize()
        {
            _view.SetEvent((type) => UpdateCommand(type));
            _view.OpenAnimation();
            _busy = false;
        }

        private void UpdateCommand(CreditViewEvent viewEvent)
        {
            if (_busy || _view.AnimationBusy)
            {
                return;
            }
        }
    }

    public class CreditInfo
    {
    }
}