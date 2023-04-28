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
        
        private async Task<List<AudioClip>> GetBgmData(string bgmKey){
            BGMData bGMData = DataSystem.Data.GetBGM(bgmKey);
            List<string> data = new List<string>();
            if (bGMData.Loop)
            {
                data.Add("BGM/" + bGMData.FileName + "_intro.ogg");
                data.Add("BGM/" + bGMData.FileName + "_loop.ogg");
            } else{
                data.Add("BGM/" + bGMData.FileName + ".ogg");
            }
            AudioClip result1 = null;
            AudioClip result2 = null;
            result1 = await ResourceSystem.LoadAsset<AudioClip>(data[0]);
            if (data.Count > 1)
            {
                result2 = await ResourceSystem.LoadAsset<AudioClip>(data[1]);
            }
            return new List<AudioClip>(){
                result1,result2
            };    
        }
        
        //コマンド実行
        public async override void DoCommand(AdvEngine engine)
        {
            var bgm = await GetBgmData(bgmKey);
            Ryneus.SoundManager.Instance.PlayBgm(bgm,1.0f,true);
        }
    }
}
