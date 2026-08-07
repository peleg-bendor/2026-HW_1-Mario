using UnityEngine;

// One collectable effect. A pickup controller that detects Mario hands one of these to
// PlayerPowerUp, which applies it without knowing which kind it was given.
public interface IPowerUp
{
    void ApplyPowerUp(GameObject player);
}
