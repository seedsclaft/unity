using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuActor : ListItem ,IListViewItem  
{    
    private ActorsData.ActorData _data; 
    private Sprite _image; 
    [SerializeField] private Image thumb;

    public void SetData(ActorsData.ActorData data,Sprite image){
        _data = data;
        _image = image;
    }

    public void SetCallHandler(System.Action<ActorsData.ActorData> handler)
    {
        if (_data == null) return;
        clickButton.onClick.AddListener(() => handler(_data));
    }

    public void UpdateViewItem()
    {
        if (_data == null) return;
        thumb.sprite = _image;
    }
}
