
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.MLAgents;
using Unity.MLAgents.Actuators;

public class ThreeDirt : Agent
{
    public int counter = 0;

    public float moveSpeed = 3f;

    public float turnSpeed = 300;

    private Vector3 startPosition;

    new private Rigidbody rigidbody;

    [SerializeField] private GameObject[] goals = new GameObject[3];

    public override void Initialize()
    {
        startPosition = transform.position;

        //get rigidbody of agent
        rigidbody = GetComponent<Rigidbody>();
    }

    public override void OnEpisodeBegin()
    {

        transform.position = startPosition;
        transform.rotation = Quaternion.Euler(Vector3.up * Random.Range(0f, 360f));
        rigidbody.velocity = Vector3.zero;

        for(int i = 0; i < goals.Length; i++){ 
            goals[i].SetActive(true);
            goals[i].transform.position = startPosition + Quaternion.Euler(Vector3.up * Random.Range(0f, 360f)) * Vector3.forward * Random.Range(5f, 20f);
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        int vertical = Mathf.RoundToInt(Input.GetAxisRaw("Vertical"));
        int horizontal = Mathf.RoundToInt(Input.GetAxisRaw("Horizontal"));
        bool jump = Input.GetKey(KeyCode.Space);

        ActionSegment<int> actions = actionsOut.DiscreteActions;
        actions[0] = vertical >= 0 ? vertical : 2;
        actions[1] = horizontal >= 0 ? horizontal : 2;
        actions[2] = jump ? 1 : 0;
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        // Punish and end episode if the agent strays too far
        if (Vector3.Distance(startPosition, transform.position) > 25f)
        {
            AddReward(-1f);
            EndEpisode();
        }

        // Convert actions from Discrete (0, 1, 2) to expected input values (-1, 0, +1)
        // of the character controller
        float vertical = actions.DiscreteActions[0] <= 1 ? actions.DiscreteActions[0] : -1;
        float horizontal = actions.DiscreteActions[1] <= 1 ? actions.DiscreteActions[1] : -1;
        bool jump = actions.DiscreteActions[2] > 0;

        // Turning
        if (horizontal != 0f)
        {
            float angle = Mathf.Clamp(horizontal, -1f, 1f) * turnSpeed;
            transform.Rotate(Vector3.up, Time.fixedDeltaTime * angle);
        }

        // Movement
        Vector3 move = transform.forward * Mathf.Clamp(vertical, -1f, 1f) *
            moveSpeed * Time.fixedDeltaTime;
        rigidbody.MovePosition(transform.position + move);
    }

    private void OnTriggerEnter(Collider other)
    {
        //
        if (other.gameObject.tag == "goal")
        {
            other.gameObject.SetActive(false);
            counter += 1;
            AddReward(1f);
            if(counter == 3){
                counter = 0;
                EndEpisode();
            }
        }
    }



    
}


