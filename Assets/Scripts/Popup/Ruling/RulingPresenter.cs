using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ruling;

namespace Ryneus
{
    public class RulingPresenter : BasePresenter
    {
        RulingView _view = null;

        RulingModel _model = null;
        private bool _busy = true;
        public RulingPresenter(RulingView view)
        {
            _view = view;
            _model = new RulingModel();

            SetView(_view);
            SetModel(_model);
            Initialize();
        }

        private void Initialize()
        {
            _view.SetEvent((type) => UpdateCommand(type));
            _view.SetRuleCommand(GetListData(_model.RulingCommand()));
            CommandRefresh();
            _view.CommandSelectRule(GetListData(_model.RuleHelp()));
            _view.OpenAnimation();
            _busy = false;
        }

        
        private void UpdateCommand(RulingViewEvent viewEvent)
        {
            if (_busy || _view.AnimationBusy)
            {
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
            _view.CommandSelectRule(GetListData(_model.RuleHelp()));
        }

        private void CommandSelectCategory(int id)
        {
            _model.SetCategory(id);
            _view.SetRuleCommand(GetListData(_model.RulingCommand()));
            CommandRefresh();
        }

        private void CommandRefresh()
        {
            _view.CommandRefresh(GetListData(_model.RuleHelp()));
        }
    }
}