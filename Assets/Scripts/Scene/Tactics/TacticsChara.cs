using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TacticsChara : MonoBehaviour
{
    [SerializeField] private ActorInfoComponent actorInfoComponent;
    
    [SerializeField] private Button clickButton;
    private ActorInfo _data;
    public void SetData(ActorInfo actorInfo)
    {
        actorInfoComponent.UpdateInfo(actorInfo);
        _data = actorInfo;
        
    }
    
    public void SetCallHandler(System.Action<ActorInfo> handler)
    {
        if (_data == null) return;
        clickButton.onClick.AddListener(() => handler((ActorInfo)_data));
    }
}
