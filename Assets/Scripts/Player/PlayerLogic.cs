using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerLogic : MonoBehaviour
{
    [Header("Characteristics")]
    [SerializeField] private float _playerHealthPoints;
    static private float _playerHealth;
    [SerializeField] private float _stamina = 100f;
    [SerializeField] private float _maxStamina = 100f;
    public bool _isDead = false;
    public static bool _isTakingDamage = false;
    private PlayerController _playerController;
    private PlayerAttack _playerAttack;
    private Rigidbody _rgbd;
    private Collider _collider;
    public Shield_holder _currentShield;
    private Animator _animator;
    public static bool successfulParry = false;
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
        _animator = GetComponent<Animator>();
        _playerController = GetComponentInChildren<PlayerController>();
        _playerAttack = GetComponentInChildren<PlayerAttack>();
        _currentShield = GetComponentInChildren<Shield_holder>();
    }

    protected virtual void Start()
    {
        currentPlayerState = PlayerState.inAdventurous;

        _playerHealth = _playerHealthPoints;
    }

    protected virtual void FixedUpdate()
    {
        _playerHealthPoints = _playerHealth;
        UpdatePlayerState();
        switch (currentPlayerState)
        {
            case PlayerState.inAdventurous:
                if(_damageEffectApplied)
                {
                    _playerController.speed += 2f;
                    _damageEffectApplied = false;
                }
                break;

            case PlayerState.inAttack:
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
        if(PlayerAttack._isAttacking)
        {
            currentPlayerState = PlayerState.inAttack;
        }

        else if(_isTakingDamage)
        {
            currentPlayerState = PlayerState.takingDamage;
        }

        else if (_playerHealth <= 0)
        {
            currentPlayerState = PlayerState.deathState;
        }

        if(Input.GetKeyDown(KeyCode.LeftControl) && _currentShield != null && !_isParrying)
        {
            _isParrying = true;

            _animator.SetBool("isParrying", true);
            ShieldParry(_currentShield, Enemy._enemyDamage);
        }

    }

    public void ShieldParry(Shield_holder currentShield, float enemyDamagePoints)
    {


        if(currentPlayerState == PlayerState.takingDamage && Enemy._isAttack)
        {
            _playerHealth -= enemyDamagePoints * (currentShield.shield.protectionFactor/100);
            successfulParry = true;
            Debug.Log("Парирование");
            StartCoroutine(ResetParry());
        }
        
        else
        {
            Debug.Log("Парирование не удалось");
            StartCoroutine(ResetParry());
            successfulParry = false;
        }
    
       
    }

    public static void TakeDamage(float enemyDamagePoints)
    {
        _playerHealth -= enemyDamagePoints;
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
        
        yield return new WaitForSeconds(_currentShield.shield.rollbackTime);
        _isParrying = false;
        if(successfulParry)
        {
            successfulParry = false;
        }

        _animator.SetBool("isParrying", false);
        _animator.SetBool("isIdle", true);
    }
}
