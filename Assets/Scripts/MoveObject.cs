
using UnityEngine;

public class MoveObject : MonoBehaviour
{
    public float moveSpeed = 5f;

    void Update()
    {
        // Get input from WASD or Arrow keys
        float moveX = Input.GetAxis("Horizontal"); // A/D or Left/Right Arrow
        float moveY = Input.GetAxis("Vertical");   // W/S or Up/Down Arrow

        // Create a movement vector
        Vector3 move = new Vector3(moveX, 0f, moveY);

        // Move the object
        transform.Translate(move * moveSpeed * Time.deltaTime, Space.World);
    }
}
