using TMPro;
using UnityEngine;

// Draws which weapon is selected. Receives the name as a string rather than the weapon itself,
// so the GUI never holds a reference to anything in the weapon system.
public class SelectedWeaponManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI selectedWeaponText;

    private void OnEnable()
    {
        WeaponsHandler.OnWeaponSelected += OnWeaponSelected;
    }

    private void OnDisable()
    {
        WeaponsHandler.OnWeaponSelected -= OnWeaponSelected;
    }

    private void OnWeaponSelected(string weaponName)
    {
        if (selectedWeaponText != null)
            selectedWeaponText.text = "Selected weapon: " + weaponName;
    }
}
