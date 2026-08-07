using UnityEngine;

// Keeps the camera centred on Mario, smoothed rather than snapping. Runs in LateUpdate so it
// reads his fully resolved position for the frame instead of trailing physics by a step.
public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float smoothTime = 0.15f;

    // Only x and y follow. Depth is read once at startup rather than hardcoded, so moving the
    // camera in the Editor doesn't silently leave a stale number in here.
    private float cameraZ;

    // SmoothDamp needs somewhere to keep its working velocity between calls. Nothing else
    // reads it.
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

        // Mario's world position already accounts for whatever his parent objects are offset
        // by, so following it needs no correction for how the level is nested.
        Vector3 desiredPosition = new Vector3(target.position.x, target.position.y, cameraZ);
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref followVelocity, smoothTime);
    }
}
