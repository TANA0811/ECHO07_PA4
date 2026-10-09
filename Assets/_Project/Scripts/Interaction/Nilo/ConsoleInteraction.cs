using TMPro;
using UnityEngine;

// PA4 - Sistema de interacción, narrativa y flujo de misión validado por Nilo.

public class ConsoleInteraction : MonoBehaviour
{
    [Header("Configuración de interacción")]
    [SerializeField] private float interactionDistance = 3f;
    [SerializeField] private Transform player;
    [SerializeField] private Light targetLight;

    [Header("Diagnóstico del reactor")]
    [SerializeField] private GameObject diagnosticPanel;
    [SerializeField] private TMP_Text consoleStatusText;
    [SerializeField] private TMP_Text consoleHeaderText;
    [SerializeField] private TMP_Text reactorStatusText;
    [SerializeField] private TMP_Text energyStatusText;

    [Header("Misión de recuperación energética")]
    [SerializeField]
    private EnergyCoreSequenceManager sequenceManager;

    [Header("Restauración del reactor")]
    [SerializeField]
    private ReactorRestorationManager reactorRestorationManager;

    [Header("Panel inicial")]
    [SerializeField]
    private float initialPanelDuration = 5f;

    private bool diagnosticOpen;

    private void Start()
    {
        diagnosticOpen = false;

        if (diagnosticPanel != null)
        {
            diagnosticPanel.SetActive(false);
        }

        if (targetLight != null)
        {
            targetLight.enabled = false;
        }
    }

    private void Update()
    {
        if (!Input.GetKeyDown(KeyCode.E))
            return;

        // =====================================================
        // SI LA CONSOLA YA ESTÁ ABIERTA
        // =====================================================

        if (diagnosticOpen)
        {
            HandleOpenConsoleAction();
            return;
        }

        // =====================================================
        // VALIDAR PLAYER
        // =====================================================

        if (player == null)
        {
            Debug.LogWarning(
                "ECHO-07: Falta asignar el personaje."
            );

            return;
        }

        // =====================================================
        // VALIDAR DISTANCIA
        // =====================================================

        float distance = Vector3.Distance(
            player.position,
            transform.position
        );

        if (distance > interactionDistance)
        {
            Debug.Log(
                "ECHO-07: Acércate más a la consola."
            );

            return;
        }

        OpenConsole();
    }

    // =========================================================
    // ACCIONES CUANDO LA CONSOLA YA ESTÁ ABIERTA
    // =========================================================

    private void HandleOpenConsoleAction()
    {
        // Si el reactor ya fue restaurado,
        // E simplemente cierra el panel.
        if (reactorRestorationManager != null &&
            reactorRestorationManager.IsRestored)
        {
            CloseConsole();
            return;
        }

        // Si ya tenemos los 6 Energy Cores,
        // el segundo E inicia la reparación.
        if (sequenceManager != null &&
            sequenceManager.AllCoresCollected)
        {
            if (reactorRestorationManager != null)
            {
                Debug.Log(
                    "ECHO-07: Iniciando reparación del reactor."
                );

                SetConsoleMessage(
                    "RESTAURANDO REACTOR...\nESPERE."
                );

                reactorRestorationManager.StartRestoration();
            }
            else
            {
                Debug.LogWarning(
                    "ECHO-07: Falta asignar ReactorRestorationManager."
                );
            }

            return;
        }

        // Durante cualquier otro estado,
        // E permite cerrar manualmente la consola.
        CloseConsole();
    }

    // =========================================================
    // ABRIR CONSOLA
    // =========================================================

    private void OpenConsole()
    {
        // Evitar temporizadores anteriores pendientes.
        CancelInvoke(nameof(CloseConsole));

        diagnosticOpen = true;

        if (diagnosticPanel != null)
        {
            diagnosticPanel.SetActive(true);
        }

        if (targetLight != null)
        {
            targetLight.enabled = true;
        }

        // =====================================================
        // REACTOR YA RESTAURADO
        // =====================================================

        if (reactorRestorationManager != null &&
            reactorRestorationManager.IsRestored)
        {
            ShowRestoredState();
            return;
        }

        // =====================================================
        // VALIDAR MANAGER DE ENERGY CORES
        // =====================================================

        if (sequenceManager == null)
        {
            SetConsoleMessage(
                "ERROR: Falta EnergyCoreSequenceManager."
            );

            Debug.LogWarning(
                "ECHO-07: Falta asignar EnergyCoreSequenceManager."
            );

            return;
        }

        // =====================================================
        // PRIMERA INTERACCIÓN:
        // PRESENTAR MISIÓN
        // =====================================================

        if (!sequenceManager.MissionStarted)
        {
            SetConsoleMessage(
                "DIAGNÓSTICO COMPLETADO.\n" +
                "Localiza y recupera los 6 Energy Cores " +
                "para restablecer el reactor."
            );

            sequenceManager.StartMission();

            // =================================================
            // INTEGRACIÓN DE GEORGE
            // El mensaje inicial se cierra automáticamente
            // después de 5 segundos.
            // =================================================

            Invoke(
                nameof(CloseConsole),
                initialPanelDuration
            );

            Debug.Log(
                "ECHO-07: Diagnóstico del reactor consultado. " +
                "La misión ha comenzado."
            );

            return;
        }

        // =====================================================
        // 6/6 ENERGY CORES
        // =====================================================

        if (sequenceManager.AllCoresCollected)
        {
            // IMPORTANTE:
            // Aquí NO usamos Invoke.
            // El panel debe quedarse abierto para que
            // el jugador presione E nuevamente y repare.
            
            SetConsoleMessage(
                "ENERGY 6/6\n" +
                "ECHO, repara el reactor.\n" +
                "Presiona E nuevamente."
            );

            Debug.Log(
                "ECHO-07: Los 6 núcleos fueron recuperados. " +
                "El sistema está listo para restablecerse."
            );

            return;
        }

        // =====================================================
        // MISIÓN EN PROGRESO
        // =====================================================

        SetConsoleMessage(
            "RECUPERACIÓN EN PROGRESO.\nENERGY " +
            sequenceManager.CollectedCount +
            "/" +
            sequenceManager.TotalCores
        );

        Debug.Log(
            "ECHO-07: Diagnóstico del reactor consultado."
        );
    }

    // =========================================================
    // CERRAR CONSOLA
    // =========================================================

    private void CloseConsole()
    {
        // =====================================================
        // INTEGRACIÓN DE GEORGE
        // Si el jugador cerró el panel manualmente antes
        // de los 5 segundos, cancelamos el cierre pendiente.
        // =====================================================

        CancelInvoke(nameof(CloseConsole));

        diagnosticOpen = false;

        if (diagnosticPanel != null)
        {
            diagnosticPanel.SetActive(false);
        }

        if (targetLight != null)
        {
            targetLight.enabled = false;
        }

        Debug.Log(
            "ECHO-07: Diagnóstico cerrado."
        );
    }

    // =========================================================
    // ACTUALIZAR MENSAJE DE CONSOLA
    // =========================================================

    private void SetConsoleMessage(string message)
    {
        if (consoleStatusText != null)
        {
            consoleStatusText.text = message;
        }
    }

    // =========================================================
    // ESTADO FINAL:
    // REACTOR RESTAURADO
    // =========================================================

    public void ShowRestoredState()
    {
        // Por seguridad, impedir que un temporizador del
        // panel inicial cierre el panel final.
        CancelInvoke(nameof(CloseConsole));

        if (consoleHeaderText != null)
        {
            consoleHeaderText.text =
                "ECHO-07\nSISTEMA RESTAURADO";
        }

        if (reactorStatusText != null)
        {
            reactorStatusText.text =
                "REACTOR OPERATIVO";
            reactorStatusText.color = new Color(0.1f, 0.94f, 1f, 1f);
        }

        if (energyStatusText != null)
        {
            energyStatusText.text =
                "SUMINISTRO ENERGÉTICO ESTABLE";
        }

        if (consoleStatusText != null)
        {
            consoleStatusText.text =
                "ESTADO: NORMAL\nMISIÓN CUMPLIDA";
        }

        Debug.Log(
            "ECHO-07: Consola actualizada al estado restaurado."
        );
    }
}