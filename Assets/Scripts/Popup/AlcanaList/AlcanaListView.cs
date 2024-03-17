using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ryneus
{
    public class AlcanaListView : BaseView
    {
        [SerializeField] private BaseList alcanaList = null;
        private new System.Action<AlcanaListViewEvent> _commandData = null;
        
        public override void Initialize() 
        {
            base.Initialize();
            new AlcanaListPresenter(this);
            alcanaList.SetInputHandler(InputKeyType.Cancel,() => BackEvent());
            alcanaList.SetInputHandler(InputKeyType.Decide,() => {});
            SetInputHandler(alcanaList.GetComponent<IInputHandlerEvent>());
        }
        
        public void SetEvent(System.Action<AlcanaListViewEvent> commandData)
        {
            _commandData = commandData;
        }

        public void SetAlcanaList(List<ListData> alcanaLists)
        {
            alcanaList.SetData(alcanaLists);
            alcanaList.Activate();
        }
    }
}

namespace AlcanaList
{
    public enum CommandType
    {
        None = 0,
    }
}

public class AlcanaListViewEvent
{
    public AlcanaList.CommandType commandType;
    public object template;

    public AlcanaListViewEvent(AlcanaList.CommandType type)
    {
        commandType = type;
    }
}