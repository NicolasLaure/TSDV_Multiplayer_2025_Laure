using UnityEngine;
using UnityEngine.Events;

namespace Events.Scriptables
{ 
    public class TwoValuesEventChannelSO<T1, T2> : VoidEventChannelSO
    {
        public UnityEvent<T1, T2> onTypedEvent;

        public void RaiseEvent(T1 value1, T2 value2)
        {
            if (onTypedEvent != null)
            {
                onTypedEvent.Invoke(value1, value2);
            }
            else
            {
                LogNullEventError();
            }
        }
    }
}
