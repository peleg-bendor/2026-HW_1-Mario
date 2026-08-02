using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SC_CoinsManager : MonoBehaviour
{
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
        GameObject.Find("Txt_Coins").GetComponent<TextMeshProUGUI>().text = "Coins: " + coins.ToString();
    }

}
