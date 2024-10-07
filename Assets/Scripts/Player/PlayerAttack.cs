using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    // в корутинах просто поставить время анимации для задержки, иначе анимация повторяется, что нам не надо
    public Weapon currentWeapon; // ScriptableObject для текущего оружия
    public bool isAttacking = false;
    [SerializeField] public bool IsReposting {get; set;} = false;
    public static bool _isEnhancedAttacking = false;
    private Animator animator;
    public Weapon_holder _weaponHolder;
    public Shield_holder _shieldHolder;
    private PlayerLogic _playerLogic;


    void Start()
    {
        animator = GetComponent<Animator>();
        _playerLogic = GetComponent<PlayerLogic>();
    }

    void Update()
    {
        bool canAttack = !isAttacking && !IsReposting && !PlayerLogic._isParrying;
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

        if (Input.GetKeyDown(KeyCode.Mouse0) && canAttack && _playerLogic.Stamina >= _weaponHolder.weapon.staminaCost)
        {
            Attack();
        }

        else if (Input.GetKeyDown(KeyCode.LeftShift) && canAttack && _playerLogic.Stamina >= _weaponHolder.weapon.staminaCost)
        {
            EnhancedAttack();
        }

        else if (Input.GetKeyDown(KeyCode.Mouse0) && PlayerLogic._successfulParry && !IsReposting)
        {
            Repost();
        }
    }

    void Attack()
    {
        isAttacking = true;

        if (_playerLogic.Stamina >= _weaponHolder.weapon.staminaCost)
        {
            _playerLogic.Stamina -= _weaponHolder.weapon.staminaCost;
        }
        
        SetAnimatorFlags(isAttacking: true, isIdle: false, isParrying: false, isReposting: false);
        StartCoroutine(ResetAttack());   
    }

    private void Repost()
    {
        if (PlayerLogic._successfulParry) 
        {
            IsReposting = true;
            if (_playerLogic.Stamina >= _weaponHolder.weapon.staminaCost)
            {
                _playerLogic.Stamina -= _weaponHolder.weapon.staminaCost + 15f;
            }
            SetAnimatorFlags(isAttacking: false, isIdle: false, isParrying: false, isReposting: true);
            StartCoroutine(ResetRepost());
        }
    }

    void EnhancedAttack()
    {
        Debug.Log("Сделана сильная атака");
        _isEnhancedAttacking = true;
        if (_playerLogic.Stamina >= _weaponHolder.weapon.staminaCost)
        {
            _playerLogic.Stamina -= _weaponHolder.weapon.staminaCost + 10f;
        }
        SetAnimatorFlags(isAttacking: true, isIdle: false, isParrying: false, isReposting: false);
        StartCoroutine(ResetAttack());  
    }

    public IEnumerator ResetAttack()
    {
        if(isAttacking)
        {
            yield return new WaitForSeconds(currentWeapon.attackDelay);
            isAttacking = false;
        }

        else if(_isEnhancedAttacking)
        {
            yield return new WaitForSeconds(currentWeapon.attackDelay + 2f); // тут заменить (потому что будет другая анимация для усиленной атаки и надо будет просто передавать время текущей анимации)
            _isEnhancedAttacking = false;
        }        
    }

    public IEnumerator ResetRepost()
    {
        yield return new WaitForSeconds(currentWeapon.attackDelay + 3f);
        IsReposting = false;

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
