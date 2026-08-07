using UnityEngine;

// Marks a GameObject as a floor tile. Doubles as the allowlist the rest of the project uses to
// tell real terrain from everything else, which is how projectiles and the jump check exclude
// pickups, a landed axe and an enemy's head without naming any of them.
public class SC_Floor : MonoBehaviour
{
    public delegate void FloorCollisionHandler();
    public static event FloorCollisionHandler OnFloorCollision;

    void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.tag == "Player")
        {
            float _playerY = col.gameObject.transform.position.y;
            float _tileY = transform.position.y;

            // Collision2D doesn't say which face was hit, so landing on top is told from bumping
            // the side by whether Mario's centre clears the tile's by his own collider's half
            // height - read live off that collider so it survives him ever being resized.
            float playerColliderHalfHeight = col.collider.bounds.extents.y;

            if (_playerY > _tileY + playerColliderHalfHeight)
            {
                // Raised for every tile Mario steps onto, not only real landings, since each
                // tile is its own collider. Whether it counts as a landing is PlayerJump's call,
                // because that is what tracks jump state.
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
