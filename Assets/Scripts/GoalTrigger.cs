using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GoalTrigger : MonoBehaviour
{
    // Cached Components
    BallBalanceAgent agent;

    private void Start()
    {
        agent = GameObject.FindGameObjectWithTag("Agent").GetComponent<BallBalanceAgent>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Ball")
        {
            agent.UpdateGoal(true);
        }
    }
}
