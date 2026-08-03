using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class WeaponsHandler : MonoBehaviour
{
    public static event System.Action<string> OnWeaponSelected;

    private readonly List<IWeapon> weapons = new List<IWeapon>();
    private int selectedIndex = 0;

    public void AddWeapon(IWeapon weapon)
    {
        if (weapon == null || weapons.Contains(weapon))
            return;

        weapons.Add(weapon);
        Debug.Log("Weapon registered: " + weapon.GetType().Name);

        if (weapons.Count == 1)
            NotifySelection();
    }

    void Update()
    {
        if (Keyboard.current == null)
            return;

        if (Keyboard.current.qKey.wasPressedThisFrame)
            SelectNextAvailableWeapon();

        if (Keyboard.current.leftCtrlKey.wasPressedThisFrame && selectedIndex < weapons.Count)
            weapons[selectedIndex].Attack();
    }

    private void SelectNextAvailableWeapon()
    {
        if (weapons.Count < 2)
            return;

        for (int step = 1; step < weapons.Count; step++)
        {
            int candidateIndex = (selectedIndex + step) % weapons.Count;
            if (weapons[candidateIndex].IsAvailable())
            {
                selectedIndex = candidateIndex;
                NotifySelection();
                return;
            }
        }

        Debug.Log("Weapon switch ignored - no other weapon available yet");
    }

    private void NotifySelection()
    {
        string weaponName = weapons[selectedIndex].GetType().Name.Replace("Weapon", "");
        Debug.Log("Weapon selected: " + weaponName);
        OnWeaponSelected?.Invoke(weaponName);
    }
}
