using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class Enemy : MonoBehaviour
{
	#region Fields

	[Header("Characteristics")]
	[SerializeField] protected float _enemyHealthPoints;
	[SerializeField] protected float _damage = 2f;
	public static float _enemyDamage; // для ссылки
	// public float Damage => _damage;


	[Header("Patrol")] protected Vector3 _currentPatrolPoint;
	[SerializeField] protected float _patrolPointRange = 15f;
	[SerializeField] protected bool _isPatrolPointSet;
	[SerializeField] protected float _patrolInterval = 5f;


	[Header("Chase")]
	[SerializeField] protected float _chaseRange = 3f;
	[SerializeField] private bool _playerInChaseRange;
	[SerializeField] private bool _isFollowingReset = false; // управление корутиной преследования 


	[Header("Attack")]
	[SerializeField] protected float _attackRange = 1f;
	[SerializeField] public bool _playerInAttackRange;
	public static bool _isAttack = false;
	[SerializeField] protected float _attackCooldown = 3f;
	[SerializeField] private bool _canAttack = true;
	// [SerializeField] protected bool _isAlreadyAttacked;
	[SerializeField] private bool _attackComplete = true;


	[SerializeField] protected bool _isParried = false; // Индивидуальная переменная для каждого врага
	[SerializeField] protected bool _hasBeenTargeted;
	public Text hasBeenTargetedText;
	

	[Header("Navigation")] 
	protected NavMeshAgent _agent;
	public LayerMask Ground, Player;
	protected Rigidbody _rgbd;
	protected Collider _collider;
	protected Transform _target;

	[SerializeField] protected Animator _animator;


	[Header("Movement Settings")]
	[SerializeField] protected float _patrolSpeed = 3f;
	[SerializeField] protected float _chaseSpeed = 5f;


	[Header("Vision Settings")]
	[SerializeField] private float _fieldOfView = 120f;
	[SerializeField] private bool _isVisible = false;
	[SerializeField] private float distanceToPlayer;
	[SerializeField] private LayerMask _obstacleMask;


	[Header("State")]
	public EnemyState currentEnemyState;


	[Header("Objects-links")]
	public AttackTrigger attackTrigger;
	private PlayerAttack _playerAttack;

	#endregion Fields


	public enum EnemyState
	{
		Patrolling,
		Chasing,
		Attacking,
		Parried,
		DeathState,
	}


	#region Events

	public delegate void ChaseEventHandler();
	public event ChaseEventHandler Chase;

	public delegate void AttackEventHandler();
	public event AttackEventHandler Attack;

	public delegate void RunEventHandler();
	public event RunEventHandler Run;

	public delegate void DeathEventHandler();
	public event DeathEventHandler Death;

	public delegate void IdleEventHandler();
	public event IdleEventHandler Idle;

	public delegate void ParriedEventHandler();
	public event ParriedEventHandler Parried;
	
	#endregion Events


	protected virtual void Awake()
	{
		_agent = GetComponent<NavMeshAgent>();
		_rgbd = GetComponent<Rigidbody>();
		_collider = GetComponent<Collider>();
		_animator = GetComponent<Animator>();
		attackTrigger = GetComponentInChildren<AttackTrigger>();
		_enemyDamage = _damage;
	}

	protected virtual void Start()
	{
		currentEnemyState = EnemyState.Patrolling;
		_agent.speed = _patrolSpeed;
		_hasBeenTargeted = false;

		try
		{
			_target = GameObject.FindWithTag("Player").transform;
		}
		catch
		{
			Debug.Log("Player's object not found!");
		}
	}

	protected virtual void FixedUpdate()
	{
		distanceToPlayer = Vector3.Distance(transform.position, _target.position);

		CheckForPlayerVisibility();

		DetermineCurrentState();

		switch (currentEnemyState)
		{
			case EnemyState.Patrolling:
				Patrolling();
				_agent.speed = _patrolSpeed;
				break;

			case EnemyState.Chasing:
				PlayerChase();
				_agent.speed = _chaseSpeed;
				break;

			case EnemyState.Attacking:
				if (_canAttack)
				{
					PlayerAttack();
				}
				break;

			case EnemyState.DeathState:
				EnemyDeath();
				break;

			case EnemyState.Parried:
				Parried?.Invoke();
				StartCoroutine(ResetParried());
				break;

			default:
				Debug.Log("Non-existent enemy state!");
				break;
		}
		
		hasBeenTargetedText.text = "HasBeenTargeted: " + _hasBeenTargeted.ToString();
		
		if (_playerAttack.isAttacking)
		{
			TakingPlayerDmg(_playerAttack.currentWeapon.damage);
		}
	}

	protected virtual void DetermineCurrentState()
	{
		if (_enemyHealthPoints <= 0)
		{
			currentEnemyState = EnemyState.DeathState;
			return;	
		}
		
		// Если враг в состоянии "Parried", это всегда имеет наивысший приоритет
		if (_isParried)
		{
			currentEnemyState = EnemyState.Parried;
			return;
		}

		// Если игрок в зоне атаки, переходим в состояние атаки
		if (_playerInAttackRange && !_isParried && _isVisible)
		{
			currentEnemyState = EnemyState.Attacking;
			return;
		}

		// Если игрок видим и в зоне преследования, начинаем преследование
		if ((_isVisible && !_playerInAttackRange || _hasBeenTargeted && !_isVisible) && _attackComplete)
		{
			currentEnemyState = EnemyState.Chasing;
			
			// поворот игрока в сторону игрока
			Vector3 directionToPlayer = (_target.position - transform.position).normalized;
			Quaternion lookRotation = Quaternion.LookRotation(new Vector3(directionToPlayer.x, 0, directionToPlayer.z));
			transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
			
			return;
		}

		// Если прошло 7 секунд и игрок вне зоны видимости и зоны преследования — сбрасываем флаг и возвращаемся к патрулированию
		if (!_hasBeenTargeted)
		{
			currentEnemyState = EnemyState.Patrolling;
			return;
		}
		
		// Если ничего не подходит — лог ошибки (на случай неожиданного поведения)
		// Debug.LogError("Не удалось определить текущее состояние врага. Проверьте флаги!");
	}

	#region Patrolling

	protected virtual void Patrolling()
	{
		if (!_isPatrolPointSet)
		{
			// SearchPatrolPoint();
			SearchPatrolPoint2();
		}
		else
		{
			Run?.Invoke();
			_agent.SetDestination(_currentPatrolPoint);
		}

		Vector3 distanceToPatrolPoint = transform.position - _currentPatrolPoint;

		if (distanceToPatrolPoint.magnitude < 1f)
		{
			_isPatrolPointSet = false;
		}
	}

	protected virtual void SearchPatrolPoint2()
	{
		NavMeshHit hit;
		Vector3 randomPoint2;
		float safeDistanceFromEdge = 3f; // Минимальное расстояние до границы NavMesh

		bool validPointFound = false;

		// Ищем точку, которая будет достаточно далеко от границ NavMesh
		while (!validPointFound)
		{
			// Генерация случайной точки внутри радиуса патрулирования
			randomPoint2 = transform.position + UnityEngine.Random.insideUnitSphere * _patrolPointRange;

			// Проверяем, что точка находится на NavMesh
			if (NavMesh.SamplePosition(randomPoint2, out hit, _patrolPointRange, NavMesh.AllAreas))
			{
				NavMeshHit edgeHit;

				// Проверяем расстояние до ближайшей границы NavMesh
				if (NavMesh.FindClosestEdge(hit.position, out edgeHit, NavMesh.AllAreas))
				{
					// Если расстояние до границы больше безопасного, точка считается подходящей
					if (edgeHit.distance >= safeDistanceFromEdge)
					{
						validPointFound = true;
						_currentPatrolPoint = hit.position;
						_isPatrolPointSet = true;
					}
				}
			}
		}
	}

	protected virtual void SearchPatrolPoint()
	{
		NavMeshHit hit;
		Vector3 randomPoint = transform.position + UnityEngine.Random.insideUnitSphere * _patrolPointRange;

		if (NavMesh.SamplePosition(randomPoint, out hit, _patrolPointRange, NavMesh.AllAreas))
		{
			_currentPatrolPoint = hit.position;
			_isPatrolPointSet = true;
		}
	}

	#endregion Patrolling


	public void ApplyParry()
	{
		_isParried = true;
	}

	
	
	protected virtual void PlayerChase()
  	{
  	  	if (currentEnemyState == EnemyState.Chasing)
  	  	{
  	  	  // Если игрок в поле зрения, враг продолжает преследование
  	  	  	if (_isVisible)
  	  	  	{
  	  	  	  	Run?.Invoke();
  	  	  	  	_agent.SetDestination(_target.position);
  	  	  	  	_hasBeenTargeted = true;
  	  	  	}
  	  	  	else if (_hasBeenTargeted && !_isVisible)
  	  	  	{
  	  	  	  	if (!_isFollowingReset)
  	  	  	  	{
  	  	  	  	  	StartCoroutine(ResetFollowingPlayer());
  	  	  	  	  	_isFollowingReset = true; // Устанавливаем флаг для блокировки повторного запуска
  	  	  	  	}
  	  	  	}
  	  	}
  	}


	protected virtual void PlayerAttack()
	{
		//_agent.isStopped = true;
	
			_agent.SetDestination(transform.position); // остановка врага

			Attack?.Invoke();	
			_isAttack = true;
			_attackComplete = false;
			
			
			StartCoroutine(AttackCooldown());
	
	}

	 
	public virtual void TakingPlayerDmg(float playerDamagePoints)
	{
		_enemyHealthPoints -= playerDamagePoints;
		Debug.Log($"Текущее здоровье врага: {_enemyHealthPoints} ед.");
	}

	protected virtual void EnemyDeath()
	{
		Death?.Invoke();
		gameObject.tag = "Untagged";
		Destroy(_collider);
		StartCoroutine(C_OnDefeat());
	}


	#region Coroutines
	
	protected IEnumerator AttackCooldown() // Корутина для создания задержки между атаками
	{
		_canAttack = false;
		yield return new WaitForSeconds(_attackCooldown);
		_canAttack = true;

		_attackComplete = true;
		_agent.isStopped = false;
	}

	public virtual IEnumerator C_OnDefeat() // для удаления тела через время   (временное средство, потом переделать)
	{
		float animationLength = _animator.GetCurrentAnimatorStateInfo(0).length;
		yield return new WaitForSeconds(animationLength);
		Destroy(gameObject);
	}

	public virtual IEnumerator ResetParried()
	{
		float animationLength = _animator.GetCurrentAnimatorStateInfo(0).length;
		yield return new WaitForSeconds(animationLength + 5f);
		_isParried = false;
		Debug.Log("Станлок врага");
		currentEnemyState = EnemyState.Patrolling;
	}

	public virtual IEnumerator ResetFollowingPlayer()
	{
		yield return new WaitForSeconds(7f);

		// Если по истечении 7 секунд игрок всё еще не виден, сбрасываем флаг
		if (!_isVisible)
		{
			_hasBeenTargeted = false;
			currentEnemyState = EnemyState.Patrolling;
			Debug.Log("Игрок потерян, враг прекращает преследование и возвращается к патрулированию.");
		}

		_isFollowingReset = false; // Разблокировка корутины для повторного использования
	}

	#endregion Coroutines

	
	#region Player Accessibility

	private void CheckForPlayerVisibility()
	{
		if (_target == null)
		{
			Debug.LogWarning("Player target is not assigned.");
			return;
		}

		if (Physics.CheckSphere(transform.position, _chaseRange, Player))
		{
			Vector3 directionToPlayer = (_target.position - transform.position).normalized;
			Vector3 rayOrigin = transform.position + Vector3.up * 1.5f; // Поднятие луча на высоту глаз

			_playerInChaseRange = true;

			// Проверяем, находится ли игрок в пределах угла зрения врага
			if (Vector3.Angle(transform.forward, directionToPlayer) < _fieldOfView / 2)
			{
				Debug.DrawRay(rayOrigin, directionToPlayer * distanceToPlayer, Color.red);

				bool hasObstacle = Physics.Raycast(
					rayOrigin,
					directionToPlayer,
					out RaycastHit hit,
					distanceToPlayer,
					_obstacleMask
				);
				Debug.Log($"Есть ли препятствие: {hasObstacle}");

				if (!hasObstacle)
				{
					Debug.Log("Противник заметил игрока напрямую!");
					_isVisible = true;
				}
				else
				{
					Debug.Log($"Объект {hit.collider.gameObject.name} мешает обзору.");
					_isVisible = false;
				}
			}
			else
			{
				_isVisible = false;
			}
		}
		else
		{
			_playerInChaseRange = false;
		}

		if (Physics.CheckSphere(transform.position, _attackRange, Player))
		{
			_playerInAttackRange = true;
		}
		else
		{
			_playerInAttackRange = false;
		}
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.yellow;
		Gizmos.DrawWireSphere(transform.position, _chaseRange);

		Vector3 forwardView =
			Quaternion.Euler(0, _fieldOfView / 2, 0) * transform.forward * _chaseRange;
		Vector3 backwardView =
			Quaternion.Euler(0, -_fieldOfView / 2, 0) * transform.forward * _chaseRange;

		Gizmos.color = Color.blue;
		Gizmos.DrawLine(transform.position, transform.position + forwardView);
		Gizmos.DrawLine(transform.position, transform.position + backwardView);

		Gizmos.color = Color.red;
		Gizmos.DrawWireSphere(transform.position, _attackRange);

		Vector3 forwardView2 =
			Quaternion.Euler(0, _fieldOfView / 2, 0) * transform.forward * _attackRange;
		Vector3 backwardView2 =
			Quaternion.Euler(0, -_fieldOfView / 2, 0) * transform.forward * _attackRange;

		Gizmos.color = Color.blue;
		Gizmos.DrawLine(transform.position, transform.position + forwardView2);
		Gizmos.DrawLine(transform.position, transform.position + backwardView2);
	}

	#endregion Player Accessibility
}
