using UnityEngine;

public class Portal : MonoBehaviour
{
    // Raised the moment Mario reaches the portal. GameEndManager is the only subscriber,
    // and it doesn't reload the scene until its own display timer runs out - so unlike the
    // old deferred-to-Update() flag this replaced, firing this inline is safe. It's also
    // what makes the win-beats-death tie explicitly decidable now instead of a coin flip:
    // GameEndManager gives this event priority over StrikesManager.OnGameOver if both
    // arrive the same frame.
    public static event System.Action OnGameWon;

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.tag == "Player")
        {
            Debug.Log("Mario reached the portal");
            OnGameWon?.Invoke();
        }
    }
}
