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

    // How far past the collider's own edge to check for ground/wall - a small buffer
    // beyond the collider's actual bounds, not the whole check distance.
    [SerializeField] private float groundCheckBuffer = 0.1f;
    [SerializeField] private float wallCheckBuffer = 0.1f;

    private Rigidbody2D rb;
    private Collider2D col;
    private float facingDirection;

    private bool wasGrounded;
    private bool groundStateKnown = false;

    // Both checks cast outward from the collider's own centre, so the cast begins inside this
    // enemy's collider. This project has Physics2D's "Queries Start In Colliders" enabled, which
    // makes the enemy's own collider the nearest hit every single time. Reading just the closest
    // hit would therefore always return the enemy itself and never see the floor, so we gather
    // every hit along the ray and look for the first one that is real terrain.
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
        // airborne rather than zeroing it. The ground ray starts missing the moment the enemy's
        // centre passes a tile edge, but at that point the collider is still resting on that
        // tile's corner - zeroing the velocity there pins it in place, supported enough not to
        // fall yet not grounded enough to walk. Keeping its existing momentum carries it over
        // the edge, the way something that walks off a ledge actually behaves.
        if (grounded)
            rb.linearVelocity = new Vector2(facingDirection * speed, rb.linearVelocity.y);
    }

    private bool IsGrounded()
    {
        return HasSolidWithin(Vector2.down, col.bounds.extents.y + groundCheckBuffer);
    }

    private bool IsWallAhead()
    {
        return HasSolidWithin(new Vector2(facingDirection, 0f), col.bounds.extents.x + wallCheckBuffer);
    }

    private bool HasSolidWithin(Vector2 direction, float distance)
    {
        Physics2D.Raycast(col.bounds.center, direction, castFilter, castHits, distance);

        foreach (RaycastHit2D hit in castHits)
        {
            if (hit.collider == null || hit.collider == col)
                continue;

            // Mario is deliberately invisible to these checks. If he counted as a wall, the enemy
            // would turn around just before reaching him and never land the touch that costs a
            // strike. Touching Mario is SC_Death's job, not this script's.
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
