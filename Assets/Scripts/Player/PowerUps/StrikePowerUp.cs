using UnityEngine;

public class StrikePowerUp : IPowerUp
{
    // Fired instead of reaching into the player's own components, the way AxePowerUp/
    // FireFlowerPowerUp do (player.GetComponentInChildren<...>()). Strikes are tracked on
    // StrikesManager, a scene-level manager, not anything that lives under the player -
    // there's nothing on `player` to reach into. StrikesManager subscribes to this event
    // the same way it already subscribes to SC_Death.OnSpikeCollision.
    public static event System.Action OnStrikeGained;

    public void ApplyPowerUp(GameObject player)
    {
        Debug.Log("Strike power-up applied");
        OnStrikeGained?.Invoke();
    }
}
