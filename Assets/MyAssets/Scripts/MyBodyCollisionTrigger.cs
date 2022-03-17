using System;
using UnityEngine;

namespace MBaske
{
    public class MyBodyCollisionTrigger : MonoBehaviour
    {
        public MyAgent myAgent;
        public WalkerAgent myWalkInference;
        public ArticulationBody m_ArtBody;
        public MeshRenderer groundMeshRenderer;
        public Material Green_Material;
        public Material Red_Material;
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

            if (other.gameObject.layer == Layer.Obstacle)
            {
                m_ArtBody.immovable = true;
                groundMeshRenderer.material = Red_Material;
                myAgent.AddReward(-1f);
                myWalkInference.SetAgentActive(false);

                myAgent.EndEpisode();
            }

            if (other.gameObject.tag == "goal")
            {
                m_ArtBody.immovable = true;
                groundMeshRenderer.material = Green_Material;
                myAgent.AddReward(+1f);
                myWalkInference.SetAgentActive(false);

                myAgent.EndEpisode();
            }
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
