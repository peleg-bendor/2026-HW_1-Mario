using UnityEngine;

public class LaserWeapon  : MonoBehaviour,IWeapon
{
    public GameObject projectile;

    public void Attack()
    {
        Debug.Log("Shoot Laser");
    }
}

