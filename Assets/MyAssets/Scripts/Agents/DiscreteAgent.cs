// https://www.youtube.com/watch?v=zPFU30tbyKs&list=PLzDRvYVwl53vehwiN_odYJkPBzcqFw110&index=1&ab_channel=CodeMonkey

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Policies;
using MBaske;

public class DiscreteAgent : Agent
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
    public float dist = 10f;
    public float angle = 0f;
    public Transform[] DestinationTransforms;

    public bool training = false;

    public int DestinationQueue = 0;
    public Queue<string> DestinationQueueQ = new Queue<string>();
    public Text DestinationQueueText;
    public GameObject Goal_go;
    public GameObject ThePackage_go;
    public Transform rigTransform;
    public Transform goalTransform;
    public ArticulationBody Dummy_Artic;
    public Transform TrainingAreaTransform;
    public DontRotate dontRotate;
    public Vector3 WorldPosition => Util.Position(m_Walker.Matrix);
    public Vector3 WordForward => Util.Forward(m_Walker.Matrix);

    protected IControllableWalker m_Walker;
    protected float[] m_PrevActions;
    protected bool m_IsPreDecisionStep;

    protected string m_BehaviorName;
    protected MBaske.DecisionRequester m_Requester;
    protected bool IsActive => m_Requester.Active;

    void Start()
    {
        if (training == false)
        {
            DestinationTransforms[0].gameObject.SetActive(true);
            DestinationTransforms[1].gameObject.SetActive(true);
            DestinationTransforms[2].gameObject.SetActive(true);
            DestinationTransforms[3].gameObject.SetActive(true);
            DestinationTransforms[4].gameObject.SetActive(true);

            Goal_go.SetActive(false);

            DestinationQueueQ.Enqueue("A");
            DestinationQueueText.text = Queue_To_Array(DestinationQueueQ);
        }
        else
        {
            DestinationTransforms[0].gameObject.SetActive(false);
            DestinationTransforms[1].gameObject.SetActive(false);
            DestinationTransforms[2].gameObject.SetActive(false);
            DestinationTransforms[3].gameObject.SetActive(false);
            DestinationTransforms[4].gameObject.SetActive(false);

            Goal_go.SetActive(true);
        }
    }

    void Update()
    {
        if (training == false)
        {
            if (Input.GetKeyDown(KeyCode.A))
            {
                DestinationQueueQ.Enqueue("A");
                DestinationQueueText.text = Queue_To_Array(DestinationQueueQ);
            }
            else if (Input.GetKeyDown(KeyCode.B))
            {
                DestinationQueueQ.Enqueue("B");
                DestinationQueueText.text = Queue_To_Array(DestinationQueueQ);
            }
            else if (Input.GetKeyDown(KeyCode.C))
            {
                DestinationQueueQ.Enqueue("C");
                DestinationQueueText.text = Queue_To_Array(DestinationQueueQ);
            }
            else if (Input.GetKeyDown(KeyCode.Q))
            {
                Debug.Log("Peek: " + DestinationQueueQ.Peek());
                Debug.Log("Total elements: " + DestinationQueueQ.Count);
            }

            if (DestinationQueueQ.Count == 0)
            {
                goalTransform = DestinationTransforms[4];
            }
            else
            {
                if (ThePackage_go.activeInHierarchy == false)
                {
                    goalTransform = DestinationTransforms[0];
                }
                else
                {
                    if (DestinationQueueQ.Peek() == "A")
                    {
                        goalTransform = DestinationTransforms[1];
                    }
                    else if (DestinationQueueQ.Peek() == "B")
                    {
                        goalTransform = DestinationTransforms[2];
                    }
                    else if (DestinationQueueQ.Peek() == "C")
                    {
                        goalTransform = DestinationTransforms[3];
                    }
                }
            }
        }
        else
        {
            goalTransform = Goal_go.transform;
        }
    }

    public string Queue_To_Array(Queue<string> strQ)
    {
        string[] strQ_to_array = strQ.ToArray();
        string output_str = "Package Queue : ";

        for (int i = 0; i < strQ_to_array.Length; i++)
        {
            output_str = output_str + strQ_to_array[i];
            // Debug.Log(strQ_to_array[i]);
        }

        return output_str;
    }

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

        // For 
        Goal_go.transform.localPosition = new Vector3(Random.Range(-7.75f, 7.75f), 0, Random.Range(-7.75f, 7.75f));

        Dummy_Artic.TeleportRoot(new Vector3(Random.Range(-7.75f, 6f) + TrainingAreaTransform.position.x, 0.25f + TrainingAreaTransform.position.y, Random.Range(-7.75f, 7.75f) + TrainingAreaTransform.position.z), Quaternion.Euler(new Vector3(0f, Random.Range(0f, 360f), 0f)));

        // dummyTransform.localPosition = new Vector3(Random.Range(-3f, 3f), 0.25f, Random.Range(-3f, 3f));
        // dummyTransform.rotation = Quaternion.Euler(new Vector3(0f, Random.Range(0f, 360f), 0f));

        m_Walker.ResetAgent();
        m_PrevActions = new float[2];
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(dontRotate.transform.localPosition.x);
        sensor.AddObservation(dontRotate.transform.localPosition.z);
        sensor.AddObservation(goalTransform.localPosition.x);
        sensor.AddObservation(goalTransform.localPosition.z);

        sensor.AddObservation(dist);

        sensor.AddObservation(angle);

        // sensor.AddObservation(m_PrevActions);

        // sensor.AddObservation(dontRotate.goalOnTheWay);
    }
    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        // Debug.Log(GetCumulativeReward());

        Vector3 Spot_To_Goal_Direction = new Vector3(goalTransform.position.x - dontRotate.transform.position.x, goalTransform.position.y - dontRotate.transform.position.y, goalTransform.position.z - dontRotate.transform.position.z);
        Vector3 Spot_To_Forward_Direction = dontRotate.transform.forward;

        dist = (float)System.Math.Round(Vector3.Distance(goalTransform.position, dontRotate.transform.position) / 20f, 3);
        angle = (float)System.Math.Round(Vector3.SignedAngle(Spot_To_Goal_Direction, Spot_To_Forward_Direction, Vector3.up) / 180f, 3);

        // Debug.Log(dist);
        // Debug.Log(angle);

        if (MaxStep > 0)
        {
            if (Mathf.Abs(angle) < 0.05f)
            {
                AddReward(1f / MaxStep);
            }
            else if (Mathf.Abs(angle) < 0.25f)
            {
                AddReward(0.33f / MaxStep);
            }
            else if (Mathf.Abs(angle) < 0.5f)
            {
                AddReward(0.1f / MaxStep);
            }
            else if (Mathf.Abs(angle) < 0.75f)
            {
                AddReward(0.05f / MaxStep);
            }
            // -0.001
            AddReward(-1f / MaxStep);
        }


        var DisActions = actionBuffers.DiscreteActions.Array;
        float[] convertContinuous = new float[2];

        if (DisActions[0] == 0)
        {
            convertContinuous[0] = -1f;
            convertContinuous[1] = 0f;
        }
        else if (DisActions[0] == 1)
        {
            convertContinuous[0] = -0.5f;
            convertContinuous[1] = 0.5f;
        }
        else if (DisActions[0] == 2)
        {
            convertContinuous[0] = -0.5f;
            convertContinuous[1] = -0.5f;
        }
        else if (DisActions[0] == 3)
        {
            convertContinuous[0] = 0.2f;
            convertContinuous[1] = 0f;
        }

        var step = m_Requester.ActionStepCount % m_Requester.DecisionInterval;
        m_IsPreDecisionStep = step == 0;

        if (m_IsPreDecisionStep)
        {
            System.Array.Copy(convertContinuous, 0, m_PrevActions, 0, convertContinuous.Length);
        }
        else
        {
            float t = step / (float)m_Requester.DecisionInterval;
            for (int i = 0; i < convertContinuous.Length; i++)
            {
                convertContinuous[i] = Mathf.Lerp(m_PrevActions[i], convertContinuous[i], t);
            }
        }

        m_Walker.NormTargetSpeed = convertContinuous[0];
        m_Walker.NormTargetWalkAngle = 0f;
        m_Walker.NormTargetLookAngle = convertContinuous[1];

        // Debug.Log("DisActions[0] : " + DisActions[0] + "\tSpeed : " + m_Walker.NormTargetSpeed + "\tLookAngle : " + m_Walker.NormTargetLookAngle + "\tWalkAngle : " + m_Walker.NormTargetWalkAngle);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<int> discreteActions = actionsOut.DiscreteActions;

        // 0 - Nothing
        // 1 - Turn Right
        // 2 - Turn Left
        // 3 - Forward

        if (Input.GetAxisRaw("Horizontal") == 1f)
        {
            if (Input.GetAxisRaw("Vertical") == 1f)
            {
                // SpeedRange = ConstantSpeedRange;
                // LookRange = 0f;
                // 3 - Forward
                discreteActions[0] = 3;
                // Speed = 0.2f;
                // Look = 0f;

            }
            else if (Input.GetAxisRaw("Vertical") == 0f)
            {
                // SpeedRange = ConstantTurnSpeedRange;
                // LookRange = StandingTurnSpeedRange;
                // 1 - Turn Right                
                discreteActions[0] = 1;
                // Speed = -0.5f;
                // Look = 0.5f;
            }
            else if (Input.GetAxisRaw("Vertical") == -1f)
            {
                // SpeedRange = ConstantTurnSpeedRange;
                // LookRange = 0.2f;
                // 1 - Turn Right
                discreteActions[0] = 1;
                // Speed = -0.5f;
                // Look = 0.2f;
            }
        }
        else if (Input.GetAxisRaw("Horizontal") == 0f)
        {
            if (Input.GetAxisRaw("Vertical") == 1f)
            {
                // SpeedRange = ConstantSpeedRange;
                // LookRange = 0f;
                // 3 - Forward
                discreteActions[0] = 3;
                // Speed = 0.2f;
                // Look = 0f;
            }
            else if (Input.GetAxisRaw("Vertical") == 0f)
            {
                // SpeedRange = -1f;
                // LookRange = 0f;
                // 0 - Nothing
                discreteActions[0] = 0;
                // Speed = -1f;
                // Look = 0f;
            }
            else if (Input.GetAxisRaw("Vertical") == -1f)
            {
                // SpeedRange = -1f;
                // LookRange = 0f;
                // 0 - Nothing
                discreteActions[0] = 0;
                // Speed = -1f;
                // Look = 0f;
            }
        }
        else if (Input.GetAxisRaw("Horizontal") == -1f)
        {
            if (Input.GetAxisRaw("Vertical") == 1f)
            {
                // SpeedRange = ConstantSpeedRange;
                // LookRange = 0f;
                // 3 - Forward
                discreteActions[0] = 3;
                // Speed = 0.2f;
                // Look = 0f;
            }
            else if (Input.GetAxisRaw("Vertical") == 0f)
            {
                // SpeedRange = ConstantTurnSpeedRange;
                // LookRange = -1f * StandingTurnSpeedRange;
                // 2 - Turn Left
                discreteActions[0] = 2;
                // Speed = -0.5f;
                // Look = -0.5f;
            }
            else if (Input.GetAxisRaw("Vertical") == -1f)
            {
                // SpeedRange = ConstantTurnSpeedRange;
                // LookRange = -1f * StandingTurnSpeedRange;
                // 2 - Turn Left
                discreteActions[0] = 2;
                // Speed = -0.5f;
                // Look = -0.5f;
            }
        }

        // continuousActions[0] = SpeedRange;
        // continuousActions[1] = WalkRange;
        // continuousActions[2] = LookRange;
    }
}
