using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EnergyCoreSequenceManager : MonoBehaviour
{
    [Header("Energy Cores en orden de recolección")]
    [SerializeField]
    private EnergyCore3D[] energyCores = new EnergyCore3D[6];

    [Header("Tiempo antes de mostrar el siguiente núcleo")]
    [SerializeField]
    private float nextCoreDelay = 2.2f;

    [Header("Interfaz del contador")]
    [SerializeField]
    private GameObject energyCounterUI;

    [SerializeField]
    private TMP_Text energyCounterText;

    [Header("Segmentos visuales de carga del reactor")]
    [SerializeField]
    private Image[] reactorSegments = new Image[6];

    [Header("Animación del segmento nuevo")]
    [SerializeField]
    private float segmentPopScale = 1.45f;

    [SerializeField]
    private float segmentPopDuration = 0.38f;

    [Header("Pulso general de los segmentos")]
    [SerializeField]
    private RectTransform reactorSegmentsVisual;

    [SerializeField]
    private float groupPulseScale = 1.15f;

    [SerializeField]
    private float groupPulseDuration = 0.30f;

    [Header("Guía del objetivo")]
    [SerializeField]
    private ObjectiveGuideManager objectiveGuideManager;

    [SerializeField]
    private Transform consoleTarget;

    [Header("Audio de recolección")]
    [SerializeField]
    private AudioSource pickupAudioSource;

    [SerializeField]
    private AudioClip pickupClip;

    private int collectedCount = 0;
    private bool missionStarted = false;

    private Coroutine[] segmentAnimationCoroutines;
    private Coroutine groupPulseCoroutine;

    public int CollectedCount => collectedCount;

    public int TotalCores =>
        energyCores != null ? energyCores.Length : 0;

    public bool MissionStarted => missionStarted;

    public bool AllCoresCollected =>
        missionStarted &&
        collectedCount == TotalCores;

    private void Awake()
    {
        if (energyCounterUI != null)
        {
            energyCounterUI.SetActive(false);
        }

        collectedCount = 0;
        missionStarted = false;

        InitializeSegments();
        UpdateCounter();

        if (reactorSegmentsVisual != null)
        {
            reactorSegmentsVisual.localScale =
                Vector3.one;
        }

        if (energyCores == null ||
            energyCores.Length != 6)
        {
            Debug.LogError(
                "ECHO-07: Debes configurar exactamente 6 Energy Cores."
            );

            return;
        }

        foreach (EnergyCore3D core in energyCores)
        {
            if (core != null)
            {
                core.gameObject.SetActive(false);
            }
        }

        Debug.Log(
            "ECHO-07: Esperando el diagnóstico para iniciar la misión."
        );
    }

    public void StartMission()
    {
        if (missionStarted)
            return;

        if (energyCores == null ||
            energyCores.Length != 6)
        {
            Debug.LogError(
                "ECHO-07: No se puede iniciar la misión. Se necesitan 6 Energy Cores."
            );

            return;
        }

        for (int i = 0; i < energyCores.Length; i++)
        {
            if (energyCores[i] == null)
            {
                Debug.LogError(
                    "ECHO-07: Falta asignar el Energy Core " +
                    (i + 1)
                );

                return;
            }
        }

        missionStarted = true;

        UpdateCounter();

        if (energyCounterUI != null)
        {
            energyCounterUI.SetActive(true);
        }

        energyCores[0].gameObject.SetActive(true);

        UpdateGuideToCurrentCore();

        Debug.Log(
            "ECHO-07: Misión iniciada. Energía recuperada 0/6."
        );
    }

    public void RegisterCollection(
        EnergyCore3D collectedCore
    )
    {
        if (!missionStarted)
            return;

        if (collectedCount >= TotalCores)
            return;

        if (energyCores[collectedCount] !=
            collectedCore)
        {
            Debug.LogWarning(
                "ECHO-07: Se intentó recoger un núcleo fuera de orden."
            );

            return;
        }

        collectedCount++;

        UpdateCounter();

        // Índice del segmento recién cargado.
        int newSegmentIndex =
            collectedCount - 1;

        // Animación individual.
        PlaySegmentActivation(
            newSegmentIndex
        );

        // Golpe visual del conjunto.
        PlayGroupPulse();

        // Sonido del Energy Core.
        if (pickupAudioSource != null &&
            pickupClip != null)
        {
            pickupAudioSource.PlayOneShot(
                pickupClip
            );
        }

        Debug.Log(
            "ECHO-07: Energía recuperada " +
            collectedCount +
            "/" +
            TotalCores
        );

        if (AllCoresCollected)
        {
            Debug.Log(
                "ECHO-07: Energía recuperada 6/6. Regresa a la consola para restablecer el sistema."
            );

            if (objectiveGuideManager != null &&
                consoleTarget != null)
            {
                objectiveGuideManager.SetTarget(
                    consoleTarget,
                    "Regresa a la consola y repara el reactor presionando E."
                );
            }

            return;
        }

        int nextCoreIndex =
            collectedCount;

        StartCoroutine(
            ActivateNextCore(
                nextCoreIndex
            )
        );
    }

    private IEnumerator ActivateNextCore(
        int nextCoreIndex
    )
    {
        yield return new WaitForSeconds(
            nextCoreDelay
        );

        if (nextCoreIndex >= TotalCores)
            yield break;

        EnergyCore3D nextCore =
            energyCores[nextCoreIndex];

        if (nextCore != null)
        {
            nextCore.gameObject.SetActive(true);

            Debug.Log(
                "ECHO-07: Energy Core " +
                (nextCoreIndex + 1) +
                " desbloqueado."
            );

            UpdateGuideToCurrentCore();
        }
    }

    private void UpdateGuideToCurrentCore()
    {
        if (objectiveGuideManager == null)
            return;

        if (collectedCount >= TotalCores)
            return;

        EnergyCore3D currentCore =
            energyCores[collectedCount];

        if (currentCore != null)
        {
            objectiveGuideManager.SetTarget(
                currentCore.transform,
                "Recolecta el Energy Core " +
                (collectedCount + 1) +
                " de " +
                TotalCores +
                "."
            );
        }
    }

    private void InitializeSegments()
    {
        if (reactorSegments == null ||
            reactorSegments.Length != 6)
        {
            Debug.LogWarning(
                "ECHO-07: Debes asignar los 6 segmentos del reactor."
            );

            return;
        }

        segmentAnimationCoroutines =
            new Coroutine[reactorSegments.Length];

        Color inactiveColor =
            GetInactiveColor();

        for (int i = 0;
             i < reactorSegments.Length;
             i++)
        {
            if (reactorSegments[i] == null)
                continue;

            reactorSegments[i].color =
                inactiveColor;

            reactorSegments[i]
                .rectTransform
                .localScale =
                Vector3.one;
        }
    }

    private void UpdateCounter()
    {
        Color chargeColor =
            GetChargeColor(
                collectedCount
            );

        if (energyCounterText != null)
        {
            energyCounterText.text =
                collectedCount +
                "/" +
                TotalCores;

            energyCounterText.color =
                chargeColor;
        }

        UpdateSegments();
    }

    private void UpdateSegments()
    {
        if (reactorSegments == null ||
            reactorSegments.Length != 6)
        {
            return;
        }

        Color activeColor =
            GetChargeColor(
                collectedCount
            );

        Color inactiveColor =
            GetInactiveColor();

        for (int i = 0;
             i < reactorSegments.Length;
             i++)
        {
            if (reactorSegments[i] == null)
                continue;

            if (i < collectedCount)
            {
                reactorSegments[i].color =
                    activeColor;
            }
            else
            {
                reactorSegments[i].color =
                    inactiveColor;
            }
        }
    }

    private void PlaySegmentActivation(
        int segmentIndex
    )
    {
        if (reactorSegments == null)
            return;

        if (segmentIndex < 0 ||
            segmentIndex >=
            reactorSegments.Length)
        {
            return;
        }

        Image segment =
            reactorSegments[
                segmentIndex
            ];

        if (segment == null)
            return;

        if (segmentAnimationCoroutines != null &&
            segmentAnimationCoroutines[
                segmentIndex
            ] != null)
        {
            StopCoroutine(
                segmentAnimationCoroutines[
                    segmentIndex
                ]
            );
        }

        segmentAnimationCoroutines[
            segmentIndex
        ] =
            StartCoroutine(
                AnimateSegmentActivation(
                    segment,
                    segmentIndex
                )
            );
    }

    private IEnumerator AnimateSegmentActivation(
        Image segment,
        int segmentIndex
    )
    {
        RectTransform rect =
            segment.rectTransform;

        Vector3 normalScale =
            Vector3.one;

        Vector3 maximumScale =
            Vector3.one *
            segmentPopScale;

        Color normalColor =
            GetChargeColor(
                collectedCount
            );

        // Destello casi blanco/amarillo.
        Color flashColor =
            Color.Lerp(
                normalColor,
                Color.white,
                0.85f
            );

        float halfDuration =
            Mathf.Max(
                0.01f,
                segmentPopDuration *
                0.5f
            );

        float timer = 0f;

        // FASE 1:
        // El segmento crece y destella.
        while (timer < halfDuration)
        {
            timer +=
                Time.unscaledDeltaTime;

            float t =
                Mathf.Clamp01(
                    timer /
                    halfDuration
                );

            // Suavizado.
            float smooth =
                Mathf.SmoothStep(
                    0f,
                    1f,
                    t
                );

            rect.localScale =
                Vector3.Lerp(
                    normalScale,
                    maximumScale,
                    smooth
                );

            segment.color =
                Color.Lerp(
                    normalColor,
                    flashColor,
                    smooth
                );

            yield return null;
        }

        timer = 0f;

        // FASE 2:
        // Vuelve a su tamaño y color normal.
        while (timer < halfDuration)
        {
            timer +=
                Time.unscaledDeltaTime;

            float t =
                Mathf.Clamp01(
                    timer /
                    halfDuration
                );

            float smooth =
                Mathf.SmoothStep(
                    0f,
                    1f,
                    t
                );

            rect.localScale =
                Vector3.Lerp(
                    maximumScale,
                    normalScale,
                    smooth
                );

            segment.color =
                Color.Lerp(
                    flashColor,
                    normalColor,
                    smooth
                );

            yield return null;
        }

        rect.localScale =
            normalScale;

        segment.color =
            normalColor;

        if (segmentAnimationCoroutines != null)
        {
            segmentAnimationCoroutines[
                segmentIndex
            ] = null;
        }
    }

    private void PlayGroupPulse()
    {
        if (reactorSegmentsVisual == null)
            return;

        if (groupPulseCoroutine != null)
        {
            StopCoroutine(
                groupPulseCoroutine
            );
        }

        groupPulseCoroutine =
            StartCoroutine(
                AnimateGroupPulse()
            );
    }

    private IEnumerator AnimateGroupPulse()
    {
        Vector3 normalScale =
            Vector3.one;

        Vector3 pulseScale =
            Vector3.one *
            groupPulseScale;

        float halfDuration =
            Mathf.Max(
                0.01f,
                groupPulseDuration *
                0.5f
            );

        float timer = 0f;

        // CRECER
        while (timer < halfDuration)
        {
            timer +=
                Time.unscaledDeltaTime;

            float t =
                Mathf.Clamp01(
                    timer /
                    halfDuration
                );

            float smooth =
                Mathf.SmoothStep(
                    0f,
                    1f,
                    t
                );

            reactorSegmentsVisual
                .localScale =
                Vector3.Lerp(
                    normalScale,
                    pulseScale,
                    smooth
                );

            yield return null;
        }

        timer = 0f;

        // VOLVER
        while (timer < halfDuration)
        {
            timer +=
                Time.unscaledDeltaTime;

            float t =
                Mathf.Clamp01(
                    timer /
                    halfDuration
                );

            float smooth =
                Mathf.SmoothStep(
                    0f,
                    1f,
                    t
                );

            reactorSegmentsVisual
                .localScale =
                Vector3.Lerp(
                    pulseScale,
                    normalScale,
                    smooth
                );

            yield return null;
        }

        reactorSegmentsVisual.localScale =
            normalScale;

        groupPulseCoroutine = null;
    }

    private Color GetInactiveColor()
    {
        return HexToColor(
            "#3A0A0A"
        );
    }

    private Color GetChargeColor(
        int level
    )
    {
        switch (level)
        {
            case 0:
                return HexToColor(
                    "#FF3B30"
                );

            case 1:
                return HexToColor(
                    "#FF4730"
                );

            case 2:
                return HexToColor(
                    "#FF5A28"
                );

            case 3:
                return HexToColor(
                    "#FF6A1A"
                );

            case 4:
                return HexToColor(
                    "#FF8A00"
                );

            case 5:
                return HexToColor(
                    "#FFC533"
                );

            case 6:
                return HexToColor(
                    "#FFD21F"
                );

            default:
                return HexToColor(
                    "#FFD21F"
                );
        }
    }

    private Color HexToColor(
        string hex
    )
    {
        if (ColorUtility.TryParseHtmlString(
            hex,
            out Color color
        ))
        {
            return color;
        }

        return Color.white;
    }
}