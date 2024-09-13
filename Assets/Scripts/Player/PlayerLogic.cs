using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerLogic : MonoBehaviour
{
    [Header("Characteristics")]
    [SerializeField] private float _playerHealthPoints;
    static private float _playerHealth;
    [SerializeField] private float _minPlayerDamage = 1;
    [SerializeField] private float _maxPlayerDamage = 7;    
    [SerializeField] private float _attackRange = 5f;
    [SerializeField] private float _stamina = 100f;
    [SerializeField] private float _maxStamina = 100f;


    [SerializeField] protected bool _enemyInAttackRange;
    public LayerMask Ground;
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

    public static PlayerState currentPlayerState;

    protected virtual void Awake()
    {
        _rgbd = GetComponent<Rigidbody>();
        _collider = GetComponent<Collider>();
    }

    protected virtual void Start()
    {
        currentPlayerState = PlayerState.inAdventurous;

        _playerHealth = _playerHealthPoints;
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
                // добавить
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

    public void EnemyAttacking() // delete
    {
        float currentPlayerDamage = _minPlayerDamage;

        Debug.Log($"Игрок наносит врагу {currentPlayerDamage} ед. урона!");
    }

    public static void TakeDamage(float enemyDamagePoints)
    {
        _playerHealth -= enemyDamagePoints;

        Debug.Log($"Текущее здоровье игрока: {_playerHealth} ед.");

        if (_playerHealth <= 0)
        {
            currentPlayerState = PlayerState.deathState;
            
            Debug.Log($"Игрок погибает!");
        }

        else
        {
            //СЮда добавить звук получения урона
        }
    }
}
