using System;

namespace Global
{
    public class StateEvent<T>
    {
        private T _data;
        public T Data
        {
            get => _data;
            private set
            {
                if (!_data.Equals(value))
                {
                    OnStateChange?.Invoke(value);
                }
                _data = value;
            }
        }

        public StateEvent(T data)
        {
            Data = data;
        }
        
        public event Action<T> OnStateChange;
        
        public void UpdateData(T newData)
        {
            Data = newData;
        }
    }
}