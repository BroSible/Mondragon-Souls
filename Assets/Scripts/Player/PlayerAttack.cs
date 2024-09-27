using System.Collections;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    public Weapon currentWeapon; // ScriptableObject для текущего оружия
    public bool isAttacking = false;
    public static bool _isAttacking = false;
    public static bool _isReposting = false;
    private CameraCursor cameraCursor;
    private Animator animator;
    public Weapon_holder _weaponHolder;
    public Shield_holder _shieldHolder;


    void Start()
    {
        cameraCursor = GetComponent<CameraCursor>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        _weaponHolder = GetComponentInChildren<Weapon_holder>();
        _shieldHolder = GetComponentInChildren<Shield_holder>();
        #region смена оружия
        if (_weaponHolder != null)
        {
            currentWeapon = _weaponHolder.weapon;
            Debug.Log("Текущее оружие " + currentWeapon.name);
        }
        else
        {
            Debug.LogWarning("WeaponHolder не найден на объекте или его дочерних объектах.");
        }
        #endregion

        // Атака, если игрок не атакует, не парирует и не делает репост
        if (Input.GetKeyDown(KeyCode.Mouse0) && !isAttacking && !_isReposting && !PlayerLogic._isParrying)
        {
            Attack();
        }

        // Сильная атака
        else if (Input.GetKeyDown(KeyCode.LeftShift) && !isAttacking && !_isReposting && !PlayerLogic._isParrying)
        {
            EnhancedAttack();
        }

        // Репост, если успешное парирование
        else if (Input.GetKeyDown(KeyCode.Mouse0) && PlayerLogic._successfulParry && !_isReposting)
        {
            Repost();
        }
        else
        {
            animator.SetBool("isAttacking", false);
        }
    }

    void Attack()
    {
        if (!PlayerLogic._isParrying && !_isReposting) // Не атакуем во время парирования и репоста
        {
            isAttacking = true;
            _isAttacking = true;
            SetAnimatorFlags(isAttacking: true, isIdle: false, isParrying: false, isReposting: false);
            cameraCursor.enabled = false;
            StartCoroutine(ResetAttack());
        }
    }

    private void Repost()
    {
        if (PlayerLogic._successfulParry) // Проверяем, если парирование было успешным
        {
            _isReposting = true;
            SetAnimatorFlags(isAttacking: false, isIdle: false, isParrying: false, isReposting: true);
            cameraCursor.enabled = false;
            StartCoroutine(ResetRepost());
        }
    }

    void EnhancedAttack()
    {
        Debug.Log("Сделана сильная атака");
        // добавить логику усиленной атаки
    }

    public IEnumerator ResetAttack()
    {
        yield return new WaitForSeconds(currentWeapon.attackDelay);
        cameraCursor.enabled = true;
        isAttacking = false;
        _isAttacking = false;

        // Если парирование и репост не активны, вернуться в состояние Idle
        if (!PlayerLogic._isParrying && !_isReposting)
        {
            SetAnimatorFlags(isAttacking: false, isIdle: true, isParrying: false, isReposting: false);
        }
    }

    public IEnumerator ResetRepost()
    {
        yield return new WaitForSeconds(currentWeapon.attackDelay + 3f);
        _isReposting = false;
        cameraCursor.enabled = true;

        // Возвращаем флаги в состояние Idle после завершения репоста
        SetAnimatorFlags(isAttacking: false, isIdle: true, isParrying: false, isReposting: false);

        // Сброс флага успешного парирования после репоста
        PlayerLogic._successfulParry = false;
    }

    public void SetWeapon(Weapon newWeapon) // метод для обновления текущего оружия (для инвентаря)
    {
        currentWeapon = newWeapon;
    }

    void SetAnimatorFlags(bool isAttacking, bool isIdle, bool isParrying, bool isReposting)
    {
        animator.SetBool("isAttacking", isAttacking);
        animator.SetBool("isReposting", isReposting);
        animator.SetBool("isIdle", isIdle);
        animator.SetBool("isParrying", isParrying);
    }
}
