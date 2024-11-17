using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Battle;
using Effekseer;
using TMPro;
using Cysharp.Threading.Tasks;

namespace Ryneus
{
    public partial class BattleView : BaseView ,IInputHandlerEvent
    {

        private Battle3DView _battle3dView = null;

        private void Create3DView()
        {
            var prefab = Instantiate(battle3DViewPrefab);
            CommandCreateMapObject(prefab);
            _battle3dView = prefab.GetComponent<Battle3DView>();
            battle3DDamageView.Initialize(_battle3dView.BattleCamera);
            battle3DStatusView.Initialize(_battle3dView.BattleCamera);
        }

        public void StartBattle(int enemyCount)
        {
            _battle3dView.StartBattle(enemyCount);
        }
    }
}
