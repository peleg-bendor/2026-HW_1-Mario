using UnityEngine;
using UnityEngine.InputSystem;

public class TempInit : MonoBehaviour
{
    public WeaponsHandler weaponsHandler;

    public FireballWeapon fireballWeapon;

    public AxeWeapon axeWeapon;

    void Start()
    {
        if(weaponsHandler != null)
        {
            if (axeWeapon != null)
                weaponsHandler.AddWeapon(axeWeapon);
            if (fireballWeapon != null)
                weaponsHandler.AddWeapon(fireballWeapon);
        }   
    }
}
