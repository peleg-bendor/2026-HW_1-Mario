using UnityEngine;

// The level's key. Unlike the other pickups it grants Mario nothing, so it has no IPowerUp -
// it just announces that it was collected, and whatever cares about that reacts on its own.
public class KeyPickupController : MonoBehaviour
{
    public static event System.Action OnKeyCollected;

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.tag == "Player")
        {
            Debug.Log("Key collected");
            OnKeyCollected?.Invoke();
            this.gameObject.SetActive(false);
        }
    }
}
