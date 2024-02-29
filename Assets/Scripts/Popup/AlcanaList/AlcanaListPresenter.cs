using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AlcanaList;

namespace Ryneus
{
    public class AlcanaListPresenter 
    {
        AlcanaListModel _model = null;
        AlcanaListView _view = null;

        private bool _busy = true;
        public AlcanaListPresenter(AlcanaListView view)
        {
            _view = view;
            _model = new AlcanaListModel();

            Initialize();
        }

        private void Initialize()
        {
            _view.SetEvent((type) => UpdateCommand(type));
            //_view.SetHelpInputInfo("AlcanaList");
            _view.SetAlcanaList(_model.AlcanaList());
        }

        private void UpdateCommand(AlcanaListViewEvent viewEvent)
        {
            if (_busy){
                return;
            }
        }
    }
}