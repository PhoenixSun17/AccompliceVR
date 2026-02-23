using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalSync : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField]
    public GameObject portal;

    // Update is called once per frame
    void Update()
    {
        this.transform.rotation = portal.transform.rotation;
    }
}
