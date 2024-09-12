using System.Collections;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    public Weapon currentWeapon; // ScriptableObject для текущего оружия
    private float attackDelay; // Время задержки атаки
    public bool isAttacking = false;
    private CameraCursor cameraCursor;
    private Animator animator;
    public Weapon_holder _weaponHolder;


    void Start()
    {
        cameraCursor = GetComponent<CameraCursor>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        _weaponHolder = GetComponentInChildren<Weapon_holder>();
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

        else
        {
            animator.SetBool("isAttacking", false);
        }

        
    }

    void Attack()
    {
        isAttacking = true;
        animator.SetBool("isAttacking", true);
        cameraCursor.enabled = false;
        StartCoroutine(ResetAttack());
    }

    void EnhancedAttack()
    {
        Debug.Log("Сделана сильная атака");
    }

    public IEnumerator ResetAttack()
    {
        yield return new WaitForSeconds(currentWeapon.attackDelay);
        cameraCursor.enabled = true;
        isAttacking = false;
    }

    public void SetWeapon(Weapon newWeapon) // метод для обновления текущего оружия (для инвентаря)
    {
        currentWeapon = newWeapon;
    }

}
