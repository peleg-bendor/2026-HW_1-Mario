using TMPro;
using UnityEngine;

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
