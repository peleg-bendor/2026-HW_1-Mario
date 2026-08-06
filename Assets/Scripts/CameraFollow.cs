using UnityEngine;

/// Keeps the camera centered on a target (Mario) on both axes, smoothed rather than
/// snapping straight to his position every frame. Runs in LateUpdate deliberately - Mario's
/// own position is driven by physics in FixedUpdate, so following in LateUpdate guarantees
/// the camera reads his fully-resolved position for the frame instead of trailing it by one
/// physics step.
public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float smoothTime = 0.15f;

    // Only x/y ever follow the target - z is read once at startup so the camera's own
    // depth is preserved instead of a hardcoded -10 that would silently go stale if the
    // camera's starting position ever changes.
    private float cameraZ;

    // SmoothDamp needs a persistent velocity reference between calls - not read anywhere
    // else, it's just the state the smoothing math carries frame to frame.
    private Vector3 followVelocity;

    private void Awake()
    {
        cameraZ = transform.position.z;

        if (target == null)
            Debug.LogWarning("CameraFollow has no target assigned - camera will not follow.");
    }

    private void LateUpdate()
    {
        if (target == null)
            return;

        Vector3 desiredPosition = new Vector3(target.position.x, target.position.y, cameraZ);
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref followVelocity, smoothTime);
    }
}
