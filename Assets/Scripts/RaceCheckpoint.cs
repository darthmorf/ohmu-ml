using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaceCheckpoint : MonoBehaviour
{
    // Cached Components
    Rigidbody rigidbody;

    // State
    bool kartHit = false;

    private void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
    }

    private void OnTriggerEnter(Collider collider)
    {
        if (collider.gameObject.tag == "Agent")
        {
            kartHit = true;
        }
    }

    public void Reset()
    {
        kartHit = false;
    }

    public bool KartHitCheckpoint()
    {
        return kartHit;
    }
}
