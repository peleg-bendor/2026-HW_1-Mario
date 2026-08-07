using UnityEngine;

// Puts Mario back where the level started him whenever a hazard hits him. Deliberately counts
// nothing: StrikesManager subscribes to the same event independently and owns whether that hit
// was the last one.
public class PlayerDeath : MonoBehaviour
{
    private Vector3 startPosition;

    private void OnEnable()
    {
        SC_Death.OnHazardCollision += OnHazardCollision;
    }

    private void OnDisable()
    {
        SC_Death.OnHazardCollision -= OnHazardCollision;
    }

    void Awake()
    {
        // Captured from wherever Mario actually sits at load rather than hardcoded, so moving
        // his starting position in the Editor moves the respawn point with it.
        startPosition = transform.position;
    }

    private void OnHazardCollision()
    {
        transform.position = startPosition;
        Debug.Log("Mario respawned at start position");
    }
}
