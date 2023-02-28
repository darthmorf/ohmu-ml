using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using UnityEngine.InputSystem;
using System;
using UnityEditor.PackageManager.Requests;
using Unity.VisualScripting;

public class KartAgent : Agent
{
    // Config Params
    [SerializeField] KartController kartController;
    [SerializeField] TerrainColliderDetector terrainCollider;
    [SerializeField] RaceCheckpoint[] checkpoints;
    [SerializeField] bool handBreakEnabled = false;
    [SerializeField] bool reverseEnabled = false;
 
    [Header("Rewards")]
    [SerializeField] float stepReward = 0.001f;
    [SerializeField] float failReward = -1f;
    [SerializeField] float checkpointReward = 0.5f;

    // Cached Components

    // State
    bool failed = false;
    int checkpointIndex = 0;

    public override void Initialize()
    {
       // ResetScene();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(kartController.GetRigidbody().velocity.magnitude);
        sensor.AddObservation(Vector3.Distance(transform.position, checkpoints[checkpointIndex].transform.position));
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        const float steeringRange = 0.3f;

        kartController.forward = false;
        kartController.backward = false;
        kartController.left = false;
        kartController.right = false;
        kartController.handbreak = false;

        kartController.forward = actions.ContinuousActions[0] > 0;
        kartController.backward = actions.ContinuousActions[0] < 0 && reverseEnabled;
        kartController.left = actions.ContinuousActions[1] < -steeringRange;
        kartController.right = actions.ContinuousActions[1] > steeringRange;
        kartController.handbreak = actions.ContinuousActions[2] > 0 && handBreakEnabled;

        failed = terrainCollider.GetAgentCollided();

        CheckCheckpoints();

        AddReward(kartController.GetRigidbody().velocity.magnitude * stepReward);

        if (failed || Keyboard.current.rKey.isPressed)
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

            AddReward(checkpointReward);

            checkpoints[checkpointIndex].Reset();
            checkpoints[checkpointIndex].gameObject.SetActive(false);

            checkpointIndex = (checkpointIndex + 1) % checkpoints.Length;
            checkpoints[checkpointIndex].gameObject.SetActive(true);
        }
    }

    void Failure()
    {
        AddReward(failReward);
        ShowReward();
        ResetScene();
        EndEpisode();
    }

    public override void OnEpisodeBegin()
    {
        ResetScene();
    }

    void ResetScene()
    {
        failed = false;

        foreach (RaceCheckpoint checkpoint in checkpoints)
        {
            checkpoint.gameObject.SetActive(false);
        }

        checkpointIndex = 0;
        checkpoints[checkpointIndex].gameObject.SetActive(true);

        kartController.Reset_();
        terrainCollider.Reset_();
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
