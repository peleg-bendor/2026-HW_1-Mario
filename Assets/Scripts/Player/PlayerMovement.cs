using UnityEngine;
using UnityEngine.InputSystem;

// Drives Mario's horizontal movement and which way his sprite faces. Jumping is PlayerJump's
// job: the two are split because they answer to different keys, and only one of them carries
// state from one frame to the next.
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
            // Brake to a real stop. Rigidbody2D's Linear Damping decays velocity exponentially,
            // so it approaches zero without arriving and Mario keeps drifting - that drift is
            // what "slippery" meant. MoveTowards is linear, so it reaches zero and stays there.
            // Braking in the air as well is what keeps a ground check out of this class.
            float brakedX = Mathf.MoveTowards(rigid.linearVelocity.x, 0f, deceleration * Time.fixedDeltaTime);
            rigid.linearVelocity = new Vector2(brakedX, rigid.linearVelocity.y);
        }
    }
}
