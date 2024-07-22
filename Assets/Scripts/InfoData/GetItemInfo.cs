using System.Collections.Generic;

namespace Ryneus
{
    [System.Serializable]
    public class GetItemInfo 
    {
        private GetItemType _getItemType = GetItemType.None;
        public GetItemType GetItemType => _getItemType;
        private int _param1 = -1;
        public int Param1 => _param1;
        public void SetParam1(int param1) => _param1 = param1;
        private int _param2 = -1;
        public int Param2 => _param2;
        public void SetParam2(int param2) => _param2 = param2;
        private int _skillId = -1;
        public int SkillId => _skillId;
        private bool _getFlag = false;
        public bool GetFlag => _getFlag;
        public void SetGetFlag(bool getFlag) => _getFlag = getFlag;

        public GetItemInfo(GetItemData getItemData)
        {
            if (getItemData != null)
            {
                _param1 = getItemData.Param1;
                _param2 = getItemData.Param2;
                _getItemType = getItemData.Type;
                if (IsSkill())
                {
                    var skillData = DataSystem.FindSkill(_param1);
                    _skillId = skillData.Id;
                }
            }
        }

        public void CopyData(GetItemInfo getItemInfo)
        {
            _param1 = getItemInfo.Param1;
            _param2 = getItemInfo.Param2;
            _getItemType = getItemInfo.GetItemType;
            _skillId = getItemInfo.SkillId;
        }

        public string GetTitleData()
        {
            switch (_getItemType)
            {
                case GetItemType.Numinous:
                    return "+" + _param2.ToString() + "/" + _param1.ToString() + DataSystem.GetText(1000);
                case GetItemType.Skill:
                    var skillData = DataSystem.FindSkill(_param1);
                    return skillData.Name;
                case GetItemType.Demigod:
                    return DataSystem.GetText(14042) + "+" + _param1.ToString();
                case GetItemType.Ending:
                    break;
                case GetItemType.StatusUp:
                    return DataSystem.GetReplaceText(14070,_param1.ToString());
                case GetItemType.Regeneration:
                    return DataSystem.GetReplaceText(3240,_param1.ToString());
                case GetItemType.ReBirth:
                    break;
                case GetItemType.LearnSkill:
                    return DataSystem.FindSkill(_param2).Name;
                case GetItemType.AddActor:
                    return DataSystem.FindActor(_param1).Name;
                case GetItemType.SelectAddActor:
                    return "選択して仲間召喚";
                case GetItemType.SaveHuman:
                    return DataSystem.GetText(14100) + DataSystem.GetReplaceDecimalText(_param2) + "/" + DataSystem.GetReplaceDecimalText(_param1);
                case GetItemType.SelectRelic:
                    return "選択してレア魔法入手";
                case GetItemType.RemakeHistory:
                    break;
                case GetItemType.ParallelHistory:
                    break;
                case GetItemType.ChangeInitActor:
                    break;
            }
            return "";
        }
        
        public bool IsSkill()
        {
            return _getItemType == GetItemType.Skill || _getItemType == GetItemType.SelectRelic;
        }

        public bool IsAttributeSkill()
        {
            return (int)_getItemType >= (int)GetItemType.AttributeFire && (int)_getItemType <= (int)GetItemType.AttributeDark;
        }
    }
}