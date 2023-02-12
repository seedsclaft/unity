using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.AddressableAssets;
using TMPro;

public class SkillAttribute : ListItem ,IListViewItem ,IClickHandlerEvent 
{
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI valueText;
    private AttributeType _data; 
    private string _valueText; 
    private int _index; 
    private EventTrigger eventTrigger;
    private EventTrigger.Entry entry1;
    private System.Action<int> _selectHandler;
    public void SetData(AttributeType data,string valueText,int index){
        _data = data;
        _valueText = valueText;
        _index = index;
    }

    public int listIndex(){
        return _index;
    }

    public void SetCallHandler(System.Action<AttributeType> handler)
    {
        if (_data == AttributeType.None) return;
        clickButton.onClick.AddListener(() => handler((AttributeType)_index));
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
        if (_data == AttributeType.None) return;
        if (icon != null)
        {
            UpdateSkillIcon(_index-1);
        }
        
        if (valueText != null && _valueText != null)
        {
            valueText.text = _valueText;
        }
    }

    public void ClickHandler()
    {
    }

    void OnMyPointerEnter(BaseEventData data) {
        _selectHandler(_index);
	}

    
    private void UpdateSkillIcon(int index)
    {
        Addressables.LoadAssetAsync<IList<Sprite>>(
            "Assets/Images/System/ElementIcon.png"
        ).Completed += op => {
            icon.sprite = op.Result[index];
        };
    }

}
