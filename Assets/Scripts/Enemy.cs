using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System;

public class Enemy : MonoBehaviour
{
    [Header("Characteristics")]
    [SerializeField] private int _enemyHealthPoints;
    [SerializeField] private int _minEnemyDamage;
    [SerializeField] private int _maxEnemyDamage;

    [SerializeField] private float _attackRange = 1f;
    [SerializeField] private float _chaseRange = 3f;
    [SerializeField] protected float _patrolPointRange = 15f;

    [Header("Patroling")]
    protected Vector3 _currentPatrolPoint;
    [SerializeField] protected bool _isPatrolPointSet;

    [SerializeField] private float _patrolInterval = 5f;
    [SerializeField] protected bool _isAlreadyAttacked;
    [SerializeField] protected bool _playerInChaseRange, _playerInAttackRange;
    [SerializeField] protected bool _hasBeenTargeted;

    [Header("Navigation")]
    private NavMeshAgent _agent;
    public LayerMask Ground, Player, Rock;

    public Player player;
    protected Rigidbody _rgbd;
    protected Collider _collider;
    protected Transform _target;

    private System.Random _enemyRandom = new System.Random();

    [Header("Movement Settings")]
    [SerializeField] private float _patrolSpeed = 3f; // Скорость патрулирования
    [SerializeField] private float _chaseSpeed = 5f;  // Скорость преследования

    // Задержка перед возможностью следующей атаки
    [SerializeField] private float _attackCooldown = 3f; // Время перезарядки
    private bool _canAttack = true; // Может ли враг атаковать

    public enum EnemyState
    {
        inPatrolling,
        inChasing,
        inAttack,
        deathState,
        takingPlayerDamage
    }

    public EnemyState currentEnemyState;

    #region Events
    public delegate void ChaseEventHandler();
    public event ChaseEventHandler Chase;

    public delegate void AttackEventHandler();
    public event AttackEventHandler Attack;

    public delegate void RunEventHandler();
    public event RunEventHandler Run;

    public delegate void DeathEventHandler();
    public event DeathEventHandler Death;

    public delegate void HitEventHandler();
    public event HitEventHandler Hit;
    #endregion Events

    protected virtual void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _rgbd = GetComponent<Rigidbody>();
        _collider = GetComponent<Collider>();

        _agent.angularSpeed = 360f; // Быстрая угловая скорость для плавных поворотов
    }

    protected virtual void Start()
    {
        currentEnemyState = EnemyState.inPatrolling;

        try
        {
            _target = GameObject.FindWithTag("Player").transform;
        }
        catch
        {
            Debug.Log("Player's object not found!");
        }

        // Устанавливаем скорость патрулирования по умолчанию
        _agent.speed = _patrolSpeed;
    }

    protected virtual void FixedUpdate()
    {
        _playerInChaseRange = Physics.CheckSphere(transform.position, _chaseRange, Player);
        _playerInAttackRange = Physics.CheckSphere(transform.position, _attackRange, Player);

        CheckDistanceBetweenPlayerAndEnemy();
        UpdateEnemyState();

        Debug.Log($"Current enemy state: {currentEnemyState}");

        switch (currentEnemyState)
        {
            case EnemyState.inPatrolling:
                Patrolling();
                break;

            case EnemyState.inChasing:
                PlayerChasing();
                break;

            case EnemyState.inAttack:
                if (_canAttack)
                {
                    PlayerAttacking();
                }
                break;

            case EnemyState.deathState:
                Destroy(gameObject);
                break;

            case EnemyState.takingPlayerDamage:
                player.EnemyAttacking();
                break;

            default:
                Debug.Log("Non-existent enemy state!");
                break;
        }
    }

    protected virtual void UpdateEnemyState()
    {
        if (currentEnemyState != EnemyState.deathState)
        {
            if (_playerInAttackRange)
            {
                currentEnemyState = EnemyState.inAttack;
                Debug.Log("Враг атакует игрока!");
            }
            else if (_playerInChaseRange)
            {
                currentEnemyState = EnemyState.inChasing;
                Debug.Log("Враг преследует игрока!");
            }
            else
            {
                currentEnemyState = EnemyState.inPatrolling;
                Debug.Log("Враг патрулирует территорию.");
            }
        }

        AdjustMovementSpeed();
    }

    protected virtual void CheckDistanceBetweenPlayerAndEnemy()
    {
        if (_playerInChaseRange)
        {
            currentEnemyState = EnemyState.inChasing;
            Debug.Log("Враг заметил игрока!");
        }
    }

    protected virtual void Patrolling()
    {
        if (!_isPatrolPointSet)
        {
            SearchPatrolPoint();
        }
        else
        {
            _agent.SetDestination(_currentPatrolPoint);
        }

        Vector3 distanceToPatrolPoint = transform.position - _currentPatrolPoint;

        if (distanceToPatrolPoint.magnitude < 1f)
        {
            _isPatrolPointSet = false;
        }
    }

    protected virtual void SearchPatrolPoint()
    {
        NavMeshHit hit;
        NavMeshQueryFilter filter = new NavMeshQueryFilter();

        filter.areaMask = NavMesh.AllAreas & ~Rock.value;
        Vector3 randomPoint = transform.position + UnityEngine.Random.insideUnitSphere * _patrolPointRange;

        if (NavMesh.SamplePosition(randomPoint, out hit, _patrolPointRange, filter))
        {
            _currentPatrolPoint = hit.position;
            _isPatrolPointSet = true;
        }
    }

    protected virtual void PlayerChasing()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, _target.position);

        if (distanceToPlayer > _attackRange)
        {
            // Разрешаем агенту двигаться к игроку, если он вне зоны атаки
            _agent.isStopped = false;
            _agent.SetDestination(_target.position);
        }
        else
        {
            // Останавливаем врага при приближении к игроку на расстояние атаки
            StopAgentMovement();
            currentEnemyState = EnemyState.inAttack;
        }

        // Переход в патрулирование, если игрок вне зоны преследования
        if (!_playerInChaseRange)
        {
            currentEnemyState = EnemyState.inPatrolling;
            Debug.Log("Игрок вышел за пределы радиуса преследования. Враг возвращается к патрулированию.");
        }
    }

    public int GetRandomEnemyDamage()
    {
        return _enemyRandom.Next(_minEnemyDamage, _maxEnemyDamage + 1);
    }

    public virtual void PlayerAttacking()
    {
        if (_canAttack)
        {
            // Полная остановка движения врага и отключение агентства во время атаки
            StopAgentMovement();

            // Поворачиваем врага лицом к игроку
            // transform.LookAt(new Vector3(_target.position.x, transform.position.y, _target.position.z));

            // Наносим урон игроку
            int currentEnemyDamage = GetRandomEnemyDamage();
            player.TakingEnemyDamage(currentEnemyDamage);
            Debug.Log($"Враг наносит игроку {currentEnemyDamage} ед. урона!");

            // Запускаем задержку перед следующей атакой
            StartCoroutine(AttackCooldown());
        }
    }

    public virtual void TakingPlayerDamage(int playerDamagePoints)
    {
        _enemyHealthPoints -= playerDamagePoints;
        Debug.Log($"Текущее здоровье врага: {_enemyHealthPoints} ед.");

        if (_enemyHealthPoints <= 0)
        {
            currentEnemyState = EnemyState.deathState;
            Debug.Log("Враг погибает!");
        }
    }

    // Метод для изменения скорости врага в зависимости от его состояния
    protected virtual void AdjustMovementSpeed()
    {
        switch (currentEnemyState)
        {
            case EnemyState.inPatrolling:
                _agent.speed = _patrolSpeed;
                break;
            case EnemyState.inChasing:
                _agent.speed = _chaseSpeed;
                break;
        }
    }

    // Корутина для создания задержки между атаками
    protected IEnumerator AttackCooldown()
    {
        _canAttack = false; // Отключаем возможность атаковать
        yield return new WaitForSeconds(_attackCooldown); // Ждем время перезарядки атаки

        _canAttack = true; // Возвращаем возможность атаковать

        // После перезарядки враг снова оценивает состояние
        if (_playerInAttackRange)
        {
            currentEnemyState = EnemyState.inAttack;
        }
        else
        {
            currentEnemyState = EnemyState.inChasing;
            _agent.isStopped = false; // Возвращаем движение
        }
    }

    // Метод для полной остановки движения врага
    private void StopAgentMovement()
    {
        _agent.isStopped = true; // Останавливаем NavMesh агент
        _agent.ResetPath(); // Сбрасываем текущий путь
        _agent.velocity = Vector3.zero; // Обнуляем скорость

        // Замораживаем физические движения врага для предотвращения тряски
        _rgbd.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotation;
    }

    // Метод для разблокировки физики после атаки или в состоянии преследования
    private void UnlockMovement()
    {
        _rgbd.constraints = RigidbodyConstraints.None; // Разблокируем физику
        _agent.isStopped = false; // Включаем NavMesh агент
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Debug.Log("Объект вошел в зону триггера: " + other.name);

            //_agent.ResetPath();
            // _rgbd.constraints = RigidbodyConstraints.FreezeAll; // Замораживает все движения и вращения
            _agent.SetDestination(transform.position);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        // Если вам нужно что-то делать, пока объект внутри триггера
        if (other.CompareTag("Player"))
        {
            Debug.Log("Объект находится внутри триггера: " + other.name);

            _agent.SetDestination(transform.position);
        }
    }



    // Этот метод вызывается, когда объект выходит из триггера
    private void OnTriggerExit(Collider other)
    {
        // Проверяем, что это за объект (например, если нужно проверить по тэгу)
        if (other.CompareTag("Player"))
        {
            Debug.Log("Объект вышел из зоны триггера: " + other.name);

            // _rgbd.constraints = RigidbodyConstraints.None;  // Возвращаем физику
        }
    }
}
