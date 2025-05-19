using System.Collections;
using TMPro;
using UnityEngine;

namespace UI
{
    public class ErrorMessagePanel : MonoBehaviour
    {
        [SerializeField] private float messageDuration;
        [SerializeField] private TextMeshProUGUI errorMessage;

        private void OnEnable()
        {
            StartCoroutine(DelayedDisable());
        }

        public void SetText(string text)
        {
            errorMessage.text = text;
        }

        IEnumerator DelayedDisable()
        {
            yield return new WaitForSeconds(messageDuration);
            gameObject.SetActive(false);
        }
    }
}