using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class WeaponsHandler : MonoBehaviour
{
    private List<IWeapon> weapons = new List<IWeapon>();    
    public int index = 0;

    public void Awake()
    {
        weapons = new List<IWeapon>();
    }

    public void AddWeapon(IWeapon weapon)
    {
        if (weapon == null)
            return;

        if(!weapons.Contains(weapon))
        {
            weapons.Add(weapon);
            Debug.Log("Weapon registered: " + weapon.GetType().Name);
        }
    }

    void Update()
    {
        if (Keyboard.current == null)
            return;

        if(Keyboard.current.leftCtrlKey.wasPressedThisFrame && weapons != null && index < weapons.Count)
            weapons[index].Attack();
    }
}
