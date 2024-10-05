using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerLogic : MonoBehaviour
{
    [Header("Characteristics")]
    [SerializeField] private float _totalPlayerHealthPoints;
    static public float _totalPlayerHealth;

    [SerializeField] private float staminaRecoverDelay = 3f; // Задержка перед восстановлением
    [SerializeField] private float staminaRecoveryCounter = 0f; // Счётчик времени для отслеживания задержки
    [SerializeField] private float staminaRecoverRateIdle = 10f; // Скорость восстановления при idle
    [SerializeField] private float staminaRecoverRateWalking = 0.001f; // Скорость восстановления при ходьбе
    [SerializeField] public float Stamina { get; set; } = 100f;
    [SerializeField] private float _maxStamina = 100f;
    [SerializeField] private float _staminaRecoveryThreshold = 20f; // Порог, когда начинается ускоренное восстановление

    [SerializeField] public float headHealthPoints;
    [SerializeField] public float leftArmHealthPoints;
    [SerializeField] public float rightArmHealthPoints;
    [SerializeField] public float leftLegHealthPoints;
    [SerializeField] public float rightLegHealthPoints;
    [SerializeField] public float bodyHealthPoints;


    public bool _isDead = false;
    public static bool _isTakingDamage = false;
    private PlayerController _playerController;
    private PlayerAttack _playerAttack;
    public Shield_holder _currentShield;
    private Animator _animator;
    private CameraCursor _cameraCursor;
    public static bool _successfulParry = false;
    public static bool _isParrying = false;
    private bool _damageEffectApplied = false;

    public enum PlayerState
    {
        inAdventurous,
        inParry,
        deathState,
        takingDamage,
        Idle,
        Walking // Добавляем новое состояние для ходьбы
    }

    public PlayerState currentPlayerState;

    protected virtual void Awake()
    {
        _maxStamina = Stamina;
        _animator = GetComponent<Animator>();
        _cameraCursor = GetComponent<CameraCursor>();
        _playerController = GetComponentInChildren<PlayerController>();
        _playerAttack = GetComponentInChildren<PlayerAttack>();
        _currentShield = GetComponentInChildren<Shield_holder>();
    }

    protected virtual void Start()
    {
        currentPlayerState = PlayerState.inAdventurous;
        _totalPlayerHealth = _totalPlayerHealthPoints;
    }

    protected virtual void FixedUpdate()
    {
        staminaRecoveryCounter += Time.deltaTime;
        Debug.Log($"stamina: {Stamina}");
        _totalPlayerHealthPoints = _totalPlayerHealth;
        Debug.Log($"Success parry: {_successfulParry}");

        UpdatePlayerState();

        if (Input.GetKey(KeyCode.LeftControl) && _currentShield != null && !_isParrying)
        {
            _isParrying = true;
            _animator.SetBool("isParrying", true);
            ShieldParry(_currentShield, Enemy._enemyDamage);
        }

        bool canRecoverStamina = _playerAttack.isAttacking || _isTakingDamage || _isParrying || Stamina >= _maxStamina;

        if (canRecoverStamina)
        {
            staminaRecoveryCounter = 0f;
        }

        switch (currentPlayerState)
        {
            case PlayerState.inAdventurous:
                if (_damageEffectApplied)
                {
                    _playerController.speed += 2f;
                    _damageEffectApplied = false;
                }
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

            case PlayerState.Idle:
            case PlayerState.Walking:
                HandleStaminaRecovery();
                break;

            default:
                Debug.Log("Non-existent player state!");
                break;

            
        }
    }

    public void UpdatePlayerState()
    {
        bool canTransitionToIdle = !_playerAttack.isAttacking && !_isTakingDamage && !PlayerAttack._isEnhancedAttacking && !PlayerAttack._isReposting && !_isParrying;

        if (_isTakingDamage)
        {
            currentPlayerState = PlayerState.takingDamage;
        }
        else if (_totalPlayerHealth <= 0)
        {
            currentPlayerState = PlayerState.deathState;
        }
        else if (_playerController.moveDirection.magnitude <= 1f && canTransitionToIdle)
        {
            currentPlayerState = PlayerState.Idle;
        }
        else if (_playerController.moveDirection.magnitude > 1f && canTransitionToIdle)
        {
            currentPlayerState = PlayerState.Walking;
        }
        else
        {
            currentPlayerState = PlayerState.inAdventurous;
        }
    }

    private void HandleStaminaRecovery()
    {
        if (staminaRecoveryCounter >= staminaRecoverDelay)
        {
            switch (currentPlayerState)
            {
                case PlayerState.Idle:
                    Stamina += staminaRecoverRateIdle * Time.deltaTime;
                    break;
                case PlayerState.Walking:
                    Stamina += staminaRecoverRateWalking * Time.deltaTime * 0.2f;
                    break;
            }

            Stamina = Mathf.Clamp(Stamina, 0, _maxStamina);
        }
    }

    public void ShieldParry(Shield_holder currentShield, float enemyDamagePoints)
    {
        _playerAttack.isAttacking = false;
        if(Stamina >= 5f)
        {
            Stamina -= 5f;
        }
        _cameraCursor.enabled = false;
        _totalPlayerHealth -= enemyDamagePoints * (currentShield._shield.protectionFactor / 100);
        _animator.Play("Parry");
        StartCoroutine(ResetParry());
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
        yield return new WaitForSeconds(_currentShield._shield.rollbackTime);
        _cameraCursor.enabled = true;
        _isParrying = false;
        _animator.SetBool("isParrying", false);

        if (_successfulParry)
        {
            _successfulParry = false;
        }

        if (PlayerAttack._isReposting)
        {
            SetAnimatorFlags(isParrying: false, isIdle: false, isAttacking: false);
        }
    }

    void SetAnimatorFlags(bool isParrying, bool isIdle, bool isAttacking)
    {
        _animator.SetBool("isParrying", isParrying);
        _animator.SetBool("isIdle", isIdle);
        _animator.SetBool("isAttacking", isAttacking);
    }
}
