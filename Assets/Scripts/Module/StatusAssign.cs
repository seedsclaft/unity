using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ryneus
{
    public class StatusAssign : MonoBehaviour
    {
        [SerializeField] private GameObject statusRoot = null;
        public GameObject StatusRoot => statusRoot;
        [SerializeField] private GameObject statusPrefab = null;
        [SerializeField] private GameObject enemyDetailPrefab = null;
        [SerializeField] private GameObject sideMenuPrefab = null;
        private BaseView _statusView;
        public GameObject CreatePopup(StatusType popupType,HelpWindow helpWindow)
        {
            var prefab = Instantiate(GetStatusObject(popupType));
            prefab.transform.SetParent(statusRoot.transform, false);
            statusRoot.gameObject.SetActive(true);
            _statusView = prefab.GetComponent<BaseView>();
            _statusView?.SetHelpWindow(helpWindow);
            return prefab;
        }

        private GameObject GetStatusObject(StatusType popupType)
        {
            switch (popupType)
            {
                case StatusType.Status:
                return statusPrefab;
                case StatusType.EnemyDetail:
                return enemyDetailPrefab;
                case StatusType.SideMenu:
                return sideMenuPrefab;
            }
            return null;
        }    
        
        public void CloseStatus()
        {
            foreach(Transform child in statusRoot.transform){
                Destroy(child.gameObject);
            }
            statusRoot.gameObject.SetActive(false);
        }

        public void SetBusy(bool isBusy)
        {
            _statusView?.SetBusy(isBusy);
        }
    }

    public enum StatusType{
        Status,
        EnemyDetail,
        SideMenu,
    }
}