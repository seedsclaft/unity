using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


abstract public class ListWindow : MonoBehaviour
{
    private bool _active = true;
    public bool Active => _active;

    private int _index = 0;
    public int Index => _index;
    private int _defaultInputFrame = 0;

    private int _listMoveInputFrameFirst = 36;
    private int _listMoveGamePadFrameFirst = 39;
    private int _listMoveInputFrame = 6;
    private int _listMoveGamePadFrame = 9;
    public void SetInputFrame(int frame)
    {
        _defaultInputFrame = frame;
    }
    private int _inputBusyFrame = 0;

    //[SerializeField] private bool isScrollList = true; // 表示数が初期プレハブ数より多くなるか
    [SerializeField] private bool horizontal = false; 
    [SerializeField] private bool reverse = false; 
    [SerializeField] private bool warpMode = true; 
    [SerializeField] private ScrollRect scrollRect = null; 
    public ScrollRect ScrollRect => scrollRect; 
    [SerializeField] private GameObject itemPrefab = null; 
    private List<GameObject> _itemPrefabList = new ();
    public List<GameObject> ItemPrefabList => _itemPrefabList;
    private List<ListData> _listData = new ();
    public int DataCount => _listData.Count;
    private Vector2 _itemSize;
    private int _lastStartIndex = -1;
    private LinkedList<IListViewItem> _itemList = new();
    private List<GameObject> _objectList = new ();
    public List<GameObject> ObjectList => _objectList;

    private System.Action<InputKeyType> _inputCallHandler = null;
    private Dictionary<InputKeyType, System.Action> _inputHandler = new ();

    private System.Action _selectedHandler = null;

    public HelpWindow _helpWindow = null;

    private System.Action _cancelEvent = null;

    private GameObject _blankObject;
    public void SetCancelEvent(System.Action cancelEvent)
    {
        _cancelEvent = cancelEvent;
    }

    public void Activate()
    {
        ResetInputOneFrame();
        _active = true;
    }
    
    public void Deactivate()
    {
        _active = false;
    }

    private void DestroyListChildren()
    {
        foreach(Transform child in scrollRect.content.transform){
            Destroy(child.gameObject);
        }
        _objectList = new List<GameObject>();
    }

    public void InitializeListView()
    {
        DestroyListChildren();
        SetValueChangedEvent();
        SetItemSize();
        _inputCallHandler = null;
    }

    public void CreateObjectList()
    {
        CreateList();
    }

    public void SetListData(List<ListData> listData)
    {
        _listData = listData;
    }

    private void SetValueChangedEvent()
    {
        scrollRect.onValueChanged.AddListener(ValueChanged);
    }

    private void ValueChanged(Vector2 scrollPosition)
    {
        if (EnableValueChanged())
        {
            UpdateListItem();
        }
    }

    private bool EnableValueChanged()
    {
        int startIndex = GetStartIndex();
        if (startIndex < 0) return false;    
        if (_lastStartIndex == startIndex) return false;
        return true;
    }

    public void SetItemSize()
    {
        _itemSize = itemPrefab.GetComponent<RectTransform>().sizeDelta;
    }

    private void CreateList()
    {
        _blankObject = new GameObject("blank");
        _blankObject.AddComponent<RectTransform>();
        var rect = _blankObject.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector3(_itemSize.x,_itemSize.y,0);
        rect.pivot = new Vector3(0,1,0);
        int createCount = _listData.Count;
        for (var i = 0; i < createCount;i++){
            var prefab = Instantiate(_blankObject);
            prefab.transform.SetParent(scrollRect.content, false);
            _objectList.Add(prefab);
        }
        for (var i = 0; i < _objectList.Count;i++){
            if (_listData.Count <= i){
                _objectList[i].SetActive(false);
            }
        }
        var listCount = ListItemCount();
        for (var i = 0; i < (listCount+2);i++){
            var prefab = Instantiate(itemPrefab);
            _itemPrefabList.Add(prefab);
            var view = prefab.GetComponent<IListViewItem>();
            if (view != null)
            {
                _itemList.AddLast(view);
            }
        }
        var startIndex = 0;
        for (int i = startIndex;i < _itemPrefabList.Count;i++)
        {
            if (_listData.Count <= i){
                continue;
            }
            _itemPrefabList[i].transform.SetParent(_objectList[i].transform,false);
        }
        Refresh();
    }

    public void UpdateItemPrefab()
    {
        var startIndex = GetStartIndex();
        for (int i = 0;i < _itemPrefabList.Count;i++)
        {
            if (_listData.Count <= i+startIndex){
                continue;
            }
            _itemPrefabList[i].transform.SetParent(_objectList[i+startIndex].transform,false);
        }
        Refresh();
    }

    public void Refresh(int selectIndex = 0)
    {
        var startIndex = Math.Max(0,GetStartIndex());
        for (int i = 0; i < ItemPrefabList.Count;i++)
        {
            if (startIndex < 0) continue;
            if (i + startIndex >= _listData.Count) continue;
            if (i < _listData.Count) 
            {
                var listItem = ItemPrefabList[i].GetComponent<ListItem>();
                listItem.SetListData(_listData[i + startIndex],i + startIndex);
            }
            ItemPrefabList[i].SetActive(i < _listData.Count);
        }
        UpdateAllItems();
        UpdateSelectIndex(selectIndex);
    }

    public void AddCreateList(int count)
    {
        int createCount = count;
        for (var i = 0; i < createCount;i++){
            var prefab = Instantiate(_blankObject);
            prefab.transform.SetParent(scrollRect.content, false);
            _objectList.Add(prefab);
        }
    }

    private float GetViewPortWidth()
    {
        return scrollRect.viewport.rect.width;
    }

    private float GetViewPortHeight()
    {
        return scrollRect.viewport.rect.height;
    }

    private float GetScrolledWidth()
    {
        return (scrollRect.content.rect.width - GetViewPortWidth()) * (1.0f - scrollRect.normalizedPosition.x);
    }

    private float GetScrolledHeight()
    {
        return (scrollRect.content.rect.height - GetViewPortHeight()) * (1.0f - scrollRect.normalizedPosition.y);
    }

    private float ItemSpace()
    {
        if (horizontal)
        {
            return GetComponentInChildren<HorizontalLayoutGroup>().spacing;
        } else
        {
            return GetComponentInChildren<VerticalLayoutGroup>().spacing;
        }
    }

    private float ListMargin()
    {
        if (horizontal)
        {
            return GetComponentInChildren<HorizontalLayoutGroup>().padding.left;
        } else
        {
            return GetComponentInChildren<VerticalLayoutGroup>().padding.top + GetComponentInChildren<VerticalLayoutGroup>().padding.bottom;
        }
    }

    private int GetStartIndex()
    {
        if (horizontal)
        {   //Math.Truncate
            return (int)Math.Round( (GetScrolledWidth() - ListMargin()) / (_itemSize.x + ItemSpace()) );
        } else
        {
            var height = GetScrolledHeight();
            var itemSpace = ItemSpace();
            var listMargin = ListMargin();
            var space = ItemSpace();
            return (int)Math.Round( ((height - itemSpace) - listMargin) / (_itemSize.y + space) );
        }
    }

    private void UpdateListItem()
    {
        int startIndex = GetStartIndex();
        if (startIndex != _lastStartIndex)
        {
            UpdateItemPrefab();
            _lastStartIndex = startIndex;
        }
    }

    public void UpdateAllItems()
    {
        foreach (var item in _itemList)
        {
            item.UpdateViewItem();
        }
    }

    public void SelectIndex(int selectIndex)
    {
        var callHandler = _index != selectIndex;
        _index = selectIndex;
        if (callHandler)
        {
            if (_selectedHandler != null)
            {
                _selectedHandler();
            }
        }
    }

    public void Update()
    {
        UpdateInputFrame();
    }

    private void UpdateInputFrame()
    {
        if (_inputBusyFrame > 0)
        {
            _inputBusyFrame--;
        }
    }

    public bool IsInputEnable()
    {
        if (this == null) return false;
        if (_inputBusyFrame > 0) return false;
        if (_active == false) return false;
        if (!gameObject) return false;
        if (gameObject.activeSelf == false) return false;
        return true;
    }

    public void ResetInputFrame(int plusValue)
    {
        _inputBusyFrame = _defaultInputFrame + plusValue;
    }
    
    public void ResetInputOneFrame()
    {
        _inputBusyFrame = 1;
    }
    

    public void SetHelpWindow(HelpWindow helpWindow){
        _helpWindow = helpWindow;
    }

    public void InputHandler(InputKeyType keyType,bool pressed)
    {
        if (keyType == InputKeyType.None)
        {
            ResetInputOneFrame();
        }
        if (!IsInputEnable())
        {
            return;
        }
        InputSelectIndex(keyType);
        InputCallEvent(keyType);
        int plusValue = 0;
        if (keyType == InputKeyType.Up || keyType == InputKeyType.Down || keyType == InputKeyType.Left || keyType == InputKeyType.Right)
        {
            if (InputSystem.IsGamePad)
            {
                plusValue = pressed ? _listMoveGamePadFrame : _listMoveGamePadFrameFirst;
            } else
            {
                plusValue = pressed ? _listMoveInputFrame : _listMoveInputFrameFirst;
            }
            UpdateSelectIndex(Index);
        }
        ResetInputFrame(plusValue);
        //Debug.Log(this.gameObject.name);
    }

    public void InputSelectIndex(InputKeyType keyType){
        var currentIndex = Index;
        var selectIndex = Index;
        var plusKey = (horizontal == true) ? InputKeyType.Right : InputKeyType.Down;
        var minusKey = (horizontal == true) ? InputKeyType.Left : InputKeyType.Up;
        if (reverse)
        {
            plusKey = (horizontal == true) ? InputKeyType.Left : InputKeyType.Up;
            minusKey = (horizontal == true) ? InputKeyType.Right : InputKeyType.Down;
        }
        var nextIndex = Index;
        if (keyType == plusKey){
            for (int i = 0;i < _listData.Count;i++)
            {
                nextIndex = Index + i + 1;
                if (nextIndex >= _listData.Count){
                    nextIndex -= _listData.Count;
                }
                var listItem = ObjectList[nextIndex].GetComponent<ListItem>();
                if (listItem.Disable == null)
                {
                    break;
                }
                if (listItem.Disable != null && listItem.Disable.activeSelf == false)
                {
                    break;
                }
            }
            selectIndex = nextIndex;
            if (selectIndex >= _listData.Count){
                if (warpMode)
                {
                    selectIndex = 0;
                }
            }
        } else
        if (keyType == minusKey){
            for (int i = 0;i < _listData.Count;i++)
            {
                nextIndex = Index - i - 1;
                if (nextIndex < 0){
                    nextIndex += _listData.Count;
                }
                if (nextIndex < 0)
                {
                    break;
                }
                var listItem = ObjectList[nextIndex].GetComponent<ListItem>();
                if (listItem.Disable == null)
                {
                    break;
                }
                if (listItem.Disable != null && listItem.Disable.activeSelf == false)
                {
                    break;
                }
            }
            selectIndex = nextIndex;
            if (selectIndex < 0){
                if (warpMode)
                {
                    selectIndex = _listData.Count-1;
                }
            }
        }
        if (currentIndex != selectIndex){
            Ryneus.SoundManager.Instance.PlayStaticSe(SEType.Cursor);
            SelectIndex(selectIndex);
        }
    }

    public void UpdateSelectIndex(int index){
        SelectIndex(index);
        UpdateHelpWindow();
        for (int i = 0; i < ObjectList.Count;i++)
        {
            if (ObjectList[i] == null) continue;
            var listItem = ObjectList[i].GetComponentInChildren<ListItem>();
            if (listItem == null) continue;
            if (index == listItem.Index){
                listItem.SetSelect();
            } else{
                listItem.SetUnSelect();
            }
        }
    }

    public void SetInputCallHandler(System.Action<InputKeyType> callHandler)
    {
        _inputCallHandler = callHandler;
    }
    
    public void SetInputHandler(InputKeyType keyType,System.Action handler)
    {
        _inputHandler[keyType] = handler;
    }

    public void SetSelectedHandler(System.Action selectedHandler)
    {
        _selectedHandler = selectedHandler;
    }

    public void CallListInputHandler(InputKeyType keyType)
    {
        if (!IsInputEnable())
        {
            return;
        }
        if (_inputHandler.ContainsKey(keyType))
        {
            _inputHandler[keyType]();
        }
    }
    
    private void InputCallEvent(InputKeyType keyType){
        if (!IsInputEnable())
        {
            return;
        }
        if (_inputCallHandler != null)
        {
            _inputCallHandler(keyType);
        }
        CallListInputHandler(keyType);
    }

    public void MouseCancelHandler()
    {
        if (!IsInputEnable())
        {
            return;
        }
        if (_cancelEvent != null)
        {
            _cancelEvent();
        }
    }

    public virtual void UpdateHelpWindow(){
    }

    public virtual void RefreshListItem(GameObject gameObject,int itemIndex)
    {
        ListItem listItem = gameObject.GetComponent<ListItem>();
        listItem.SetUnSelect();
    }
    
    public void CallSelectHandler(InputKeyType keyType)
    {
        if (keyType == InputKeyType.Down || keyType == InputKeyType.Up)
        {
            UpdateScrollRect(keyType);
        }
    }

    public void UpdateScrollRect(InputKeyType keyType){
        if (_index < 0) return;
        var listCount = ListItemCount();
        var dataCount = _listData.Count;
        var _displayDownCount = Index - GetStartIndex();
        if (keyType == InputKeyType.Down){
            _displayDownCount--;
            if (Index == 0)
            {
                ScrollRect.normalizedPosition = new Vector2(0,1);
            } else
            if (Index > (listCount-1) && _displayDownCount == (listCount-1))
            {
                var num = 1.0f / (float)(dataCount - listCount);
                ScrollRect.normalizedPosition = new Vector2(0,1.0f - (num * (Index - (listCount-1))));
            }
        } else
        if (keyType == InputKeyType.Up){
            _displayDownCount++;
            if (Index == (_listData.Count-1))
            {
                ScrollRect.normalizedPosition = new Vector2(0,0);
            } else
            if (Index < (dataCount-listCount) && _displayDownCount == 0)
            {
                var num = 1.0f / (float)(dataCount - listCount);
                ScrollRect.normalizedPosition = new Vector2(0,1.0f - (num * Index));
            }
        }
    }

    public void UpdateScrollRect(int selectIndex){
        if (_index < 0) return;
        var listCount = ListItemCount();
        var dataCount = _listData.Count;
        var listIndex = selectIndex - (listCount - 1);
        if (listIndex > 0)
        {
            var num = 1.0f / (float)(dataCount - listCount);
            var normalizedPosition = 1.0f - (num * (Index - (listCount-1)));
            if (horizontal)
            {
                ScrollRect.normalizedPosition = new Vector2(normalizedPosition,0);
            } else
            {
                ScrollRect.normalizedPosition = new Vector2(0,normalizedPosition);
            }
        }
    }

    private int ListItemCount()
    {
        if (horizontal)
        {   //Math.Truncate
            return (int)Math.Round( (GetViewPortWidth() - ListMargin()) / (_itemSize.x + ItemSpace()));
        } else
        {
            var height = GetViewPortHeight();
            var listMargin = ListMargin();
            var space = ItemSpace();
            return (int)Math.Round( (height - listMargin) / (_itemSize.y + space));
        }
    }

    public void ResetScrollRect(){
        ScrollRect.normalizedPosition = new Vector2(0,1);
    }

    private void OnDestroy() {
        if (_blankObject != null)
        {
            Destroy(_blankObject);
        }
        for (int i = 0;i < _itemPrefabList.Count;i++)
        {
            Destroy(_itemPrefabList[i]);
        }
        for (int i = 0;i < _objectList.Count;i++)
        {
            Destroy(_objectList[i]);
        }
    }
}
