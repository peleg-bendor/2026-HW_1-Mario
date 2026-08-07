using TMPro;
using UnityEngine;

// Draws how many axes Mario is holding. Owns no count of its own - it listens for AxeWeapon
// saying the number changed, so the axe is the only place that number actually lives.
public class AxeCountManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI axeCountText;

    private void OnEnable()
    {
        AxeWeapon.OnAxeCountChanged += OnAxeCountChanged;
    }

    private void OnDisable()
    {
        AxeWeapon.OnAxeCountChanged -= OnAxeCountChanged;
    }

    private void OnAxeCountChanged(int axeCount)
    {
        if (axeCountText != null)
            axeCountText.text = "Axes: " + axeCount;
    }
}
