using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerLogic : MonoBehaviour
{
    [Header("Characteristics")]
    [SerializeField] private float _totalPlayerHealthPoints;
    static private float _totalPlayerHealth;

    [SerializeField] public float headHealthPoints;

    [SerializeField] public float leftArmHealthPoints;
    [SerializeField] public float rightArmHealthPoints;

    [SerializeField] public float leftLegHealthPoints;
    [SerializeField] public float rightLegHealthPoints;

    [SerializeField] public float bodyHealthPoints;


    [SerializeField] private float _stamina = 100f;
    [SerializeField] private float _maxStamina = 100f;
    public bool _isDead = false;
    public static bool _isTakingDamage = false;
    private PlayerController _playerController;
    private PlayerAttack _playerAttack;
    protected Rigidbody _rgbd;
    protected Collider _collider;
    private Shield _currentShield;
    public static bool _isParrying = false;
    private bool _damageEffectApplied = false; // Новая переменная для отслеживания применения эффекта урона

    public enum PlayerState
    {
        inAdventurous, // Базовое состояние игрока при изучении локации
        inAttack,
        inParry,
        deathState,
        takingDamage
    }

    public PlayerState currentPlayerState;

    protected virtual void Awake()
    {
        _rgbd = GetComponent<Rigidbody>();
        _collider = GetComponent<Collider>();
        _playerController = GetComponentInChildren<PlayerController>();
        _playerAttack = GetComponentInChildren<PlayerAttack>();
        _currentShield = GetComponentInParent<Shield>();
    }

    protected virtual void Start()
    {
        currentPlayerState = PlayerState.inAdventurous;

        _totalPlayerHealth = _totalPlayerHealthPoints;
    }

    protected virtual void FixedUpdate()
    {
        UpdatePlayerState();
        switch (currentPlayerState)
        {
            case PlayerState.inAdventurous:
                if (_damageEffectApplied)
                {
                    _playerController.speed += 2f;
                    _damageEffectApplied = false;
                }
                break;

            case PlayerState.inAttack:
                break;

            case PlayerState.inParry:
                ShieldParry(_currentShield, Enemy._enemyDamage);
                break;

            case PlayerState.deathState:
                _isDead = true;
                Destroy(gameObject);
                break;

            case PlayerState.takingDamage:
                if (!_damageEffectApplied)
                {
                    _playerController.speed -= 2;
                    _damageEffectApplied = true;
                }

                StartCoroutine(ResetTakingDamage());
                break;

            default:
                Debug.Log("Non-existent enemy state!");
                break;
        }
    }

    public void UpdatePlayerState()
    {
        if (PlayerAttack._isAttacking)
        {
            currentPlayerState = PlayerState.inAttack;
        }

        else if (_isTakingDamage)
        {
            currentPlayerState = PlayerState.takingDamage;
        }

        else if (_totalPlayerHealth <= 0)
        {
            currentPlayerState = PlayerState.deathState;
        }

        else if (Input.GetKeyDown(KeyCode.LeftControl) && _currentShield != null)
        {
            currentPlayerState = PlayerState.inParry;
        }

    }

    public void ShieldParry(Shield shield, float enemyDamagePoints)
    {
        _playerAttack.isAttacking = false;
        _totalPlayerHealth -= enemyDamagePoints * (shield.protectionFactor / 100);
        Debug.Log("Парирование");
    }

    public static void TakeDamage(float enemyDamagePoints)
    {
        _totalPlayerHealth -= enemyDamagePoints;
        _isTakingDamage = true;
    }

    public IEnumerator ResetTakingDamage()
    {
        yield return new WaitForSeconds(2f);
        _isTakingDamage = false;
        currentPlayerState = PlayerState.inAdventurous;
    }

    public IEnumerator ResetParry()
    {
        yield return new WaitForSeconds(_currentShield.rollbackTime);
        _isParrying = false;
        currentPlayerState = PlayerState.inAdventurous;
    }
}
