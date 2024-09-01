using System.Collections.Generic;

namespace Ryneus
{
    [System.Serializable]
    public class GetItemInfo 
    {
        private GetItemData _getItemData = null;
        public GetItemData Master => _getItemData;
        private GetItemType _getItemType = GetItemType.None;
        public GetItemType GetItemType => _getItemType;
        private int _resultParam1 = -1;
        public int ResultParam1 => _resultParam1;
        public void SetResultParam1(int param1) => _resultParam1 = param1;
        private int _resultParam2 = -1;
        public int ResultParam2 => _resultParam2;
        public void SetResultParam2(int param2) => _resultParam2 = param2;
        private int _skillId = -1;
        public int SkillId => _skillId;
        private bool _getFlag = false;
        public bool GetFlag => _getFlag;
        public void SetGetFlag(bool getFlag) => _getFlag = getFlag;

        public GetItemInfo(GetItemData getItemData)
        {
            if (getItemData == null)
            {
                return;
            }
            _getItemData = getItemData;
            ResetData();
        }

        public void CopyData(GetItemInfo getItemInfo)
        {
            _resultParam1 = getItemInfo.ResultParam1;
            _resultParam2 = getItemInfo.ResultParam2;
            _getItemType = getItemInfo.GetItemType;
            _skillId = getItemInfo.SkillId;
            _getFlag = getItemInfo.GetFlag;
        }

        public void ResetData()
        {
            _resultParam1 = _getItemData.Param1;
            _resultParam2 = _getItemData.Param2;
            _getItemType = _getItemData.Type;
            if (IsSkill())
            {
                var skillData = DataSystem.FindSkill(_resultParam1);
                _skillId = skillData.Id;
            }
            _getFlag = false;
        }

        public string GetTitleData()
        {
            switch (_getItemType)
            {
                case GetItemType.Numinous:
                    return "+" + _resultParam2.ToString() + "/" + _resultParam1.ToString() + DataSystem.GetText(1000);
                case GetItemType.Skill:
                    var skillData = DataSystem.FindSkill(_resultParam1);
                    return skillData.Name;
                case GetItemType.Demigod:
                    return DataSystem.GetText(20210) + "+" + _resultParam1.ToString();
                case GetItemType.Ending:
                    break;
                case GetItemType.StatusUp:
                    return DataSystem.GetReplaceText(20220,_resultParam1.ToString());
                case GetItemType.Regeneration:
                    return DataSystem.GetReplaceText(20230,_resultParam1.ToString());
                case GetItemType.ReBirth:
                    break;
                case GetItemType.LearnSkill:
                    return DataSystem.FindSkill(_resultParam2).Name;
                case GetItemType.AddActor:
                    return DataSystem.FindActor(_resultParam1).Name;
                case GetItemType.SelectAddActor:
                    return DataSystem.GetText(20240);
                case GetItemType.SaveHuman:
                    return DataSystem.GetText(19100) + DataSystem.GetReplaceDecimalText(_resultParam2) + "/" + DataSystem.GetReplaceDecimalText(_resultParam1);
                case GetItemType.SelectRelic:
                    return DataSystem.GetText(20250);
                case GetItemType.RemakeHistory:
                case GetItemType.ParallelHistory:
                case GetItemType.Multiverse:
                case GetItemType.LvLink:
                    break;
            }
            return "";
        }
        
        public bool IsSkill()
        {
            return _getItemType == GetItemType.Skill;
        }

        public bool IsAttributeSkill()
        {
            return (int)_getItemType >= (int)GetItemType.AttributeFire && (int)_getItemType <= (int)GetItemType.AttributeDark;
        }

        public bool IsAddActor()
        {
            return _getItemType == GetItemType.AddActor || _getItemType == GetItemType.SelectAddActor;
        }
    }
}