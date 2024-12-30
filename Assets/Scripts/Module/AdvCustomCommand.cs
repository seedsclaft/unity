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
                case "PlayBgs":
                    command = new AdvCommandPlayBgs(row);
                    break;
                case "PlaySe":
                    command = new AdvCommandPlaySe(row);
                    break;
                case "StopBgm2":
                    command = new AdvCommandStopBgm2(row);
                    break;
                case "StopBgs":
                    command = new AdvCommandStopBgs(row);
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
        private bool? loop = true;
        public AdvCommandPlayBgm(StringGridRow row)
            :base(row)
        {
            bgmKey = ParseCell<string>(AdvColumnName.Arg1);
            loop = ParseCell<bool>(AdvColumnName.Arg2);
        }
        
        //コマンド実行
        public override async void DoCommand(AdvEngine engine)
        {
            var bgm = await Ryneus.ResourceSystem.LoadBGMAsset(bgmKey);
            Ryneus.SoundManager.Instance.PlayBgm(bgm,1.0f,(bool)loop);
        }
    }

    public class AdvCommandPlayBgs : AdvCommand
    {
        private string bgsKey = "";
        public AdvCommandPlayBgs(StringGridRow row)
            :base(row)
        {
            bgsKey = ParseCell<string>(AdvColumnName.Arg1);
        }
        
        //コマンド実行
        public override async void DoCommand(AdvEngine engine)
        {
            var bgs = await Ryneus.ResourceSystem.LoadBGSAsset(bgsKey);
            Ryneus.SoundManager.Instance.PlayBgs(bgs,1.0f,true);
        }
    }

    public class AdvCommandPlaySe : AdvCommand
    {
        private string fileName = "";
        private int? volume = 80;
        public AdvCommandPlaySe(StringGridRow row)
            :base(row)
        {
            fileName = ParseCell<string>(AdvColumnName.Arg1);
            //volume = ParseCell<int?>(AdvColumnName.Arg2);
        }
        
        //コマンド実行
        public override async void DoCommand(AdvEngine engine)
        {
            var se = await Ryneus.ResourceSystem.LoadSeAsset(fileName);
            Ryneus.SoundManager.Instance.PlaySe(se,(int)volume * 0.01f,0);
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

    public class AdvCommandStopBgs : AdvCommand
    {
        public AdvCommandStopBgs(StringGridRow row)
            :base(row)
        {
        }
    
        public override void DoCommand(AdvEngine engine)
        {
            Ryneus.SoundManager.Instance.StopBgs();
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
            if (Ryneus.GameSystem.CurrentStageData == null) return;
            //if (Ryneus.GameSystem.CurrentStageData.CurrentStage == null) return;
            if (Ryneus.GameSystem.CurrentStageData.PartyInfo.ActorInfos.Count == 0) return;
            int actorId = Ryneus.GameSystem.CurrentStageData.PartyInfo.ActorInfos[0].ActorId;
            var actorData = Ryneus.DataSystem.FindActor(actorId);
            if (actorData != null)
            {
                engine.Param.SetParameterString("Select1",actorData.Name);
            }
        }
    }
}
