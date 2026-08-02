using UnityEngine;

public class PlayerPowerUp : MonoBehaviour
{
    public void CollectPowerUp(IPowerUp powerUp)
    {
        Debug.Log("Power-up collected: " + powerUp.GetType().Name);
        powerUp.ApplyPowerUp(this.gameObject);
    }
}
