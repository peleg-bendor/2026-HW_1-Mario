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
        isJumping = false;
    }

    private void Jump()
    {
        if (isJumping == false)
        {
            rigid.AddForce(new Vector2(0, jumpSpeed));
            isJumping = true;
        }
    }

}
