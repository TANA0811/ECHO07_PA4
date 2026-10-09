using TMPro;
using UnityEngine;

public class DiagnosticAlertPulse : MonoBehaviour
{
    [SerializeField] private TMP_Text reactorAlert;
    [SerializeField] private TMP_Text energyAlert;

    private void LateUpdate()
    {
        if (reactorAlert == null || energyAlert == null) return;
        bool failure = reactorAlert.text.Contains("INESTABLE");
        float pulse = failure ? Mathf.Lerp(0.4f, 1f,
            (Mathf.Sin(Time.unscaledTime * Mathf.PI * 2f) + 1f) * 0.5f) : 1f;
        reactorAlert.alpha = pulse;
        energyAlert.ForceMeshUpdate();
        TMP_TextInfo info = energyAlert.textInfo;
        for (int i = 0; i < info.characterCount; i++)
        {
            TMP_CharacterInfo character = info.characterInfo[i];
            if (!character.isVisible) continue;
            Color32[] colors = info.meshInfo[character.materialReferenceIndex].colors32;
            byte alpha = (byte)Mathf.RoundToInt(255f *
                (character.lineNumber == 0 ? pulse : 1f));
            for (int corner = 0; corner < 4; corner++)
                colors[character.vertexIndex + corner].a = alpha;
        }
        energyAlert.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
    }

    private void OnDisable()
    {
        if (reactorAlert != null) reactorAlert.alpha = 1f;
        if (energyAlert != null) energyAlert.ForceMeshUpdate();
    }
}
