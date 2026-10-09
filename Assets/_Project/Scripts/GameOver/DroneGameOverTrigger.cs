using UnityEngine;

public class DroneGameOverTrigger : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Transform player;
    [SerializeField] private GameOverController gameOverController;

    [Header("Deteccion")]
    [SerializeField, Min(0.1f)] private float contactDistance = 1.2f;
    [SerializeField, Min(0.1f)] private float damageCooldown = 2f;

    private float nextDamageTime;

    private void Update()
    {
        if (player == null || gameOverController == null || Time.time < nextDamageTime)
            return;

        float distance = Vector3.Distance(transform.position, player.position);
        if (distance <= contactDistance)
        {
            nextDamageTime = Time.time + damageCooldown;
            gameOverController.LoseLife();
        }
    }
}
