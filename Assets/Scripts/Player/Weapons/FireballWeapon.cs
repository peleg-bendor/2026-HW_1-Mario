using UnityEngine;

public class FireballWeapon : MonoBehaviour,IUseableWeapon
{
    public GameObject projectile;
    private bool _isEquip = false;

    public void Attack()
    {
        if (projectile != null && _isEquip)
        {
            GameObject curProjectile = Instantiate(projectile, transform.position, Quaternion.identity);
            ProjectileFireball scProjectile =  curProjectile.GetComponent<ProjectileFireball>();
            if(scProjectile != null)
            {
                float direction = 1;
                if(transform.parent != null)
                    direction = transform.parent.localScale.x;
                scProjectile.Attack(direction);
            }
            Debug.Log("Fireball shot");
        }
        else
        {
            Debug.Log("Fireball attack ignored - not equipped");
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
