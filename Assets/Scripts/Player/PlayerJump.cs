using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerJump : MonoBehaviour
{
    public float jumpSpeed = 100;

    // How far below Mario's feet to look for a floor tile.
    [SerializeField] private float groundProbeDepth = 0.2f;

    // Keeps the probe inside Mario's own silhouette. A full-width box would touch a wall tile he
    // is pressed flat against while falling, and read that as ground.
    private const float GroundProbeWidthFactor = 0.9f;

    private bool isJumping = false;

    private Rigidbody2D rigid;
    private Collider2D bodyCollider;

    private void OnEnable()
    {
        SC_Floor.OnFloorCollision += OnFloorCollision;
    }

    private void OnDisable()
    {
        SC_Floor.OnFloorCollision -= OnFloorCollision;
    }

    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        bodyCollider = GetComponent<Collider2D>();

        if (rigid == null)
            Debug.LogWarning("PlayerJump: no Rigidbody2D found, jumping will do nothing");

        if (bodyCollider == null)
            Debug.LogWarning("PlayerJump: no Collider2D found, the ground check will always fail");
    }

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
            Jump();
    }

    private void OnFloorCollision()
    {
        // Only a real state transition (was jumping, now grounded) is worth logging - SC_Floor
        // calls this on every tile Mario walks over, and isJumping is already false for those.
        if (isJumping)
        {
            Debug.Log("Mario landed on floor");
            isJumping = false;
        }
    }

    // Sampled once, at the moment Space is pressed, rather than tracked every frame on purpose.
    // This floor is ~90 separate 1x1 tile colliders, and per-frame grounded tracking on it was
    // flaky enough in EnemyMovement to need a grace period. A single sample carries no state
    // that can go wrong: a momentarily wrong reading costs one jump input and nothing else.
    private bool IsGrounded()
    {
        if (bodyCollider == null)
            return false;

        Bounds body = bodyCollider.bounds;

        // A box spanning Mario's whole footprint rather than a small circle under his centre.
        // Standing on a platform's last tile leaves his centre hanging past the tile edge while
        // his round collider is still perched on the corner, and a centre-only probe sees nothing
        // there. Everything is read live off the collider so no size is duplicated in this file.
        Vector2 probeSize = new Vector2(body.size.x * GroundProbeWidthFactor, groundProbeDepth);
        Vector2 probeCenter = new Vector2(body.center.x, body.min.y - groundProbeDepth * 0.5f);

        Collider2D[] hits = Physics2D.OverlapBoxAll(probeCenter, probeSize, 0f);

        foreach (Collider2D hit in hits)
        {
            // Only floor tiles count as ground. SC_Floor is what marks a tile as a tile (the same
            // allowlist the projectiles use for wall detection), which excludes Mario's own
            // collider, pickups, a landed axe and an enemy's head without naming any of them.
            if (hit != null && hit.GetComponent<SC_Floor>() != null)
                return true;
        }

        return false;
    }

    private void Jump()
    {
        // Two separate conditions doing two different jobs. isJumping blocks a second jump during
        // an ascent that hasn't landed yet (the probe still finds the tile for a few frames after
        // take-off). IsGrounded() blocks jumping out of a fall Mario never jumped into - walking
        // off a ledge leaves isJumping false the whole way down.
        if (isJumping)
        {
            Debug.Log("Jump ignored - Mario has not landed from his last jump yet");
            return;
        }

        if (IsGrounded() == false)
        {
            Debug.Log("Jump ignored - Mario is not on the ground");
            return;
        }

        if (rigid == null)
            return;

        rigid.AddForce(new Vector2(0, jumpSpeed), ForceMode2D.Impulse);
        isJumping = true;
        Debug.Log("Mario jumped");
    }
}
