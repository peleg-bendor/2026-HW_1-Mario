using UnityEngine;

public class Gateway : MonoBehaviour
{
    // The portal is a child object, off until the key's collected. Gateway's whole job is
    // reacting to that one event - it doesn't know or care what happens once the portal is on.
    [SerializeField] private GameObject portal;

    private void OnEnable()
    {
        KeyPickupController.OnKeyCollected += OnKeyCollected;
    }

    private void OnDisable()
    {
        KeyPickupController.OnKeyCollected -= OnKeyCollected;
    }

    private void OnKeyCollected()
    {
        if (portal == null)
        {
            Debug.LogWarning("Gateway has no portal assigned - nothing to activate.");
            return;
        }

        Debug.Log("Gateway lit up - key collected");
        portal.SetActive(true);
    }
}
