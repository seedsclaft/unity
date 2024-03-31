using System.Collections;
using System.Collections.Generic;

namespace Ryneus
{
    public class BattleRecord 
    {
        private int _battlerIndex = -1;
        public int BattlerIndex => _battlerIndex;
        private int _damageValue = 0;
        public int DamageValue => _damageValue;
        private int _damagedValue = 0;
        public int DamagedValue => _damagedValue;
        private int _healValue = 0;
        public int HealValue => _healValue;
        public BattleRecord(int battlerIndex)
        {
            _battlerIndex = battlerIndex;
        }

        public void GainDamageValue(int damageValue)
        {
            _damageValue += damageValue;
        }

        public void GainDamagedValue(int damagedValue)
        {
            _damagedValue += damagedValue;
        }

        public void GainHealValue(int healValue)
        {
            _healValue += healValue;
        }
    }
}