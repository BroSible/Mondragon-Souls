using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System;

public class Enemy : MonoBehaviour
{
    [Header("Characteristics")]
    [SerializeField] protected float _enemyHealthPoints;
    [SerializeField] protected float _damage = 2f;

    public static float _enemyDamage; // для ссылки
    // public float Damage => _damage;

    [SerializeField] protected float _attackRange = 1f;
    [SerializeField] protected float _chaseRange = 3f;
    [SerializeField] protected float _patrolPointRange = 15f;

    [SerializeField] protected Animator _animator;
    public static bool _isAttack = false;

    [Header("Patroling")]
    protected Vector3 _currentPatrolPoint;
    [SerializeField] protected bool _isPatrolPointSet;
    [SerializeField] protected float _patrolInterval = 5f;
    [SerializeField] protected bool _isAlreadyAttacked;
    [SerializeField] protected bool _isParried = false; // Индивидуальная переменная для каждого врага
    [SerializeField] public bool _playerInChaseRange, _playerInAttackRange;

    [SerializeField] protected bool _hasBeenTargeted;

    [Header("Navigation")]
    protected NavMeshAgent _agent;
    public LayerMask Ground, Player;

    protected Rigidbody _rgbd;
    protected Collider _collider;
    protected Transform _target;


    [Header("Movement Settings")]
    [SerializeField] protected float _patrolSpeed = 3f; // Скорость патрулирования
    [SerializeField] protected float _chaseSpeed = 5f;  // Скорость преследования

    // Задержка перед возможностью следующей атаки
    [SerializeField] protected float _attackCooldown = 3f; // Время перезарядки
    private bool _canAttack = true; // Может ли враг атаковать

    [Header("Vision Settings")]
    [SerializeField] private float _fieldOfView = 360f; // Угол зрения врага
    [SerializeField] private float _sightRange;   // Дальность зрения врага
    [SerializeField] private LayerMask _obstacleMask;   // Слой препятствий

    public enum EnemyState
    {
        inPatrolling,
        inChasing,
        inAttack,
        deathState,
        parried,
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
        _enemyDamage = _damage;

        _sightRange = _chaseRange;
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

        //_playerInAttackRange = Physics.CheckSphere(transform.position, _attackRange, Player);

        CheckForPlayerVisibility();
        
        UpdateEnemyState();

        switch (currentEnemyState)
        {
            case EnemyState.inPatrolling:
                Patrolling();
                _agent.speed = _patrolSpeed;
                break;

            case EnemyState.inChasing:
                PlayerChasing();
                _agent.speed = _chaseSpeed;
                break;

            case EnemyState.inAttack:
                PlayerAttacking();
                break;

            case EnemyState.deathState:
                Destroy(gameObject);
                break;

            case EnemyState.parried:
                Parried?.Invoke();
                StartCoroutine(ResetParried());
                break;

            case EnemyState.takingPlayerDamage:
                // тут добавить какую-нибудь interesting logic
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
            if (_playerInChaseRange && currentEnemyState != EnemyState.inAttack && !_isParried)
            {
                currentEnemyState = EnemyState.inChasing;
            }
            else if (!_playerInChaseRange && !_isParried)
            {
                currentEnemyState = EnemyState.inPatrolling;
            }

            else if (_isParried)
            {
                currentEnemyState = EnemyState.parried;
            }
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
            Run?.Invoke();
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
        Vector3 randomPoint = transform.position + UnityEngine.Random.insideUnitSphere * _patrolPointRange;

        if (NavMesh.SamplePosition(randomPoint, out hit, _patrolPointRange, NavMesh.AllAreas)) // Проблема тут
        {
            _currentPatrolPoint = hit.position;
            _isPatrolPointSet = true;
        }
    }


    public void ApplyParry()
    {
        _isParried = true;
        currentEnemyState = EnemyState.parried;
    }

    protected virtual void PlayerChasing()
    {
        if (_playerInChaseRange  ) // && дописать проверку, связаннную с методом CheckForPlayerVisibility()
        {
            _agent.isStopped = false;
            Run?.Invoke();
            _agent.SetDestination(_target.position);
        }
        // else if (_playerInAttackRange)
        // {
        //     currentEnemyState = EnemyState.inAttack;
        // }
        else if (!_playerInChaseRange) // Переход в патрулирование, если игрок вне зоны преследования
        {
            currentEnemyState = EnemyState.inPatrolling;
        }
    }


    public virtual void PlayerAttacking()
    {
        _agent.SetDestination(transform.position);

        if (_canAttack && !_isParried)
        {
            Attack?.Invoke();
            _isAttack = true;

            StartCoroutine(AttackCooldown());
            transform.LookAt(new Vector3(_target.position.x, transform.position.y, _target.position.z));
        }
    }

    public virtual void TakingPlayerDamage(float playerDamagePoints)
    {
        _enemyHealthPoints -= playerDamagePoints;
        Debug.Log($"Текущее здоровье врага: {_enemyHealthPoints} ед.");

        if (_enemyHealthPoints <= 0)
        {
            Death?.Invoke();
            gameObject.tag = "Untagged";
            Destroy(_collider);
            StartCoroutine(C_OnDefeat());
            currentEnemyState = EnemyState.deathState;
        }
    }

    protected IEnumerator AttackCooldown()   // Корутина для создания задержки между атаками
    {
        _canAttack = false;
        yield return new WaitForSeconds(_attackCooldown);
        _isAttack = false;
        _canAttack = true;

        // После перезарядки враг снова оценивает состояние
        if (_playerInAttackRange)
        {
            currentEnemyState = EnemyState.inAttack;
        }
        else
        {
            currentEnemyState = EnemyState.inChasing;
            _agent.isStopped = false;
        }
    }

    public virtual IEnumerator C_OnDefeat()  // для удаления тела через время   (временное средство, потом переделать)
    {
        float animationLength = _animator.GetCurrentAnimatorStateInfo(0).length;
        yield return new WaitForSeconds(animationLength + 1f);
        Destroy(gameObject);
    }

    public virtual IEnumerator ResetParried()
    {
        float animationLength = _animator.GetCurrentAnimatorStateInfo(0).length;
        yield return new WaitForSeconds(animationLength + 5f);
        _isParried = false;
        Debug.Log("Станлок врага");
        currentEnemyState = EnemyState.inPatrolling;
    }

    private void CheckForPlayerVisibility()
    {
        if (!_playerInChaseRange)
            return;

        // Получаем направление к игроку
        Vector3 directionToPlayer = (_target.position - transform.position).normalized;

        // Проверяем, находится ли игрок в пределах угла зрения врага
        if (Vector3.Angle(transform.forward, directionToPlayer) < _fieldOfView / 2)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, _target.position);

            // Проверяем, есть ли прямая видимость к игроку
            if (!Physics.Raycast(transform.position, directionToPlayer, distanceToPlayer, _obstacleMask))
            {
                // Игрок виден, враг начинает преследование
                _playerInChaseRange = true;
                currentEnemyState = EnemyState.inChasing;
            }
            else
            {
                // Игрок не виден из-за препятствия
                _playerInChaseRange = false;
                currentEnemyState = EnemyState.inPatrolling;
            }
        }
        else
        {
            // Игрок вне угла зрения врага
            _playerInChaseRange = false;
            currentEnemyState = EnemyState.inPatrolling;
        }
    }
}
