using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class AlcanaInfoComponent : MonoBehaviour
{
    [SerializeField] private GameObject alcana;
    [SerializeField] private TextMeshProUGUI alcanaValue;
    public void UpdateInfo(AlcanaInfo alcanaInfo)
    {
        if (alcana != null){
            alcana.gameObject.SetActive(alcanaInfo.IsAlcana == true);
        }
        if (alcanaValue != null){
            alcanaValue.text = alcanaInfo.OwnAlcanaIds.Count.ToString();
        }
    }
}
