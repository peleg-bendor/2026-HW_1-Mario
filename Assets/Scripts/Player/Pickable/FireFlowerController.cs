using UnityEngine;

public class FireFlowerController : MonoBehaviour
{

    void OnTriggerEnter2D(Collider2D col)
    {
        Debug.Log("OnTriggerEnter2D " + col.gameObject.name);
        if (col.gameObject.tag == "Player")
        {
            Debug.Log("Fire flower collected");
            this.gameObject.SetActive(false);
            col.gameObject.GetComponent<PlayerPowerUp>().CollectPowerUp(new FireFlowerPowerUp());
        }
    }
}
