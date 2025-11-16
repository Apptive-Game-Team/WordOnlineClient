using System;

namespace Script.Global
{
    public class StateEvent<T>
    {
        private T _data;
        public T Data
        {
            get => _data;
            private set
            {
                _data = value;
                OnStateChange?.Invoke(_data);
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