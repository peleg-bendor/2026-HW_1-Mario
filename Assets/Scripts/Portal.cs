using UnityEngine;

// The way out, once the gateway has switched it on. It only notices Mario arriving and says
// so - it needs no key check of its own, because it cannot be touched before it exists.
public class Portal : MonoBehaviour
{
    // Given priority over StrikesManager.OnGameOver when both land on the same frame, so a win
    // reached on the frame Mario also died is settled by design rather than by whichever
    // physics callback Unity happened to dispatch first.
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
