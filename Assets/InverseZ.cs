using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InverseZ : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 currentEulerAngles = transform.localEulerAngles;

        // 2. Modify the roll (z-axis) by negating it
        float newRoll = -currentEulerAngles.z;

        // 3. Write the new Euler angles back to the transform
        // We use the original pitch (x) and yaw (y)
        transform.localEulerAngles = new Vector3(currentEulerAngles.x, currentEulerAngles.y, newRoll);
    }
}
