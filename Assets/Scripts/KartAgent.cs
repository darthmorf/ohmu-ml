using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using UnityEngine.InputSystem;
using System;
using UnityEditor.PackageManager.Requests;

public class KartAgent : Agent
{
    // Config Params
    [SerializeField] float debugRaycastTime = 2f;
    [SerializeField] float raycastDistance = 10;
    [SerializeField] float failRaycastDistance = 1;
    [SerializeField] Transform[] raycasts;
    [SerializeField] LayerMask raycastLayers;
    [SerializeField] KartController kartController;
    [SerializeField] RaceCheckpoint[] checkpoints;
    [SerializeField] bool handBreakEnabled = false;
    [SerializeField] bool reverseEnabled = false;

    // Cached Components

    // State
    Vector3 startPos;
    Quaternion startRot;
    bool failed = false;
    int checkpointIndex = 0;

    public override void Initialize()
    {
        startPos = kartController.transform.position;
        startRot = kartController.transform.rotation;
        ResetScene();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(kartController.GetRigidbody().velocity.magnitude);
       
        foreach(Transform raycast in raycasts)
        {
            AddRaycastVectorObservation(raycast, sensor);
        }
    }

    void AddRaycastVectorObservation(Transform ray, VectorSensor sensor)
    {
        RaycastHit hitInfo = new RaycastHit();
        bool hit = Physics.Raycast(ray.position, ray.forward, out hitInfo, raycastDistance, raycastLayers.value, QueryTriggerInteraction.Ignore);
        float distance = hitInfo.distance;

        if (!hit)
        {
            distance = raycastDistance;
        }

        float obs = distance / raycastDistance;
        sensor.AddObservation(obs);

        if (distance < failRaycastDistance)
        {
            failed = true;
        }

        Debug.DrawRay(ray.position, ray.forward * distance, Color.Lerp(Color.red, Color.green, obs), Time.deltaTime * debugRaycastTime);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        kartController.forward = false;
        kartController.backward = false;
        kartController.left = false;
        kartController.right = false;
        kartController.handbreak = false;

        kartController.forward = actions.ContinuousActions[0] > 0;
        kartController.backward = actions.ContinuousActions[0] < 0 && reverseEnabled;
        kartController.left = actions.ContinuousActions[1] < -0.25f;
        kartController.right = actions.ContinuousActions[1] > 00.25f;
        kartController.handbreak = actions.ContinuousActions[2] > 0 && handBreakEnabled;

        CheckCheckpoints();

        AddReward(kartController.GetRigidbody().velocity.magnitude * 0.001f);

        if (failed)
        {
            Failure();
        }

        ShowReward();
    }

    void CheckCheckpoints()
    {
        if (checkpoints[checkpointIndex].KartHitCheckpoint())
        {
            Debug.Log($"Checkpoint {checkpointIndex+1} hit!");
            AddReward(0.5f);
            checkpoints[checkpointIndex].Reset();
            checkpoints[checkpointIndex].gameObject.SetActive(false);

            checkpointIndex = (checkpointIndex + 1) % checkpoints.Length;
            checkpoints[checkpointIndex].gameObject.SetActive(true);
        }
    }

    void Failure()
    {
        AddReward(-1f);
        ShowReward();
        EndEpisode();
    }

    public override void OnEpisodeBegin()
    {
        ResetScene();

        foreach(RaceCheckpoint checkpoint in checkpoints)
        {
            checkpoint.gameObject.SetActive(false);
        }

        checkpointIndex = 0;
        checkpoints[checkpointIndex].gameObject.SetActive(true);
    }

    void ResetScene()
    {
        kartController.transform.position = startPos;
        kartController.transform.rotation = startRot;
        failed = false;
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        base.Heuristic(actionsOut);
    }

    private void ShowReward()
    {
        Debug.Log($"Current Reward: {GetCumulativeReward()}");
    }
}
