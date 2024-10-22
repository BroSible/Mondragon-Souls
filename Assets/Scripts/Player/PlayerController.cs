using System;
using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float speed = 5f; 
    public float gravity = 9.81f;
    public float rotationSpeed = 10f;
    public float acceleration = 10f; 
    public float deceleration = 5f; 
    private CharacterController controller;
    public Vector3 moveDirection = Vector3.zero;
    private PlayerAttack _playerAttack;
    private PlayerLogic _playerLogic;
    private Animator animator;
    public Transform _cameraTransform;
    private Transform _playerTransform;
    private CameraCursor _cameraCursor;
    [SerializeField] public bool IsDashing {get; set;} = false;
    [SerializeField] private float _dashTime = 0.5f;
    [SerializeField] private float _dashSpeed;
    [SerializeField] private AnimationCurve _dashSpeedCurve;

    [SerializeField] private float currentSpeed = 0f; // Текущая скорость

    void Start()
    {
        controller = GetComponent<CharacterController>();
        _playerAttack = GetComponent<PlayerAttack>();
        _playerLogic = GetComponent<PlayerLogic>();
        _cameraCursor = GetComponent<CameraCursor>();
        _playerTransform = GetComponent<Transform>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        bool canMove = !_playerAttack.isAttacking && !PlayerAttack._isEnhancedAttacking && !PlayerLogic._isParrying;

        CameraCursorEnabled();
        UpdateAnimatorFlags();

        if (controller.isGrounded)
        {
            Vector3 targetDirection = GetMovementDirection();

            // Плавный поворот к направлению движения
            if (targetDirection.magnitude > 0.1f && canMove)
            {
                Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }

            bool isMovingInput = Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0;
            if (canMove && isMovingInput)
            {
                // Увеличиваем скорость плавно до максимальной
                currentSpeed = Mathf.MoveTowards(currentSpeed, speed, acceleration * Time.deltaTime);
            }

            else
            {
                // Плавно замедляемся до нуля
                currentSpeed = Mathf.MoveTowards(currentSpeed, 0f, deceleration * Time.deltaTime);
            }

            moveDirection = targetDirection * currentSpeed;

            //Dash
            if(Input.GetKey(KeyCode.Space) && _playerLogic.Stamina >= 20f)
            {
               StartCoroutine(Dash(moveDirection));
            }
        }

        else
        {
            currentSpeed = Mathf.MoveTowards(currentSpeed, 0f, deceleration * Time.deltaTime);
            moveDirection.y -= gravity * Time.deltaTime; 
        }

        controller.Move(moveDirection * Time.deltaTime);
    }

    private Vector3 GetMovementDirection()
    {
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");

        // Получаем направление движения относительно камеры
        Vector3 cameraForward = _cameraTransform.forward;
        Vector3 cameraRight = _cameraTransform.right;
        cameraForward.y = 0f;
        cameraRight.y = 0f;
        cameraForward.Normalize();
        cameraRight.Normalize();

        // Вычисляем вектор направления движения
        Vector3 targetDirection = (moveHorizontal * cameraRight + moveVertical * cameraForward).normalized;

        return targetDirection;      
    }


    //Метод отключения поворота камеры на курсором, если игрок ходит. Если он стоит, то скрипт будет включен обратно.
    void CameraCursorEnabled()
    {
        if(moveDirection.magnitude >= 1f || _playerAttack.isAttacking || PlayerAttack._isEnhancedAttacking || PlayerLogic._isParrying || IsDashing)
        {
            _cameraCursor.enabled = false;
        }

        else
        {
            _cameraCursor.enabled = true;
        }
    }

    public IEnumerator Dash(Vector3 direction)
    {
        float staminaCost;
        if (IsDashing)
        {
            yield break;
        }

        if (direction == Vector3.zero)
        {
            direction = transform.forward;
            staminaCost = 10f;
        }

        else
        {
            staminaCost = 20f;
        }

        IsDashing = true;
        float elapsedTime = 0f;

        while (elapsedTime < _dashTime)
        {
            
            float speedMultiplier = _dashSpeed * _dashSpeedCurve.Evaluate(elapsedTime / _dashTime);

            moveDirection = direction * speedMultiplier;
            controller.Move(moveDirection * Time.deltaTime);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        _playerLogic.Stamina -= staminaCost;
        IsDashing = false;
        yield break;
    }


    void UpdateAnimatorFlags()
    {
        bool isWalking = currentSpeed >= 1f && !_playerAttack.isAttacking && !PlayerLogic._isParrying && !IsDashing; 
        bool isIdle = !isWalking && !_playerAttack.isAttacking && !PlayerLogic._isParrying && !PlayerAttack._isEnhancedAttacking && !IsDashing;
        bool isParrying = PlayerLogic._isParrying;
        bool isAttacking = _playerAttack.isAttacking || PlayerAttack._isEnhancedAttacking; 
        bool isDashing = IsDashing;
        
        animator.SetBool("isWalking", isWalking);
        animator.SetBool("isIdle", isIdle);
        animator.SetBool("isParrying", isParrying);
        animator.SetBool("isAttacking", isAttacking);
        animator.SetBool("isDashing",isDashing);
    }
}
