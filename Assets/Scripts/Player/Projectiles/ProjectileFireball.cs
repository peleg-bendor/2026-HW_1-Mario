using UnityEngine;

// One fireball in flight. Kills the first enemy it passes through and stops at the first wall,
// then destroys itself either way.
public class ProjectileFireball : MonoBehaviour
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
        if(rb != null)
        {
            transform.localScale = new Vector3(direction, 1, 1);
            // Impulse, not Force: a one-shot push has to be applied as one, or only a single
            // physics step's worth of it ever becomes velocity.
            rb.AddForce(new Vector2(direction * speed, 0), ForceMode2D.Impulse);
            Destroy(gameObject, lifetime);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        IEnemy enemy = other.GetComponent<IEnemy>();
        if (enemy != null)
        {
            enemy.Kill();
            Destroy(gameObject);
            return;
        }

        // An allowlist rather than "stop at anything unrecognised": SC_Floor is what marks a
        // tile as a tile, so coins, pickups and Mario himself are flown through untouched.
        if (other.GetComponent<SC_Floor>() != null)
        {
            Debug.Log("Fireball hit a wall");
            Destroy(gameObject);
        }
    }
}
