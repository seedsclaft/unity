using System.Collections.Generic;

namespace Ryneus
{
    [System.Serializable]
    public class GetItemInfo 
    {
        private GetItemData _getItemData = null;
        public GetItemData Master => _getItemData;
        public int Param1 => Master.Param1;
        public int Param2 => Master.Param2;
        private GetItemType _getItemType = GetItemType.None;
        public GetItemType GetItemType => _getItemType;
        private int _resultParam = -1;
        public int ResultParam => _resultParam;
        public void SetResultParam(int resultParam) => _resultParam = resultParam;
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
            _resultParam = getItemInfo.ResultParam;
            _getItemType = getItemInfo.GetItemType;
            _skillId = getItemInfo.SkillId;
            _getFlag = getItemInfo.GetFlag;
        }

        public void ResetData()
        {
            _resultParam = 0;
            _getItemType = _getItemData.Type;
            if (IsSkill())
            {
                var skillData = DataSystem.FindSkill(_getItemData.Param1);
                _skillId = skillData.Id;
            }
            _getFlag = false;
        }

        public string GetTitleData()
        {
            switch (_getItemType)
            {
                case GetItemType.Numinous:
                    return "+" + _resultParam.ToString() + "/" + Param1.ToString() + DataSystem.GetText(1000);
                case GetItemType.Skill:
                    var skillData = DataSystem.FindSkill(Param1);
                    return skillData.Name;
                case GetItemType.Demigod:
                    return DataSystem.GetText(20210) + "+" + Param1.ToString();
                case GetItemType.Ending:
                    break;
                case GetItemType.StatusUp:
                    return DataSystem.GetReplaceText(20220,_resultParam.ToString());
                case GetItemType.Regeneration:
                    return DataSystem.GetReplaceText(20230,_resultParam.ToString());
                case GetItemType.ReBirth:
                    break;
                case GetItemType.LearnSkill:
                    return DataSystem.FindSkill(Param2).Name;
                case GetItemType.AddActor:
                    return DataSystem.FindActor(Param1).Name;
                case GetItemType.SelectAddActor:
                    return DataSystem.GetText(20240);
                case GetItemType.SaveHuman:
                    return DataSystem.GetText(19100) + DataSystem.GetReplaceDecimalText(_resultParam) + "/" + DataSystem.GetReplaceDecimalText(Param1);
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