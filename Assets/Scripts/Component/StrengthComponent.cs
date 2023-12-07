using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

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
            var textData = DataSystem.System.GetTextData(300 + (int)statusParamType);
            nameText.text = textData.Text;
        }
        if (currentStatus != null)
        {
            currentStatus.text = (_actorInfo.CurrentStatus.GetParameter((StatusParamType)statusParamType)).ToString();
        }
        if (afterStatus != null)
        {
            afterStatus.gameObject.SetActive(_actorInfo.TempStatus.GetParameter((StatusParamType)statusParamType) > 0);
            afterStatus.text = (_actorInfo.CurrentStatus.GetParameter((StatusParamType)statusParamType) + _actorInfo.TempStatus.GetParameter((StatusParamType)statusParamType)).ToString();
        }
        if (usePoint != null)
        {
            int UseCost = _actorInfo.UsePointCost((StatusParamType)statusParamType);
            var _currentAlcana = GameSystem.CurrentData.CurrentAlcana;
            if (_currentAlcana != null && _currentAlcana.IsStatusCostDown((StatusParamType)statusParamType))
            {
                UseCost -= 1;
            }
            usePoint.text = UseCost.ToString();
        }
    }
}
