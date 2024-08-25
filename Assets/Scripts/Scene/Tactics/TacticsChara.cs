using System.Collections;
using System.Collections.Generic;
using ES3Types;
using UnityEngine;

namespace Ryneus
{
    public class TacticsChara : OnOffButton
    {
        [SerializeField] private ActorInfoComponent actorInfoComponent;

        public ActorInfo ActorInfo => _data;
        private ActorInfo _data;
        private bool _isInit = false;
        public bool IsInit => _isInit;
        public void Initialize(GameObject parent,float x,float y,float scale)
        {
            _isInit = true;
        }

        public void SetData(ActorInfo actorInfo)
        {
            actorInfoComponent.UpdateInfo(actorInfo,null);
            _data = actorInfo;
            HideCursor();
        }

        public void HideCursor()
        {
            SetActiveCursor(false);
        }

        public void Show()
        {
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}