using UnityEngine;

public class PlayerDeath : MonoBehaviour
{
     private Vector3 startPosition;

    private void OnEnable()
    {
        SC_Death.OnSpikeCollision += OnSpikeCollision;
    }

    private void OnDisable()
    {
        SC_Death.OnSpikeCollision -= OnSpikeCollision;
    }
    void Awake()
    {
        startPosition = transform.position;
    }

    private void OnSpikeCollision()
    {
        transform.position = startPosition;
    }
}
