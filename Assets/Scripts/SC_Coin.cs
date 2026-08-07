using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

public class SC_Coin : MonoBehaviour
{
    public delegate void CoinCollisionHandler();
    public static event CoinCollisionHandler OnCoinCollision;

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.tag == "Player")
        {
            // Names this coin, not the player who walked into it - col.gameObject is Mario,
            // so the old version logged "Coin collected: Sprite_Mario" on every pickup.
            Debug.Log("Coin collected: " + gameObject.name);
            if (OnCoinCollision != null)
                OnCoinCollision();

            this.gameObject.SetActive(false);
        }
    }
}
