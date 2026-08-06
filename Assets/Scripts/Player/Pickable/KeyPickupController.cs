using UnityEngine;

public class KeyPickupController : MonoBehaviour
{
    // Modeled on SC_Coin/SC_Death rather than the IPowerUp pickups (AxePickupController,
    // StrikePickupController) - the key isn't a power-up, so there's no ApplyPowerUp() to
    // call. It just announces itself directly, the same way SC_Coin announces a coin pickup.
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
