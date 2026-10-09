using UnityEngine;

/// <summary>
/// Zona final del nivel.
/// Solo completa la misión si el jugador ya recuperó los 4 Energy Cores.
/// </summary>
public class ExitZone : MonoBehaviour
{
    [Header("Final Feedback")]
    [SerializeField] private GameObject finalMessage;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (GameManager.Instance == null)
            return;

        if (GameManager.Instance.Energy < 4)
            return;

        if (finalMessage != null)
        {
            finalMessage.SetActive(true);
        }
    }
}