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

            Vector3 cameraForward = cameraTransform.forward;
            Vector3 cameraRight = cameraTransform.right;

            cameraForward.y = 0f;
            cameraRight.y = 0f;

            cameraForward.Normalize();
            cameraRight.Normalize();

            moveDirection = (moveHorizontal * cameraRight + moveVertical * cameraForward).normalized;
            moveDirection *= speed;

            // Проверка движения и установка флагов анимации
            if (moveDirection.magnitude > 0 && !PlayerLogic._isParrying && !playerAttack.isAttacking)
            {
                SetAnimatorFlags(isWalking: true, isIdle: false, isParrying: false, isAttacking: false);
            }
            else
            {
                SetAnimatorFlags(isWalking: false, isIdle: !PlayerLogic._isParrying && !playerAttack.isAttacking, isParrying: PlayerLogic._isParrying, isAttacking: playerAttack.isAttacking);
            }

            // Остановка движения при парировании или атаке
            if (PlayerLogic._isParrying)
            {
                moveDirection = Vector3.zero;
                SetAnimatorFlags(isWalking: false, isIdle: false, isParrying: true, isAttacking: false);
            }

            else if(playerAttack.isAttacking)
            {
                SetAnimatorFlags(isWalking: false, isIdle: false, isParrying: false, isAttacking: true);
            }
        }

        // Гравитация
        moveDirection.y -= gravity * Time.deltaTime;
        controller.Move(moveDirection * Time.deltaTime);
    }

    // Метод для упрощения установки флагов анимации
    void SetAnimatorFlags(bool isWalking, bool isIdle, bool isParrying, bool isAttacking)
    {
        animator.SetBool("isWalking", isWalking);
        animator.SetBool("isIdle", isIdle);
        animator.SetBool("isParrying", isParrying);
        animator.SetBool("isAttacking", isAttacking);
    }
}
