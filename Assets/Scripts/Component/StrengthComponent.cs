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

    public void UpdateInfo(ActorInfo actorInfo,int statusId)
    {
        _actorInfo = actorInfo;
        
        if (nameText != null)
        {
            TextData textData = DataSystem.System.GetTextData(300 + statusId);
            nameText.text = textData.Text;
        }
        if (currentStatus != null)
        {
            currentStatus.text = (_actorInfo.CurrentStatus.GetParameter((StatusParamType)statusId)).ToString();
        }
        if (afterStatus != null)
        {
            afterStatus.gameObject.SetActive(_actorInfo.TempStatus.GetParameter((StatusParamType)statusId) > 0);
            afterStatus.text = (_actorInfo.CurrentStatus.GetParameter((StatusParamType)statusId) + _actorInfo.TempStatus.GetParameter((StatusParamType)statusId)).ToString();
        }
        if (usePoint != null)
        {
            int UseCost = _actorInfo.UsePointCost((StatusParamType)statusId);
            var _currentAlcana = GameSystem.CurrentData.CurrentAlcana;
            if (_currentAlcana != null && _currentAlcana.IsStatusCostDown((StatusParamType)statusId))
            {
                UseCost -= 1;
            }
            usePoint.text = UseCost.ToString();
        }
    }
}
