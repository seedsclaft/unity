using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class SystemData : ScriptableObject
{	
	public List<MenuCommandData> TacticsCommandData;
	public List<MenuCommandData> StatusCommandData;
	public List<MenuCommandData> TitleCommandData;
	public List<int> InitActors;
	public int InitCurrency;
	public List<TextData> SystemTextData;

	public TextData GetTextData(int id)
	{
		return SystemTextData.Find(a => a.Id == id);
	}

	[System.SerializableAttribute]
	public class MenuCommandData
	{
		
		public int Id;
		public string Key;
		public string Name;
		public string Help;
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
	NewGame = 0,
	Continue,
	Option
}


public enum StatusComandType {
	Strength = 0,
	SkillActionList = 1,
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
	Demigod = 3
}