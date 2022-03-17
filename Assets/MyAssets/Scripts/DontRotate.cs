using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DontRotate : MonoBehaviour
{
    public Transform rigTransform;
    public DiscreteAgent myAgent;
    public float goalOnTheWay = -1f;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        transform.localPosition = rigTransform.localPosition;
        transform.localEulerAngles = new Vector3(0f, rigTransform.localEulerAngles.y, 0f);
    }


    // void FixedUpdate()
    // {
    //     RaycastHit hit;
    //     if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit))
    //     {
    //         if (hit.transform.tag == "goal")
    //         {
    //             // Debug.Log("Goal on the way");
    //             myAgent.AddReward(1f / myAgent.MaxStep);
    //             goalOnTheWay = 1f;
    //         }
    //         else
    //         {
    //             goalOnTheWay = -1f;
    //         }
    //     }
    // }
}
