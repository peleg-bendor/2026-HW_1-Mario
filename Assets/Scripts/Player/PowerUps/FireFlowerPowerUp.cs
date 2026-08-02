using UnityEngine;

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
