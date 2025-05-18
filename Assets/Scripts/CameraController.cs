using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private float smoothTime = 0.3f; // Smooth time for camera movement
    [SerializeField] private Transform player; // Reference to the player

    private Vector3 velocity = Vector3.zero;
    private float targetPosX; // Target X position for snapping to a room

    private void Update()
    {
        if (player != null)
        {
            // Smoothly move the camera to follow the player's position
            Vector3 targetPosition = new Vector3(player.position.x, player.position.y, transform.position.z);
            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
        }
    }

    public void MoveToNewRoom(Transform newRoom)
    {
        if (newRoom != null)
        {
            // Snap the camera to the new room's position
            targetPosX = newRoom.position.x;
            transform.position = new Vector3(targetPosX, transform.position.y, transform.position.z);
        }
        else
        {
            Debug.LogWarning("MoveToNewRoom called with a null Transform.");
        }
    }
}