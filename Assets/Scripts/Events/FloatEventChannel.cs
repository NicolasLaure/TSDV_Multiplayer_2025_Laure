using UnityEngine;
using UnityEngine.Events;

namespace Events.Scriptables
{
    [CreateAssetMenu(menuName = "Events/Float Channel")]
    public class FloatEventChannel : VoidEventChannelSO
    {
        public UnityEvent<float> onFloatEvent;

        public void RaiseEvent(float value)
        {
            if (onFloatEvent != null)
            {
                onFloatEvent.Invoke(value);
                onEvent.Invoke();
            }
            else
            {
                LogNullEventError();
            }
        }
    }
}