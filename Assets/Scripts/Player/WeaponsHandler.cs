using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

// Owns which weapon is selected and dispatches attacks to it. Holds whatever it was given as
// IWeapon and never learns the concrete types, so adding a weapon means registering one more
// rather than editing anything in here.
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

        // The first weapon registered is the one Mario starts holding, so the GUI needs telling
        // about it even though nobody pressed anything.
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

        // Walks forward from the current selection and stops at the first weapon that says it
        // can be used, so a weapon Mario hasn't unlocked yet gets skipped rather than selected
        // and then silently refusing to fire.
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
