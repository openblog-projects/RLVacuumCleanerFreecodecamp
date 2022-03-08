using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.MLAgents;
using Unity.MLAgents.Actuators;

public class OneDirt : Agent
{
    //counter for later
    //we generally set everything to private that we dont see it in the inspector (we configure everything here so we dont need to configure in inspector)
    private int counter = 0;

    //movespeed of agent
    private float moveSpeed = 3f;

    //turnspeed of agent
    private float turnSpeed = 300;

    //Vector3 object with name startPosition
    private Vector3 startPosition;

    //rigidbody object
    new private Rigidbody rigidbody;

    //still private but with serializefield accessible in inspector (we need this for gameobject goal to attach the goal in the inspector)
    [SerializeField] private GameObject goal;

    //gets once called at beginning after hitting play in the unity editor
   public override void Initialize()
    {
        //we store our first transform position (agent position)
        startPosition = transform.position;
        //gets rigidbody from transform (agent)
        rigidbody = GetComponent<Rigidbody>();
    }

    //is called when EndEpisode() is called or maxsteps are achieved --> one step is one fixed update and fixed update is a function which is called approx twice per frame
    public override void OnEpisodeBegin()
    {
        //accesing the through inspector attached goal object and setActive for very episode begin (make it visible)
        goal.SetActive(true);
        //Agent gets reseted to its original starting position from initialize() method
        transform.position = startPosition;
        //take goal object which we will later attach thorugh inspector and position it with 5f distance
        //Quaternion.Euler = rotates goal object in a certain degree (360) Vector3.up = (0,1,0) Vector.forward = (0,0,1)
        goal.transform.position = startPosition + Quaternion.Euler(Vector3.up * Random.Range(0f, 360f)) * Vector3.forward * 5f;
    }

    //heuristic method is needed if we wanna control our agent manually with keyboard 
    //parameter are action from keyboard input
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        //takes all vertical movements (straight and back movements)
        int vertical = Mathf.RoundToInt(Input.GetAxisRaw("Vertical"));
        //takes all horizontal movements for turns
        int horizontal = Mathf.RoundToInt(Input.GetAxisRaw("Horizontal"));
        //actions gets the discretection values from the keyboard input
        ActionSegment<int> actions = actionsOut.DiscreteActions;
        //actions werden übergeben und auf agent ausgeführt
        actions[0] = vertical >= 0 ? vertical : 2;
        actions[1] = horizontal >= 0 ? horizontal : 2;
    }

    //specifies agent behavior at every step 
    //actions = contains the buffers of actions to be executed at this step
    public override void OnActionReceived(ActionBuffers actions)
    {
        //calculates vector distance between start position and the current positino of the agent 
        //if bigger than 25 minus reward and endepisode
        if (Vector3.Distance(startPosition, transform.position) > 25f)
        {
            AddReward(-1f);
            EndEpisode();
        }

        //assign values for movement or turning
        float vertical = actions.DiscreteActions[0] <= 1 ? actions.DiscreteActions[0] : -1;
        float horizontal = actions.DiscreteActions[1] <= 1 ? actions.DiscreteActions[1] : -1; //-1/1 if turn

        // Turning
        if (horizontal != 0f)
        {
            //determines in which direction turning -1 or 1
            float angle = Mathf.Clamp(horizontal, -1f, 1f) * turnSpeed;
            //rotate the agent finally
            transform.Rotate(Vector3.up, Time.fixedDeltaTime * angle);
        }

        // Movement
        //transform.forward = vector that points forward 
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
            if(counter == 1){
                counter = 0;
                EndEpisode();
            }
        }
    }    
}


