using UnityEngine;

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
        startPosition = transform.position;
    }

    private void OnHazardCollision()
    {
        transform.position = startPosition;
        Debug.Log("Mario respawned at start position");
    }
}
