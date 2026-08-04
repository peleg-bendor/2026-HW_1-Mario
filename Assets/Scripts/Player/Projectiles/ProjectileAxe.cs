using UnityEngine;

public class ProjectileAxe : MonoBehaviour
{
    public float speedX = 5f;
    public float speedY = 5f;
    public float lifetime = 10f;
    [SerializeField] private float warningDuration = 3f;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Color baseColor;
    private float age = 0f;
    private bool hasLanded = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
            baseColor = spriteRenderer.color;
    }

    void Update()
    {
        age += Time.deltaTime;

        float warningStart = lifetime - warningDuration;
        if (spriteRenderer != null && age >= warningStart)
        {
            float fadeProgress = Mathf.Clamp01((age - warningStart) / warningDuration);
            float alpha = Mathf.Lerp(baseColor.a, 0f, fadeProgress);
            spriteRenderer.color = new Color(baseColor.r, baseColor.g, baseColor.b, alpha);
        }

        if (age >= lifetime)
        {
            Debug.Log("Axe despawned");
            Destroy(gameObject);
        }
    }

    public void Attack(float direction)
    {
        if(rb != null)
        {
            transform.localScale = new Vector3(direction, 1, 1);
            rb.AddForce(new Vector2(direction * speedX, speedY));
        }
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        Debug.Log("OnCollisionEnter2D " + col.gameObject.name);

        if (!hasLanded)
        {
            if (col.gameObject.tag != "Player")
            {
                Debug.Log("Axe landed");
                hasLanded = true;
                rb.linearVelocity = Vector2.zero;
                rb.constraints = RigidbodyConstraints2D.FreezeAll;
            }
            return;
        }

        if (col.gameObject.tag == "Player")
        {
            Debug.Log("Axe picked back up");
            PlayerPowerUp playerPowerUp = col.gameObject.GetComponent<PlayerPowerUp>();
            if (playerPowerUp != null)
                playerPowerUp.CollectPowerUp(new AxePowerUp());
            Destroy(gameObject);
        }
    }
}
