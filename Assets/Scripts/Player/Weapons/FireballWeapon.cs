using UnityEngine;

// Mario's fireball: unlimited, but locked until the Fire Flower unlocks it. The lock is the
// whole reason this implements IUseableWeapon while the axe implements IReloadWeapon instead.
public class FireballWeapon : MonoBehaviour, IUseableWeapon
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
                // Mario's facing lives on his sprite's scale, one level up from this weapon.
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

    // False until the Fire Flower is collected, which is what makes weapon cycling skip this
    // weapon instead of selecting one that would refuse to fire.
    public bool IsAvailable()
    {
        return _isEquip;
    }
}
