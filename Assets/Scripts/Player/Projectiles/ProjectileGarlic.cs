using UnityEngine;

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
            rb.AddForce(new Vector2(direction * speed, 0), ForceMode2D.Impulse);
            Destroy(gameObject, lifetime);
        }
    }

    // Deliberately no IEnemy check here, unlike ProjectileFireball - the garlic doesn't kill
    // enemies, and checking IEnemy would have it kill the vampire that just fired it, since it
    // spawns overlapping the vampire's own collider. See HW-1_PLAN.md's Stage 7 decisions.
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.tag == "Player")
        {
            Debug.Log("Garlic hit Mario");
            Destroy(gameObject);
            return;
        }

        // SC_Floor marks an actual tile - the only other thing the garlic stops for.
        // Everything else, including enemies, it passes straight through - it isn't a weapon
        // against them, unlike ProjectileFireball.
        if (other.GetComponent<SC_Floor>() != null)
        {
            Debug.Log("Garlic hit a wall");
            Destroy(gameObject);
        }
    }
}
