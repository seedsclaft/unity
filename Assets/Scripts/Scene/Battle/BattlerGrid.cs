using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using TMPro;
using System;

namespace Ryneus
{
    public class BattlerGrid : MonoBehaviour
    {
        [SerializeField] private BattlerInfoComponent battlerInfoComponent;
        [SerializeField] private CanvasGroup canvasGroup;

        public void UpdateInfo(BattlerInfo battlerInfo)
        {
            battlerInfoComponent.UpdateInfo(battlerInfo);
        }

        public void RefreshStatus()
        {
            battlerInfoComponent.RefreshStatus();
        }
    }
}