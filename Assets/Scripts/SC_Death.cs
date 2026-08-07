using UnityEngine;

// Marks a GameObject as something that hurts Mario on contact. Attached unchanged to spikes,
// to both enemies, and to the garlic they throw - what happens next is decided by whoever
// subscribes, not here.
public class SC_Death : MonoBehaviour
{
    public delegate void HazardCollisionHandler();
    public static event HazardCollisionHandler OnHazardCollision;

    public delegate void HazardCollisionGeneralHandler(GameObject collidedObject);
    public static event HazardCollisionGeneralHandler OnHazardCollisionGeneral;

    void OnCollisionEnter2D(Collision2D col)
    {
        HandleContact(col.gameObject);
    }

    // Trigger counterpart of the check above, needed for hazards whose collider has to be a
    // trigger so it can fly through terrain. Both funnel into the same events, so subscribers
    // never have to care which kind of collider detected the hit.
    void OnTriggerEnter2D(Collider2D other)
    {
        HandleContact(other.gameObject);
    }

    private void HandleContact(GameObject other)
    {
        if (other.tag == "Player")
        {
            Debug.Log("Mario hit hazard: " + gameObject.name);
            if (OnHazardCollision != null)
                OnHazardCollision();
        }
        else
        {
            if (OnHazardCollisionGeneral != null)
                OnHazardCollisionGeneral(other);
        }
    }
}
