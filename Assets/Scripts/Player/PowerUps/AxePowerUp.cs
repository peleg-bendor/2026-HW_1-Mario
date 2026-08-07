using UnityEngine;

// Grants Mario one more axe. Looks up IReloadWeapon rather than AxeWeapon itself, so this
// knows only that something on Mario can be given more ammo, not which weapon it is.
public class AxePowerUp : IPowerUp
{
    public void ApplyPowerUp(GameObject player)
    {
        if(player != null)
        {
            Debug.Log("AxePowerUp applied to " + player.name);
            IReloadWeapon reloadWeapon = player.GetComponentInChildren<IReloadWeapon>();
            if(reloadWeapon != null)
            {
                reloadWeapon.Reload();
            }
        }
    }
}
