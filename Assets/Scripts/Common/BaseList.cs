using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class BaseList : ListWindow , IInputHandlerEvent
{
    private List<ListData> _listData = new ();
    private bool _isInit = false;
    public bool IsInit => _isInit;
    public void SetData(List<ListData> listData)
    {
        Initialize(listData.Count);
        _listData = listData;
        if (_listData.Count > ObjectList.Count)
        {
            var objectListCount = ObjectList.Count;
            AddCreateList(_listData.Count-ObjectList.Count);
            for (int i = 0; i < ObjectList.Count;i++)
            {
                if (i >= objectListCount)
                {
                    var listItem = ObjectList[i].GetComponent<ListItem>();
                    listItem.SetCallHandler(() => CallListInputHandler(InputKeyType.Decide));
                    listItem.SetSelectHandler((index) => 
                    {
                        UpdateSelectIndex(index);
                    });
                }
            }
            SetDataCount(_listData.Count);
            SetItemCount(_listData.Count);
        }
        Refresh(_listData.FindIndex(a => a.Selected));
    }

    public ListData ListData 
    { 
        get {
            if (Index > -1 && _listData.Count > Index)
            {
                return _listData[Index];
            }
            return null;
        }
    }
    
    public void Initialize(int listCount)
    {
        if (_isInit)
        {
            return;
        }
        InitializeListView(listCount);
        InitializeList();
        /*
        UpdateAllItems();
        */
        UpdateSelectIndex(0);
        SetInputCallHandler((a) => CallSelectHandler(a));
        _isInit = true;
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
            if (i < _listData.Count) 
            {
                var listItem = ObjectList[i].GetComponent<ListItem>();
                listItem.SetListData(_listData[i],i);
            }
            ObjectList[i].SetActive(i < _listData.Count);
        }
        //ResetScrollPosition();
        UpdateAllItems();
        UpdateSelectIndex(selectIndex);
        if (selectIndex > 3)
        {
            //await UniTask.DelayFrame(1);
            UpdateScrollRect(selectIndex);
        } else
        {        
            ResetScrollRect();
        }
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


[System.Serializable]
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