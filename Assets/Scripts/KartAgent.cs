using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class KartAgent : MonoBehaviour
{
    // Cached Components
    Rigidbody rigidBody;

    // Config Params
    [SerializeField] float speed = 10.0f;
    [SerializeField] float steerAngle = 30.0f;
    [SerializeField] WheelCollider frontL;
    [SerializeField] WheelCollider frontR;
    [SerializeField] WheelCollider backL;
    [SerializeField] WheelCollider backR;


    void Start()
    {
        rigidBody = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        DoMovement();
    }

    void DoMovement()
    {
        if (Keyboard.current.wKey.isPressed)
        {
            frontL.motorTorque = speed;
            frontR.motorTorque = speed;
        }

        if (Keyboard.current.sKey.isPressed)
        {
            frontL.motorTorque = -speed;
            frontR.motorTorque = -speed;
        }

        if (Keyboard.current.aKey.isPressed)
        {
            frontL.steerAngle = -steerAngle;
            frontR.steerAngle = -steerAngle;
        }


        if (Keyboard.current.dKey.isPressed)
        {
            frontL.steerAngle = steerAngle;
            frontR.steerAngle = steerAngle;
        }
    }
}
