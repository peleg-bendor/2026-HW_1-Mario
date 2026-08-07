using UnityEngine;

// An axe lying in the level. Detects Mario and hands over an AxePowerUp - it never touches
// the axe count itself, so what collecting an axe means stays in one place.
public class AxePickupController : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.tag == "Player")
        {
            Debug.Log("Axe pickup collected");
            this.gameObject.SetActive(false);
            col.gameObject.GetComponent<PlayerPowerUp>().CollectPowerUp(new AxePowerUp());
        }
    }
}
