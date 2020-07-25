using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowCam : MonoBehaviour
{
    public bool slowFollow;
    public Transform target;
    public Vector3 offset;

    private void Awake()
    {
        slowFollow = true;
    }

    void FixedUpdate()
    {
        if(slowFollow==true)
        {
            if (target != null && ((target.position.y - transform.position.y > 2.5) || (target.position.y - transform.position.y < -2.5)))
            {
                transform.position = new Vector3(target.position.x, target.position.y, offset.z);
            }


            if (target != null)
            {
                if (target.position.y - transform.position.y > 0.1f)
                {
                    transform.position = new Vector3(target.position.x + offset.x, transform.position.y + 0.02f, offset.z);
                }
                else if (target.position.y - transform.position.y < -0.1f)
                {
                    transform.position = new Vector3(target.position.x + offset.x, transform.position.y - 0.02f, offset.z);
                }
                else
                {
                    transform.position = new Vector3(target.position.x + offset.x, transform.position.y, offset.z);
                }
            }
        }
        else
        {
            transform.position = new Vector3(target.position.x, target.position.y, offset.z);
        }
    

    }


}
