using TMPro;
using UnityEngine;

// Shows the game-over or game-won message for a moment after the level restarts. Runs its own
// countdown and knows nothing about how the game ended, only what it was asked to display.
public class GameEndMessageManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private float displayDuration = 1f;

    private float remaining;

    private void Start()
    {
        // Nothing pending means this is an ordinary level start rather than a restart.
        if (!GameEndManager.TryConsumePendingMessage(out string message, out Color color))
            return;

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
