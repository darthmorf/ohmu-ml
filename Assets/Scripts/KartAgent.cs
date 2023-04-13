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
using Unity.MLAgents.Policies;
using Assets.Scripts;
using System.Linq;

public class KartAgent : Agent
{
    // Config Params
    [SerializeField] KartController kartController;
    [SerializeField] TerrainColliderDetector[] terrainColliders;
    [SerializeField] GameObject checkpointParent;
    [SerializeField] bool handBreakEnabled = false;
    [SerializeField] bool reverseEnabled = false;
    [SerializeField] float steeringRange = 0.3f;
    [SerializeField] bool manualControl = false;

    [Header("Rewards")]
    [SerializeField] float stepReward = 0.001f;
    [SerializeField] float failReward = -1f;
    [SerializeField] float checkpointReward = 0.5f;
    [SerializeField] float timeOut = 30.0f;
    [SerializeField] [Range(1f, 20f)] float timeScale = 1f;

    // Cached Components

    // State
    bool failed = false;
    int checkpointIndex = 0;
    float cumulativeElapsedTime = 0;
    RaceCheckpoint[] checkpoints;

    // Logging
    LogItem logItem = new LogItem();
    int checkpointCount = 0;
    string startDateTime = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
    List<float> velocities = new List<float>();

    public override void Initialize()
    {
        // ResetScene();
        terrainColliders = FindObjectsOfType<TerrainColliderDetector>();
        checkpoints = checkpointParent.GetComponentsInChildren<RaceCheckpoint>(true);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(kartController.GetRigidbody().velocity.magnitude);

        if (checkpoints.Length > 0)
        {
            sensor.AddObservation(Vector3.Distance(transform.position, checkpoints[checkpointIndex].transform.position));
        }
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        Time.timeScale = timeScale; // This shouldn't be needed, but is nice for demos

        if (!manualControl)
        {
            kartController.SetSpeed(Mathf.Abs(actions.ContinuousActions[0]));
            kartController.SetTurn(actions.ContinuousActions[1]);
        }


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

        cumulativeElapsedTime += Time.deltaTime;

        velocities.Add(kartController.GetRigidbody().velocity.magnitude);

        if (failed || Keyboard.current.rKey.isPressed)
        {
            AddReward(failReward);
            ResetScene();
        }

        if (cumulativeElapsedTime > timeOut)
        {
            ResetScene();
        }

        ShowReward();
    }

    void CheckCheckpoints()
    {
        if (checkpoints.Length > 0 && checkpoints[checkpointIndex].KartHitCheckpoint())
        {
            Debug.Log($"Checkpoint {checkpointIndex+1} hit!");

            AddReward(checkpointReward);

            checkpoints[checkpointIndex].Reset();
            checkpoints[checkpointIndex].gameObject.SetActive(false);

            checkpointIndex = (checkpointIndex + 1) % checkpoints.Length;
            checkpoints[checkpointIndex].gameObject.SetActive(true);

            checkpointCount++;
        }
    }

    public override void OnEpisodeBegin()
    {
        //ResetScene();
    }

    void Log()
    {
        logItem.iterationTime = cumulativeElapsedTime;
        logItem.reward = GetCumulativeReward();
        logItem.checkpointCount = checkpointCount;
        logItem.averageVelocity = velocities.Average();
        logItem.timedOut = cumulativeElapsedTime > timeOut;

        LoggingController.LogItem(logItem, startDateTime);
    }

    void ResetScene()
    {
        Log();
        
        logItem = new LogItem();
        checkpointCount = 0;
        velocities = new List<float>();

        failed = false;
        cumulativeElapsedTime = 0;

        foreach (RaceCheckpoint checkpoint in checkpoints)
        {
            checkpoint.gameObject.SetActive(false);
        }

        checkpointIndex = 0;
        if (checkpoints.Length > 0)
        {
            checkpoints[checkpointIndex].gameObject.SetActive(true);
        }

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
