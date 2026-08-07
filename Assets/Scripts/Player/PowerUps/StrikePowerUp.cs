using UnityEngine;

// Grants Mario an extra strike. Unlike the other power-ups it ignores the player argument
// entirely: strikes live on StrikesManager, a scene-level manager, so there is nothing on
// Mario to reach into. It announces the gain instead and lets the owner of the count apply it.
public class StrikePowerUp : IPowerUp
{
    public static event System.Action OnStrikeGained;

    public void ApplyPowerUp(GameObject player)
    {
        Debug.Log("Strike power-up applied");
        OnStrikeGained?.Invoke();
    }
}
