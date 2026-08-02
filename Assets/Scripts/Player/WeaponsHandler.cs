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
        if(!weapons.Contains(weapon))
            weapons.Add(weapon);
    }

    void Update()
    {
        if (Keyboard.current == null)
            return;

        if(Keyboard.current.leftCtrlKey.wasPressedThisFrame && weapons != null && weapons.Count >= 0 && index < weapons.Count)
            weapons[index].Attack();
    }
}
