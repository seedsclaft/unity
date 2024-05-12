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
            _view.SetRuleCommand(_model.RulingCommand());
            CommandRefresh();
            _view.CommandSelectRule(_model.RuleHelp());
            _busy = false;
        }

        
        private void UpdateCommand(RulingViewEvent viewEvent)
        {
            if (_busy){
                return;
            }
            switch (viewEvent.commandType)
            {
                case CommandType.SelectRule:
                    CommandSelectRule((int)viewEvent.template);
                    break;
                case CommandType.SelectCategory:
                    CommandSelectCategory((int)viewEvent.template);
                    break;
            }
        }

        private void CommandSelectRule(int id)
        {
            _model.SetId(id);
            _view.CommandSelectRule(_model.RuleHelp());
        }

        private void CommandSelectCategory(int id)
        {
            _model.SetCategory(id);
            _view.SetRuleCommand(_model.RulingCommand());
            CommandRefresh();
        }

        private void CommandRefresh()
        {
            _view.CommandRefresh(_model.RuleHelp());
        }
    }
}