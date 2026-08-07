using UnityEngine;

// Makes an enemy destructible. Attached alongside whatever else an enemy does, so movement,
// touch damage and destructibility can each be present or absent independently.
public class EnemyHealth : MonoBehaviour, IEnemy
{
    public void Kill()
    {
        Debug.Log("Enemy destroyed: " + gameObject.name);
        Destroy(gameObject);
    }
}
