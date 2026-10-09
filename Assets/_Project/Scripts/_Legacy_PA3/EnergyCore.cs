using UnityEngine;

/// <summary>
/// Gestiona la recolección del Energy Core.
/// </summary>
public class EnergyCore : MonoBehaviour
{
    [Header("Mission Feedback")]
    [SerializeField] private GameObject missionCompleteText;

    [Header("Audio")]
    [SerializeField] private AudioSource pickupAudio;

    [Header("Particles")]
    [SerializeField] private ParticleSystem pickupParticles;

    [Header("Visual")]
    [SerializeField] private Collider2D coreCollider;
    [SerializeField] private GameObject coreLight;

    private bool collected;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (collected || !other.CompareTag("Player"))
            return;

        collected = true;

        // 1. OCULTAR INMEDIATAMENTE TODO EL CORE VISUAL
        SpriteRenderer[] renderers = GetComponentsInChildren<SpriteRenderer>(true);

        foreach (SpriteRenderer renderer in renderers)
        {
            renderer.enabled = false;
        }

        // Apagar la luz inmediatamente
        if (coreLight != null)
        {
            coreLight.SetActive(false);
        }

        // Desactivar collider para evitar doble recolección
        if (coreCollider != null)
        {
            coreCollider.enabled = false;
        }

        // 2. Actualizar energía
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddEnergy(1);
        }

        // 3. Sonido
        if (pickupAudio != null)
        {
            pickupAudio.Play();
        }

        // 4. Partículas
        if (pickupParticles != null)
        {
            pickupParticles.Play();
        }

        // 5. Mostrar CORE RECOVERED después de desaparecer
        Invoke(nameof(ShowMissionMessage), 0.3f);

        // Mantener objeto vivo temporalmente
        // para permitir sonido, partículas y mensaje
        Destroy(gameObject, 3f);
    }

    private void ShowMissionMessage()
    {
        if (missionCompleteText != null)
        {
            missionCompleteText.SetActive(true);
            Invoke(nameof(HideMissionMessage), 2.5f);
        }
    }

    private void HideMissionMessage()
    {
        if (missionCompleteText != null)
        {
            missionCompleteText.SetActive(false);
        }
    }
}