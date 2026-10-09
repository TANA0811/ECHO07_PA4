using TMPro;
using UnityEngine;

/// <summary>
/// Escucha los cambios de energía y actualiza el HUD.
/// Implementa el rol de Observer.
/// </summary>
public class HUDController : MonoBehaviour
{
    [SerializeField] private TMP_Text energyText;

    private void Start()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnEnergyChanged += UpdateEnergyText;

            // Mostrar valor inicial
            UpdateEnergyText(GameManager.Instance.Energy);
        }
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnEnergyChanged -= UpdateEnergyText;
        }
    }

    private void UpdateEnergyText(int energy)
    {
        if (energyText != null)
        {
            energyText.text = "Energy Cores: " + energy + " / 4";
        }
    }
}