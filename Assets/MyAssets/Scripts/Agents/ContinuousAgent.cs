// https://www.youtube.com/watch?v=zPFU30tbyKs&list=PLzDRvYVwl53vehwiN_odYJkPBzcqFw110&index=1&ab_channel=CodeMonkey

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Policies;
using MBaske;

public class ContinuousAgent : Agent
{
    [Range(-1f, 1f)]
    public float ConstantSpeedRange = 0.2f;
    [Range(-1f, 1f)]
    public float ConstantTurnSpeedRange = -0.5f;
    [Range(0f, 1f)]
    public float StandingTurnSpeedRange = 0.5f;

    [Range(-1f, 1f)]
    public float SpeedRange;
    [Range(-1f, 1f)]
    public float WalkRange;
    [Range(-1f, 1f)]
    public float LookRange;
    public Transform rigTransform;
    public Transform goalTransform;
    public Vector3 WorldPosition => Util.Position(m_Walker.Matrix);
    public Vector3 WordForward => Util.Forward(m_Walker.Matrix);

    protected IControllableWalker m_Walker;
    protected float[] m_PrevActions;
    protected bool m_IsPreDecisionStep;

    protected string m_BehaviorName;
    protected MBaske.DecisionRequester m_Requester;
    protected bool IsActive => m_Requester.Active;

    public override void Initialize()
    {
        m_BehaviorName = GetComponent<BehaviorParameters>().BehaviorName;
        m_Requester = GetComponent<MBaske.DecisionRequester>();
        m_Walker = GetComponentInChildren<IControllableWalker>();

        if (m_Requester == null)
        {
            m_Requester = gameObject.AddComponent<MBaske.DecisionRequester>();
        }
        SetAgentActive(true);
    }

    public virtual void SetAgentActive(bool active, int decisionInterval = 0)
    {
        m_Requester.Active = active;
        m_Requester.DecisionInterval = decisionInterval > 0 ? decisionInterval : m_Requester.DecisionInterval;
        m_Requester.PerStepActions = m_Requester.DecisionInterval > 1;

        // m_Walker.SetAgentActive(active);
    }

    public override void OnEpisodeBegin()
    {
        // Debug.Log("New Episode");
        // rigTransform.localPosition = new Vector3(Random.Range(0f, +4f), 0, Random.Range(-3f, 3f));
        goalTransform.localPosition = new Vector3(Random.Range(-3f, 3f), 0, Random.Range(1f, 3f));

        m_Walker.ResetAgent();
        m_PrevActions = new float[3];
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(rigTransform.localPosition.x);
        sensor.AddObservation(rigTransform.localPosition.z);
        sensor.AddObservation(goalTransform.localPosition.x);
        sensor.AddObservation(goalTransform.localPosition.z);

        sensor.AddObservation(m_PrevActions);
    }
    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        AddReward(-1f / MaxStep);
        var actions = actionBuffers.ContinuousActions.Array;
        var step = m_Requester.ActionStepCount % m_Requester.DecisionInterval;
        m_IsPreDecisionStep = step == 0;

        if (m_IsPreDecisionStep)
        {
            System.Array.Copy(actions, 0, m_PrevActions, 0, actions.Length);
        }
        else
        {
            float t = step / (float)m_Requester.DecisionInterval;
            for (int i = 0; i < actions.Length; i++)
            {
                actions[i] = Mathf.Lerp(m_PrevActions[i], actions[i], t);
            }
        }

        m_Walker.NormTargetSpeed = actions[0];
        m_Walker.NormTargetWalkAngle = actions[1];
        m_Walker.NormTargetLookAngle = actions[2];
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<float> continuousActions = actionsOut.ContinuousActions;

        if (Input.GetAxisRaw("Horizontal") == 1f)
        {
            if (Input.GetAxisRaw("Vertical") == 1f)
            {
                SpeedRange = ConstantSpeedRange;
                LookRange = 0.2f;
            }
            else if (Input.GetAxisRaw("Vertical") == 0f)
            {
                SpeedRange = ConstantTurnSpeedRange;
                LookRange = StandingTurnSpeedRange;
            }
            else if (Input.GetAxisRaw("Vertical") == -1f)
            {
                SpeedRange = ConstantTurnSpeedRange;
                LookRange = 0.2f;
            }
        }
        else if (Input.GetAxisRaw("Horizontal") == 0f)
        {
            if (Input.GetAxisRaw("Vertical") == 1f)
            {
                SpeedRange = ConstantSpeedRange;
                LookRange = 0f;
            }
            else if (Input.GetAxisRaw("Vertical") == 0f)
            {
                SpeedRange = -1f;
                LookRange = 0f;
            }
            else if (Input.GetAxisRaw("Vertical") == -1f)
            {
                SpeedRange = -1f;
                LookRange = 0f;
            }
        }
        else if (Input.GetAxisRaw("Horizontal") == -1f)
        {
            if (Input.GetAxisRaw("Vertical") == 1f)
            {
                SpeedRange = ConstantSpeedRange;
                LookRange = -0.2f;
            }
            else if (Input.GetAxisRaw("Vertical") == 0f)
            {
                SpeedRange = ConstantTurnSpeedRange;
                LookRange = -1f * StandingTurnSpeedRange;
            }
            else if (Input.GetAxisRaw("Vertical") == -1f)
            {
                SpeedRange = ConstantTurnSpeedRange;
                LookRange = -0.2f;
            }
        }

        continuousActions[0] = SpeedRange;
        continuousActions[1] = WalkRange;
        continuousActions[2] = LookRange;
    }
}
