using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    [SerializeField] private Transform previousRoom;
    [SerializeField] private Transform nextRoom;
    [SerializeField] private CameraController cam;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Check if the colliding object has the "Player" tag
        if (collision.CompareTag("Player"))
        {
            // Ensure the CameraController reference is assigned
            if (cam == null)
            {
                Debug.LogError("CameraController reference is not assigned in the Door script.");
                return;
            }

            // Determine which room to move to based on the player's position
            if (collision.transform.position.x < transform.position.x)
            {
                if (nextRoom != null)
                {
                    cam.MoveToNewRoom(nextRoom);
                }
                else
                {
                    Debug.LogWarning("Next room is not assigned in the Door script.");
                }
            }
            else
            {
                if (previousRoom != null)
                {
                    cam.MoveToNewRoom(previousRoom);
                }
                else
                {
                    Debug.LogWarning("Previous room is not assigned in the Door script.");
                }
            }
        }
    }
}