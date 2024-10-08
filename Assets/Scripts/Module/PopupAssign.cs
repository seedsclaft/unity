using System.Collections.Generic;
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
        [SerializeField] private GameObject learnSkillPrefab = null;
        [SerializeField] private GameObject skillTriggerPrefab = null;
        [SerializeField] private GameObject skillLogPrefab = null;
        [SerializeField] private GameObject scorePrizePrefab = null;
        [SerializeField] private GameObject clearPartyPrefab = null;
        [SerializeField] private GameObject checkConflictPrefab = null;
        [SerializeField] private GameObject guidePrefab = null;
        [SerializeField] private GameObject battlePartyPrefab = null;
        [SerializeField] private GameObject sideMenuPrefab = null;
        [SerializeField] private GameObject dictionaryPrefab = null;
        [SerializeField] private GameObject tutorialPrefab = null;

        private List<BaseView> _stackPopupView = new ();
        
        public GameObject CreatePopup(PopupType popupType,HelpWindow helpWindow)
        {
            var prefab = Instantiate(GetPopupObject(popupType));
            prefab.transform.SetParent(confirmRoot.transform, false);
            confirmRoot.SetActive(true);
            var view = prefab.GetComponent<BaseView>();
            view?.SetHelpWindow(helpWindow);
            _stackPopupView.Add(view);
            return prefab;
        }

        private GameObject GetPopupObject(PopupType popupType)
        {
            return popupType switch
            {
                PopupType.SkillDetail => skillDetailPrefab,
                PopupType.Ruling => rulingPrefab,
                PopupType.Option => optionPrefab,
                PopupType.Ranking => rankingPrefab,
                PopupType.Credit => creditPrefab,
                PopupType.CharacterList => characterListPrefab,
                PopupType.Help => helpPrefab,
                PopupType.AlcanaList => alcanaListPrefab,
                PopupType.SlotSave => slotSavePrefab,
                PopupType.LearnSkill => learnSkillPrefab,
                PopupType.SkillTrigger => skillTriggerPrefab,
                PopupType.SkillLog => skillLogPrefab,
                PopupType.ScorePrize => scorePrizePrefab,
                PopupType.ClearParty => clearPartyPrefab,
                PopupType.CheckConflict => checkConflictPrefab,
                PopupType.Guide => guidePrefab,
                PopupType.BattleParty => battlePartyPrefab,
                PopupType.SideMenu => sideMenuPrefab,
                PopupType.Dictionary => dictionaryPrefab,
                PopupType.Tutorial => tutorialPrefab,
                _ => null,
            };
        }

        public void ClosePopup()
        {
            if (_stackPopupView.Count > 0)
            {
                var lastPopupView = _stackPopupView[_stackPopupView.Count-1];
                _stackPopupView.Remove(lastPopupView);
                Destroy(lastPopupView.gameObject);
            }
            if (_stackPopupView.Count == 0)
            {
                confirmRoot.SetActive(false);
            }
        }

    }

    public enum PopupType
    {
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
        LearnSkill,
        SkillTrigger,
        SkillLog,
        ScorePrize,
        ClearParty,
        CheckConflict,
        Guide,
        BattleParty,
        SideMenu,
        Dictionary,
        Tutorial,
    }
}