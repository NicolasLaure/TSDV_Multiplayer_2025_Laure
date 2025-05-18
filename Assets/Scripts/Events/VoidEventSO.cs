using UnityEngine;
using UnityEngine.Events;

namespace Events
{
    [CreateAssetMenu(menuName = "Events/Void Channel")]
    public class VoidEventChannelSO : ScriptableObject
    {
        public UnityEvent onEvent;

        public void RaiseEvent()
        {
            if (onEvent != null)
            {
                onEvent.Invoke();
            }
            else
            {
                LogNullEventError();
            }
        }

        protected void LogNullEventError()
        {
            Debug.LogError($"{this.name} has no events. Please check if" +
                           $"events have been added correctly.");
        }
    }
}
