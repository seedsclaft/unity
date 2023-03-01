using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using TMPro;

public class StatusStrength : ListItem ,IListViewItem ,IClickHandlerEvent 
{
    
    [SerializeField] private StrengthComponent strengthComponent;
    [SerializeField] private Button plusButton;
    [SerializeField] private Button minusButton;

    private ActorInfo _data; 
    private int _index; 
    private EventTrigger eventTrigger;
    private EventTrigger.Entry entry1;
    private System.Action<int> _selectHandler;
    public void SetData(ActorInfo data,int index){
        _data = data;
        _index = index;
    }

    public int listIndex(){
        return _index;
    }

    public void SetCallHandler(System.Action<int> handler)
    {
        if (_data == null) return;
        clickButton.onClick.AddListener(() => handler(_index));
    }

    public void SetPlusHandler(System.Action<int> handler)
    {
        plusButton.onClick.AddListener(() => handler(_index));
    }

    public void SetMinusHandler(System.Action<int> handler)
    {
        minusButton.onClick.AddListener(() => handler(_index));
    }

    public void SetSelectHandler(System.Action<int> handler){
        //　EventTriggerコンポーネントを取り付ける
		eventTrigger = clickButton.gameObject.AddComponent<EventTrigger> ();
        //　ボタン内にマウスが入った時のイベントリスナー登録（ラムダ式で設定）
		entry1 = new EventTrigger.Entry ();
		entry1.eventID = EventTriggerType.PointerEnter;
		entry1.callback.AddListener (data => OnMyPointerEnter((BaseEventData) data));
		eventTrigger.triggers.Add (entry1);

        _selectHandler = handler;
    }

    public void UpdateViewItem()
    {
        if (_data == null) return;
        strengthComponent.UpdateInfo(_data,_index);
    }

    public void ClickHandler()
    {
    }

    void OnMyPointerEnter(BaseEventData data) {
        _selectHandler(_index);
	}
}
