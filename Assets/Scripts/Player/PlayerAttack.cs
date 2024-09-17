using System.Collections;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    public Weapon currentWeapon; // ScriptableObject для текущего оружия
    public bool isAttacking = false;
    public static bool _isAttacking = false;
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

        if (Input.GetKeyDown(KeyCode.Mouse0) && !isAttacking)
        {
            Attack();
        }

        else if (Input.GetKeyDown(KeyCode.LeftShift) && !isAttacking)
        {
            EnhancedAttack();
        }

        else if(Input.GetKeyDown(KeyCode.Mouse0) && PlayerLogic.successfulParry)
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
        animator.SetBool("isAttacking", true);
        cameraCursor.enabled = false;
        StartCoroutine(ResetAttack());
    }

    private void Repost()
    {
        animator.SetBool("isReposting", true);
        animator.SetBool("isIdle", false);
        cameraCursor.enabled = false;
        StartCoroutine(ResetAttack());
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
    }

    public void SetWeapon(Weapon newWeapon) // метод для обновления текущего оружия (для инвентаря)
    {
        currentWeapon = newWeapon;
    }

}
