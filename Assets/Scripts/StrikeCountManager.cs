using TMPro;
using UnityEngine;

// Draws how many strikes Mario has left. Owns no count of its own - it listens for
// StrikesManager saying the number changed, so the count lives in exactly one place.
public class StrikeCountManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI strikesText;

    private void OnEnable()
    {
        StrikesManager.OnStrikeCountChanged += OnStrikeCountChanged;
    }

    private void OnDisable()
    {
        StrikesManager.OnStrikeCountChanged -= OnStrikeCountChanged;
    }

    private void OnStrikeCountChanged(int strikeCount)
    {
        if (strikesText != null)
            strikesText.text = "Strikes: " + strikeCount;
    }
}
