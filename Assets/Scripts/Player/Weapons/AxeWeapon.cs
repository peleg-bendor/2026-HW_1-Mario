using UnityEngine;

public class AxeWeapon : MonoBehaviour,IReloadWeapon
{
    public static event System.Action<int> OnAxeCountChanged;

    public GameObject projectile;
    private int axesHeld = 1;

    void Start()
    {
        Debug.Log("Starting with " + axesHeld + " axe(s)");
        OnAxeCountChanged?.Invoke(axesHeld);
    }

    public void Attack()
    {
        if (projectile != null && axesHeld > 0)
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
            axesHeld--;
            Debug.Log("Axe thrown - " + axesHeld + " left");
            OnAxeCountChanged?.Invoke(axesHeld);
        }
        else
        {
            Debug.Log("Axe attack ignored - no axes held");
        }
    }

    public void Reload()
    {
        axesHeld++;
        Debug.Log("Axe gained - now holding " + axesHeld);
        OnAxeCountChanged?.Invoke(axesHeld);
    }

    public bool IsAvailable()
    {
        return true;
    }
}
