using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerialiseFIeld] private float speed;
    private float currentPosX;
    private Vector3 velocity = Vector3.zero;
    // Start is called before the first frame update
    private void update
    {
        transform.position = Vector3.SmoothDamp(transform.position, new Vector3(currentPosX, transform.position.y, transform.position.z), ref velocity, speed * Time.deltaTime);
    }
}

    public void MoveToNewRoom(Transform_newRoom)
    {
    currentPosX = MoveToNewRoom.position.x;
    }