using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuStageList: ListWindow , IInputHandlerEvent
{   
    [SerializeField] private int rows = 0;
    [SerializeField] private int cols = 0;
    private List<StageInfo> _data = new List<StageInfo>();
    [SerializeField] private StageInfoComponent component;

    public int selectIndex{
        get {return Index;}
    }

    public void Initialize(List<StageInfo> stages,System.Action<StageInfo> callEvent)
    {
        InitializeListView(stages.Count);
        for (var i = 0; i < stages.Count;i++){
            _data.Add(stages[i]);
        }
        for (int i = 0; i < ObjectList.Count;i++)
        {
            var stage = ObjectList[i].GetComponent<MainMenuStage>();
            stage.SetData(stages[i],i);
            stage.SetCallHandler(callEvent);
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
