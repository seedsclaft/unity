using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ruling;

namespace Ryneus
{
    public class RulingPresenter 
    {
        RulingView _view = null;

        RulingModel _model = null;
        private bool _busy = true;
        public RulingPresenter(RulingView view)
        {
            _view = view;
            _model = new RulingModel();

            Initialize();
        }

        private void Initialize()
        {
            _view.SetEvent((type) => UpdateCommand(type));
            _view.SetRulingCommand(_model.RulingCommand());
            CommandRefresh();
            _view.CommandSelectTitle(_model.RuleHelp());
            _busy = false;
        }

        
        private void UpdateCommand(RulingViewEvent viewEvent)
        {
            if (_busy){
                return;
            }
            if (viewEvent.commandType == Ruling.CommandType.SelectTitle)
            {
                CommandSelectTitle((int)viewEvent.template);
            }
            if (viewEvent.commandType == Ruling.CommandType.SelectCategory)
            {
                CommandSelectCategory((int)viewEvent.template);
            }
        }

        private void CommandSelectTitle(int id)
        {
            _model.SetId(id);
            _view.CommandSelectTitle(_model.RuleHelp());
        }

        private void CommandSelectCategory(int id)
        {
            _model.SetCategory(id);
            _model.SetId(-1);
            CommandRefresh();
        }

        private void CommandRefresh()
        {
            _view.CommandRefresh(_model.RuleHelp());
        }
    }
}