using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WheelVisuals : MonoBehaviour
{
    // Cached Components
    WheelCollider wheelCollider;

    Quaternion originalRotation;

    void Start()
    {
        wheelCollider = GetComponent<WheelCollider>();
        originalRotation = transform.rotation;
    }

    void Update()
    {
        Vector3 position;
        Quaternion rotation;

        wheelCollider.GetWorldPose(out position, out rotation);
        transform.rotation = rotation * Quaternion.Euler(0, 0, 90);
    }
}
