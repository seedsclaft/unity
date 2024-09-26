using UnityEngine;

namespace Ryneus
{
    public class SceneAssign : MonoBehaviour
    {
        [SerializeField] private GameObject uiRoot = null;
        [SerializeField] private GameObject bootScene = null;
        [SerializeField] private GameObject titleScene = null;
        [SerializeField] private GameObject nameEntryScene = null;
        [SerializeField] private GameObject mainMenuScene = null;
        [SerializeField] private GameObject battleScene = null;
        [SerializeField] private GameObject statusScene = null;
        [SerializeField] private GameObject tacticsScene = null;
        [SerializeField] private GameObject strategyScene = null;
        [SerializeField] private GameObject slotScene = null;
        [SerializeField] private GameObject fastBattleScene = null;
        [SerializeField] private GameObject resultScene = null;
        public GameObject CreateScene(Scene scene,HelpWindow helpWindow)
        {
            var prefab = Instantiate(GetSceneObject(scene));
            prefab.transform.SetParent(uiRoot.transform, false);
            var view = prefab.GetComponent<BaseView>();
            view?.SetHelpWindow(helpWindow);
            return prefab;
        }

        private GameObject GetSceneObject(Scene scene)
        {
            return scene switch
            {
                Scene.Boot => bootScene,
                Scene.Title => titleScene,
                Scene.NameEntry => nameEntryScene,
                Scene.MainMenu => mainMenuScene,
                Scene.Battle => battleScene,
                Scene.Status => statusScene,
                Scene.Tactics => tacticsScene,
                Scene.Strategy => strategyScene,
                Scene.Slot => slotScene,
                Scene.FastBattle => fastBattleScene,
                Scene.Result => resultScene,
                _ => null,
            };
        }

        public void ShowUI()
        {
            var view = uiRoot.GetComponentInChildren<BaseView>();
            view?.ChangeUIActive(true);
        }

        public void HideUI()
        {
            var view = uiRoot.GetComponentInChildren<BaseView>();
            view?.ChangeUIActive(false);
        }
    }
}