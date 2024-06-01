using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SkillLog;

namespace Ryneus
{
    public class SkillLogPresenter : BasePresenter
    {
        SkillLogModel _model = null;
        SkillLogView _view = null;

        private bool _busy = true;
        public SkillLogPresenter(SkillLogView view)
        {
            _model = new SkillLogModel();
            _view = view;
            SetModel(_model);
            SetView(_view);
            Initialize();
        }

        private void Initialize()
        {
            SoundManager.Instance.PlayStaticSe(SEType.Decide);
            _busy = false;
            _view.SetEvent((type) => UpdateCommand(type));
            _view.SetHelpInputInfo("SkillLog");
        }

        private void UpdateCommand(SkillLogViewEvent viewEvent)
        {
            if (_busy){
                return;
            }
        }
    }
    public class SkillLogViewInfo
    {
        private System.Action _endEvent = null;
        public System.Action EndEvent => _endEvent;
        private List<SkillLogListInfo> _skillLogListInfos;
        public List<SkillLogListInfo> SkillLogListInfos => _skillLogListInfos;
        public SkillLogViewInfo(List<SkillLogListInfo> skillLogListInfos,System.Action endEvent)
        {
            _endEvent = endEvent;
            _skillLogListInfos = skillLogListInfos;
        }
    }
}