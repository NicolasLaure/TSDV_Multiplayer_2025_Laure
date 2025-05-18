using System;
using Events.ScriptableObjects;
using UnityEngine;

namespace Events.Scriptables
{
    [CreateAssetMenu(menuName = "Events/String Channel")]
    public class StringEventChannelSO : EventChannelSO<String>
    { }
}
