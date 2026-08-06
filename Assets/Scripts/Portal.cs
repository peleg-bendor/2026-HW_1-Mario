using UnityEngine;
using UnityEngine.SceneManagement;

public class Portal : MonoBehaviour
{
    // Deferred to Update() for the same reason StrikesManager defers its own reload:
    // SceneManager.LoadScene() is blocking, and reaching the portal on the same frame as
    // losing the last strike would otherwise race a synchronous reload here against
    // StrikesManager's already-deferred one. See HW-1_PLAN.md's Decisions Log.
    private bool levelComplete = false;

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.tag == "Player")
        {
            Debug.Log("Mario reached the portal");
            levelComplete = true;
        }
    }

    private void Update()
    {
        if (levelComplete)
        {
            Debug.Log("Game won - restarting");
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
