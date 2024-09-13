using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class CameraRotation : MonoBehaviour
{
    public CinemachineVirtualCamera playerCamera;
    public Enemy enemy;
    public Player player;

    public float sensitivity = 500f;                // Чувствительность вращения
    private float rotationX = 0f;
    private float rotationY = 0f;
    private Transform cameraPos;

    void Start()
    {
        cameraPos = playerCamera.transform;
        Cursor.visible = false;

        // Сохраняем начальное вращение камеры
        Vector3 initialRotation = cameraPos.rotation.eulerAngles;
        rotationX = initialRotation.x; 
        rotationY = initialRotation.y;
    }

    void Update()
    {
        if(Input.GetMouseButton(1))
        {
            Cursor.lockState = CursorLockMode.Locked;   
            float mouseX = Input.GetAxis("Mouse X") * sensitivity * Time.deltaTime;
            rotationY += mouseX;
            cameraPos.rotation = Quaternion.Euler(rotationX, rotationY, 0f);
        }
        
        else
        {
            Cursor.lockState = CursorLockMode.None;
        }

        if(player.isDead)
        {
            playerCamera.Follow = enemy.transform;
        }
    }
}
