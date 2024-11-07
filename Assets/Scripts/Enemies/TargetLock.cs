using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TargetLock : MonoBehaviour
{
   	public Transform player; // Ссылка на игрока
	public Transform lockedTarget; // Ссылка на выбранного врага
	public float smoothSpeed = 0.125f; // Скорость плавного перехода
	public Vector3 offset = new Vector3(0, 2, -5); // Смещение камеры позади игрока
	public LayerMask enemyLayer; // Слой врагов для поиска
	
	[SerializeField] private float lockRange = 15f; // Дистанция захвата цели
	private bool isTargetLocked = false;
	
	
	
	[Header("Indicator")]
	public GameObject targetIndicatorPrefab; // Префаб иконки
	
	
	
	[SerializeField] private GameObject _targetCanvas;	
	public Text targetLockText;


	void Update()
	{
		if (Input.GetKeyDown(KeyCode.Q)) // Нажатие Q для включения/выключения Target Lock
		{
			if (!isTargetLocked)
			{
				FindTarget(); // Найти ближайшего врага
			}
			else
			{
				ReleaseTarget(); // Снять цель
			}
		}

		if (isTargetLocked && lockedTarget != null)
		{
			SmoothFollowTarget();
			Debug.Log("SMMMMMOOOOOOTHHHHHH!!!!!");
		}
	}

	void FindTarget()
	{
		// Найти ближайшего врага в пределах диапазона
		Collider[] enemiesInRange = Physics.OverlapSphere(player.position, lockRange, enemyLayer);
		float closestDistance = Mathf.Infinity;
		Transform closestEnemy = null;

		foreach (Collider enemy in enemiesInRange)
		{
			float distance = Vector3.Distance(player.position, enemy.transform.position);
			
			if (distance < closestDistance)
			{
				closestDistance = distance;
				closestEnemy = enemy.transform;
			}
		}

		if (closestEnemy != null)
		{
			lockedTarget = closestEnemy;
			isTargetLocked = true;
			Debug.Log("GGGGGGGGGGGGGGGGGGGGGGGGGGGGGG");
			targetLockText.text = "Target lock: ON";
			// ToggleTargetIndicator(lockedTarget, true); // Включаем индикатор цели
		}
	}

	void ReleaseTarget()
	{
		if (lockedTarget != null)
		{
			Debug.Log("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF");
			targetLockText.text = "Target lock: OFF";
			// ToggleTargetIndicator(lockedTarget, false); // Выключаем индикатор цели
			lockedTarget = null;
		}
		
		isTargetLocked = false;
	}

	void SmoothFollowTarget()
	{
		// Плавный поворот игрока к цели, оставляя камеру на той же высоте
		Vector3 targetDirection = new Vector3(lockedTarget.position.x - player.position.x, 0, lockedTarget.position.z - player.position.z);
		Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
		player.rotation = Quaternion.Slerp(player.rotation, targetRotation, smoothSpeed * Time.deltaTime);
	}

	void ToggleTargetIndicator(Transform target, bool enable)
	{
		Canvas targetCanvas = target.GetComponentInChildren<Canvas>();
		if (targetCanvas != null)
		{
			targetCanvas.enabled = enable;
		}
	}
}
