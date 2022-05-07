using System;
using UnityEngine;

namespace MBaske
{
    public class DiscreteMyBodyCollisionTrigger : MonoBehaviour
    {
        public bool endEpisode_control = true;
        public DiscreteAgent myAgent;
        public WalkerAgent myWalkInference;
        public ArticulationBody m_ArtBody;
        public MeshRenderer groundMeshRenderer;
        public Material Green_Material;
        public Material Red_Material;
        public GameObject ThePackage_go;
        public event Action<int, bool> CollisionEvent;

        private void OnTriggerEnter(Collider other)
        {
            // Debug.Log(other.gameObject.name);
            CheckCollision(other.gameObject.layer, true);

            // CHANGED invoked by bullet.
            //if (other.gameObject.layer == Layer.Bullet)
            //{
            //    if (!transform.CompareTag(other.GetComponent<Bullet>().AgentTag))
            //    {
            //        // Ignores friendly fire.
            //        BulletHitSufferedEvent?.Invoke();
            //    }
            //}

            if (other.gameObject.layer == Layer.Obstacle || other.gameObject.layer == Layer.Trigger)
            {
                // if (endEpisode_control)
                if (myAgent.training == true)
                {
                    m_ArtBody.immovable = true;
                    groundMeshRenderer.material = Red_Material;
                    myAgent.AddReward(-1f);
                    myWalkInference.SetAgentActive(false);

                    myAgent.EndEpisode();
                }
            }

            if (other.gameObject.tag == "goal")
            {
                // if (endEpisode_control)
                if (myAgent.training == true)
                {
                    // Debug.Log("GOAAAAAAAAL");
                    m_ArtBody.immovable = true;
                    groundMeshRenderer.material = Green_Material;
                    myAgent.AddReward(+1f);
                    myWalkInference.SetAgentActive(false);

                    myAgent.EndEpisode();
                }
            }

            if (other.gameObject.tag == "destination")
            {
                if (myAgent.goalTransform.gameObject.name == other.gameObject.name)
                {
                    if (ThePackage_go.activeInHierarchy == true)
                    {
                        if (other.gameObject.name == "A" || other.gameObject.name == "B" || other.gameObject.name == "C")
                        {
                            myAgent.DestinationQueueQ.Dequeue();
                            myAgent.DestinationQueueText.text = myAgent.Queue_To_Array(myAgent.DestinationQueueQ);
                            ThePackage_go.SetActive(false);
                        }
                    }
                    else
                    {
                        if (other.gameObject.name == "P")
                        {
                            ThePackage_go.SetActive(true);
                        }
                    }
                }
            }

            // if (other.gameObject.tag == "destination")
            // {
            //     if (myAgent.DestinationQueue == 0 && other.gameObject.name == "P")
            //     {
            //         Debug.Log("0");
            //         myAgent.goalTransform = myAgent.DestinationTransforms[1];
            //         ThePackage_go.SetActive(true);

            //         if (myAgent.DestinationQueue < 6)
            //         {
            //             myAgent.DestinationQueue++;
            //         }
            //         else
            //         {
            //             myAgent.DestinationQueue = 0;
            //         }
            //     }
            //     else if (myAgent.DestinationQueue == 1 && other.gameObject.name == "A")
            //     {
            //         Debug.Log("1");
            //         myAgent.goalTransform = myAgent.DestinationTransforms[0];
            //         ThePackage_go.SetActive(false);

            //         if (myAgent.DestinationQueue < 6)
            //         {
            //             myAgent.DestinationQueue++;
            //         }
            //         else
            //         {
            //             myAgent.DestinationQueue = 0;
            //         }
            //     }
            //     else if (myAgent.DestinationQueue == 2 && other.gameObject.name == "P")
            //     {
            //         Debug.Log("2");
            //         myAgent.goalTransform = myAgent.DestinationTransforms[2];
            //         ThePackage_go.SetActive(true);

            //         if (myAgent.DestinationQueue < 6)
            //         {
            //             myAgent.DestinationQueue++;
            //         }
            //         else
            //         {
            //             myAgent.DestinationQueue = 0;
            //         }
            //     }
            //     else if (myAgent.DestinationQueue == 3 && other.gameObject.name == "B")
            //     {
            //         Debug.Log("3");
            //         myAgent.goalTransform = myAgent.DestinationTransforms[0];
            //         ThePackage_go.SetActive(false);

            //         if (myAgent.DestinationQueue < 6)
            //         {
            //             myAgent.DestinationQueue++;
            //         }
            //         else
            //         {
            //             myAgent.DestinationQueue = 0;
            //         }
            //     }
            //     else if (myAgent.DestinationQueue == 4 && other.gameObject.name == "P")
            //     {
            //         Debug.Log("4");
            //         myAgent.goalTransform = myAgent.DestinationTransforms[3];
            //         ThePackage_go.SetActive(true);

            //         if (myAgent.DestinationQueue < 6)
            //         {
            //             myAgent.DestinationQueue++;
            //         }
            //         else
            //         {
            //             myAgent.DestinationQueue = 0;
            //         }
            //     }
            //     else if (myAgent.DestinationQueue == 5 && other.gameObject.name == "C")
            //     {
            //         Debug.Log("5");
            //         myAgent.goalTransform = myAgent.DestinationTransforms[4];
            //         ThePackage_go.SetActive(false);

            //         if (myAgent.DestinationQueue < 6)
            //         {
            //             myAgent.DestinationQueue++;
            //         }
            //         else
            //         {
            //             myAgent.DestinationQueue = 0;
            //         }
            //     }
            //     else if (myAgent.DestinationQueue == 6 && other.gameObject.name == "X")
            //     {
            //         Debug.Log("6");
            //         myAgent.goalTransform = myAgent.DestinationTransforms[0];

            //         if (myAgent.DestinationQueue < 6)
            //         {
            //             myAgent.DestinationQueue++;
            //         }
            //         else
            //         {
            //             myAgent.DestinationQueue = 0;
            //         }
            //     }
            // }
        }

        private void OnTriggerStay(Collider other)
        {
            CheckCollision(other.gameObject.layer, false);
        }

        private void CheckCollision(int layer, bool isEnter)
        {
            if (layer == Layer.Obstacle || layer == Layer.Trigger)
            {
                CollisionEvent?.Invoke(layer, isEnter);
            }
        }
    }
}
