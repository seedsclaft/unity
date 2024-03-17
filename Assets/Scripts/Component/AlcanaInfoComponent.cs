using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace Ryneus
{
    public class AlcanaInfoComponent : MonoBehaviour
    {
        [SerializeField] private GameObject alcana;
        [SerializeField] private TextMeshProUGUI alcanaCount;
        [SerializeField] private _2dxFX_Shiny_Reflect shinyReflect;
        public void UpdateInfo(List<SkillInfo> skillInfos)
        {
            if (alcana != null){
                alcana.gameObject.SetActive(skillInfos.Count > 0);
            }
            if (alcanaCount != null){
                alcanaCount.text = skillInfos.Count.ToString();
            }
            if (shinyReflect != null)
            {
                //shinyReflect.enabled = alcanaInfo.EnableOwnAlcanaList.Count > 0;
            }
        }
    }
}