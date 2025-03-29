using UnityEngine;

namespace Ryneus
{
    public class SceneAssign : MonoBehaviour
    {
        [SerializeField] private GameObject uiRoot = null;
        [SerializeField] private GameObject bootScene = null;
        [SerializeField] private GameObject mapScene = null;
        [SerializeField] private GameObject titleScene = null;
        [SerializeField] private GameObject battleScene = null;
        [SerializeField] private GameObject tacticsScene = null;
        public GameObject CreateScene(Scene scene)
        {
            var prefab = Instantiate(GetSceneObject(scene));
            prefab.transform.SetParent(uiRoot.transform, false);
            var view = prefab.GetComponent<BaseView>();
            return prefab;
        }

        private GameObject GetSceneObject(Scene scene)
        {
            return scene switch
            {
                Scene.Boot => bootScene,
                Scene.Map => mapScene,
                Scene.Title => titleScene,
                Scene.Battle => battleScene,
                Scene.Tactics => tacticsScene,
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