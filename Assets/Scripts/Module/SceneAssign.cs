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

    public GameObject CreateScene(Scene scene)
    {
        var prefab = Instantiate(GetSceneObject(scene));
        prefab.transform.SetParent(uiRoot.transform, false);
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
        }
        return null;
    }
}
