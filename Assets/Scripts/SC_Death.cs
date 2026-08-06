using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

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

    // Trigger counterpart of the check above - needed for hazards whose collider has to be a
    // trigger (a thrown projectile, so it can fly through terrain) rather than solid (spikes,
    // the ghost). Both funnel into the same events below, so PlayerDeath/StrikesManager don't
    // need to know or care which kind of collider actually detected the hit.
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
