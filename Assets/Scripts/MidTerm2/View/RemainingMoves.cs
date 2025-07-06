using TMPro;
using UnityEngine;

namespace MidTerm2.View
{
    public class RemainingMoves : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI textObject;

        public void SetText(int remainingMoves)
        {
            textObject.text = $"Remaining Moves: {remainingMoves}";
        }
    }
}