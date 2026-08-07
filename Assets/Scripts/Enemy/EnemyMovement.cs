using System.Collections.Generic;
using UnityEngine;

// Patrols an enemy back and forth using the level's own geometry - it turns at whatever wall
// it meets and falls off whatever edge it reaches, rather than following a set distance or
// waypoints. Nothing in here is specific to the ghost currently using it.
public class EnemyMovement : MonoBehaviour
{
    private enum Direction { Left, Right }

    [SerializeField] private Direction startingFacing = Direction.Left;

    // Which way this enemy's artwork already points before any mirroring. Mario's sprite faces
    // right, the ghost's faces left, so the same facing direction has to mirror them opposite
    // ways.
    [SerializeField] private Direction spriteNativeFacing = Direction.Right;

    [SerializeField] private float speed = 2f;

    // How far past the collider's own edge to look for a wall ahead.
    [SerializeField] private float wallCheckBuffer = 0.1f;

    // How long ground contact has to stay missing before the enemy counts as airborne. The
    // floor is many separate tile colliders, and a round collider crossing a seam loses contact
    // for a physics step or two while the solver hands it from one tile to the next, so a
    // single frame's answer isn't trustworthy. A real fall lasts far longer than a seam does.
    [SerializeField] private float groundLossGrace = 0.15f;

    // A floor contact pushes back along Y and a wall contact along X, so this only has to tell
    // the two apart. The cutoff sits at 60 degrees off vertical, which keeps a contact with a
    // tile's corner on the "standing on it" side where it belongs.
    private const float MinVerticalContactNormal = 0.5f;

    // Contacts one enemy can have at once: the ground, maybe a wall, maybe Mario. Comfortably
    // more than needed, and GetContacts stops filling once it runs out of room.
    private const int MaxTrackedContacts = 8;

    private Rigidbody2D rb;
    private Collider2D col;
    private float facingDirection;

    private float timeSinceGroundContact = 0f;

    private bool wasGrounded;
    private bool groundStateKnown = false;

    private readonly ContactPoint2D[] contacts = new ContactPoint2D[MaxTrackedContacts];

    // The wall check starts inside this enemy's own collider, and this project has Physics2D's
    // "Queries Start In Colliders" enabled, so the nearest hit is always the enemy itself.
    // Gathering every hit along the ray and skipping its own collider is what gets past that.
    private ContactFilter2D castFilter;
    private readonly List<RaycastHit2D> castHits = new List<RaycastHit2D>();

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();

        castFilter = new ContactFilter2D();
        // Pickups are triggers, and a trigger is never something to turn around at.
        castFilter.useTriggers = false;

        facingDirection = startingFacing == Direction.Left ? -1f : 1f;
        UpdateSpriteFacing();
    }

    void FixedUpdate()
    {
        if (rb == null || col == null)
            return;

        if (HasGroundContact())
            timeSinceGroundContact = 0f;
        else
            timeSinceGroundContact += Time.fixedDeltaTime;

        bool grounded = timeSinceGroundContact < groundLossGrace;
        ReportGroundChange(grounded);

        if (grounded && IsWallAhead())
        {
            facingDirection = -facingDirection;
            UpdateSpriteFacing();
            Debug.Log("Enemy hit a wall - turned around");
        }

        // Velocity is only ever driven while grounded and left alone while airborne, so momentum
        // carries the enemy over an edge instead of pinning it there the moment it steps off.
        if (grounded)
            rb.linearVelocity = new Vector2(facingDirection * speed, rb.linearVelocity.y);
    }

    // Reads what the collider is genuinely touching rather than casting a ray downward. A ray
    // answers "is there ground below my centre", physics answers "is anything holding me up",
    // and the two disagree when a round collider rests on a tile's corner - too supported to
    // fall, too airborne to walk. This is the instant's answer; FixedUpdate smooths it.
    private bool HasGroundContact()
    {
        int contactCount = col.GetContacts(contacts);

        for (int i = 0; i < contactCount; i++)
        {
            Collider2D other = contacts[i].collider == col
                ? contacts[i].otherCollider
                : contacts[i].collider;

            if (other == null || other.isTrigger)
                continue;

            // Mario is not scenery to stand on. If he were, an enemy landing on him mid-fall
            // would decide it had touched down.
            if (other.CompareTag("Player"))
                continue;

            // Only the axis matters, not the sign, so this holds whichever way round Unity
            // happens to report the normal.
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

            // Mario is invisible here too. Counting as a wall would make the enemy turn around
            // just short of him and never land the touch that costs a strike.
            if (hit.collider.CompareTag("Player"))
                continue;

            return true;
        }

        return false;
    }

    private void ReportGroundChange(bool grounded)
    {
        // The first check establishes a baseline rather than reporting a transition - an enemy
        // placed in mid-air hasn't walked off anything, it just started there.
        if (!groundStateKnown)
        {
            groundStateKnown = true;
            wasGrounded = grounded;
            return;
        }

        if (grounded == wasGrounded)
            return;

        wasGrounded = grounded;

        // Named, because several enemies can be patrolling at once and the Console gives no
        // other way to tell which one this is.
        Debug.Log(grounded
            ? "Enemy landed - resuming patrol: " + gameObject.name
            : "Enemy left the ground - falling: " + gameObject.name);
    }

    private void UpdateSpriteFacing()
    {
        // localScale.x mirrors the sprite, but which sign counts as "facing right" depends on
        // which way the artwork already points, hence spriteNativeFacing.
        float nativeSign = spriteNativeFacing == Direction.Right ? 1f : -1f;
        transform.localScale = new Vector3(facingDirection * nativeSign, 1, 1);
    }
}
