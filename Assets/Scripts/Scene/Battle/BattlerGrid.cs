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
        [SerializeField] private TextMeshProUGUI turnWait;
        [SerializeField] private CanvasGroup canvasGroup;

        public void UpdateInfo(BattlerInfo battlerInfo,int turnWaitValue,int sortIndex)
        {
            battlerInfoComponent.UpdateInfo(battlerInfo);
            var wait = Math.Max(turnWaitValue,0);
            turnWait.SetText(wait.ToString());
        }

        public void UpdateAlpha(bool show)
        {
            var alpha = show ? 1 : 0;
            canvasGroup.alpha = alpha;
        }
    }
}