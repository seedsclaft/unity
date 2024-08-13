﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Ryneus
{
    abstract public partial class ListWindow : MonoBehaviour
    {
        private bool _active = true;
        public bool Active => _active;

        private int _index = 0;
        public int Index => _index;
        private List<int> _selectIndexes = new ();
        public List<int> SelectIndexes => _selectIndexes;
        public void SetSelectIndexes(List<int> selectIndexes)
        {
            _selectIndexes = selectIndexes;
        }
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
        [SerializeField] private bool itemPrefabMode = true; 
        public ScrollRect ScrollRect => scrollRect; 
        [SerializeField] private GameObject itemPrefab = null; 
        private List<GameObject> _itemPrefabList = new ();
        public List<GameObject> ItemPrefabList => _itemPrefabList;
        private GameObject _prevPrefab = null;
        private List<ListData> _listDates = new ();
        public List<ListData> ListDates => _listDates;
        public int DataCount => _listDates.Count;
        private Vector2 _itemSize;
        private int _lastStartIndex = -1;
        private LinkedList<IListViewItem> _itemList = new();
        private List<GameObject> _objectList = new ();
        public List<GameObject> ObjectList => _objectList;

        private Action<InputKeyType> _inputCallHandler = null;
        private Dictionary<InputKeyType, Action> _inputHandler = new ();

        private Action _selectedHandler = null;

        public HelpWindow _helpWindow = null;

        private Action _cancelEvent = null;

        private GameObject _blankObject;
        public void SetCancelEvent(Action cancelEvent)
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
            foreach(Transform child in scrollRect.content.transform)
            {
                Destroy(child.gameObject);
            }
        }

        public void InitializeListView()
        {
            DestroyListChildren();
            _objectList = new List<GameObject>();
            SetValueChangedEvent();
            SetItemSize();
            _inputCallHandler = null;
            scrollRect.scrollSensitivity = 10;
        }

        public void SetListData(List<ListData> listData)
        {
            if (reverse)
            {
                listData.Reverse();
            }
            _listDates = listData;
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
                UpdateSelectIndex(Index);
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

        public void CreateList()
        {
            if (_itemPrefabList.Count > 0) return;
            if (itemPrefabMode == false)
            {
                CreateListPrefab(ListDates.Count);
            } else
            {
                CreateObjectPrefab();
                CreateListItemPrefab();
            }
        }

        public void UpdateObjectList()
        {
            for (var i = 0; i < _objectList.Count;i++)
            {
                _objectList[i].SetActive(ListDates.Count > i);
            }
        }

        private void CreateObjectPrefab()
        {
            _blankObject = new GameObject("blank");
            _blankObject.AddComponent<RectTransform>();
            _blankObject.transform.SetParent(scrollRect.content, false);
            _objectList.Add(_blankObject);
            var rect = _blankObject.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector3(_itemSize.x,_itemSize.y,0);
            rect.pivot = new Vector3(0,1,0);
            int createCount = ListDates.Count;
            for (var i = 0; i < createCount-1;i++)
            {
                var prefab = Instantiate(_blankObject);
                prefab.transform.SetParent(scrollRect.content, false);
                _objectList.Add(prefab);
            }
            if (reverse)
            {
                _objectList.Reverse();
            }
        }

        private void CreateListItemPrefab()
        {
            var listCount = ListItemCount();
            for (var i = 0; i < (listCount+1);i++)
            {
                var prefab = Instantiate(itemPrefab);
                prefab.name = i.ToString();
                _itemPrefabList.Add(prefab);
                var view = prefab.GetComponent<IListViewItem>();
                if (view != null)
                {
                    _itemList.AddLast(view);
                }
            }
            var prev = Instantiate(itemPrefab);
            _prevPrefab = prev;
            var prevView = prev.GetComponent<IListViewItem>();
            if (prevView != null)
            {
                _itemList.AddLast(prevView);
            }
        }

        public void UpdateItemPrefab(int selectIndex = -1)
        {
            if (itemPrefabMode == false) 
            {
                UpdateListItemData();
                return;
            }
            var startIndex = selectIndex == -1 ? GetStartIndex(): selectIndex;
            for (int i = 0;i < _itemPrefabList.Count;i++)
            {
                var itemPrefab = _itemPrefabList[i];
                var itemIndex = i+startIndex;
                itemPrefab.SetActive(false);
                //itemPrefab.transform.SetParent(scrollRect.content,false);
                var listItem = itemPrefab.GetComponent<ListItem>();
                if (ListDates.Count <= itemIndex)
                {
                    listItem.SetListData(null,-1);
                    listItem.SetUnSelect();
                    continue;
                }
                if (_objectList.Count <= itemIndex || itemIndex < 0)
                {
                    listItem.SetListData(null,-1);
                    listItem.SetUnSelect();
                    continue;
                }
                listItem.SetListData(ListDates[itemIndex],itemIndex);
                itemPrefab.transform.SetParent(_objectList[itemIndex].transform,false);
                itemPrefab.SetActive(true);
            }
            if (startIndex > 0)
            {   
                if (_objectList.Count > startIndex)
                {
                    if (_objectList[startIndex-1].transform.childCount == 0)
                    {
                        _prevPrefab.SetActive(true);
                        _prevPrefab.transform.SetParent(_objectList[startIndex-1].transform,false);
                    } else
                    {
                        _prevPrefab.SetActive(false);
                        var childObject = _objectList[startIndex-1].transform.GetChild(0).gameObject;
                        if (childObject.activeSelf == false)
                        {
                            childObject.SetActive(true);
                        }
                    }
                }
            } else
            {
                if (_prevPrefab != null)
                {
                    _prevPrefab.SetActive(false);
                }
            }
        }

        public void Refresh(int selectIndex = 0)
        {
            UpdateItemPrefab();
            UpdateAllItems();
            UpdateSelectIndex(selectIndex);
        }

        public void AddCreateList(int count)
        {
            if (itemPrefabMode == false) 
            {
                AddCreateListPlus(count);
                return;
            }
            int createCount = count;
            foreach (var objectList in _objectList)
            {
                for (int i = 0;i < objectList.transform.childCount;i++)
                {
                    var child = objectList.transform.GetChild(i);
                    child.transform.SetParent(scrollRect.content, false);
                }
            }
            for (var i = 0; i < createCount;i++)
            {
                var prefab = Instantiate(_blankObject);
                prefab.transform.SetParent(scrollRect.content, false);
                _objectList.Add(prefab);
            }
            var startIndex = 0;
            for (int i = startIndex;i < _itemPrefabList.Count;i++)
            {
                if (ListDates.Count <= i){
                    continue;
                }
                _itemPrefabList[i].transform.SetParent(_objectList[i].transform,false);
            }
        }

        public InputKeyType GetPlusKey()
        {   
            return (horizontal == true) ? InputKeyType.Right : InputKeyType.Down;
        }
        
        public InputKeyType GetMinusKey()
        {   
            return (horizontal == true) ? InputKeyType.Left : InputKeyType.Up;
        }

        public bool InputDir4(InputKeyType keyType)
        {   
            return keyType == InputKeyType.Up || keyType == InputKeyType.Down || keyType == InputKeyType.Left || keyType == InputKeyType.Right;
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
            return (scrollRect.content.rect.width - GetViewPortWidth()) * scrollRect.normalizedPosition.x;
        }

        private float GetScrolledHeight()
        {
            return (scrollRect.content.rect.height - GetViewPortHeight()) * (1.0f - scrollRect.normalizedPosition.y);
        }

        private float ItemSpace()
        {
            if (horizontal)
            {
                var horizontal = GetComponentInChildren<HorizontalLayoutGroup>();
                if (horizontal != null)
                {
                    return horizontal.spacing;
                }
            } else
            {
                var vertical = GetComponentInChildren<VerticalLayoutGroup>();
                if (vertical != null)
                {
                    return vertical.spacing;
                }
            }
            return 0;
        }

        private float ListMargin()
        {
            if (horizontal)
            {
                var horizontal = GetComponentInChildren<HorizontalLayoutGroup>();
                if (horizontal != null)
                {
                    return horizontal.padding.left + horizontal.padding.right;
                }
            } else
            {
                var vertical = GetComponentInChildren<VerticalLayoutGroup>();
                if (vertical != null)
                {
                    return vertical.padding.top + vertical.padding.bottom;
                }
            }
            return 0;
        }

        /// <summary>
        /// 始点リスト番号
        /// </summary>
        /// <returns></returns>
        public int GetStartIndex()
        {
            var itemSpace = ItemSpace();
            var listMargin = ListMargin();
            var itemSize = horizontal ? _itemSize.x : _itemSize.y;
            var rectSize = horizontal ? GetScrolledWidth() : Math.Max(0,GetScrolledHeight());
            var index = (int)Math.Floor( (rectSize - itemSpace - listMargin + 4) / (itemSize + itemSpace) );
            return Math.Max(0,index);
        }

        public void UpdateListItem()
        {
            int startIndex = GetStartIndex();
            if (startIndex != _lastStartIndex)
            {
                UpdateItemPrefab();
                UpdateAllItems();
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
                _selectedHandler?.Invoke();
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
        
        public void SetHelpWindow(HelpWindow helpWindow)
        {
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
            if (InputDir4(keyType))
            {
                if (InputSystem.IsGamePad)
                {
                    plusValue = pressed ? _listMoveGamePadFrame : _listMoveGamePadFrameFirst;
                } else
                {
                    plusValue = pressed ? _listMoveInputFrame : _listMoveInputFrameFirst;
                }
            }
            ResetInputFrame(plusValue);
        }

        public void InputSelectIndex(InputKeyType keyType)
        {
            var currentIndex = Index;
            var selectIndex = Index;
            var plusKey = GetPlusKey();
            var minusKey = GetMinusKey();
            var nextIndex = Index;
            if (keyType == plusKey || keyType == minusKey)
            {
                for (int i = 0;i < ListDates.Count;i++)
                {
                    if (keyType == plusKey)
                    {
                        nextIndex = Index + i + 1;
                        if (nextIndex >= ListDates.Count)
                        {
                            nextIndex -= ListDates.Count;
                        }
                    } else
                    if (keyType == minusKey)
                    {
                        nextIndex = Index - i - 1;
                        if (nextIndex < 0)
                        {
                            nextIndex += ListDates.Count;
                        }
                    }
                    var listItem = _objectList[nextIndex].GetComponent<ListItem>();
                    if (listItem == null || listItem.Disable == null)
                    {
                        break;
                    }
                    if (listItem.Disable != null && listItem.Disable.activeSelf == false)
                    {
                        break;
                    }
                }
                selectIndex = nextIndex;
                if (warpMode)
                {   
                    if (selectIndex >= ListDates.Count)
                    {
                        selectIndex = 0;
                    } else
                    if (selectIndex < 0)
                    {
                        selectIndex = ListDates.Count-1;
                    }
                }
            }
            
            if (currentIndex != selectIndex)
            {
                SoundManager.Instance.PlayStaticSe(SEType.CursorMove);
                UpdateSelectIndex(selectIndex);
            }
        }

        public void UpdateSelectIndex(int index)
        {
            SelectIndex(index);
            UpdateHelpWindow();
            for (int i = 0; i < _objectList.Count;i++)
            {
                if (_objectList[i] == null) continue;
                var listItem = _objectList[i].GetComponentInChildren<ListItem>();
                if (listItem == null) continue;
                if (index == listItem.Index || _selectIndexes.Contains(listItem.Index))
                {
                    listItem.SetSelect();
                } else
                {
                    listItem.SetUnSelect();
                }
            }
        }

        public void SetInputCallHandler(Action<InputKeyType> callHandler)
        {
            _inputCallHandler = callHandler;
        }
        
        public void SetInputHandler(InputKeyType keyType,Action handler)
        {
            _inputHandler[keyType] = handler;
        }

        public void SetSelectedHandler(Action selectedHandler)
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
        
        private void InputCallEvent(InputKeyType keyType)
        {
            if (!IsInputEnable())
            {
                return;
            }
            _inputCallHandler?.Invoke(keyType);
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

        public virtual void UpdateHelpWindow()
        {
        }
        
        public void CallSelectHandler(InputKeyType keyType)
        {
            if (InputDir4(keyType))
            {
                UpdateScrollRect(keyType);
            }
        }

        private void UpdateScrollRect(InputKeyType keyType)
        {
            if (_index < 0) return;
            var listCount = ListItemCount();
            var dataCount = ListDates.Count;
            var _displayDownCount = Index - GetStartIndex();
            var plusKey = GetPlusKey();
            var minusKey = GetMinusKey();
            if (keyType == plusKey)
            {
                _displayDownCount--;
                if (Index == 0)
                {
                    ScrollRect.normalizedPosition = new Vector2(0,1);
                } else
                if (Index > (listCount-1) && _displayDownCount >= (listCount-1))
                {
                    var num = 1.0f / (dataCount - listCount);
                    ScrollRect.normalizedPosition = new Vector2(0,1.0f - (num * (Index - (listCount-1))));
                }
            } else
            if (keyType == minusKey)
            {
                _displayDownCount++;
                if (Index == (ListDates.Count-1))
                {
                    ScrollRect.normalizedPosition = new Vector2(0,0);
                } else
                if (Index < (dataCount-listCount) && _displayDownCount <= (listCount-1))
                {
                    var num = 1.0f / (dataCount - listCount);
                    ScrollRect.normalizedPosition = new Vector2(0,1.0f - (num * Index));
                }
            }
        }

        public void UpdateScrollRect(int selectIndex)
        {
            if (_index < 0) return;
            var listCount = ListItemCount();
            var dataCount = ListDates.Count;
            var listIndex = selectIndex - (listCount - 1);
            // 下まで表示できる場合は
            if (selectIndex <= listCount)
            {
                listIndex = listCount - 1;
            }
            if (listIndex > 0)
            {
                var num = 1.0f / (dataCount - listCount);
                var normalizedPosition = 1.0f - (num * (listIndex - (listCount-1)));
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
            var width = GetViewPortWidth();
            var height = GetViewPortHeight();
            var listMargin = ListMargin();
            var space = ItemSpace();
            if (horizontal)
            {   
                return (int)Math.Round( (width - listMargin) / (_itemSize.x + space));
            } else
            {
                return (int)Math.Round( (height - listMargin) / (_itemSize.y + space));
            }
        }

        public void ResetScrollRect()
        {
            if (horizontal)
            {   
                ScrollRect.normalizedPosition = new Vector2(1,0);
            } else
            {
                ScrollRect.normalizedPosition = new Vector2(0,1);
            }
            _lastStartIndex = -1;
        }

        public void Release() 
        {
            OnDestroy();
        }

        private void OnDestroy() 
        {
            for (int i = _itemPrefabList.Count-1;0 <= i;i--)
            {
                Destroy(_itemPrefabList[i]);
            }
            if (_prevPrefab != null)
            {
                Destroy(_prevPrefab);
            }
            for (int i = _objectList.Count-1;0 <= i;i--)
            {
                Destroy(_objectList[i]);
            }
        }
    }
}