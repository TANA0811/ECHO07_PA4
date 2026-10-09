using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class SecurityDroneAI : MonoBehaviour
{
    public enum DroneState { PATROL, CHASE }

    [Header("Referencias")]
    [SerializeField] private Transform player;
    [SerializeField] private Transform[] patrolPoints;

    [Header("Movimiento")]
    [SerializeField] private float patrolSpeed = 2f;
    [SerializeField] private float chaseSpeed = 3.5f;
    [SerializeField] private float patrolStoppingDistance = 0.2f;
    [SerializeField] private float chaseStoppingDistance = 1f;

    [Header("Detección por distancia")]
    [SerializeField] private float detectionRange = 4f;
    [SerializeField] private float loseRange = 6f;
    [SerializeField] private float repathInterval = 0.25f;

    [Header("Estado actual - observar durante Play")]
    [SerializeField] private DroneState currentState;

    private NavMeshAgent agent;
    private Vector3[] patrolDestinations;
    private int patrolIndex;
    private float nextRepathTime;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        if (player == null || patrolPoints == null ||
            patrolPoints.Length < 2)
        {
            StopWithError("Asigna Player y al menos 2 Patrol Points.");
            return;
        }

        if (!agent.isActiveAndEnabled || !agent.isOnNavMesh)
        {
            StopWithError("Coloca la raíz del dron sobre el NavMesh.");
            return;
        }

        if (detectionRange <= 0f || loseRange <= detectionRange ||
            patrolSpeed <= 0f || chaseSpeed <= 0f ||
            repathInterval <= 0f)
        {
            StopWithError("Revisa velocidades, rangos e intervalo.");
            return;
        }

        // Validar todos los puntos antes de comenzar.
        patrolDestinations = new Vector3[patrolPoints.Length];

        NavMeshQueryFilter filter = new NavMeshQueryFilter
        {
            agentTypeID = agent.agentTypeID,
            areaMask = agent.areaMask
        };

        for (int i = 0; i < patrolPoints.Length; i++)
        {
            if (patrolPoints[i] == null ||
                !NavMesh.SamplePosition(
                    patrolPoints[i].position,
                    out NavMeshHit hit,
                    0.75f,
                    filter))
            {
                StopWithError(
                    "Patrol Point " + (i + 1) +
                    ": falta asignarlo o está fuera del NavMesh.");
                return;
            }

            NavMeshPath path = new NavMeshPath();

            if (!agent.CalculatePath(hit.position, path) ||
                path.status != NavMeshPathStatus.PathComplete)
            {
                StopWithError(
                    "Patrol Point " + (i + 1) +
                    ": no tiene una ruta completa desde el dron.");
                return;
            }

            patrolDestinations[i] = hit.position;
        }

        patrolIndex = 0;
        EnterPatrol();
    }

    private void Update()
    {
        if (!agent.isActiveAndEnabled || !agent.isOnNavMesh)
        {
            StopWithError("El agente perdió acceso al NavMesh.");
            return;
        }

        if (player == null)
        {
            StopWithError("La referencia Player ya no está disponible.");
            return;
        }

        float distance = Vector3.Distance(
            transform.position, player.position);

        if (currentState == DroneState.PATROL &&
            distance <= detectionRange)
        {
            EnterChase();
        }
        else if (currentState == DroneState.CHASE &&
                 distance > loseRange)
        {
            EnterPatrol();
        }

        if (currentState == DroneState.CHASE)
        {
            UpdateChase();
        }
        else
        {
            UpdatePatrol();
        }
    }

    private void EnterPatrol()
    {
        currentState = DroneState.PATROL;
        agent.speed = patrolSpeed;
        agent.stoppingDistance = patrolStoppingDistance;
        agent.isStopped = false;
        agent.SetDestination(patrolDestinations[patrolIndex]);

        Debug.Log("SecurityDrone_PA4: PATROL", this);
    }

    private void EnterChase()
    {
        currentState = DroneState.CHASE;
        agent.speed = chaseSpeed;
        agent.stoppingDistance = chaseStoppingDistance;
        agent.isStopped = false;
        agent.ResetPath();
        nextRepathTime = 0f;

        Debug.Log("SecurityDrone_PA4: CHASE", this);
    }

    private void UpdatePatrol()
    {
        if (agent.pathPending)
            return;

        if (agent.pathStatus != NavMeshPathStatus.PathComplete)
        {
            StopWithError("La ruta de patrulla dejó de ser válida.");
            return;
        }

        if (agent.remainingDistance <=
            agent.stoppingDistance + 0.1f)
        {
            patrolIndex =
                (patrolIndex + 1) % patrolDestinations.Length;

            agent.SetDestination(patrolDestinations[patrolIndex]);
        }
    }

    private void UpdateChase()
{
    if (Time.time < nextRepathTime)
        return;

    nextRepathTime = Time.time + repathInterval;

    NavMeshQueryFilter filter = new NavMeshQueryFilter
    {
        agentTypeID = agent.agentTypeID,
        areaMask = agent.areaMask
    };

    // Aumentamos el radio de búsqueda a 5f para tolerar diferencias de altura o saltos del Player
    if (NavMesh.SamplePosition(player.position, out NavMeshHit hit, 5.0f, filter))
    {
        agent.SetDestination(hit.position);
    }
    else
    {
        // En lugar de borrar la ruta inmediatamente y quedar estático, 
        // intenta ir a la última posición conocida del jugador.
        agent.SetDestination(player.position);
    }
}

    private void StopWithError(string message)
    {
        Debug.LogError("SecurityDrone_PA4: " + message, this);

        if (agent != null &&
            agent.isActiveAndEnabled && agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.ResetPath();
        }

        enabled = false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, loseRange);

        if (patrolPoints == null || patrolPoints.Length < 2)
            return;

        Gizmos.color = Color.cyan;

        for (int i = 0; i < patrolPoints.Length; i++)
        {
            Transform point = patrolPoints[i];
            Transform next =
                patrolPoints[(i + 1) % patrolPoints.Length];

            if (point == null)
                continue;

            Gizmos.DrawSphere(point.position, 0.15f);

            if (next != null)
                Gizmos.DrawLine(point.position, next.position);
        }
    }
}