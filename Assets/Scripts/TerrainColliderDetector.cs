using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainColliderDetector : MonoBehaviour
{
    private bool agentCollided = false;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Agent")
        {
            agentCollided = true;
        }
    }

    private void Update()
    {
        //Debug.Log($"Collider:{agentCollided}");
    }

    public void Reset_()
    {
        agentCollided = false;
    }

    public bool GetAgentCollided()
    {
        return agentCollided;
    }
}
