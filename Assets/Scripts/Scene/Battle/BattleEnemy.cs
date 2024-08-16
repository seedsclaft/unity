using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Effekseer;

namespace Ryneus
{
    public class BattleEnemy : ListItem 
    {
        [SerializeField] private BattlerInfoComponent battlerInfoComponent;
        public BattlerInfoComponent BattlerInfoComponent => battlerInfoComponent;
        [SerializeField] private Image enemyImage;
        [SerializeField] private EffekseerEmitter effekseerEmitter;
        [SerializeField] private GameObject statusObject;
        [SerializeField] private EffekseerEmitter cursorEffekseerEmitter;

        private BattlerInfo _battlerInfo;
        private bool _isFront = false;

        public int EnemyIndex => _battlerInfo.Index;

        public void SetData(BattlerInfo battlerInfo,int index,bool isFront)
        {
            _battlerInfo = battlerInfo;
            _isFront = isFront;
            battlerInfoComponent.UpdateInfo(battlerInfo);
            SetIndex(index);
        }
        
        public void SetDamageRoot(GameObject damageRoot)
        {
            battlerInfoComponent.SetDamageRoot(damageRoot);
        }

        public void SetStatusRoot(GameObject statusRoot)
        {
            statusObject.transform.SetParent(statusRoot.transform, false);
            statusRoot.SetActive(true);
            battlerInfoComponent.SetStatusRoot(statusObject);
        }

        public void SetCallHandler(System.Action<int> handler)
        {
            if (_battlerInfo == null) return;
            clickButton.onClick.AddListener(() => handler(_battlerInfo.Index));
        }
        
        public new void SetSelectHandler(System.Action<int> handler)
        {
            var enterListener = clickButton.gameObject.AddComponent<ContentEnterListener>();
            enterListener.SetEnterEvent(() => handler(_battlerInfo.Index));
        }

        public void SetPressHandler(System.Action<int> handler)
        {
            var pressListener = clickButton.gameObject.AddComponent<ContentPressListener>();
            pressListener.SetPressEvent(() => handler(_battlerInfo.Index));
        }
        
        public void SetSelect(EffekseerEffectAsset effekseerEffectAsset)
        {
            if (Cursor == null) return;
            clickButton.enabled = true;
            Cursor.SetActive(true);
            cursorEffekseerEmitter.Stop();
            cursorEffekseerEmitter.Play(effekseerEffectAsset);
            cursorEffekseerEmitter.enabled = true;
            cursorEffekseerEmitter.playOnStart = true;
            cursorEffekseerEmitter.isLooping = true;
        }

        public new void SetUnSelect()
        {
            if (Cursor == null) return;
            clickButton.enabled = false;
            Cursor.SetActive(false);
            cursorEffekseerEmitter.enabled = false;
            cursorEffekseerEmitter.playOnStart = false;
            cursorEffekseerEmitter.isLooping = false;
        }
    }
}