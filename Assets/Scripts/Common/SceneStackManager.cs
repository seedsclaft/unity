using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

public class SceneStackManager
{
    private List<SceneInfo> _sceneInfo = new ();
    public Scene LastScene => _sceneInfo.Count > 0 ? _sceneInfo[_sceneInfo.Count-1].FromScene : Scene.None;
    public Scene Current => _sceneInfo.Count > 0 ? _sceneInfo[_sceneInfo.Count-1].ToScene : Scene.None;
    public object LastSceneParam => _sceneInfo.Count > 0 ? _sceneInfo[_sceneInfo.Count-1].SceneParam : null;
    public void PushSceneInfo(SceneInfo sceneInfo)
    {
        if (sceneInfo.SceneChangeType == SceneChangeType.Goto)
        {
            _sceneInfo.Clear();
            _sceneInfo.Add(sceneInfo);
        }
        if (sceneInfo.SceneChangeType == SceneChangeType.Push)
        {
            _sceneInfo.Add(sceneInfo);
        }
        if (LastScene != Scene.None && sceneInfo.SceneChangeType == SceneChangeType.Pop)
        {
            _sceneInfo.RemoveAt(_sceneInfo.Count-1);
            _sceneInfo.Add(sceneInfo);
        }
    }
}

public class SceneInfo
{
    public Scene FromScene;
    public Scene ToScene;
    public SceneChangeType SceneChangeType;
    public object SceneParam;

}
public enum SceneChangeType
{
    None = 0,
    Push = 1,
    Pop = 2,
    Goto = 3
}