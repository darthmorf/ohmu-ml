﻿using System.Collections;
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
    [SerializeField] TerrainColliderDetector[] terrainColliders;
    [SerializeField] RaceCheckpoint[] checkpoints;
    [SerializeField] bool handBreakEnabled = false;
    [SerializeField] bool reverseEnabled = false;
    [SerializeField] float steeringRange = 0.3f;

    [Header("Rewards")]
    [SerializeField] float stepReward = 0.001f;
    [SerializeField] float failReward = -1f;
    [SerializeField] float checkpointReward = 0.5f;
    [SerializeField] float timeOut = 30.0f;

    // Cached Components

    // State
    bool failed = false;
    int checkpointIndex = 0;
    float elapsedTime = 0;

    public override void Initialize()
    {
       // ResetScene();
       terrainColliders = FindObjectsOfType<TerrainColliderDetector>();

        Time.timeScale = 1f;    
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(kartController.GetRigidbody().velocity.magnitude);
        sensor.AddObservation(Vector3.Distance(transform.position, checkpoints[checkpointIndex].transform.position));
    }

    public override void OnActionReceived(ActionBuffers actions)
    {

        kartController.forward = false;
        kartController.backward = false;
        kartController.left = false;
        kartController.right = false;
        kartController.handbreak = false;

        kartController.SetSpeed(Mathf.Abs(actions.ContinuousActions[0]));
        kartController.SetTurn(actions.ContinuousActions[1]);

        elapsedTime += Time.deltaTime;

        kartController.handbreak = actions.ContinuousActions[2] > 0 && handBreakEnabled;

        foreach (TerrainColliderDetector terrainCollider in terrainColliders)
        {
            if (terrainCollider.GetAgentCollided())
            {
                failed = true;
                break;
            }
        }

        CheckCheckpoints();

        AddReward(kartController.GetRigidbody().velocity.magnitude * stepReward);
        AddReward(-Mathf.Abs(actions.ContinuousActions[1]) * stepReward);

        if (failed || Keyboard.current.rKey.isPressed)
        {
            Failure();
        }

        if (elapsedTime > timeOut)
        {
            ResetScene();
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
    }

    public override void OnEpisodeBegin()
    {
        //ResetScene();
    }

    void ResetScene()
    {
        failed = false;
        elapsedTime = 0;

        foreach (RaceCheckpoint checkpoint in checkpoints)
        {
            checkpoint.gameObject.SetActive(false);
        }

        checkpointIndex = 0;
        checkpoints[checkpointIndex].gameObject.SetActive(true);

        kartController.Reset_();

        foreach(TerrainColliderDetector terrainColliderDetector in terrainColliders) 
        {
            terrainColliderDetector.Reset_();
        }

        EndEpisode();
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
