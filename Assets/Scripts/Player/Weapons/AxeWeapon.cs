using UnityEngine;

public class AxeWeapon : MonoBehaviour,IReloadWeapon
{
    public GameObject projectile;
    private bool _loaded = false;


    public void Attack()
    {
        if (projectile != null && _loaded)
        {
            GameObject curProjectile = Instantiate(projectile, transform.position, Quaternion.identity);
            ProjectileAxe scProjectile =  curProjectile.GetComponent<ProjectileAxe>();
            if(scProjectile != null)
            {
                float direction = 1;
                if(transform.parent != null)
                    direction = transform.parent.localScale.x;
                scProjectile.Attack(direction);
            }
            _loaded = false;
            Debug.Log("Axe thrown");
        }
        else
        {
            Debug.Log("Axe attack ignored - not loaded");
        }
    }

    public void Reload()
    {
        Debug.Log("Reloading Axe"); 
        _loaded = true;
    }
}