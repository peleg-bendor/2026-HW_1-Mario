using UnityEngine;

// One thrown axe, from the throw until it either kills something, is picked back up, or fades
// out. What collecting it grants lives in AxePowerUp, so all that is left here is this one
// axe's physical life.
public class ProjectileAxe : MonoBehaviour
{
    public float speedX = 5f;
    public float speedY = 5f;
    public float lifetime = 10f;

    // How long before the end of its life the axe starts fading, as a warning that walking
    // over to reclaim it is about to stop being an option.
    [SerializeField] private float warningDuration = 3f;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Color baseColor;
    private float age = 0f;

    // Until the axe has hit something that isn't Mario, touching Mario is ignored. Without
    // this, an axe spawning at his position counts as instantly reclaimed and vanishes.
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

        // Fading alpha rather than tinting the sprite: SpriteRenderer.color is a multiplicative
        // tint already sitting at white, so lightening it further does nothing visible.
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
        if (!hasLanded)
        {
            // Checked before the landing branch below, because an enemy is something a thrown
            // axe destroys in flight rather than a surface it comes to rest on. Otherwise the
            // "anything that isn't Mario is the floor" rule underneath would claim it first.
            IEnemy enemy = col.gameObject.GetComponent<IEnemy>();
            if (enemy != null)
            {
                enemy.Kill();
                Destroy(gameObject);
                return;
            }

            if (col.gameObject.tag != "Player")
            {
                // Freezing every constraint is what makes a landed axe rest in place. It also
                // makes it a solid obstacle, which is why a patrolling enemy turns around at one.
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
