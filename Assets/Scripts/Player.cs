using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("Characteristics")]
    [SerializeField] private int _playerHealthPoints;
    [SerializeField] private int _minPlayerDamage = 1;
    [SerializeField] private int _maxPlayerDamage = 7;
    [SerializeField] private float _attackRange = 5f;

    // private int _currentPlayerDamage = System.Random.Range(_minPlayerDamage, _maxPlayerDamage + 1);

    [SerializeField] protected bool _enemyInAttackRange;
    public LayerMask Ground, Rock;

    public Enemy enemy;
    protected Rigidbody _rgbd;
    protected Collider _collider;
    private System.Random _playerRandom = new System.Random();

    public enum PlayerState
    {
        inAdventurous, // Базовое состояние игрока при изучении локации
        inHealingState,
        inAttack,
        inDefense, // Состояние защиты игрока щитом (или подобное)
        deathState,
        takingEnemyDamage
    }

    public PlayerState currentPlayerState;

    protected virtual void Awake()
    {
        _rgbd = GetComponent<Rigidbody>();
        _collider = GetComponent<Collider>();
    }

    protected virtual void Start()
    {
        currentPlayerState = PlayerState.inAdventurous;
    }

    protected virtual void FixedUpdate()
    {
        // _enemyInAttackRange = Physics.CheckSphere(transform.position, _attackRange, Player);

        CheckDistanceBetweenPlayerAndEnemy();

        switch (currentPlayerState)
        {
            case PlayerState.inAdventurous:
                break;

            case PlayerState.inHealingState:

                break;

            case PlayerState.inAttack:
                EnemyAttacking();
                break;

            case PlayerState.inDefense:
            // Здесь должен вызываться метод, в котором описана блокировка атаки игрока противником
                break;

            case PlayerState.deathState:
                Destroy(gameObject);
                break;
            
            case PlayerState.takingEnemyDamage:
                enemy.PlayerAttacking();
                break;

            default:
                Debug.Log("Non-existent enemy state!");
                break;
        }
    }

    protected virtual void CheckDistanceBetweenPlayerAndEnemy()
    {
        if (_enemyInAttackRange)
        {
            currentPlayerState = PlayerState.inAttack;

            Debug.Log($"Игрок атакует врага!");
        }
    }

    void Update()
    {

    }

    public int GetRandomPlayerDamage()
    {
        return _playerRandom.Next(_minPlayerDamage, _maxPlayerDamage + 1);
    }

    public void EnemyAttacking()
    {
        int currentPlayerDamage = GetRandomPlayerDamage();
        enemy.TakingPlayerDamage(currentPlayerDamage);

        Debug.Log($"Игрок наносит врагу {currentPlayerDamage} ед. урона!");
    }

    public void TakingEnemyDamage(int enemyDamagePoints)
    {
        _playerHealthPoints -= enemyDamagePoints;

        Debug.Log($"Текущее здоровье игрока: {_playerHealthPoints} ед.");

        if (_playerHealthPoints <= 0)
        {
            currentPlayerState = PlayerState.deathState;

            Debug.Log($"Игрок погибает!");
        }
    }
}
