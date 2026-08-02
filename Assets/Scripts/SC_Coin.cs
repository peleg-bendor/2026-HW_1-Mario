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
        Debug.Log("OnTriggerEnter2D " + col.gameObject.name);
        if (col.gameObject.tag == "Player")
        {
            Debug.Log("Coin collected: " + col.gameObject.name);
            if (OnCoinCollision != null)
                OnCoinCollision();

            this.gameObject.SetActive(false);
        }
    }
}
