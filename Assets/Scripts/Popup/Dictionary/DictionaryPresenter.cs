using System.Collections;
using System.Collections.Generic;

namespace Ryneus
{
    public class DictionaryPresenter 
    {
        DictionaryModel _model = null;
        DictionaryView _view = null;

        private bool _busy = true;
        public DictionaryPresenter(DictionaryView view)
        {
            _view = view;
            _model = new DictionaryModel();

            Initialize();
        }

        private void Initialize()
        {
            _view.SetEvent((type) => UpdateCommand(type));
            _view.OpenAnimation();
            _view.SetCategoryList(ListData.MakeListData(_model.SkillCategory()));
            CommandDictionary(SkillType.Active);
            _busy = false;
        }

        private void UpdateCommand(DictionaryViewEvent viewEvent)
        {
            if (_busy || _view.AnimationBusy)
            {
                return;
            }
            switch (viewEvent.commandType)
            {
                case Dictionary.CommandType.SelectCategory:
                    CommandDictionary((SkillType)viewEvent.template);
                    break;
            }
        }

        private void CommandDictionary(SkillType skillType)
        {
            var skillList = ListData.MakeListData(_model.CategorySkillList(skillType),(a) => { return true;},0);
            _view.SetMagicList(skillList);
        }
    }
}