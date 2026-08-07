using UnityEngine;

// Fires a projectile on a repeating timer, alternating sides. Named for the behaviour rather
// than the enemy using it, so it carries nothing specific to the vampire it currently sits on.
public class EnemyRangedAttack : MonoBehaviour
{
    private enum Direction { Left, Right }

    [SerializeField] private GameObject projectilePrefab;

    // Two fire points, not one: a projectile thrown right has to spawn on the enemy's right,
    // or it flies back through the enemy's own collider on its way past. Both need enough
    // clearance that the projectile's collider doesn't overlap the enemy's at spawn.
    [SerializeField] private Transform leftFirePoint;
    [SerializeField] private Transform rightFirePoint;

    [SerializeField] private float fireInterval = 3f;

    // Which way the first shot goes. Every shot after it alternates - see Shoot().
    [SerializeField] private Direction startingDirection = Direction.Left;

    private float directionValue;
    private float timeSinceLastShot = 0f;

    void Awake()
    {
        directionValue = startingDirection == Direction.Left ? -1f : 1f;
    }

    void Update()
    {
        timeSinceLastShot += Time.deltaTime;
        if (timeSinceLastShot >= fireInterval)
        {
            Shoot();
            timeSinceLastShot = 0f;
        }
    }

    private void Shoot()
    {
        if (projectilePrefab == null)
        {
            Debug.Log("Enemy ranged attack ignored - no projectile prefab assigned: " + gameObject.name);
            return;
        }

        Transform firePoint = directionValue < 0 ? leftFirePoint : rightFirePoint;
        Vector3 spawnPosition = firePoint != null ? firePoint.position : transform.position;
        GameObject projectileObject = Instantiate(projectilePrefab, spawnPosition, Quaternion.identity);

        ProjectileGarlic garlic = projectileObject.GetComponent<ProjectileGarlic>();
        if (garlic != null)
            garlic.Attack(directionValue);

        Debug.Log("Enemy fired a projectile: " + gameObject.name + " (" + (directionValue < 0 ? "left" : "right") + ")");

        directionValue = -directionValue;
    }
}
