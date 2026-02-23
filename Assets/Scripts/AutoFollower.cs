using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoFollower : MonoBehaviour
{
    public GameObject projectorCamera;
    public Transform selfTransform;

    public bool isFollowing = false;
    //LayerMask layerMask = LayerMask.GetMask("Calibrator");
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            isFollowing = !isFollowing;
            Debug.Log("Follow State changed");
        }
        if (isFollowing)
        {
            //Debug.Log("Following");
            RaycastHit hit;
            Physics.Raycast(projectorCamera.transform.position, projectorCamera.transform.TransformDirection(Vector3.forward), out hit, Mathf.Infinity);
            //Debug.Log(hit.point);
            Debug.DrawLine(projectorCamera.transform.position, hit.point,Color.red,1000);
            selfTransform.LookAt(hit.point);
        }
    }

}
