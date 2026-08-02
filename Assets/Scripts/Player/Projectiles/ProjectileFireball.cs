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
            rb.AddForce(new Vector2(direction * speed, 0));
            Destroy(gameObject, lifetime);
        }
    }
}
