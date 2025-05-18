using UnityEngine;
using UnityEngine.Events;

namespace Events.ScriptableObjects
{
    public class EventChannelSO<T> : VoidEventChannelSO
    {
        public UnityEvent<T> onTypedEvent;

        public void RaiseEvent(T value)
        {
            if (onTypedEvent != null)
            {
                onTypedEvent.Invoke(value);
            }
            else
            {
                LogNullEventError();
            }
        }
    }
}
