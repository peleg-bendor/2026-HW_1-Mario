using UnityEngine;

public class FireballWeapon : MonoBehaviour,IUseableWeapon
{
    public GameObject projectile;
    private bool _isEquip = false;

    public void Attack()
    {
        if (projectile != null && _isEquip)
        {
            GameObject curProjectile = Instantiate(projectile, transform.position, new Quaternion(0, 0, 0, 0));
            ProjectileFireball scProjectile =  curProjectile.GetComponent<ProjectileFireball>();
            if(scProjectile != null)
            {
                float direction = 1;
                if(transform.parent != null)
                    direction = transform.parent.localScale.x;
                scProjectile.Attack(direction);
            }
        }
    }

    public void Equip()
    {
        _isEquip = true;
    }

    public void UnEquip()
    {
        _isEquip = false;
    }
}
