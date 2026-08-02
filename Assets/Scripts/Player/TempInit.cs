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
            if(fireballWeapon != null)
                weaponsHandler.AddWeapon(fireballWeapon);
            if(axeWeapon != null)
                weaponsHandler.AddWeapon(axeWeapon);
        }   
    }

    void Update()
    {
        if (Keyboard.current != null && axeWeapon != null)
        {
            if (Keyboard.current.qKey.wasPressedThisFrame)
                axeWeapon.Reload();
        }
    }
}
