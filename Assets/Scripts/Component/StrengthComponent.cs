using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

namespace Ryneus
{
    public class StrengthComponent : MonoBehaviour
    {
        private ActorInfo _actorInfo = null;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI currentStatus;
        [SerializeField] private TextMeshProUGUI afterStatus;
        [SerializeField] private TextMeshProUGUI usePoint;

        public void UpdateInfo(ActorInfo actorInfo,StatusParamType statusParamType)
        {
            _actorInfo = actorInfo;
            
            if (nameText != null)
            {
                var textData = DataSystem.GetText(2100 + (int)statusParamType);
                nameText.text = textData;
            }
            if (currentStatus != null)
            {
                var before = _actorInfo.LevelUpStatus(_actorInfo.LinkedLevel()-1).GetParameter(statusParamType);
                currentStatus.text = before.ToString();
            }
            if (afterStatus != null)
            {
                var before = _actorInfo.LevelUpStatus(_actorInfo.LinkedLevel()-1).GetParameter(statusParamType);
                var plus = _actorInfo.LevelUpStatus(_actorInfo.LinkedLevel()).GetParameter(statusParamType);
                afterStatus.gameObject.SetActive(plus > before);
                afterStatus.text = plus.ToString();
            }
            if (usePoint != null)
            {
                int UseCost = _actorInfo.LevelGrowthRate(statusParamType,actorInfo.LinkedLevel());
                //var _currentAlcana = GameSystem.CurrentStageData.CurrentAlcana;
                //if (_currentAlcana != null && _currentAlcana.IsStatusCostDown((StatusParamType)statusParamType))
                //{
                //    UseCost -= 1;
                //}
                usePoint.text = UseCost.ToString();
            }
        }
    }
}