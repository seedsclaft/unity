using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using TMPro;

public class StatusCondition : ListItem ,IListViewItem ,IClickHandlerEvent 
{
    [SerializeField] private StateInfoComponent stateInfoComponent;

    private StateInfo _stateInfo; 
    private int _index; 
    private EventTrigger eventTrigger;
    private EventTrigger.Entry entry1;
    private System.Action<int> _selectHandler;
    public void SetData(StateInfo stateInfo,int index){
        _stateInfo = stateInfo;
        _index = index;
    }

    public int listIndex(){
        return _index;
    }

    public void SetCallHandler(System.Action<int> handler)
    {
        if (_stateInfo == null) return;
        clickButton.onClick.AddListener(() => handler(_index));
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
        if (_stateInfo == null) return;
        stateInfoComponent.UpdateInfo(_stateInfo);
    }

    public void ClickHandler()
    {
    }

    void OnMyPointerEnter(BaseEventData data) {
        _selectHandler(_index);
	}
}
