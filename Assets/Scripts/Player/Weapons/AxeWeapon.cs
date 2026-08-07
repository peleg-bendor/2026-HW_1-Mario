using UnityEngine;

// Mario's axe: a stockpile he can throw from and refill by collecting more. Owns the count
// and announces every change, so nothing else has to track how many axes he has.
public class AxeWeapon : MonoBehaviour, IReloadWeapon
{
    public static event System.Action<int> OnAxeCountChanged;

    public GameObject projectile;
    private int axesHeld = 1;

    void Start()
    {
        // Announced once at startup so a display reading this count doesn't need its own copy
        // of the starting value.
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
                // Mario's facing lives on his sprite's scale, one level up from this weapon.
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

    // Called only from the pickup path, so this means "gained an axe" rather than any kind of
    // manual reload action.
    public void Reload()
    {
        axesHeld++;
        Debug.Log("Axe gained - now holding " + axesHeld);
        OnAxeCountChanged?.Invoke(axesHeld);
    }

    // Always true: unlike the fireball, the axe is never locked, so it is always something
    // weapon cycling can land on.
    public bool IsAvailable()
    {
        return true;
    }
}
