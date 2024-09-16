using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Ryneus
{
    public class ViewEvent
    {
        public Base.CommandType commandType;
        public object template;

        public ViewEvent(Base.CommandType type)
        {
            commandType = type;
        }
        
    }


    public interface IClickHandlerEvent
    {
        void ClickHandler();
    }

    public interface IListViewItem
    {
        void UpdateViewItem();
    }

    public interface IInputHandlerEvent
    {
        void InputHandler(InputKeyType keyType,bool pressed);
        void MouseCancelHandler();
    }

    abstract public class ListItem : MonoBehaviour
    {    
        public Button clickButton;
        private int _index = -1;
        public int Index => _index;
        private ListData _listData;
        public ListData ListData => _listData;
        public void SetListData(ListData listData,int index)
        {
            _listData = listData;
            _index = index;
        }
        [SerializeField] private GameObject cursor;
        public GameObject Cursor => cursor;
        [SerializeField] private bool changeCursorColor = true;
        [SerializeField] private GameObject disable;
        public GameObject Disable => disable;

        [SerializeField] private Color selectColor;
        [SerializeField] private Color unSelectColor;
        [SerializeField] private List<TextMeshProUGUI> textUguiList = new ();
        [SerializeField] private List<Image> imageUguiList = new ();
        
        private bool _addListenHandler = false;
        public void SetAddListenHandler(bool add)
        {
            _addListenHandler = add;
        }
        public void Awake()
        {
            if (changeCursorColor)
            {
                SetCursorColor();
            }
        }

        public void SetSelect()
        {
            if (cursor == null) return;
            //if (disable != null && disable.activeSelf) return;
            cursor.SetActive(true);
            foreach (var text in textUguiList)
            {
                text.color = selectColor;
            }
            foreach (var image in imageUguiList)
            {
                image.color = selectColor;
            }
        }
        
        public void SetUnSelect()
        {
            if (cursor == null) return;
            cursor.SetActive(false);
            foreach (var text in textUguiList)
            {
                text.color = unSelectColor;
            }
            foreach (var image in imageUguiList)
            {
                image.color = unSelectColor;
            }
        }

        public void SetIndex(int index)
        {
            _index = index;
        }
        
        public void SetSelectHandler(Action<int> handler,Action exitAction = null)
        {
            if (clickButton == null)
            {
                return;
            }
            if (_addListenHandler)
            {
                return;
            }
            var enterListener = clickButton.gameObject.AddComponent<ContentEnterListener>();
            enterListener.SetEnterEvent(() => 
            {
                if (_index != -1)
                {
                    handler(_index);
                }
            });
            enterListener.SetExitEvent(() => 
            {
                exitAction?.Invoke();
            });
        }

        public void SetCallHandler(Action handler)
        {        
            if (clickButton == null)
            {
                return;
            }
            if (_addListenHandler)
            {
                return;
            }
            clickButton.onClick.AddListener(() => handler());
        }

        public void SetCursorColor()
        {
            if (cursor != null)
            {
                var images = cursor.GetComponentsInChildren<Image>();
                var cursorColor = new Color(200/255f,128/255f,12/255f);
                foreach (var image in images)
                {
                    if (image.sprite == null)
                    {
                        var alpha = image.color.a;
                        image.color = new Color(cursorColor.r,cursorColor.g,cursorColor.b,alpha);
                    }
                }
            }
        }
    }
}