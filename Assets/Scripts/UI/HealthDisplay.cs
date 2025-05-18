using System;
using Events;
using TMPro;
using UnityEngine;

public class HealthDisplay : MonoBehaviour
{
    [SerializeField] private IntEventChannelSO onPlayerTakeDamage;
    [SerializeField] private TextMeshProUGUI healthText;

    private void Start()
    {
        onPlayerTakeDamage.onIntEvent.AddListener(UpdateText);
    }

    public void UpdateText(int health)
    {
        healthText.text = health.ToString();
    }
}