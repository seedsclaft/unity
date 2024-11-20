using UnityEngine;
using UnityEngine.UI;

namespace Ryneus
{
    public class BattleSkillSelect : MonoBehaviour
    {
        [SerializeField] private SkillInfoComponent skillInfoComponent;
        [SerializeField] private GameObject cursor;
        [SerializeField] private InputInfoComponent inputInfoComponent;

        public void UpdateInfo(SkillInfo skillInfo)
        {
            skillInfoComponent.UpdateInfo(skillInfo);
        }

        public void Clear()
        {
            skillInfoComponent.Clear();
        }

        public void SetSelect(bool isSelect)
        {
            cursor?.gameObject.SetActive(isSelect);
        }        
        
        public void SetButtonImage(InputKeyType inputKeyType)
        {
            inputInfoComponent.UpdateGuideIcon((int)inputKeyType);
        }
    }
}
