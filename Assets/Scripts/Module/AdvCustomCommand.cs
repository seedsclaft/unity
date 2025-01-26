using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Unity.VisualStudio.Editor;
using Ryneus;
using UnityEngine;
using DG.Tweening;

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
                case "Balloon":
                    command = new AdvCommandBalloon(row);
                    break;
            }
        }
    }

    // カスタムコマンド
    public class AdvCommandPlayBgm : AdvCommand
    {
        private string fileName = "";
        private int? volume = 80;
        private int? pitch = 100;
        //private bool? loop = true;
        public AdvCommandPlayBgm(StringGridRow row)
            :base(row)
        {
            fileName = ParseCell<string>(AdvColumnName.Arg1);
            volume = ParseCell<int?>(AdvColumnName.Arg2);
            pitch = ParseCell<int?>(AdvColumnName.Arg3);
            //loop = ParseCell<bool>(AdvColumnName.Arg2);
        }
        
        //コマンド実行
        public override async void DoCommand(AdvEngine engine)
        {
            var bgmData = DataSystem.Data.BGM.Find(a => a.FileName == fileName);
            if (bgmData != null)
            {
                var bgm = await ResourceSystem.LoadBGMAsset(bgmData.Key);
                Ryneus.SoundManager.Instance.PlayBgm(bgm,bgmData.Volume,bgmData.Loop);
            }
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
            var bgs = await ResourceSystem.LoadBGSAsset(bgsKey);
            Ryneus.SoundManager.Instance.PlayBgs(bgs,1.0f,true);
        }
    }

    public class AdvCommandPlaySe : AdvCommand
    {
        private string fileName = "";
        private int? volume = 80;
        private int? pitch = 100;
        public AdvCommandPlaySe(StringGridRow row)
            :base(row)
        {
            fileName = ParseCell<string>(AdvColumnName.Arg1);
            volume = ParseCell<int?>(AdvColumnName.Arg2);
            pitch = ParseCell<int?>(AdvColumnName.Arg3);
        }
        
        //コマンド実行
        public override async void DoCommand(AdvEngine engine)
        {
            var se = await ResourceSystem.LoadSeAsset(fileName);
            Ryneus.SoundManager.Instance.PlaySe(se,(int)volume * 0.01f,(int)pitch * 0.01f);
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
            if (GameSystem.GameInfo == null) return;
            //if (Ryneus.GameSystem.CurrentStageData.CurrentStage == null) return;
            if (GameSystem.GameInfo.PartyInfo.ActorInfos.Count == 0) return;
            int actorId = GameSystem.GameInfo.PartyInfo.ActorInfos[0].ActorId;
            var actorData = DataSystem.FindActor(actorId);
            if (actorData != null)
            {
                engine.Param.SetParameterString("Select1",actorData.Name);
            }
        }
    }

    public class AdvCommandBalloon : AdvCommand
    {
        private string layerName = "";
        private int type = 0;
        private List<AnimationBalloon> _animationBalloons = new ();
        private bool _isInitialized = false;
        public AdvCommandBalloon(StringGridRow row)
            :base(row)
        {
            layerName = ParseCell<string>(AdvColumnName.Arg1);
            type = ParseCell<int>(AdvColumnName.Arg2);
        }
        
        //コマンド実行
        public override void DoCommand(AdvEngine engine)
        {
            AdvGraphicLayer layer = engine.GraphicManager.FindLayer(layerName);
            AdvGraphicLayer balloonLayer = engine.GraphicManager.FindLayer("Balloon");
            if (layer == null)
            {
                return;
            }
            var pixelsToUnits = engine.GraphicManager.PixelsToUnits;
            var balloon = CreateBalloon(layer,balloonLayer,pixelsToUnits);
            balloon.Play(layer,(AnimationBalloonType)type);
            _animationBalloons.Add(balloon);
            if (_isInitialized == false)
            {
			    engine.MessageWindowManager.OnTextChange.AddListener(OnBeginCommand);
                _isInitialized = true;
            }
        }

        private AnimationBalloon CreateBalloon(AdvGraphicLayer layer,AdvGraphicLayer balloonLayer,float pixelsToUnits)
        {
            var prefabObject = ResourceSystem.LoadResource<GameObject>(ResourceSystem.PrefabPath + "Common/Balloon");
            var prefab = GameObject.Instantiate(prefabObject);
            prefab.transform.SetParent(balloonLayer.gameObject.transform,false);
            prefab.transform.SetAsLastSibling();
            prefab.transform.GetComponent<Transform>().localPosition = layer.transform.localPosition * pixelsToUnits;
            return prefab.GetComponent<AnimationBalloon>();
        }

		private void OnBeginCommand(AdvMessageWindowManager messageWindowManager)
		{
            var deleteList = new List<AnimationBalloon>();
            foreach (var animationBalloons in _animationBalloons)
            {
                var deleteFlag = animationBalloons.StopAnimation();
                if (deleteFlag)
                {
                    deleteList.Add(animationBalloons);
                }
            }
            for(int i = _animationBalloons.Count-1;i >= 0;i--)
            {
                if (deleteList.Contains(_animationBalloons[i]))
                {
                    _animationBalloons.Remove(_animationBalloons[i]);
                }
            }
		}
    }
}
