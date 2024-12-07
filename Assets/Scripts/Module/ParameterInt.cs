namespace Ryneus
{
    [System.Serializable]
    public class ParameterInt
    {
        private int _value = 0;
        public int Value => _value;

        public void SetValue(int value) 
        {
            _value = value;
        }

        public void GainValue(int value)
        {
            _value += value;
        }
    }
}
