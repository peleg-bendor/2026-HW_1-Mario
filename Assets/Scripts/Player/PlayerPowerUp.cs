using UnityEngine;

// Mario's single entry point for collecting anything. Pickups hand their own IPowerUp here
// instead of reaching into Mario's components themselves, so what a pickup grants stays with
// the pickup and this class never grows a branch per power-up type.
public class PlayerPowerUp : MonoBehaviour
{
    public void CollectPowerUp(IPowerUp powerUp)
    {
        Debug.Log("Power-up collected: " + powerUp.GetType().Name);
        powerUp.ApplyPowerUp(this.gameObject);
    }
}
