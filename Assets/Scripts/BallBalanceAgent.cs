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
    [SerializeField] GameObject ballFallenThreshold;
    [SerializeField] float randomBallOffsetRange;

    // Cached Components
    Rigidbody ballRigidBody;
    EnvironmentParameters defaultParameters;

    // State
    Vector3 initialBallPos;
    bool goalReached = false;

    public override void Initialize()
    {
        ballRigidBody = ball.GetComponent<Rigidbody>();
        defaultParameters = Academy.Instance.EnvironmentParameters;

        initialBallPos = ball.transform.position;

        ResetScene();
    }
    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(ballRigidBody.velocity);
        sensor.AddObservation(ball.transform.position);
        sensor.AddObservation(platform.transform.rotation.z);
        sensor.AddObservation(platform.transform.rotation.x);
    }
    public override void OnActionReceived(ActionBuffers actions)
    {
        const float angleThreshold = 0.25f;

        const float fallFailureReward = -1f;
        const float successReward = 1f;

        float ballFallenValue = ballFallenThreshold.transform.position.y;

        float zAngle = 2f * Mathf.Clamp(actions.ContinuousActions[0], -1f, 1f);
        float xAngle = 2f * Mathf.Clamp(actions.ContinuousActions[1], -1f, 1f);

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

        if (ball.transform.position.y < ballFallenValue)
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
        // Reset Platform
        platform.gameObject.transform.rotation = new Quaternion(0f, 0f, 0f, 0f);
        //platform.gameObject.transform.Rotate(new Vector3(1, 0, 0), Random.Range(-10f, 10f));
        //platform.gameObject.transform.Rotate(new Vector3(0, 0, 1), Random.Range(-10f, 10f));

        // Reset & Randomise Ball
        Vector3 randomBallOffset = new Vector3(Random.Range(-randomBallOffsetRange, randomBallOffsetRange), 0f, Random.Range(-randomBallOffsetRange, randomBallOffsetRange));
        ballRigidBody.velocity = new Vector3(0f, 0f, 0f);
        ball.transform.position = initialBallPos + randomBallOffset;
        
        ResetScene();
    }

    void ResetScene()
    {
        goalReached = false;
        float scale = defaultParameters.GetWithDefault("scale", 1f);
        ball.transform.localScale = new Vector3(scale, scale, scale);
        ballRigidBody.mass = defaultParameters.GetWithDefault("mass", 1f);
    }

    public void UpdateGoal(bool status)
    {
        goalReached = status;
    }
}
