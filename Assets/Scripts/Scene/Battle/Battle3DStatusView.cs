using System.Collections.Generic;
using UnityEngine;

namespace Ryneus
{
    public class Battle3DStatusView : MonoBehaviour
    {
        [SerializeField] private GameObject statusPrefab = null;
        [SerializeField] private GameObject statusRoot = null;
        private Canvas _canvas = null;
        private Camera _worldCamera = null;

        private Dictionary<int,GameObject> _3dModels = new ();
        private Dictionary<int,GameObject> _prefabs = new ();
        private Dictionary<int,BattlerInfoComponent> _battlers = new ();

        public void Initialize(Camera worldCamera)
        {
            _canvas = GameSystem.UiCanvas;
            _worldCamera = worldCamera;
        }

        public void Set3DGameObjects(int index,GameObject gameObject)
        {
            var prefab = Instantiate(statusPrefab);
            prefab.transform.SetParent(statusRoot.transform,false);
            _3dModels[index] = gameObject;
            _prefabs[index] = prefab;
            _battlers[index] = prefab.GetComponent<BattlerInfoComponent>();
        }

        public void UpdateBattlerInfo(BattlerInfo battlerInfo)
        {
            _battlers[battlerInfo.Index].UpdateInfo(battlerInfo);
        }

        public void RefreshStatus()
        {
            foreach (var item in _battlers)
            {
                item.Value.RefreshStatus();
            }
        }

        public void StartDamage(int index,DamageType damageType,int value)
        {
            if (_battlers.ContainsKey(index))
            {
                _battlers[index].StartDamage(damageType,value,false);
            }
        }

        public void StartHeal(int index,DamageType damageType,int value)
        {
            if (_battlers.ContainsKey(index))
            {
                _battlers[index].StartHeal(damageType,value,false);
            }
        }

        public void Death(int index,bool isAlive)
        {
            if (_battlers.ContainsKey(index))
            {
                _prefabs[index].SetActive(isAlive);
            }
        }

        private void StatusPosition(int targetIndex)
        {
            var target = _3dModels[targetIndex];
            var pos = Vector2.zero;
            var uiCamera = Camera.main;
            var worldCamera = _worldCamera;
            var canvasRect = _canvas.GetComponent<RectTransform> ();

            var screenPos = RectTransformUtility.WorldToScreenPoint (worldCamera, target.transform.position);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPos, uiCamera, out pos);
            _prefabs[targetIndex].GetComponent<RectTransform>().localPosition = pos;
        }

        private void Update() 
        {
            foreach (var _3dModel in _3dModels)
            {
                StatusPosition(_3dModel.Key);
            }
        }
    }
}
