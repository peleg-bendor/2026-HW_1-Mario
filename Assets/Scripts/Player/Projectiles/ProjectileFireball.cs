using UnityEngine;

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

        // SC_Floor marks an actual tile - the only other thing a fireball stops for.
        // Everything else (coins, pickups, Mario himself) it flies straight through.
        if (other.GetComponent<SC_Floor>() != null)
        {
            Debug.Log("Fireball hit a wall");
            Destroy(gameObject);
        }
    }
}
