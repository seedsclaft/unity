using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace Ryneus
{
    public class OnOffButton : ListItem ,IListViewItem 
    {
        [SerializeField] private TextMeshProUGUI commandName;
        private System.Action _handler = null;
        private System.Action _exitHandler = null;
        public void SetText(string text)
        {
            commandName.text = text;
        }

        public void SetCallHandler(System.Action handler,System.Action exitHandler = null)
        {
            _exitHandler = exitHandler;
            SetSelectHandler(
                (a) => 
                {
                    SetActiveCursor(true);
                },
                () => 
                {
                    SetActiveCursor(false);
                    _exitHandler?.Invoke();
                }
            );
            if (_handler == null && handler != null)
            {
                clickButton.onClick.AddListener(() => 
                {
                    handler();
                });
                _handler = handler;
            }
        }

        public void UpdateViewItem()
        {
            if (ListData == null) return;
        }

        public void SetActiveCursor(bool isActive)
        {
            Cursor.SetActive(isActive);
            if (isActive)
            {
                SetSelect();
            } else
            {
                SetUnSelect();
            }
        }
    }
}
