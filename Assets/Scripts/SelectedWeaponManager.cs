using TMPro;
using UnityEngine;

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
