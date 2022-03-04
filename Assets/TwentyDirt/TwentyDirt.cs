

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.MLAgents;
using Unity.MLAgents.Actuators;


public class TwentyDirt : Agent
{
    //couter for later//
    public int counter = 0;

    //movespeed of agent
    public float moveSpeed = 3f;

    //turnspeed of agent
    public float turnSpeed = 300;

    //vector to which the start position is passed later
    private Vector3 startPosition;

    new private Rigidbody rigidbody;

    public override void Initialize()
    {
        startPosition = transform.position;

        //get rigidbody of agent
        rigidbody = GetComponent<Rigidbody>();
    }
    
    //init dirt objects. SerializeField for accessing dirt objects through Unity Editor
    [SerializeField] private GameObject[] goals = new GameObject[20];
    
    public override void OnEpisodeBegin()
    {
        // Reset agent position, rotation
        transform.position = startPosition;
        transform.rotation = Quaternion.Euler(Vector3.up * Random.Range(0f, 360f));
        rigidbody.velocity = Vector3.zero;

        for(int i = 0; i < goals.Length; i++){ 
            goals[i].SetActive(true);
            /*
            Quaternion = used to represent rotations 
            Quaternion.Euler = returns rotation that rotates z degrees around the x axis and y degrees around the y axis; applied in that order
            Vektor3.up = shorthand for Vector3(0, 1, 0)
            Vektor3.forward =  shorthand for Vector3(0, 0, 1)
            */
            //
            goals[i].transform.position = startPosition + Quaternion.Euler(Vector3.up * Random.Range(0f, 360f)) * Vector3.forward * Random.Range(5f, 15f);
        }

    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        // Read input values and round them. GetAxisRaw works better in this case
        // because of the DecisionRequester, which only gets new decisions periodically.
        int vertical = Mathf.RoundToInt(Input.GetAxisRaw("Vertical"));
        int horizontal = Mathf.RoundToInt(Input.GetAxisRaw("Horizontal"));
        bool jump = Input.GetKey(KeyCode.Space);

        // Convert the actions to Discrete choices (0, 1, 2)
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
        if (other.gameObject.tag == "goal")
        {
            other.gameObject.SetActive(false);
            counter += 1;
            AddReward(1f);
            if(counter == 20){
                counter = 0;
                EndEpisode();
            }
        }
    }
}
