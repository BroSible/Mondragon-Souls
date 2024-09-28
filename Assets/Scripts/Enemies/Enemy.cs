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

    public delegate void IdleEventHandler();
    public event IdleEventHandler Idle;

    public delegate void HitEventHandler();
    public event HitEventHandler Hit;
    #endregion Events

    protected virtual void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _rgbd = GetComponent<Rigidbody>();
        _collider = GetComponent<Collider>();
        _animator = GetComponent<Animator>();
        _enemyDamage = _damage;
        
        // Damage = _damage;
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
            if (_playerInChaseRange && currentEnemyState != EnemyState.inAttack)
            {
                currentEnemyState = EnemyState.inChasing;
            }
            else if (!_playerInChaseRange)
            {
                currentEnemyState = EnemyState.inPatrolling;
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
            Debug.Log(_currentPatrolPoint);
            _isPatrolPointSet = true;
        }
    }

    protected virtual void PlayerChasing()
    {
        if (_playerInChaseRange)
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
        if (_canAttack)
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
        yield return new WaitForSeconds(animationLength + 10f);
        Destroy(gameObject);
    }
}
