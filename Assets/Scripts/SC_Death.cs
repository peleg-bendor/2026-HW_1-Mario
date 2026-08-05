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
        if (col.gameObject.tag == "Player")
        {
            Debug.Log("Mario hit hazard: " + gameObject.name);
            if (OnHazardCollision != null)
                OnHazardCollision();
        }
        else
        {
            if (OnHazardCollisionGeneral != null)
                OnHazardCollisionGeneral(col.gameObject);
        }
    }
}
