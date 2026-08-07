using UnityEngine;

// The Fire Flower lying in the level. Same shape as every other pickup controller: detect
// Mario, hand over a power-up, disappear.
public class FireFlowerController : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.tag == "Player")
        {
            Debug.Log("Fire flower collected");
            this.gameObject.SetActive(false);
            col.gameObject.GetComponent<PlayerPowerUp>().CollectPowerUp(new FireFlowerPowerUp());
        }
    }
}
