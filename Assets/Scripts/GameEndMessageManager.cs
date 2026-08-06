using TMPro;
using UnityEngine;

public class GameEndMessageManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private float displayDuration = 1f;

    private float remaining;

    private void Start()
    {
        if (!GameEndManager.TryConsumePendingMessage(out string message, out Color color))
            return; // normal level start - nothing to show

        remaining = displayDuration;

        if (messageText != null)
        {
            messageText.text = message;
            messageText.color = color;
        }
    }

    private void Update()
    {
        if (remaining <= 0f)
            return;

        remaining -= Time.deltaTime;
        if (remaining <= 0f && messageText != null)
            messageText.text = "";
    }
}
