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

    [SerializeField] private bool isScrollList = true; // 表示数が初期プレハブ数より多くなるか
    [SerializeField] private bool horizontal = false; 
    [SerializeField] private ScrollRect scrollRect = null; 
    [SerializeField] private GameObject itemPrefab = null; 
    private float _itemSize = 0.0f;
    private int _itemCount = 0;
    private int _dataCount = 0;
    private RectTransform _prevRect;
    private RectTransform _lastRect;
    private int _lastStartIndex = 0;
    private LinkedList<IListViewItem> _itemList = new();
    public LinkedList<IListViewItem> ItemList => _itemList;
    private List<GameObject> _objectList = new ();
    public List<GameObject> ObjectList => _objectList;

    private System.Action<InputKeyType> _inputCallHandler = null;

    public HelpWindow _helpWindow = null;
    public void Activate()
    {
        ResetInputFrame();
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
        _objectList = new List<GameObject>();
    }

    public void InitializeListView(int count)
    {
        DestroyListChildrens();
        SetValueChangedEvent();
        SetDataCount(count);
        SetItemCount(count);
        CreatePrevObject();
        CreateList(count);
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
            UpdateListItem(false);
            UpdateSizeDelta();
            _lastStartIndex = GetStartIndex();
        }
    }

    private bool EnableValueChanged()
    {
        int startIndex = GetStartIndex();
        if (startIndex < 0) return false;    
        if (startIndex > (_dataCount - _itemCount)) return false; 
        if (_lastStartIndex == startIndex) return false;
        return true;
    }

    private void SetItemCount(int itemCount)
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
        _itemCount = itemCount;//(int)Math.Floor( scrollSize / _itemSize);
    }

    public void SetDataCount(int count)
    {
        _dataCount = count;
    }

    private void CreateList(int count)
    {
        int createCount = count;
        if (isScrollList == true)
        {
            createCount++;
        }
        for (var i = 0; i < createCount;i++){
            GameObject prefab = Instantiate(itemPrefab);
            prefab.transform.SetParent(scrollRect.content, false);
            _objectList.Add(prefab);
            IListViewItem view = prefab.GetComponent<IListViewItem>();
            _itemList.AddLast(view);
        }
        for (var i = 0; i < _objectList.Count;i++){
            if (_dataCount <= i){
                _objectList[i].SetActive(false);
            }
        }
    }

    private void CreatePrevObject()
    {
        if (isScrollList == false) return;
        GameObject prevObject = new GameObject();
        prevObject.AddComponent<RectTransform>();
        prevObject.transform.SetParent(scrollRect.content, false);
        prevObject.transform.SetSiblingIndex(-1);
        _prevRect = prevObject.GetComponent<RectTransform>();
        _prevRect.sizeDelta = new Vector2(0,0);
    }

    private void SetPrevRectSetSiblingIndex(int index)
    {
        if (isScrollList == false) return;
        _prevRect.gameObject.transform.SetSiblingIndex(index);
    }

    private void CreateLastObject()
    {
        if (isScrollList == false) return;
        GameObject lastObject = new GameObject();
        lastObject.AddComponent<RectTransform>();
        lastObject.transform.SetParent(scrollRect.content, false);
        _lastRect = lastObject.GetComponent<RectTransform>();
        _lastRect.sizeDelta = new Vector2(0, (_dataCount - _itemCount) * _itemSize );
        lastObject.transform.SetSiblingIndex(9999);
    }

    private void SetLastRectSetSiblingIndex(int index)
    {
        if (isScrollList == false) return;
        _lastRect.gameObject.transform.SetSiblingIndex(index);
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
        {
            return (int)Math.Truncate( (GetScrolledWidth() - ListMargin()) / (_itemSize + ItemSpace()) );
        } else
        {
            return (int)Math.Truncate( (GetScrolledHeight() - ListMargin()) / (_itemSize + ItemSpace()) );
        }
    }

    private void UpdateListItem(bool forceRepaint)
    {
        int startIndex = GetStartIndex();

        if (startIndex > _lastStartIndex)
        {
            //Debug.LogError("downer");
            UpdateListDown();
        }
        else if (0 < _lastStartIndex)
        {
            //Debug.LogError("upper");
            UpdateListUp();
        }
    }

    private void UpdateItemViews(bool forceRepaint)
    {
        int startIndex = GetStartIndex();
        for (int i = 0;i < _itemCount;i++)
        {
            RefreshListItem(_objectList[i], startIndex + i);
        }
    }

    private void UpdateSizeDelta()
    {
        if (isScrollList == false) return;
        int startIndex = GetStartIndex();
        float scrolledSize;
        if (horizontal == true)
        {
            scrolledSize = (GetScrolledWidth() - ListMargin());
        } else
        {
            scrolledSize = (GetScrolledHeight() - ListMargin());
        }

        float prevSize;
        float lastSize;
        float rectHeight = (_itemSize+ItemSpace());
        if (startIndex > _lastStartIndex)
        {
            prevSize = Math.Max(0, Math.Min(scrolledSize, (_dataCount - _itemCount) * rectHeight));
            lastSize = Math.Max(0, (_dataCount - _itemCount - 1) * rectHeight  - scrolledSize);
        } else
        {
            prevSize = Math.Min(scrolledSize - rectHeight, (_dataCount - _itemCount) * rectHeight);
            prevSize = Math.Max(0,prevSize);
            lastSize = Math.Max(0, (_dataCount - _itemCount) * rectHeight - scrolledSize);
            //lastSize = Math.Max(0,lastSize);
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

    private void UpdateItemView(GameObject gameObject, int itemIndex, LinkedListNode<IListViewItem> item)
    {
        if (itemIndex < 0 || itemIndex > _dataCount-1){
            return;
        }
        RefreshListItem(gameObject,itemIndex);
        //item.Value.UpdateViewItem(itemIndex);
    }

    private void UpdateListDown()
    {
        int startIndex = GetStartIndex();
        var itemIndex = Math.Max(startIndex, _lastStartIndex + _itemCount);
        while (itemIndex < startIndex + _itemCount)
        {
            var itemView = _itemList.First;
            _itemList.RemoveFirst();
            UpdateItemView(_objectList[0], itemIndex + 1, itemView);
            _itemList.AddLast(itemView);
            var targetObj = _objectList[0];
            targetObj.transform.SetSiblingIndex(itemIndex + 1);
            _objectList.Remove(targetObj);
            _objectList.Add(targetObj);
            SetLastRectSetSiblingIndex(9999);
            targetObj.gameObject.SetActive(itemIndex < (_dataCount - 1));
            itemIndex++;
        }
        UpdateItemViews(false);
    }

    private void UpdateListUp()
    {
        int startIndex = GetStartIndex();
        var itemIndex =  Math.Min(_lastStartIndex + _itemCount - 1, _lastStartIndex - 1);
        while (itemIndex >= startIndex)
        {
            var itemView = _itemList.Last;
            _itemList.RemoveLast();
            UpdateItemView(_objectList[ObjectList.Count-1], itemIndex, itemView);
            _itemList.AddFirst(itemView);
            var targetObj = _objectList[_objectList.Count - 1];
            targetObj.transform.SetSiblingIndex(1);
            _objectList.Remove(targetObj);
            _objectList.Insert(0,targetObj);
            SetPrevRectSetSiblingIndex(0);
            targetObj.gameObject.SetActive(true);
            itemIndex--;
        }
        UpdateItemViews(false);
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
        if (keyType == InputKeyType.Up || keyType == InputKeyType.Down || keyType == InputKeyType.Left || keyType == InputKeyType.Right)
        {
            UpdateSelectIndex(Index);
        }
        ResetInputFrame();
        //Debug.Log(this.gameObject.name);
    }

    public void InputSelectIndex(InputKeyType keyType){
        int currentIndex = Index;
        int selectIndex = Index;
        var plusKey = (horizontal == true) ? InputKeyType.Right : InputKeyType.Down;
        var minusKey = (horizontal == true) ? InputKeyType.Left : InputKeyType.Up;

        if (keyType == plusKey){
            selectIndex = Index + 1;
            if (selectIndex >= _dataCount){
                selectIndex = 0;
            }
        } else
        if (keyType == minusKey){
            selectIndex = Index - 1;
            if (selectIndex < 0){
                selectIndex = _dataCount-1;
            }
        }
        if (currentIndex != selectIndex){
            
            if (isScrollList == true)
            {
                if (_dataCount > _itemCount)
                {
                    int startIndex = GetStartIndex();
                    var rectHei = _itemSize+ItemSpace();
                    var h = (scrollRect.content.rect.height - GetViewPortHeight());
                    
                    int count = 0;
                    if (((_itemCount+startIndex) - selectIndex) <= 0)
                    {
                        var nom = (selectIndex - (_itemCount-1)) * ((float)rectHei / (float)h);
                        nom = 1.0f - nom;

                        scrollRect.normalizedPosition = new Vector2(0,nom);
                    
                        int tempIndex = GetStartIndex();
                        while (tempIndex == startIndex)
                        {
                            nom -= 0.000001f;
                            scrollRect.normalizedPosition = new Vector2(0,nom);
                            tempIndex = GetStartIndex();
                            count++;
                            if (count > 1000) 
                            {
                                tempIndex = startIndex;
                                break;
                            }
                        }
                        if (selectIndex == _dataCount-1)
                        {
                            /*
                            nom = 0;
                            scrollRect.normalizedPosition = new Vector2(0,nom);
                        
                            int tempIndex = GetStartIndex();
                            while (tempIndex != (_dataCount-_itemCount))
                            {
                                nom += 0.000001f;
                                scrollRect.normalizedPosition = new Vector2(0,nom);
                                tempIndex = GetStartIndex();
                                count++;
                                if (count > 1000) 
                                {
                                    tempIndex = (_dataCount-_itemCount);
                                    break;
                                }
                            }
                            */
                        }
                        ValueChanged(new Vector2(0,nom));
                    } else
                    if ((keyType == minusKey) && selectIndex <= startIndex && (_dataCount - _itemCount) >= selectIndex)
                    {
                        var nom = ((selectIndex)) * ((float)rectHei / (float)h);
                        nom = 1.0f - nom;
                        scrollRect.normalizedPosition = new Vector2(0,nom);
                        int tempIndex = GetStartIndex();
                        if (tempIndex == 0 && startIndex == 0)
                        {

                        } else
                        {
                            while (tempIndex == startIndex)
                            {
                                nom += 0.000001f;
                                scrollRect.normalizedPosition = new Vector2(0,nom);
                                tempIndex = GetStartIndex();
                                count++;
                                if (count > 1000) 
                                {
                                    tempIndex = startIndex;
                                    break;
                                }
                            }
                        }
                        ValueChanged(new Vector2(0,nom));
                    } else
                    if ((keyType == plusKey) && selectIndex == 0)
                    {
                        scrollRect.normalizedPosition = new Vector2(0,1);
                        ValueChanged(new Vector2(0,1));
                    }
                }
            }
            
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
            ListItem listItem = ObjectList[i].GetComponent<ListItem>();
            if (listItem == null) continue;
            if (index == listItem.Index){
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

    public virtual void RefreshListItem(GameObject gameObject,int itemIndex)
    {
        ListItem listItem = gameObject.GetComponent<ListItem>();
        listItem.SetUnSelect();
    }

    public void ResetScrollPosition()
    {
        if (isScrollList == false) return;
        if (horizontal == true)
        {
            _prevRect.sizeDelta = new Vector2( 0, 0 );
            _lastRect.sizeDelta = new Vector2( 0, 0 );
        } else
        {
            _prevRect.sizeDelta = new Vector2( 0, 0 );
            _lastRect.sizeDelta = new Vector2( 0, (_dataCount - _itemCount - 1) * _itemSize );
        }
        LayoutRebuilder.MarkLayoutForRebuild(_prevRect);
        LayoutRebuilder.MarkLayoutForRebuild(_lastRect);
        _lastStartIndex = 0;
    }
}
