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
    [SerializeField] GameObject cube;

    // Cached Components
    Rigidbody ballRigidBody;
    EnvironmentParameters defaultParameters;

    // State
    Vector3 initialBallPos;

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
        sensor.AddObservation(cube.transform.rotation.z);
        sensor.AddObservation(cube.transform.rotation.x);
    }
    public override void OnActionReceived(ActionBuffers actions)
    {
        const float actionThreshold = 0.25f;

        float zAngle = 2f * Mathf.Clamp(actions.ContinuousActions[0], -1f, 1f);
        float xAngle = 2f * Mathf.Clamp(actions.ContinuousActions[1], -1f, 1f);

        // If the cube hasn't rotated more than the actionThreshold, rotate in that axis
        if ( (cube.transform.rotation.z <  actionThreshold && zAngle > 0f) ||
             (cube.transform.rotation.z > -actionThreshold && zAngle < 0f))
        {
            gameObject.transform.Rotate(new Vector3(0, 0, 1), zAngle);
        }

        if ((cube.transform.rotation.x <  actionThreshold && xAngle > 0f) ||
            (cube.transform.rotation.x > -actionThreshold && xAngle < 0f))
        {
            gameObject.transform.Rotate(new Vector3(1, 0, 0), xAngle);
        }

        // Calculate Rewards
        const float ballFallenValue = -3f;
        const float failureReward = -1f;
        const float successReward = 0.1f;

        if (ball.transform.position.y < ballFallenValue)
        {
            SetReward(failureReward);
            EndEpisode();
        }
        else
        {
            SetReward(successReward);
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
        // Reset Cube
        cube.gameObject.transform.rotation = new Quaternion(0f, 0f, 0f, 0f);
        cube.gameObject.transform.Rotate(new Vector3(1, 0, 0), Random.Range(-10f, 10f));
        cube.gameObject.transform.Rotate(new Vector3(0, 0, 1), Random.Range(-10f, 10f));

        // Reset & Randomise Ball
        Vector3 randomBallOffset = new Vector3(Random.Range(-1.5f, 1.5f), 0f, Random.Range(-1.5f, 1.5f));
        ballRigidBody.velocity = new Vector3(0f, 0f, 0f);
        ball.transform.position = initialBallPos + randomBallOffset;
        
        ResetScene();
    }

    void ResetScene()
    {
        float scale = defaultParameters.GetWithDefault("scale", 1f);
        ball.transform.localScale = new Vector3(scale, scale, scale);
        ballRigidBody.mass = defaultParameters.GetWithDefault("mass", 1f);
    }
}
