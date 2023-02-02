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
    [Space]
    [SerializeField] float ballFallenThreshold = -6;
    [SerializeField] float goalBallHorizontalMaxOffset = 6f;
    [SerializeField] float maxHorizontalWallWidth = 14f;
    [SerializeField] float minHorizontalWallWidth = 10f;
    [SerializeField] float horizontalWallMaxOffset = 2f;


    // Cached Components
    Rigidbody ballRigidBody;
    EnvironmentParameters defaultParameters;

    // State
    Vector3 initialBallPos;
    Vector3 initialGoalPos;
    Vector3[] initialHorizontalWallPositions;
    bool goalReached = false;

    public override void Initialize()
    {
        ballRigidBody = ball.GetComponent<Rigidbody>();
        defaultParameters = Academy.Instance.EnvironmentParameters;

        initialBallPos = ball.transform.position;
        initialGoalPos = goal.transform.position;

        initialHorizontalWallPositions = new Vector3[horizontalWalls.Length];

        for (int i = 0; i < horizontalWalls.Length; i++)
        {
            initialHorizontalWallPositions[i] = horizontalWalls[i].transform.position;
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

        // Reduce the reward if the platform has over rotated
        if ( (platform.transform.rotation.z <  angleThreshold && zAngle > 0f) ||
             (platform.transform.rotation.z > -angleThreshold && zAngle < 0f))
        {
            platform.gameObject.transform.Rotate(new Vector3(0, 0, 1), zAngle);
        }

        if ( (platform.transform.rotation.x <  angleThreshold && xAngle > 0f) ||
             (platform.transform.rotation.x > -angleThreshold && xAngle < 0f))
        {
            platform.gameObject.transform.Rotate(new Vector3(1, 0, 0), xAngle);
        }     
        

        // Calculate Rewards

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
            horizontalWalls[i].transform.position = initialHorizontalWallPositions[i] + randomWallOffset;
        }

        // Reset & randomise Ball
        Vector3 randomBallOffset = new Vector3(Random.Range(-goalBallHorizontalMaxOffset, goalBallHorizontalMaxOffset), 0f, 0f);
        
        ballRigidBody.velocity = new Vector3(0f, 0f, 0f);
        ball.transform.position = initialBallPos + randomBallOffset;

        // Reset & randomise Goal
        Vector3 randomGoalOffset = new Vector3(Random.Range(-goalBallHorizontalMaxOffset, goalBallHorizontalMaxOffset), 0f, 0f);
        goal.transform.position = initialGoalPos + randomGoalOffset;

        // Other state resets
        goalReached = false;
    }

    public void UpdateGoal(bool status)
    {
        goalReached = status;
    }
}
