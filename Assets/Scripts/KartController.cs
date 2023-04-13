using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using TreeEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

public class KartController : MonoBehaviour
{
    // Cached Components
    Rigidbody rigidBody;

    // Config Params
    [SerializeField] float speed = 10.0f;
    [SerializeField] float steerSpeed = 5;
    [SerializeField] float maxSteerAngle = 30.0f;
    [SerializeField] float brakeTorque = 50.0f;
    [SerializeField] WheelCollider frontL;
    [SerializeField] WheelCollider frontR;
    [SerializeField] WheelCollider backL;
    [SerializeField] WheelCollider backR;
    [SerializeField] GameObject positionTracker;
    [SerializeField] float trackFrequency;
    [SerializeField] bool trackMovement = true;

    // State
    float currentSteerAngle = 0;
    Vector3 startPos;
    Quaternion startRot;
    float elapsedTime = 0;
    List<GameObject> trackedPositions = new List<GameObject>();

    void Start()
    {
        rigidBody = GetComponent<Rigidbody>();
        startPos = transform.position;
        startRot = transform.rotation;
    }

    private void Update()
    {
        DoMovement();

        elapsedTime += Time.deltaTime;

        if (trackMovement && elapsedTime >= trackFrequency)
        {
            GameObject newObject = Instantiate(positionTracker);
            newObject.transform.position = transform.position;
            newObject.SetActive(true);
            elapsedTime = 0;

            trackedPositions.Add(newObject);
        }
    }

    void DoMovement()
    {
        if (Keyboard.current.spaceKey.isPressed)
        {
            frontL.brakeTorque = brakeTorque;
            frontR.brakeTorque = brakeTorque;
            backL.brakeTorque = brakeTorque;
            backR.brakeTorque = brakeTorque;
        }
        else
        {
            frontL.brakeTorque = 0;
            frontR.brakeTorque = 0;
            backL.brakeTorque = 0;
            backR.brakeTorque = 0;
        }
        
        
        if (Keyboard.current.wKey.isPressed)
        {
            frontL.motorTorque = speed;
            frontR.motorTorque = speed;
        }
        else if (Keyboard.current.sKey.isPressed)
        {
            frontL.motorTorque = -speed / 2;
            frontR.motorTorque = -speed / 2;
        }

        

        if (Keyboard.current.aKey.isPressed)
        {
            currentSteerAngle -= steerSpeed * Time.deltaTime;
        }
        else if (Keyboard.current.dKey.isPressed)
        {
            currentSteerAngle += steerSpeed * Time.deltaTime;
        }
        else if (currentSteerAngle < 0)
        {
            currentSteerAngle += steerSpeed * Time.deltaTime;
        }
        else
        {
            currentSteerAngle -= steerSpeed * Time.deltaTime;
        }

        currentSteerAngle = Mathf.Clamp(currentSteerAngle, -maxSteerAngle, maxSteerAngle);

        frontL.steerAngle = currentSteerAngle;
        frontR.steerAngle = currentSteerAngle;
    }

    public void SetSpeed(float newSpeed)
    {
        frontL.motorTorque = speed * newSpeed;
        frontR.motorTorque = speed * newSpeed;
    }

    public void SetTurn(float newAngle)
    {
        frontL.steerAngle = newAngle * maxSteerAngle;
        frontR.steerAngle = newAngle * maxSteerAngle;
    }


    public Rigidbody GetRigidbody()
    {
        return rigidBody;
    }

    public void Reset_()
    {
        transform.position = startPos;
        transform.rotation = startRot;

        frontL.steerAngle = 0;
        frontR.steerAngle = 0;

        frontL.motorTorque = 0;
        frontR.motorTorque = 0;

        rigidBody.velocity = Vector3.zero;
        rigidBody.angularVelocity = Vector3.zero;

        elapsedTime = 0;

        foreach (GameObject trackedPosition in trackedPositions)
        {
            Destroy(trackedPosition);
        }

        trackedPositions = new List<GameObject>();
    }
}
