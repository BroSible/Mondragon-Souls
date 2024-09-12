using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float speed;
    public float gravity;
    private CharacterController controller;
    private Vector3 moveDirection = Vector3.zero;
    private PlayerAttack playerAttack;
    private Animator animator;
    public Transform cameraTransform; // Ссылка на камеру для расчета направления

    void Start()
    {
        controller = GetComponent<CharacterController>();
        playerAttack = GetComponent<PlayerAttack>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (controller.isGrounded)
        {
            float moveHorizontal = Input.GetAxis("Horizontal");
            float moveVertical = Input.GetAxis("Vertical");

            // Используем forward и right камеры для направления движения
            Vector3 cameraForward = cameraTransform.forward;
            Vector3 cameraRight = cameraTransform.right;

            // Проецируем направления на плоскость XZ (обнуляем Y, чтобы избежать движения по вертикали)
            cameraForward.y = 0f;
            cameraRight.y = 0f;

            // Нормализуем векторы (чтобы их длина всегда была 1)
            cameraForward.Normalize();
            cameraRight.Normalize();

            // Рассчитываем направление движения относительно направления камеры
            moveDirection = (moveHorizontal * cameraRight + moveVertical * cameraForward).normalized;
            moveDirection *= speed;

            // Устанавливаем анимацию движения
            if (moveDirection.magnitude > 0)
            {
                animator.SetBool("isWalking", true);
            }
            else
            {
                animator.SetBool("isWalking", false);
            }
        }

        if (playerAttack.isAttacking == true)
        {
            moveDirection = Vector3.zero;
            animator.SetBool("isWalking", false);
        }

        // Применяем гравитацию
        moveDirection.y -= gravity * Time.deltaTime;
        controller.Move(moveDirection * Time.deltaTime);

        // Проверяем, стоит ли игрок
        Vector3 horizontalMove = new Vector3(moveDirection.x, 0, moveDirection.z);
        if (horizontalMove == Vector3.zero && !playerAttack.isAttacking)
        {
            animator.SetBool("isIdle", true);
        }
        else
        {
            animator.SetBool("isIdle", false);
        }
    }
}
