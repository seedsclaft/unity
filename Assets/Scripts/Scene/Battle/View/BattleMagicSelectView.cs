using System.Collections.Generic;
using UnityEngine;

namespace Ryneus
{
    public class BattleMagicSelectView : BaseList
    {
        [SerializeField] SkillInfoComponent selectMagic;
        [SerializeField] SkillInfoComponent selectMagicDescription;
        [SerializeField] BattleSkillSelect x_Magic;
        [SerializeField] BattleSkillSelect y_Magic;
        [SerializeField] BattleSkillSelect l1_Magic;
        [SerializeField] BattleSkillSelect r1_Magic;
        private Dictionary<InputKeyType,SkillInfo> _magicCommands;
        private InputKeyType _selectInputKeyType = InputKeyType.None;
        public InputKeyType SelectInputKeyType => _selectInputKeyType;

        private SkillInfo _selectSkill;
        public SkillInfo SelectSkill => _selectSkill;

        public void SetSelectSkill(InputKeyType inputKeyType)
        {
            _selectInputKeyType = inputKeyType;
            _selectSkill = _magicCommands[inputKeyType];
            selectMagic.UpdateInfo(_selectSkill);
            selectMagicDescription.UpdateInfo(_selectSkill);
            y_Magic.SetSelect(inputKeyType == InputKeyType.Option1);
            x_Magic.SetSelect(inputKeyType == InputKeyType.Option2);
            l1_Magic.SetSelect(inputKeyType == InputKeyType.SideLeft1);
            r1_Magic.SetSelect(inputKeyType == InputKeyType.SideRight1);
        }

        public void SetMagicCommands(Dictionary<InputKeyType,SkillInfo> magicCommands)
        {
            selectMagicDescription.Clear();
            _magicCommands = magicCommands;
            foreach (var magicCommand in magicCommands)
            {
                switch (magicCommand.Key)
                {
                    case InputKeyType.Option1:
                        y_Magic.UpdateInfo(magicCommand.Value);
                        break;
                    case InputKeyType.Option2:
                        x_Magic.UpdateInfo(magicCommand.Value);
                        break;
                    case InputKeyType.SideLeft1:
                        l1_Magic.UpdateInfo(magicCommand.Value);
                        break;
                    case InputKeyType.SideRight1:
                        r1_Magic.UpdateInfo(magicCommand.Value);
                        break;
                }
            }
        }
    }
}
