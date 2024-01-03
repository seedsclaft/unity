using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    [SerializeField] private GameObject resultScene = null;
    [SerializeField] private GameObject rebornScene = null;
    [SerializeField] private GameObject rebornResultScene = null;
    [SerializeField] private GameObject slotScene = null;
    [SerializeField] private GameObject fastBattleScene = null;
    [SerializeField] private GameObject alcanaSelectScene = null;
    [SerializeField] private GameObject alcanaResultScene = null;
    private static List<Scene> _scenes = new ();
    public static Scene LastScene => SceneAssign._scenes.Count > 1 ? SceneAssign._scenes[SceneAssign._scenes.Count-2] : Scene.None;
    public GameObject CreateScene(Scene scene,HelpWindow helpWindow)
    {
        var prefab = Instantiate(GetSceneObject(scene));
        prefab.transform.SetParent(uiRoot.transform, false);
        var view = prefab.GetComponent<BaseView>();
        view?.SetHelpWindow(helpWindow);
        SceneAssign._scenes.Add(scene);
        return prefab;
    }

    private GameObject GetSceneObject(Scene scene)
    {
        switch (scene)
        {
            case Scene.Boot:
            return bootScene;
            case Scene.Title:
            return titleScene;
            case Scene.NameEntry:
            return nameEntryScene;
            case Scene.MainMenu:
            return mainMenuScene;
            case Scene.Battle:
            return battleScene;
            case Scene.Status:
            return statusScene;
            case Scene.Tactics:
            return tacticsScene;
            case Scene.Strategy:
            return strategyScene;
            case Scene.Result:
            return resultScene;
            case Scene.Reborn:
            return rebornScene;
            case Scene.RebornResult:
            return rebornResultScene;
            case Scene.Slot:
            return slotScene;
            case Scene.FastBattle:
            return fastBattleScene;
            case Scene.AlcanaSelect:
            return alcanaSelectScene;
            case Scene.AlcanaResult:
            return alcanaResultScene;
        }
        return null;
    }
}
