using UnityEngine;

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
