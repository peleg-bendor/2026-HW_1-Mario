using UnityEngine;

// An extra strike lying in the level. Same shape as every other pickup controller: detect
// Mario, hand over a power-up, disappear.
public class StrikePickupController : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.tag == "Player")
        {
            Debug.Log("Strike pickup collected");
            this.gameObject.SetActive(false);
            col.gameObject.GetComponent<PlayerPowerUp>().CollectPowerUp(new StrikePowerUp());
        }
    }
}
