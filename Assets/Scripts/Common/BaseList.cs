using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseList : ListWindow , IInputHandlerEvent
{
    private List<ListData> _listData = new ();
    private System.Action _selectedHandler;
    public void SetData(List<ListData> listData)
    {
        _listData = listData;
        Refresh();
    }

    public ListData ListData 
    { 
        get {
            if (Index > -1)
            {
                return _listData[Index];
            }
            return null;
        }
    }
    
    public void Initialize(int listCount)
    {
        InitializeListView(listCount);
        InitializeList();
        /*
        UpdateAllItems();
        */
        UpdateSelectIndex(0);
        Deactivate();
    }
    
    private void InitializeList()
    {
        for (int i = 0; i < ObjectList.Count;i++)
        {
            var listItem = ObjectList[i].GetComponent<ListItem>();
            listItem.SetCallHandler(() => CallListInputHandler(InputKeyType.Decide));
            listItem.SetSelectHandler((index) => 
            {
                UpdateSelectIndex(index);
            });
        }
    }    
    
    public void Refresh(int selectIndex = 0)
    {
        for (int i = 0; i < ObjectList.Count;i++)
        {
            var listItem = ObjectList[i].GetComponent<ListItem>();
            listItem.SetListData(_listData[i],i);
        }
        //ResetScrollPosition();
        UpdateSelectIndex(selectIndex);
        UpdateAllItems();
    }
    
    public void RefreshListData(ListData listData)
    {
        var findIndex = _listData.FindIndex(a => a.Index == listData.Index);
        if (findIndex > -1)
        {
            _listData[findIndex] = listData;
        }
    }
}


[System.SerializableAttribute]
public class ListData
{    
    private int _index;
    public int Index => _index;
    private object _data;
    public object Data => _data;
    private bool _enable = true;
    public bool Enable => _enable;
    public void SetEnable(bool enable)
    {
        _enable = enable;
    }
    private bool _selected = true;
    public bool Selected => _selected;
    public void SetSelected(bool selected)
    {
        _selected = selected;
    }
    public ListData(object data,int index = 0,bool enable = true)
    {
        _data = data;
        _index = index;
        _enable = enable;
    }
}