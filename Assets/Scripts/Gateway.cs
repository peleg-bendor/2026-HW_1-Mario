using UnityEngine;

// The doorway at the end of the level. Its whole job is switching on the portal once the key
// is collected; what the portal then does is the portal's business.
public class Gateway : MonoBehaviour
{
    // A child object, inactive until the key turns it on. The gateway itself has no collider,
    // so before then Mario walks straight through with no trigger firing at all - "nothing
    // happens without the key" is the object being off, not a condition in code.
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
