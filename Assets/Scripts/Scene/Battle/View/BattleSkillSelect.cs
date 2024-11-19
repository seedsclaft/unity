using UnityEngine;

namespace Ryneus
{
    public class BattleSkillSelect : MonoBehaviour
    {
        [SerializeField] private SkillInfoComponent skillInfoComponent;
        [SerializeField] private GameObject cursor;

        public void UpdateInfo(SkillInfo skillInfo)
        {
            skillInfoComponent.UpdateInfo(skillInfo);
        }

        public void SetSelect(bool isSelect)
        {
            cursor?.gameObject.SetActive(isSelect);
        }
    }
}
