using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerJump  : MonoBehaviour
{
      public float jumpSpeed = 100;
      private bool isJumping = false;

      private Rigidbody2D rigid; 
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

    private void Jump()
    {
        if (isJumping == false)
        {
            rigid.AddForce(new Vector2(0, jumpSpeed), ForceMode2D.Impulse);
            isJumping = true;
            Debug.Log("Mario jumped");
        }
    }

}
