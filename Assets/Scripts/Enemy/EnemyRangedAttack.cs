using UnityEngine;

public class EnemyRangedAttack : MonoBehaviour
{
    private enum Direction { Left, Right }

    [SerializeField] private GameObject projectilePrefab;

    // Two fire points, not one - a projectile thrown right has to spawn on the enemy's right
    // side, or it would spawn on the left and immediately fly back through the enemy's own
    // solid collider on its way past. Both need the same clearance: far enough outside the
    // enemy's own collider that the projectile's own collider doesn't overlap it at spawn.
    [SerializeField] private Transform leftFirePoint;
    [SerializeField] private Transform rightFirePoint;

    [SerializeField] private float fireInterval = 3f;

    // Which way the very first shot goes. Every shot after that alternates automatically -
    // left, right, left, right... see Shoot().
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

        // Flip for next time.
        directionValue = -directionValue;
    }
}
