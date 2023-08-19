using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuStageList: ListWindow , IInputHandlerEvent
{   
    private List<StageInfo> _data = new List<StageInfo>();
    [SerializeField] private StageInfoComponent component;

    public StageInfo Data{
        get {
            if (Index < 0)
            {
                return null;
            }
            return _data[Index];
        }
    }

    public void Initialize(List<StageInfo> stages)
    {
        InitializeListView(stages.Count);
        _data = stages;
        for (int i = 0; i < stages.Count;i++)
        {
            MainMenuStage mainMenuStage = ObjectList[i].GetComponent<MainMenuStage>();
            mainMenuStage.SetData(stages[i],i);
            mainMenuStage.SetCallHandler(() => CallListInputHandler(InputKeyType.Decide));
            mainMenuStage.SetSelectHandler((data) => UpdateSelectIndex(data));
        }
        UpdateAllItems();
        UpdateSelectIndex(0);
        component.UpdateInfo(_data[Index]);
    }
    
    public override void UpdateHelpWindow(){
        if (component != null){
            component.UpdateInfo(_data[Index]);
        }
    }
}
