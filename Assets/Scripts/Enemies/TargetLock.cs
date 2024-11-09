using UnityEngine;
using Cinemachine;
using System.Collections.Generic;
using System.Linq;

public class TargetLock : MonoBehaviour
{
    public float lockRadius = 10f;                       // Радиус для поиска врагов
    public LayerMask enemyLayer;                         // Слой для врагов
    public LayerMask obstacleLayer;                      // Слой для препятствий (например, buildings)
    public KeyCode lockKey = KeyCode.Q;                  // Клавиша для активации таргет лока
    public CinemachineVirtualCamera virtualCamera;       // Ссылка на виртуальную камеру
    public Transform playerTransform;                    // Ссылка на трансформ игрока
    private Transform currentTarget;                     // Текущий таргет
	public CameraCursor _cameraCursor;
	public CameraRotation _cameraRotation;
    public bool isLockedOn;                           // Флаг таргет лока
    private float rotationSpeed = 2f;                    // Скорость вращения игрока

	void Start()
	{
		_cameraCursor = GetComponent<CameraCursor>();
		_cameraRotation = GetComponent<CameraRotation>();
	}

    void Update()
    {
        if (Input.GetKeyDown(lockKey))
        {
            if (!isLockedOn)
            {
                LockOnTarget();
            }
            else
            {
                ReleaseTarget();
            }
        }

        // Поворот игрока в сторону цели, если таргет лок активен
        if (isLockedOn && currentTarget != null)
        {
			float distanceToTarget = Vector3.Distance(playerTransform.position, currentTarget.position);
			if(distanceToTarget > lockRadius)
			{
				ReleaseTarget();
			}

			else
			{
				RotatePlayerTowardsTarget();
			} 
        }
    }

    void LockOnTarget()
    {
        Collider[] targetsInRange = Physics.OverlapSphere(playerTransform.position, lockRadius, enemyLayer);

        // Фильтруем цели с проверкой через рейкаст
        List<Transform> validTargets = new List<Transform>();
        foreach (var target in targetsInRange)
        {
            Vector3 directionToTarget = target.transform.position - playerTransform.position;
            float distanceToTarget = Vector3.Distance(playerTransform.position, target.transform.position);

            // Проверяем, что между игроком и целью нет препятствий
            if (!Physics.Raycast(playerTransform.position, directionToTarget, distanceToTarget, obstacleLayer))
            {
                validTargets.Add(target.transform);
            }
        }

        // Находим ближайшую цель среди валидных
        currentTarget = validTargets.OrderBy(t => Vector3.Distance(playerTransform.position, t.position)).FirstOrDefault();

        if (currentTarget != null)
        {
            isLockedOn = true;
			_cameraCursor.enabled = false;
			_cameraRotation.enabled	= false;
            virtualCamera.LookAt = currentTarget;  // Устанавливаем цель как LookAt
        }
		
    }

    void ReleaseTarget()
    {
        isLockedOn = false;
        currentTarget = null;
		_cameraCursor.enabled = true;
		_cameraRotation.enabled	= true;
        virtualCamera.LookAt = playerTransform;  // Возвращаем LookAt на игрока
    }

    // Функция для поворота игрока к цели
    void RotatePlayerTowardsTarget()
    {
        if (currentTarget != null)
        {
            // Вычисляем направление на врага
            Vector3 directionToTarget = currentTarget.position - playerTransform.position;
            directionToTarget.y = 0; // Игнорируем изменение по оси Y, чтобы поворачивать только по горизонтали

            if (directionToTarget.sqrMagnitude > 0.01f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
                playerTransform.rotation = Quaternion.Slerp(playerTransform.rotation, targetRotation, Time.deltaTime * rotationSpeed); // Плавный поворот игрока
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        // Отображаем радиус лока
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(playerTransform.position, lockRadius);
    }
}
