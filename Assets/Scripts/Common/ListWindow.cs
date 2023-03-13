using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


abstract public class ListWindow : MonoBehaviour
{
    private bool _active = true;

    private int _index = 0;
    public int Index {get{return _index;}}
    private int _defaultInputFrame = 6;
    public void SetInputFrame(int frame)
    {
        _defaultInputFrame = frame;
    }
    private int _inputBusyFrame = 0;

    [SerializeField] private bool horizontal = false; 
    [SerializeField] private ScrollRect scrollRect = null; 
    [SerializeField] private GameObject itemPrefab = null; 
    private float _itemSize = 0.0f;
    private float _margin = 0.0f;
    private float _space = 0.0f;
    private int _itemCount = 0;
    private int _dataCount = 0;
    private RectTransform _prevRect;
    private RectTransform _lastRect;
    private int _lastStartIndex = 0;
    private LinkedList<IListViewItem> _itemList = new LinkedList<IListViewItem>();
    public LinkedList<IListViewItem> ItemList {get {return _itemList;}}
    private List<GameObject> _objectList = new List<GameObject>();
    public List<GameObject> ObjectList {get {return _objectList;}}

    private System.Action<InputKeyType> _inputCallHandler = null;

    public HelpWindow _helpWindow = null;
    public void Activate()
    {
        _active = true;
    }
    
    public void Deactivate()
    {
        _active = false;
    }

    private void DestroyListChildrens()
    {
        foreach(Transform child in scrollRect.content.transform){
            Destroy(child.gameObject);
        }
    }

    public void InitializeListView(int count)
    {
        DestroyListChildrens();
        SetValueChangedEvent();
        SetDataCount(count);
        SetItemCount();
        CreatePrevObject();
        CreateList();
        CreateLastObject();
        _inputCallHandler = null;
    }

    private void SetValueChangedEvent()
    {
        scrollRect.onValueChanged.AddListener(ValueChanged);
    }

    private void ValueChanged(Vector2 scrollPosition)
    {
        if (EnableValueChanged())
        {
            UpdateItemViews(scrollPosition,false);
            UpdateSizeDelta();
            _lastStartIndex = GetStartIndex();
        }
    }

    private bool EnableValueChanged()
    {
        int startIndex = GetStartIndex();
        if (startIndex < 0) return false;    
        if (startIndex >= (_dataCount - _itemCount)) return false; 
        if (_lastStartIndex == startIndex) return false;
        return true;
    }

    private void SetItemCount()
    {
        float itemSize;
        float scrollSize;
        if (horizontal == true)
        {
            itemSize = itemPrefab.GetComponent<RectTransform>().sizeDelta.x;
            scrollSize = scrollRect.GetComponent<RectTransform>().sizeDelta.x;
        } else{
            itemSize = itemPrefab.GetComponent<RectTransform>().sizeDelta.y;
            scrollSize = scrollRect.GetComponent<RectTransform>().sizeDelta.y;
        }
        _itemSize = itemSize;
        _itemCount = (int)Math.Floor( scrollSize / itemSize);
    }

    private void SetDataCount(int count)
    {
        _dataCount = count;
    }

    private void CreateList()
    {
        for (var i = 0; i < _itemCount + 1;i++){
            if (_dataCount <= i){
                continue;
            }
            GameObject prefab = Instantiate(itemPrefab);
            prefab.transform.SetParent(scrollRect.content, false);
            _objectList.Add(prefab);
            IListViewItem view = prefab.GetComponent<IListViewItem>();
            _itemList.AddLast(view);
        }
    }

    private void CreatePrevObject()
    {
        GameObject prevObject = new GameObject();
        prevObject.AddComponent<RectTransform>();
        prevObject.transform.SetParent(scrollRect.content, false);
        prevObject.transform.SetSiblingIndex(-1);
        _prevRect = prevObject.GetComponent<RectTransform>();
        _prevRect.sizeDelta = new Vector2( 0, 0 );
    }

    private void SetPrevRectSetSiblingIndex(int index)
    {
        _prevRect.gameObject.transform.SetSiblingIndex(index);
    }

    private void CreateLastObject()
    {
        GameObject lastObject = new GameObject();
        lastObject.AddComponent<RectTransform>();
        lastObject.transform.SetParent(scrollRect.content, false);
        _lastRect = lastObject.GetComponent<RectTransform>();
        _lastRect.sizeDelta = new Vector2( 0, (_dataCount - _itemCount) * _itemSize );
        lastObject.transform.SetSiblingIndex(9999);
    }

    private void SetLastRectSetSiblingIndex(int index)
    {
        _lastRect.gameObject.transform.SetSiblingIndex(index);
    }

    private float GetViewPortWidth()
    {
        return scrollRect.viewport.rect.width;
    }

    private float GetViewPortHeight()
    {
        return scrollRect.viewport.rect.width;
    }

    private float GetScrolledWidth()
    {
        return (scrollRect.content.rect.width - GetViewPortWidth()) * (1.0f - scrollRect.normalizedPosition.x);
    }

    private float GetScrolledHeight()
    {
        return (scrollRect.content.rect.height - GetViewPortHeight()) * (1.0f - scrollRect.normalizedPosition.y);
    }

    private int GetStartIndex()
    {
        return (int)Math.Truncate( (GetScrolledHeight() - _margin) / (_itemSize + _space) );
    }

    private void UpdateItemViews(Vector2 scrollPosition, bool forceRepaint)
    {
        int startIndex = GetStartIndex();
        //if (!forceRepaint && _itemCount == 0) return;    
        var itemIndex = 0;
        /*
        if (forceRepaint)
        {
            itemIndex = startIndex;
            while (itemIndex < startIndex + itemCount)
            {
                var itemView = _itemList.First;
                _itemList.RemoveFirst();
                UpdateItemView(itemIndex, itemView);
                _itemList.AddLast(itemView);
                
                _objectList[0].transform.SetSiblingIndex(itemIndex);
                itemIndex++;
            }
            _lastStartIndex = startIndex;
            return;
        }
        */

        if (startIndex > _lastStartIndex)
        {
            UpdateListDown();
        }
        else if (itemIndex < _lastStartIndex)
        {
            UpdateListUp();
        }
    }

    private void UpdateSizeDelta()
    {
        int startIndex = GetStartIndex();
        float scrolledSize;
        if (horizontal == true)
        {
            scrolledSize = (GetScrolledWidth() - _margin);
        } else
        {
            scrolledSize = (GetScrolledHeight() - _margin);
        }

        float prevSize;
        float lastSize;
        if (startIndex > _lastStartIndex)
        {
            prevSize = Math.Max(0, Math.Min(scrolledSize, (_dataCount - _itemCount-1) * _itemSize));
            lastSize = Math.Max(0, (_dataCount - _itemCount-1) * _itemSize - scrolledSize);
        } else
        {
            prevSize = Math.Min(scrolledSize - _itemSize, (_dataCount - _itemCount) * _itemSize);
            lastSize = Math.Max(0, (_dataCount - _itemCount) * _itemSize - scrolledSize + _itemSize);
        }

        if (horizontal == true)
        {
            _prevRect.sizeDelta = new Vector2( prevSize, 0 );
            _lastRect.sizeDelta = new Vector2( lastSize, 0 );
        } else
        {
            _prevRect.sizeDelta = new Vector2( 0, prevSize );
            _lastRect.sizeDelta = new Vector2( 0, lastSize );
        }
        LayoutRebuilder.MarkLayoutForRebuild(_prevRect);
        LayoutRebuilder.MarkLayoutForRebuild(_lastRect);
    }

    public void UpdateAllItems()
    {
        foreach (var item in _itemList)
        {
            item.UpdateViewItem();
        }
    }

    private void UpdateItemView(int itemIndex, LinkedListNode<IListViewItem> item)
    {
        if (itemIndex < 0 || itemIndex > _dataCount-1){
            return;
        }
        item.Value.UpdateViewItem();
    }

    private void UpdateListDown()
    {
        int startIndex = GetStartIndex();
        var itemIndex = Math.Max(startIndex, _lastStartIndex + _itemCount);
        while (itemIndex < startIndex + _itemCount)
        {
            var itemView = _itemList.First;
            _itemList.RemoveFirst();
            UpdateItemView(itemIndex + 1, itemView);
            _itemList.AddLast(itemView);
            var targetObj = _objectList[0];
            targetObj.transform.SetSiblingIndex(itemIndex + 1);
            _objectList.Remove(targetObj);
            _objectList.Add(targetObj);
            SetLastRectSetSiblingIndex(9999);
            itemIndex++;
        }
    }

    private void UpdateListUp()
    {
        int startIndex = GetStartIndex();
        var itemIndex =  Math.Min(_lastStartIndex + _itemCount - 1, _lastStartIndex - 1);
        while (itemIndex >= startIndex)
        {
            var itemView = _itemList.Last;
            _itemList.RemoveLast();
            UpdateItemView(itemIndex, itemView);
            _itemList.AddFirst(itemView);
            var targetObj = _objectList[_objectList.Count - 1];
            targetObj.transform.SetSiblingIndex(1);
            _objectList.Remove(targetObj);
            _objectList.Insert(0,targetObj);
            SetPrevRectSetSiblingIndex(0);
            itemIndex--;
        }
    }
    public void SetSelect()
    {
    }

    public void SelectIndex(int selectIndex)
    {
        _index = selectIndex;
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

    public void ResetInputFrame()
    {
        _inputBusyFrame = _defaultInputFrame;
    }
    
    public void SetHelpWindow(HelpWindow helpWindow){
        _helpWindow = helpWindow;
    }

    
    public void InputHandler(InputKeyType keyType)
    {
        if (!IsInputEnable())
        {
            return;
        }
        InputSelectIndex(keyType);
        InputCallEvent(keyType);
        UpdateSelectIndex(Index);
        ResetInputFrame();
    }

    public void InputSelectIndex(InputKeyType keyType){
        int currentIndex = Index;
        int selectIndex = Index;
        var plusKey = (horizontal == true) ? InputKeyType.Right : InputKeyType.Down;
        var minusKey = (horizontal == true) ? InputKeyType.Left : InputKeyType.Up;

        if (keyType == plusKey){
            selectIndex = Index + 1;
            if (selectIndex > ObjectList.Count-1){
                selectIndex = 0;
            }
        } else
        if (keyType == minusKey){
            selectIndex = Index - 1;
            if (selectIndex < 0){
                selectIndex = ObjectList.Count-1;
            }
        }
        if (currentIndex != selectIndex){
            SoundManager.Instance.PlayStaticSe(SEType.Cursor);
            SelectIndex(selectIndex);
        }
    }

    public void UpdateSelectIndex(int index){
        SelectIndex(index);
        UpdateHelpWindow();
        for (int i = 0; i < ObjectList.Count;i++)
        {
            if (ObjectList[i] == null) continue;
            var listItem = ObjectList[i].GetComponent<ListItem>();
            if (listItem == null) continue;
            if (index == i){
                listItem.SetSelect();
            } else{
                listItem.SetUnSelect();
            }
        }
    }

    public void SetInputHandler(System.Action<InputKeyType> callHandler)
    {
        _inputCallHandler = callHandler;
    }
    
    private void InputCallEvent(InputKeyType keyType){
        if (!IsInputEnable())
        {
            return;
        }
        _inputCallHandler(keyType);
    }

    public virtual void UpdateHelpWindow(){
    }
}
