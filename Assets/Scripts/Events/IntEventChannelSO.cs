using UnityEngine;
using UnityEngine.Events;

namespace Events
{
    [CreateAssetMenu(menuName = "Events/Int Channel")]
    public class IntEventChannelSO : VoidEventChannelSO
    {
        public UnityEvent<int> onIntEvent;

        public void RaiseEvent(int value)
        {
            if (onIntEvent != null)
            {
                onIntEvent.Invoke(value);
                onEvent.Invoke();
            }
            else
            {
                LogNullEventError();
            }
        }
    }
}