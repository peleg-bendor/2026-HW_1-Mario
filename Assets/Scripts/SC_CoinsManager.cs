using TMPro;
using UnityEngine;

// Counts coins and draws the total. Listens for coin pickups without knowing which coin, or
// how many exist - the count lives here and nowhere else.
public class SC_CoinsManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI coinsText;

    private int coins = 0;

    private void OnEnable()
    {
        SC_Coin.OnCoinCollision += OnCoinCollision;
    }

    private void OnDisable()
    {
        SC_Coin.OnCoinCollision -= OnCoinCollision;
    }

    private void OnCoinCollision()
    {
        coins++;
        if (coinsText != null)
            coinsText.text = "Coins: " + coins.ToString();
    }
}
