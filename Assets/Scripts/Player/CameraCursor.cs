using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraCursor : MonoBehaviour
{
    private Camera mainCamera;
    public float rotationSpeed; 


    private Quaternion targetRotation;

    void Start()
    {
        mainCamera = Camera.main;
        targetRotation = transform.rotation;
    }

    void Update()
    {
        if (mainCamera != null)
        {
            Plane plane = new Plane(Vector3.up, 0);
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

            if (plane.Raycast(ray, out float distance))
            {
                Vector3 cursorPosition = ray.GetPoint(distance);

                Vector3 direction = cursorPosition - transform.position;
                direction.y = 0; 

                if (direction != Vector3.zero)
                {
                    targetRotation = Quaternion.LookRotation(direction, Vector3.up);
                }
            }

            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }
}
