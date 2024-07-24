using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Ryneus
{
    public class SkillTriggerHelp : MonoBehaviour
    {
        [SerializeField] private SkillInfoComponent skillInfoComponent = null;
        [SerializeField] private TextMeshProUGUI trigger1Help = null;
        [SerializeField] private TextMeshProUGUI trigger2Help = null;

        public void UpdateSkillInfo(SkillInfo skillInfo)
        {
            skillInfoComponent?.UpdateInfo(skillInfo);
        }

        public void UpdateSkillTriggerHelp(string help1,string help2)
        {
            if (help1 == "\"\"")
            {
                trigger1Help?.SetText("-");
            } else
            {
                trigger1Help?.SetText(help1);
            }
            if (help2 == "\"\"")
            {
                trigger2Help?.SetText("-");
            } else
            {
                trigger2Help?.SetText(help1);
            }
        }
    }
}
