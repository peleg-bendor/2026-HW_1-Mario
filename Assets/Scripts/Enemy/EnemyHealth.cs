using UnityEngine;

public class EnemyHealth : MonoBehaviour, IEnemy
{
    public void Kill()
    {
        Debug.Log("Enemy destroyed: " + gameObject.name);
        Destroy(gameObject);
    }
}
