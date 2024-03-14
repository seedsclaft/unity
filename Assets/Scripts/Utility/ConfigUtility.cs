using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ryneus
{
    public class ConfigUtility
    {

        public static void ApplyConfigData()
        {
            var saveConfigInfo = GameSystem.ConfigData;
            if (saveConfigInfo != null)
            {
                ChangeBGMValue(saveConfigInfo.BgmVolume);
                ChangeSEValue(saveConfigInfo.SeVolume);
                ChangeGraphicIndex(saveConfigInfo.GraphicIndex);
                ChangeEventSkipIndex(saveConfigInfo.EventSkipIndex);
                ChangeCommandEndCheck(saveConfigInfo.CommandEndCheck);
                ChangeBattleWait(saveConfigInfo.BattleWait);
                ChangeBattleWait(saveConfigInfo.BattleWait);
                ChangeBattleAnimation(saveConfigInfo.BattleAnimationSkip);
                ChangeInputType(saveConfigInfo.InputType);
                ChangeBattleAuto(saveConfigInfo.BattleAuto);
            }
        }
        public static void ChangeBGMValue(float bgmVolume)
        {
            SoundManager.Instance.BGMVolume = bgmVolume;
            SoundManager.Instance.UpdateBgmVolume();
            if (bgmVolume > 0 && SoundManager.Instance.BGMMute == false)
            {
                ChangeBGMMute(false);
            }
            if (bgmVolume == 0 && SoundManager.Instance.BGMMute == true)
            {
                ChangeBGMMute(true);
            }
        }

        public static void ChangeBGMMute(bool bgmMute)
        {
            SoundManager.Instance.BGMMute = bgmMute;
            SoundManager.Instance.UpdateBgmMute();
        }
        
        public static void ChangeSEValue(float seVolume)
        {
            SoundManager.Instance.SeVolume = seVolume;
            Effekseer.Internal.EffekseerSoundPlayer.SeVolume = seVolume;
            SoundManager.Instance.UpdateSeVolume();
            if (seVolume > 0 && SoundManager.Instance.SeMute == false)
            {
                ChangeSEMute(false);
            }
            if (seVolume == 0 && SoundManager.Instance.SeMute == true)
            {
                ChangeSEMute(true);
            }
        }

        public static void ChangeSEMute(bool seMute)
        {
            SoundManager.Instance.SeMute = seMute;
        }

        public static void ChangeGraphicIndex(int graphicIndex)
        {
            GameSystem.ConfigData.GraphicIndex = graphicIndex;
            QualitySettings.SetQualityLevel(graphicIndex);
        }

        public static void ChangeEventSkipIndex(bool eventSkipIndex)
        {
            GameSystem.ConfigData.EventSkipIndex = eventSkipIndex;
        }

        public static void ChangeCommandEndCheck(bool commandEndCheck)
        {
            GameSystem.ConfigData.CommandEndCheck = commandEndCheck;
        }

        public static void ChangeBattleWait(bool battleWait)
        {
            GameSystem.ConfigData.BattleWait = battleWait;
        }

        public static void ChangeBattleAnimation(bool battleAnimation)
        {
            GameSystem.ConfigData.BattleAnimationSkip = battleAnimation;
        }

        public static void ChangeInputType(bool inputType)
        {
            GameSystem.ConfigData.InputType = inputType;
        }
        
        public static void ChangeBattleAuto(bool battleAuto)
        {
            GameSystem.ConfigData.BattleAuto = battleAuto;
        }
    }
}