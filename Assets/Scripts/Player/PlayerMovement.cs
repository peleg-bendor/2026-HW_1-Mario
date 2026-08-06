using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    private float direction;
    public float speed = 5;

    // How hard Mario brakes once no movement key is held, in units per second squared.
    // Stopping distance works out to (speed * speed) / (2 * deceleration).
    [SerializeField] private float deceleration = 40f;

    private Rigidbody2D rigid;

    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();

        if (rigid == null)
            Debug.LogWarning("PlayerMovement: no Rigidbody2D found, Mario will not move");
    }

    void FixedUpdate()
    {
        direction = 0f;
        if (Keyboard.current != null)
        {
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
                direction = -1f;
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
                direction = 1f;
        }

        if (rigid == null)
            return;

        if (direction != 0)
        {
            rigid.linearVelocity = new Vector2(direction * speed, rigid.linearVelocity.y);

            if (direction > 0)
                transform.localScale = new Vector3(1, 1, 1);
            else transform.localScale = new Vector3(-1, 1, 1);
        }
        else
        {
            // Brake to an actual stop. Rigidbody2D's Linear Damping decays velocity exponentially,
            // so it approaches zero without ever arriving and Mario keeps drifting - that drift is
            // the "slippery" feel. MoveTowards is linear, so it reaches zero and stays there.
            // Applied in the air too, deliberately: keeping it grounded-only would mean a per-frame
            // ground check here, which is the flakiness Stage 6 spent three revisions on.
            float brakedX = Mathf.MoveTowards(rigid.linearVelocity.x, 0f, deceleration * Time.fixedDeltaTime);
            rigid.linearVelocity = new Vector2(brakedX, rigid.linearVelocity.y);
        }
    }
}
