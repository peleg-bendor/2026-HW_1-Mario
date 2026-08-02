using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SC_Floor : MonoBehaviour
{
    public delegate void FloorCollisionHandler();
    public static event FloorCollisionHandler OnFloorCollision;

    void OnCollisionEnter2D(Collision2D col)
    {
        Debug.Log("OnCollisionEnter2D " + col.gameObject.name);
        if (col.gameObject.tag == "Player")
        {
            float _playerY = col.gameObject.transform.position.y;
            float _tileY = transform.position.y;

            // Distinguishes "landed on top" from "bumped the side" - Collision2D alone doesn't
            // say which face was hit, so we check whether the player's center is above the tile's
            // center by at least the player collider's own half-height (reading it live off the
            // collider instead of hardcoding it, so this stays correct if Mario's collider ever
            // gets resized).
            float playerColliderHalfHeight = col.collider.bounds.extents.y;

            if (_playerY > _tileY + playerColliderHalfHeight)
            {
                Debug.Log("Mario landed on floor");
                if (OnFloorCollision != null)
                    OnFloorCollision();
            }
            else
            {
                Debug.Log("Mario touched floor tile from the side");
            }
        }
    }
}
