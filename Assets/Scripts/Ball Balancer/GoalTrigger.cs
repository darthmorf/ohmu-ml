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

    private void OnTriggerEnter(Collider collider)
    {
        if (collider.gameObject.tag == "Ball")
        {
            agent.UpdateGoal(true);
        }
    }
}
