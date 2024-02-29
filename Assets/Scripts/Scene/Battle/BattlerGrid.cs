using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using TMPro;

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
            turnWait.SetText(turnWaitValue.ToString());
        }

        public void UpdateAlpha(bool show)
        {
            var alpha = show ? 1 : 0;
            canvasGroup.alpha = alpha;
        }
    }
}