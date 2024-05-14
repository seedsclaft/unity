using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SkillLog;

namespace Ryneus
{
    public class SkillLogView : BaseView
    {
        [SerializeField] private BaseList skillLogList = null;
        private new System.Action<SkillLogViewEvent> _commandData = null;

        public override void Initialize() 
        {
            base.Initialize();
            skillLogList.Initialize();
            new SkillLogPresenter(this);
        }

        public void SetEvent(System.Action<SkillLogViewEvent> commandData)
        {
            _commandData = commandData;
        }
        
        public void SetSkillLogViewInfo(List<SkillLogListInfo> listDates)
        {
            skillLogList.gameObject.SetActive(true);
            skillLogList.SetData(ListData.MakeListData(listDates));
        }
    }
}

namespace SkillLog
{
    public enum CommandType
    {
        None = 0,
    }
}

public class SkillLogViewEvent
{
    public CommandType commandType;
    public object template;

    public SkillLogViewEvent(CommandType type)
    {
        commandType = type;
    }
}

