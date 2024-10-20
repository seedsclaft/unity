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
            alcana?.gameObject.SetActive(skillInfos.Count > 0);
            alcanaCount?.SetText(skillInfos.Count.ToString());
        }
    }
}