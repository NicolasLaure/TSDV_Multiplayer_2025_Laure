using UnityEngine;

namespace MidTerm2.View
{
    public class WarriorView : MonoBehaviour
    {
        public void OnMouseDown()
        {
            CastlesController.Instance.SelectWarrior(gameObject);
        }
    }
}