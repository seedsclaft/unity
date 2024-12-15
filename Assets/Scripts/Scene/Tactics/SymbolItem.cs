using UnityEngine;

namespace Ryneus
{
    public class SymbolItem : ListItem ,IListViewItem
    {
        [SerializeField] private SymbolComponent symbolComponent;
        private bool _isSelectInitialized = false;
        public void UpdateViewItem()
        {
            if (ListData == null) return;
            var data = ListItemData<SymbolInfo>();
            symbolComponent.UpdateInfo(data);
        }

        public void SetSelectedHandler(int? seek,System.Action<int> handler)
        {
            if (_isSelectInitialized)
            {
                return;
            }
            _isSelectInitialized = true;
            var enterListener = clickButton.gameObject.AddComponent<ContentEnterListener>();
            enterListener.SetEnterEvent(() => 
            {
                if (ListData == null) return;
                var data = ListItemData<SymbolInfo>();
                handler(data.Master.SeekIndex);
            });
        }
    }
}
