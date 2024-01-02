using System;
using System.Collections.Generic;

[Serializable]
public class SaveConfigInfo
{
	public float BgmVolume;
	public bool BgmMute;
	public float SeVolume;
	public bool SeMute;
	public int GraphicIndex;
	public bool EventSkipIndex;
	public bool CommandEndCheck;
	public bool BattleWait;
	public bool BattleAnimationSkip;
	public bool InputType;
	public bool BattleAuto;
    public SaveConfigInfo()
    {
		this.InitParameter();
	}

	public void InitParameter()
	{
		BgmVolume = 1.0f;
		BgmMute = false;
		SeVolume = 1.0f;
		SeMute = false;
		GraphicIndex = 2;
		EventSkipIndex = false;
		CommandEndCheck = true;
		BattleWait = true;
		BattleAnimationSkip = false;
		InputType = true;
		BattleAuto = false;
	}

	public void UpdateSoundParameter(float bgmVolume,bool bgmMute,float seVolume,bool seMute)
	{
		BgmVolume = bgmVolume;
		BgmMute = bgmMute;
		SeVolume = seVolume;
		SeMute = seMute;
	}
}