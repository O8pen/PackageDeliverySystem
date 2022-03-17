// https://www.youtube.com/watch?v=zPFU30tbyKs&list=PLzDRvYVwl53vehwiN_odYJkPBzcqFw110&index=1&ab_channel=CodeMonkey


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class AgentController : Agent
{
    public Transform targetTransform;

    public override void OnEpisodeBegin()
    {
        transform.localPosition = new Vector3(Random.Range(-3f, +1f), 0, Random.Range(-2f, 2f));
        targetTransform.localPosition = new Vector3(Random.Range(1.5f, 4f), 0, Random.Range(-2f, 2f));
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.localPosition);
        sensor.AddObservation(targetTransform.localPosition);
    }
    public override void OnActionReceived(ActionBuffers actions)
    {
        float moveX = actions.ContinuousActions[0];
        float moveZ = actions.ContinuousActions[1];

        float moveSpeed = 3f;
        transform.localPosition += new Vector3(moveX, 0f, moveZ) * Time.deltaTime * moveSpeed;
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<float> continuousActions = actionsOut.ContinuousActions;

        continuousActions[0] = Input.GetAxisRaw("Horizontal");
        continuousActions[1] = Input.GetAxisRaw("Vertical");
    }

    public void OnTriggerEnter(Collider other)
    {
        Debug.Log(other.tag);

        if (other.gameObject.tag == "goal")
        {
            SetReward(+1f);
            EndEpisode();
        }
        if (other.gameObject.tag == "wall")
        {
            SetReward(-1f);
            EndEpisode();
        }
    }
}
