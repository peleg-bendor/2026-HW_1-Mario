using UnityEngine;

// Unlocks Mario's fireball. Looks up IUseableWeapon rather than FireballWeapon itself, so
// this knows only that something on Mario can be unlocked, not which weapon it is.
public class FireFlowerPowerUp : IPowerUp
{
    public void ApplyPowerUp(GameObject player)
    {
        if(player != null)
        {
            Debug.Log("FireFlowerPowerUp applied to " + player.name);
            IUseableWeapon useableWeapon = player.GetComponentInChildren<IUseableWeapon>();
            if(useableWeapon != null)
            {
                useableWeapon.Equip();
            }
        }
    }
}
