using System.Collections.Generic;
using UnityEngine;

namespace Ryneus
{
    public class Battle3DDamageView : MonoBehaviour
    {
        [SerializeField] private GameObject damagePrefab = null;
        [SerializeField] private GameObject damageRoot = null;
        private Canvas _canvas = null;
        private Camera _worldCamera = null;

        private Dictionary<int,VirtualModelController> _3dModels = new ();
        private List<BattleDamage> _battleDamages = new ();

        public void Initialize(Camera worldCamera)
        {
            _canvas = GameSystem.UiCanvas;
            _worldCamera = worldCamera;
        }

        public void Set3DGameObjects(int index,VirtualModelController virtualModel)
        {
            _3dModels[index] = virtualModel;
        }

        private Vector2 DamagePosition(int targetIndex)
        {
            var target = _3dModels[targetIndex].DamagePosition;
            var pos = Vector2.zero;
            var uiCamera = Camera.main;
            var worldCamera = _worldCamera;
            var canvasRect = _canvas.GetComponent<RectTransform>();

            var screenPos = RectTransformUtility.WorldToScreenPoint(worldCamera, target.transform.position);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPos, uiCamera, out pos);
            return pos;
        }

        public void StartDamage(int targetIndex,DamageType damageType,int value,bool needPopupDelay)
        {
            var battleDamage = CreatePrefab();
            int delayCount = needPopupDelay ? _battleDamages.Count : 0;
            battleDamage.StartDamage(damageType,value,() => EndDamageAnimation(battleDamage),delayCount);
            _battleDamages.Add(battleDamage);

            battleDamage.GetComponent<RectTransform>().localPosition = DamagePosition(targetIndex);
        }

        public void StartHeal(int targetIndex,DamageType damageType,int value,bool needPopupDelay)
        {
            var battleDamage = CreatePrefab();
            int delayCount = needPopupDelay ? _battleDamages.Count : 0;
            battleDamage.StartHeal(damageType,value,() => EndDamageAnimation(battleDamage),delayCount);
            _battleDamages.Add(battleDamage);

            battleDamage.GetComponent<RectTransform>().localPosition = DamagePosition(targetIndex);
        }

        public void StartStatePopup(int targetIndex,DamageType damageType,string stateName)
        {
            var battleDamage = CreatePrefab();
            battleDamage.StartStatePopup(damageType,stateName,_battleDamages.Count,() => EndDamageAnimation(battleDamage));
            _battleDamages.Add(battleDamage);

            battleDamage.GetComponent<RectTransform>().localPosition = DamagePosition(targetIndex);
        }

        private void EndDamageAnimation(BattleDamage battleDamage)
        {
            if (_battleDamages.Contains(battleDamage))
            {
                _battleDamages.Remove(battleDamage);
            }
            if (battleDamage != null)
            {
                Destroy(battleDamage.gameObject);
            }
        }

        private BattleDamage CreatePrefab()
        {
            var prefab = Instantiate(damagePrefab);
            prefab.transform.SetParent(damageRoot.transform, false);
            return prefab.GetComponent<BattleDamage>();
        }
    }
}
