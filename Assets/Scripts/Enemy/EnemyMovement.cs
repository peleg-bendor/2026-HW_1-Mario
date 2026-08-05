using System.Collections.Generic;
using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    private enum Direction { Left, Right }

    [SerializeField] private Direction startingFacing = Direction.Left;

    // Which way this enemy's artwork already points before any mirroring is applied. Mario's
    // sprite is drawn facing right, but the ghost's is drawn facing left, so the same facing
    // direction has to mirror the two of them opposite ways.
    [SerializeField] private Direction spriteNativeFacing = Direction.Right;

    [SerializeField] private float speed = 2f;

    // How far past the collider's own edge to look for a wall ahead.
    [SerializeField] private float wallCheckBuffer = 0.1f;

    // A floor contact pushes back along Y, a wall contact along X, so this only has to tell the
    // two apart. 0.5 puts the cutoff at 60 degrees off vertical, which keeps corner contacts on
    // the "standing on it" side where they belong.
    private const float MinVerticalContactNormal = 0.5f;

    // Contacts one enemy can have at once: the ground, maybe a wall, maybe Mario. Comfortably
    // more than needed, and GetContacts just stops filling once it runs out of room.
    private const int MaxTrackedContacts = 8;

    private Rigidbody2D rb;
    private Collider2D col;
    private float facingDirection;

    private bool wasGrounded;
    private bool groundStateKnown = false;

    private readonly ContactPoint2D[] contacts = new ContactPoint2D[MaxTrackedContacts];

    // The wall check casts outward from the collider's own centre, so the cast begins inside this
    // enemy's collider. This project has Physics2D's "Queries Start In Colliders" enabled, which
    // makes the enemy's own collider the nearest hit every single time. Reading just the closest
    // hit would therefore always return the enemy itself, so we gather every hit along the ray
    // and look for the first one that is real terrain.
    private ContactFilter2D castFilter;
    private readonly List<RaycastHit2D> castHits = new List<RaycastHit2D>();

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();

        castFilter = new ContactFilter2D();
        castFilter.useTriggers = false;  // pickups (coins, axes, hearts) are not terrain

        facingDirection = startingFacing == Direction.Left ? -1f : 1f;
        UpdateSpriteFacing();
    }

    void FixedUpdate()
    {
        if (rb == null || col == null)
            return;

        bool grounded = IsGrounded();
        ReportGroundChange(grounded);

        if (grounded && IsWallAhead())
        {
            facingDirection = -facingDirection;
            UpdateSpriteFacing();
            Debug.Log("Enemy hit a wall - turned around");
        }

        // Only drive movement while grounded, and leave the horizontal velocity untouched while
        // airborne, so momentum carries the enemy over an edge instead of dropping it straight down.
        if (grounded)
            rb.linearVelocity = new Vector2(facingDirection * speed, rb.linearVelocity.y);
    }

    // Reads what the collider is genuinely touching rather than casting a ray downward. A ray
    // from the centre stops finding ground the moment the enemy's middle passes a tile edge, yet
    // the collider can still be resting on that tile's corner. An enemy that physics is holding
    // up while this script believes it is falling gets stuck: too supported to drop, too
    // "airborne" to walk. Asking physics directly means the two can never disagree.
    private bool IsGrounded()
    {
        int contactCount = col.GetContacts(contacts);

        for (int i = 0; i < contactCount; i++)
        {
            Collider2D other = contacts[i].collider == col
                ? contacts[i].otherCollider
                : contacts[i].collider;

            if (other == null || other.isTrigger)
                continue;

            // Mario is not scenery to stand on. If he were, an enemy he landed on mid-fall would
            // decide it had touched down.
            if (other.CompareTag("Player"))
                continue;

            // Only the axis matters here, not the sign, so this holds regardless of which way
            // round Unity reports the normal.
            if (Mathf.Abs(contacts[i].normal.y) > MinVerticalContactNormal)
                return true;
        }

        return false;
    }

    private bool IsWallAhead()
    {
        Vector2 origin = col.bounds.center;
        float distance = col.bounds.extents.x + wallCheckBuffer;
        Physics2D.Raycast(origin, new Vector2(facingDirection, 0f), castFilter, castHits, distance);

        foreach (RaycastHit2D hit in castHits)
        {
            if (hit.collider == null || hit.collider == col)
                continue;

            // Mario is deliberately invisible here too. If he counted as a wall, the enemy would
            // turn around just before reaching him and never land the touch that costs a strike.
            if (hit.collider.CompareTag("Player"))
                continue;

            return true;
        }

        return false;
    }

    private void ReportGroundChange(bool grounded)
    {
        // The very first check establishes a baseline rather than reporting a transition -
        // an enemy placed in mid-air hasn't "walked off" anything, it just started there.
        if (!groundStateKnown)
        {
            groundStateKnown = true;
            wasGrounded = grounded;
            return;
        }

        if (grounded == wasGrounded)
            return;

        wasGrounded = grounded;
        Debug.Log(grounded ? "Enemy landed - resuming patrol" : "Enemy left the ground - falling");
    }

    private void UpdateSpriteFacing()
    {
        // localScale.x mirrors the sprite, but which sign counts as "facing right" depends on
        // which way the artwork itself already points, hence spriteNativeFacing.
        float nativeSign = spriteNativeFacing == Direction.Right ? 1f : -1f;
        transform.localScale = new Vector3(facingDirection * nativeSign, 1, 1);
    }
}
