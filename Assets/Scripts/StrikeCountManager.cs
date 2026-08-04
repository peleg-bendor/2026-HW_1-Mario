using TMPro;
using UnityEngine;

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
