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
    private Animator animator;
    public Transform cameraTransform;
    private CameraCursor _cameraCursor;
    [SerializeField] private bool _isDashing = false;
    [SerializeField] private float _dashTime = 0.5f;
    [SerializeField] private float _dashSpeed;
    [SerializeField] private AnimationCurve _dashSpeedCurve;

    [SerializeField] private float currentSpeed = 0f; // Текущая скорость

    void Start()
    {
        controller = GetComponent<CharacterController>();
        _playerAttack = GetComponent<PlayerAttack>();
        _cameraCursor = GetComponent<CameraCursor>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        bool canMove = !_playerAttack.isAttacking && !PlayerAttack._isEnhancedAttacking && !PlayerLogic._isParrying;

        CameraCursorEnabled();
        UpdateAnimatorFlags();

        if (controller.isGrounded)
        {
            float moveHorizontal = Input.GetAxis("Horizontal");
            float moveVertical = Input.GetAxis("Vertical");

            // Получаем направление движения относительно камеры
            Vector3 cameraForward = cameraTransform.forward;
            Vector3 cameraRight = cameraTransform.right;
            cameraForward.y = 0f;
            cameraRight.y = 0f;
            cameraForward.Normalize();
            cameraRight.Normalize();

            // Вычисляем вектор направления движения
            Vector3 targetDirection = (moveHorizontal * cameraRight + moveVertical * cameraForward).normalized;

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
            if(Input.GetKey(KeyCode.Space))
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


    //Метод отключения поворота камеры на курсором, если игрок ходит. Если он стоит, то скрипт будет включен обратно.
    void CameraCursorEnabled()
    {
        if(moveDirection.magnitude >= 1f || _playerAttack.isAttacking || PlayerAttack._isEnhancedAttacking || PlayerLogic._isParrying || _isDashing)
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
        if (_isDashing)
        {
            yield break;
        }

        if (direction == Vector3.zero)
        {
            direction = transform.forward;
        }

        _isDashing = true;
        float elapsedTime = 0f;

        while (elapsedTime < _dashTime)
        {
            float speedMultiplier = _dashSpeed * _dashSpeedCurve.Evaluate(elapsedTime / _dashTime);

            moveDirection = direction * speedMultiplier;
            controller.Move(moveDirection * Time.deltaTime);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        _isDashing = false;
        yield break;
    }


    void UpdateAnimatorFlags()
    {
        bool isWalking = currentSpeed >= 1f && !_playerAttack.isAttacking && !PlayerLogic._isParrying && !_isDashing; 
        bool isIdle = !isWalking && !_playerAttack.isAttacking && !PlayerLogic._isParrying && !PlayerAttack._isEnhancedAttacking && !_isDashing;
        bool isParrying = PlayerLogic._isParrying;
        bool isAttacking = _playerAttack.isAttacking || PlayerAttack._isEnhancedAttacking;
        bool isDashing = _isDashing;
        animator.SetBool("isWalking", isWalking);
        animator.SetBool("isIdle", isIdle);
        animator.SetBool("isParrying", isParrying);
        animator.SetBool("isAttacking", isAttacking);
        animator.SetBool("isDashing",isDashing);
    }
}
