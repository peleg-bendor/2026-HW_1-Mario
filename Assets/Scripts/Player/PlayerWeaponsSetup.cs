using UnityEngine;

// Registers Mario's weapons with WeaponsHandler on level start. Kept separate from
// WeaponsHandler itself: this decides which weapons exist and is wired in the Inspector,
// while the handler only selects among whatever it was given and dispatches attacks.
public class PlayerWeaponsSetup : MonoBehaviour
{
    public WeaponsHandler weaponsHandler;

    public FireballWeapon fireballWeapon;

    public AxeWeapon axeWeapon;

    void Start()
    {
        if (weaponsHandler != null)
        {
            // Registration order is selection order - the axe is index 0, so Mario starts
            // holding the axe rather than the fireball he can't use yet.
            if (axeWeapon != null)
                weaponsHandler.AddWeapon(axeWeapon);
            if (fireballWeapon != null)
                weaponsHandler.AddWeapon(fireballWeapon);
        }
    }
}
