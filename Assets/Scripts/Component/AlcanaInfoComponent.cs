using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class AlcanaInfoComponent : MonoBehaviour
{
    [SerializeField] private GameObject alcana;
    [SerializeField] private TextMeshProUGUI alcanaCount;
    [SerializeField] private _2dxFX_Shiny_Reflect shinyReflect;
    public void UpdateInfo(AlcanaInfo alcanaInfo)
    {
        if (alcana != null){
            alcana.gameObject.SetActive(alcanaInfo.IsAlcana == true);
        }
        if (alcanaCount != null){
            alcanaCount.text = alcanaInfo.EnableOwnAlcanaList.Count.ToString();
        }
        if (shinyReflect != null)
        {
            shinyReflect.enabled = alcanaInfo.EnableOwnAlcanaList.Count > 0;
        }
    }
}
