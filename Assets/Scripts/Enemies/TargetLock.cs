using UnityEngine;
using Cinemachine;
using System.Collections.Generic;
using System.Linq;

public class TargetLock : MonoBehaviour
{
	public float lockRadius = 10f;                       // Радиус для поиска врагов
	public LayerMask enemyLayer;                         // Слой для врагов
	public LayerMask obstacleLayer;                      // Слой для препятствий (например, buildings)
	public LayerMask additionalObstacleLayers;           // Дополнительные слои препятствий
	public KeyCode lockKey = KeyCode.Q;                  // Клавиша для активации таргет лока
	public CinemachineVirtualCamera virtualCamera;       // Ссылка на виртуальную камеру
	public Transform playerTransform;                    // Ссылка на трансформ игрока

	private Transform currentTarget;                     // Текущий таргет
	private bool isLockedOn;                             // Флаг таргет лока
	private Vector3 previousTargetPosition;              // Предыдущая позиция врага для отслеживания движения

	private float rotationSpeed = 2f;                    // Скорость вращения камеры и игрока

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

		// Обновляем поворот камеры, если враг движется
		if (isLockedOn && currentTarget != null)
		{
			UpdateCameraRotation();

			// Рейкаст от камеры к цели, если таргет лок активен
			RaycastAndAdjustCamera();

			// Поворот игрока в сторону цели
			RotatePlayerTowardsTarget();
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
			if (!Physics.Raycast(playerTransform.position, directionToTarget, distanceToTarget, obstacleLayer | additionalObstacleLayers))
			{
				validTargets.Add(target.transform);
			}
		}

		// Находим ближайшую цель среди валидных
		currentTarget = validTargets.OrderBy(t => Vector3.Distance(playerTransform.position, t.position)).FirstOrDefault();

		if (currentTarget != null)
		{
			isLockedOn = true;
			virtualCamera.LookAt = currentTarget;  // Устанавливаем цель как LookAt
			previousTargetPosition = currentTarget.position;  // Сохраняем текущую позицию цели
		}
	}

	void ReleaseTarget()
	{
		isLockedOn = false;
		currentTarget = null;
		virtualCamera.LookAt= playerTransform;  // Возвращаем LookAt на игрока
	}

	void UpdateCameraRotation()
	{
		Vector3 targetMovementDirection = currentTarget.position - previousTargetPosition;
		
		// Проверяем, движется ли цель
		if (targetMovementDirection.sqrMagnitude > 0.01f)
		{
			Quaternion targetRotation = Quaternion.LookRotation(targetMovementDirection);
			virtualCamera.transform.rotation = Quaternion.Slerp(
				virtualCamera.transform.rotation, 
				targetRotation, 
				Time.deltaTime * rotationSpeed);  // Плавное поворачивание за врагом
		}

		previousTargetPosition = currentTarget.position;  // Обновляем позицию врага
	}

	void RaycastAndAdjustCamera()
	{
		Vector3 directionToTarget = currentTarget.position - virtualCamera.transform.position;
		Ray ray = new Ray(virtualCamera.transform.position, directionToTarget);
		// Проверка наличия препятствий между камерой и целью
        if (Physics.Raycast(ray, out RaycastHit hit, directionToTarget.magnitude, obstacleLayer | additionalObstacleLayers))
        {
            // Если есть препятствие, корректируем угол поворота камеры
            Vector3 hitNormal = hit.normal;
            Quaternion adjustedRotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(directionToTarget, hitNormal));
            virtualCamera.transform.rotation = Quaternion.Slerp(
                virtualCamera.transform.rotation, 
                adjustedRotation, 
                Time.deltaTime * rotationSpeed);  // Плавное вращение камеры вокруг оси
        }

        // Отрисовка рейкаста для отладки
        Debug.DrawRay(virtualCamera.transform.position, directionToTarget, Color.red);
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
                playerTransform.rotation = Quaternion.Slerp(playerTransform.rotation, targetRotation, Time.deltaTime); // Плавный поворот игрока
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