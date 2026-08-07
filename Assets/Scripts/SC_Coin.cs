using UnityEngine;

// A coin lying in the level. Detects Mario and announces itself, and that is all - who counts
// coins and who draws the total are somebody else's problem.
public class SC_Coin : MonoBehaviour
{
    public delegate void CoinCollisionHandler();
    public static event CoinCollisionHandler OnCoinCollision;

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.tag == "Player")
        {
            Debug.Log("Coin collected: " + gameObject.name);
            if (OnCoinCollision != null)
                OnCoinCollision();

            this.gameObject.SetActive(false);
        }
    }
}
