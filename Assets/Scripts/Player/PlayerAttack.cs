using System.Collections;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    // в корутинах просто поставить время анимации для задержки, иначе анимация повторяется, что нам не надо
    public Weapon currentWeapon; // ScriptableObject для текущего оружия
    public bool isAttacking = false;
    public static bool _isAttacking = false; // статик переменная для Weapon_Holder
    public static bool _isReposting = false;
    public static bool _isEnhancedAttacking = false;
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

        if (Input.GetKeyDown(KeyCode.Mouse0) && !isAttacking && !_isReposting && !PlayerLogic._isParrying)
        {
            Attack();
        }

        else if (Input.GetKeyDown(KeyCode.LeftShift) && !isAttacking && !_isReposting && !PlayerLogic._isParrying)
        {
            EnhancedAttack();
        }

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
        isAttacking = true;
        _isAttacking = true;
        SetAnimatorFlags(isAttacking: true, isIdle: false, isParrying: false, isReposting: false);
        cameraCursor.enabled = false;
        StartCoroutine(ResetAttack());   
    }

    private void Repost()
    {
        if (PlayerLogic._successfulParry) 
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
        _isEnhancedAttacking = true;
        SetAnimatorFlags(isAttacking: true, isIdle: false, isParrying: false, isReposting: false);
        cameraCursor.enabled = false;
        StartCoroutine(ResetAttack());  
    }

    public IEnumerator ResetAttack()
    {
        if(isAttacking)
        {
            yield return new WaitForSeconds(currentWeapon.attackDelay);
            cameraCursor.enabled = true;
            isAttacking = false;
            _isAttacking = false;
        }

        else if(_isEnhancedAttacking)
        {
            yield return new WaitForSeconds(currentWeapon.attackDelay + 2f); // тут заменить (потому что будет другая анимация для усиленной атаки и надо будет просто передавать время текущей анимации)
            cameraCursor.enabled = true;
            _isEnhancedAttacking = false;
        }


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

        SetAnimatorFlags(isAttacking: false, isIdle: true, isParrying: false, isReposting: false);

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
