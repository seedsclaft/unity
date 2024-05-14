using UnityEngine;

namespace Ryneus
{
    public class PopupAssign : MonoBehaviour
    {
        [SerializeField] private GameObject confirmRoot = null;
        [SerializeField] private GameObject skillDetailPrefab = null;
        [SerializeField] private GameObject rulingPrefab = null;
        [SerializeField] private GameObject optionPrefab = null;
        [SerializeField] private GameObject rankingPrefab = null;
        [SerializeField] private GameObject creditPrefab = null;
        [SerializeField] private GameObject characterListPrefab = null;
        [SerializeField] private GameObject helpPrefab = null;
        [SerializeField] private GameObject alcanaListPrefab = null;
        [SerializeField] private GameObject slotSavePrefab = null;
        [SerializeField] private GameObject stageSymbolPrefab = null;
        [SerializeField] private GameObject learnSkillPrefab = null;
        [SerializeField] private GameObject skillTriggerPrefab = null;
        [SerializeField] private GameObject skillLogPrefab = null;
        public GameObject CreatePopup(PopupType popupType,HelpWindow helpWindow)
        {
            if (confirmRoot.transform.childCount > 0)
            {
                ClosePopup();
            }
            var prefab = Instantiate(GetPopupObject(popupType));
            prefab.transform.SetParent(confirmRoot.transform, false);
            confirmRoot.gameObject.SetActive(true);
            var view = prefab.GetComponent<BaseView>();
            view?.SetHelpWindow(helpWindow);
            return prefab;
        }

        private GameObject GetPopupObject(PopupType popupType)
        {
            switch (popupType)
            {
                case PopupType.SkillDetail:
                return skillDetailPrefab;
                case PopupType.Ruling:
                return rulingPrefab;
                case PopupType.Option:
                return optionPrefab;
                case PopupType.Ranking:
                return rankingPrefab;
                case PopupType.Credit:
                return creditPrefab;
                case PopupType.CharacterList:
                return characterListPrefab;
                case PopupType.Help:
                return helpPrefab;
                case PopupType.AlcanaList:
                return alcanaListPrefab;
                case PopupType.SlotSave:
                return slotSavePrefab;
                case PopupType.StageSymbol:
                return stageSymbolPrefab;
                case PopupType.LearnSkill:
                return learnSkillPrefab;
                case PopupType.SkillTrigger:
                return skillTriggerPrefab;
                case PopupType.SkillLog:
                return skillLogPrefab;
            }
            return null;
        }

        public void ClosePopup()
        {
            foreach(Transform child in confirmRoot.transform){
                Destroy(child.gameObject);
            }
            confirmRoot.gameObject.SetActive(false);
        }

    }

    public enum PopupType{
        None,
        SkillDetail,
        Ruling,
        Option,
        Ranking,
        Credit,
        CharacterList,
        Help,
        AlcanaList,
        SlotSave,
        StageSymbol,
        LearnSkill,
        SkillTrigger,
        SkillLog
    }
}