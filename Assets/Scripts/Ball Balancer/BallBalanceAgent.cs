using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using UnityEngine.InputSystem;

public class BallBalanceAgent : Agent
{
    // Config Params
    [SerializeField] GameObject ball;
    [SerializeField] GameObject platform;
    [SerializeField] GameObject goal;
    [SerializeField] GameObject[] horizontalWalls;
    [SerializeField] GameObject[] verticalWalls;
    [Space]
    [SerializeField] float ballFallenThreshold = -6;
    [SerializeField] float goalBallHorizontalMaxOffset = 6f;
    [SerializeField] float maxHorizontalWallWidth = 14f;
    [SerializeField] float minHorizontalWallWidth = 10f;
    [SerializeField] float horizontalWallMaxOffset = 2f;
    [SerializeField] float verticalWallMaxOffset = 1f;
    [SerializeField] float maxTime = 60f;


    // Cached Components
    Rigidbody ballRigidBody;
    Rigidbody platformRigidBody;
    EnvironmentParameters defaultParameters;

    // State
    Vector3 initialBallPos;
    Vector3 initialGoalPos;
    Vector3[] initialHorizontalWallPositions;
    Vector3[] initialVerticalWallPositions;
    bool goalReached = false;
    float elapsedTime = 0f;

    public override void Initialize()
    {
        ballRigidBody = ball.GetComponent<Rigidbody>();
        platformRigidBody = platform.GetComponent<Rigidbody>();
        defaultParameters = Academy.Instance.EnvironmentParameters;

        initialBallPos = ball.transform.position;
        initialGoalPos = goal.transform.position;

        initialHorizontalWallPositions = new Vector3[horizontalWalls.Length];
        initialVerticalWallPositions = new Vector3[verticalWalls.Length];

        for (int i = 0; i < horizontalWalls.Length; i++)
        {
            initialHorizontalWallPositions[i] = horizontalWalls[i].transform.position;
        }

        for (int i = 0; i < verticalWalls.Length; i++)
        {
            initialVerticalWallPositions[i] = verticalWalls[i].transform.position;
        }

        ResetScene();
    }
    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(goal.transform.position);
        sensor.AddObservation(ballRigidBody.velocity);
        sensor.AddObservation(ball.transform.position);
        sensor.AddObservation(platform.transform.rotation.z);
        sensor.AddObservation(platform.transform.rotation.x);
    }
    public override void OnActionReceived(ActionBuffers actions)
    {
        const float angleThreshold = 0.25f;

        const float fallFailureReward = -1f;
        const float successReward = 2f;

        float zAngle = 2f * Mathf.Clamp(actions.ContinuousActions[0], -1f, 1f);
        float xAngle = 2f * Mathf.Clamp(actions.ContinuousActions[1], -1f, 1f);

        AddReward(-0.001f);

        if ( (platform.transform.rotation.z <  angleThreshold && zAngle > 0f) ||
             (platform.transform.rotation.z > -angleThreshold && zAngle < 0f))
        {
            Quaternion rotation = Quaternion.Euler(0, 0, zAngle);
            platformRigidBody.MoveRotation(platformRigidBody.rotation * rotation);
        }

        if ( (platform.transform.rotation.x <  angleThreshold && xAngle > 0f) ||
             (platform.transform.rotation.x > -angleThreshold && xAngle < 0f))
        {
            Quaternion rotation = Quaternion.Euler(xAngle, 0, 0);
            platformRigidBody.MoveRotation(platformRigidBody.rotation * rotation); 
        }

        elapsedTime += Time.deltaTime;

        // Calculate Rewards

        if (elapsedTime >= maxTime && maxTime != 0)
        {
            SetReward(fallFailureReward);
            EndEpisode();
        }

        if (ball.transform.position.y < ballFallenThreshold)
        {
            SetReward(fallFailureReward);
            EndEpisode();
        }

        if (goalReached)
        {
            SetReward(successReward);
            EndEpisode();
        }
    }
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<float> continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[0] = -Mouse.current.delta.x.ReadValue();
        continuousActionsOut[1] = Mouse.current.delta.y.ReadValue();
    }
    public override void OnEpisodeBegin()
    {
        ResetScene();
    }

    void ResetScene()
    {
        // Reset Platform
        platform.gameObject.transform.rotation = new Quaternion(0f, 0f, 0f, 0f);

        // Reset & randomise Walls
        for (int i = 0; i < horizontalWalls.Length; i++)
        {
            Vector3 randomWallOffset = new Vector3(Random.Range(-horizontalWallMaxOffset, horizontalWallMaxOffset), 0f, 0f);
          //  float randomWallWidth = Random.Range(minHorizontalWallWidth, maxHorizontalWallWidth);
            horizontalWalls[i].transform.position = initialHorizontalWallPositions[i] + randomWallOffset;
          //  horizontalWalls[i].transform.localScale = new Vector3(randomWallWidth, horizontalWalls[i].transform.localScale.y, horizontalWalls[i].transform.localScale.z);
        }

        for (int i = 0; i < verticalWalls.Length; i++)
        {
            Vector3 randomWallOffset = new Vector3(0f, 0f, Random.Range(-verticalWallMaxOffset, verticalWallMaxOffset));
            verticalWalls[i].transform.position = initialVerticalWallPositions[i] + randomWallOffset;
        }

        // Reset & randomise Ball
        Vector3 randomBallOffset = new Vector3(Random.Range(-goalBallHorizontalMaxOffset, goalBallHorizontalMaxOffset), 0f, 0f);

        ball.transform.rotation = new Quaternion(0f, 0f, 0f, 0f);
        ballRigidBody.velocity = new Vector3(0f, 0f, 0f);
        ball.transform.position = initialBallPos + randomBallOffset;

        // Reset & randomise Goal
        Vector3 randomGoalOffset = new Vector3(Random.Range(-goalBallHorizontalMaxOffset, goalBallHorizontalMaxOffset), 0f, 0f);
        goal.transform.position = initialGoalPos + randomGoalOffset;

        // Other state resets
        goalReached = false;
        elapsedTime = 0;
    }

    public void UpdateGoal(bool status)
    {
        goalReached = status;
    }
}
