using System;
using System.Collections;
using System.Collections.Generic;
 
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
                case "SetSelect1Actor":
                    command = new AdvCommandSetSelect1Actor(row);
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
        public override async void DoCommand(AdvEngine engine)
        {
            var bgm = await ResourceSystem.LoadBGMAsset(bgmKey);
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

    public class AdvCommandSetSelect1Actor : AdvCommand
    {

        public AdvCommandSetSelect1Actor(StringGridRow row)
            :base(row)
        {
        }
    
        public override void DoCommand(AdvEngine engine)
        {
            if (GameSystem.CurrentStageData == null) return;
            if (GameSystem.CurrentStageData.CurrentStage == null) return;
            if (GameSystem.CurrentStageData.Party.ActorIdList.Count == 0) return;
            int actorId = GameSystem.CurrentStageData.Party.ActorIdList[0];
            var actorData = DataSystem.FindActor(actorId);
            if (actorData != null)
            {
                engine.Param.SetParameterString("Select1",actorData.Name);
            }
        }
    }
}
