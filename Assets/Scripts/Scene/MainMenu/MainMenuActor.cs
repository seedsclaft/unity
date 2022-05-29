using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuActor : ListItem ,IListViewItem  
{   
    [SerializeField] private ActorInfoComponent component;
    private ActorInfo _data; 


    public void SetData(ActorInfo data){
        _data = data;
    }

    public void SetCallHandler(System.Action<ActorInfo> handler)
    {
        if (_data == null) return;
        clickButton.onClick.AddListener(() => handler(_data));
    }

    public void UpdateViewItem()
    {
        if (_data == null) return;
        //component.UpdateInfo(_data);
    }
}
