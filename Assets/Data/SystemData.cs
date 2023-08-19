using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class SystemData : ScriptableObject
{	
	public List<MenuCommandData> TacticsCommandData;
	public List<MenuCommandData> StatusCommandData;
	public List<OptionCommand> OptionCommandData;
	public List<MenuCommandData> TitleCommandData;
	public List<int> InitActors;
	public int InitCurrency;
	public int TrainCount;
	public int AlchemyCount;
	public int RecoveryCount;
	public int BattleCount;
	public int ResourceCount;
	public List<TextData> SystemTextData;

	public List<InputData> InputDataList;
	public TextData GetTextData(int id)
	{
		TextData textData = SystemTextData.Find(a => a.Id == id);
		if (textData != null) 
		{
			return textData;
		}
		return null;
	}

	public string GetReplaceText(int id,string replace)
	{
		TextData textData = SystemTextData.Find(a => a.Id == id);
		if (textData != null) 
		{
			return textData.Text.Replace("\\d",replace);
		}
		return "";
	}

	[System.SerializableAttribute]
	public class MenuCommandData
	{
		
		public int Id;
		public string Key;
		public string Name;
		public string Help;
	}

	[System.SerializableAttribute]
	public class OptionCommand
	{
		public int Id;
		public string Key;
		public string Name;
		public string Help;
		public bool Toggles;
		public int ToggleText1;
		public int ToggleText2;
	}

	[System.SerializableAttribute]
	public class InputData
	{
		public string Key;
		public int KeyId;
		public string Name;
	}
}

public enum TacticsComandType {
	None,
	Train,
	Alchemy,
	Recovery,
	Battle,
	Resource,
	Status,
	Turnend
}

public enum TitleComandType {
	NewGame = 1,
	Continue,
}


public enum StatusComandType {
	SkillActionList = 0,
	Strength = 1,
}

public enum ConfirmComandType {
	Yes = 0,
	No = 1,
}

[Serializable]
public class GetItemData
{   
	public GetItemType Type;
	public int Param1;
	public int Param2;
}

public enum GetItemType
{
	None = 0,
	Skill = 1,
	Numinous = 2,
	Demigod = 3,
	AttributeFire = 11,
	AttributeThunder = 12,
	AttributeIce = 13,
	AttributeShine = 14,
	AttributeDark = 15,
	Ending = 21,
	StatusUp = 22,
}