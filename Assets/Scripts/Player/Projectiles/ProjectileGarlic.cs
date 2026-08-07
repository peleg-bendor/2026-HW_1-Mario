using UnityEngine;

// One garlic thrown by an enemy. Whether hitting Mario costs him a strike is not decided here -
// that comes from the SC_Death attached alongside this script.
public class ProjectileGarlic : MonoBehaviour
{
    public float speed = 5f;
    public float lifetime = 3f;

    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Attack(float direction)
    {
        if (rb != null)
        {
            transform.localScale = new Vector3(direction, 1, 1);
            // Impulse, not Force: a one-shot push has to be applied as one, or only a single
            // physics step's worth of it ever becomes velocity.
            rb.AddForce(new Vector2(direction * speed, 0), ForceMode2D.Impulse);
            Destroy(gameObject, lifetime);
        }
    }

    // No IEnemy check here, deliberately, unlike ProjectileFireball. The garlic isn't a weapon
    // against enemies, and checking for them would kill the enemy that just threw it, since it
    // spawns beside that enemy's own collider.
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.tag == "Player")
        {
            Debug.Log("Garlic hit Mario");
            Destroy(gameObject);
            return;
        }

        // Same allowlist the fireball uses - SC_Floor is what marks a tile as a tile.
        if (other.GetComponent<SC_Floor>() != null)
        {
            Debug.Log("Garlic hit a wall");
            Destroy(gameObject);
        }
    }
}
