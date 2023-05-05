using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
 
namespace Utage
{
    public class AdvCustomCommand : AdvCustomCommandManager
    {
        public override void OnBootInit()
        {
            Utage.AdvCommandParser.OnCreateCustomCommandFromID+= CreateCustomCommand;
        }
 
        //AdvEnginのクリア処理のときに呼ばれる
        public override void OnClear()
        {
        }
         
        //カスタムコマンドの作成用コールバック
        public void CreateCustomCommand(string id, StringGridRow row, AdvSettingDataManager dataManager, ref AdvCommand command )
        {
            switch (id)
            {
                //新しい名前のコマンドを作る
                case "PlayBgm":
                    command = new AdvCommandPlayBgm(row);
                    break;
                case "StopBgm2":
                    command = new AdvCommandStopBgm2(row);
                    break;
            }
        }
    }

    // カスタムコマンド
    public class AdvCommandPlayBgm : AdvCommand
    {
        private string bgmKey = "";
        public AdvCommandPlayBgm(StringGridRow row)
            :base(row)
        {
            bgmKey = ParseCell<string>(AdvColumnName.Arg1);
        }
        
        //コマンド実行
        public override void DoCommand(AdvEngine engine)
        {
            var bgm = ResourceSystem.LoadBGMAsset(bgmKey);
            Ryneus.SoundManager.Instance.PlayBgm(bgm,1.0f,true);
        }
    }

    public class AdvCommandStopBgm2 : AdvCommand
    {
        public AdvCommandStopBgm2(StringGridRow row)
            :base(row)
        {
        }
    
        public override void DoCommand(AdvEngine engine)
        {
            Ryneus.SoundManager.Instance.StopBgm();
        }
    }
}
