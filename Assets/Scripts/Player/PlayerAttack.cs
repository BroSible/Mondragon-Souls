using System.Collections;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [Header("BasicComponents")]
    private Animator _animator;
    private System.Random _playerRandom = new System.Random();


    [Header("ScriptComponents")]
    private CameraCursor _cameraCursor;
    public Weapon_holder _weaponHolder;
    public Weapon currentWeapon; // ScriptableObject для текущего оружия
    public Enemy enemy;


    [Header("Fields")]
    [SerializeField] private int _minPlayerDamage = 1;
    [SerializeField] private int _maxPlayerDamage = 7;

    // Изменить в будущем на более гибкую формулу высчитывания урона

    [SerializeField] private float _attackRange = 5f;
    [SerializeField] protected bool _enemyInAttackRange;

    private float _attackCooldown = 3f; // Время задержки атаки
    public bool isAttacking = false;

    

    void Start()
    {
        _cameraCursor = GetComponent<CameraCursor>();
        _animator = GetComponent<Animator>();
    }

    void Update()
    {
        _weaponHolder = GetComponentInChildren<Weapon_holder>();
        
        #region Смена оружия
        
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
            _animator.SetBool("isAttacking", false);
        }
    }

    public int GetRandomPlayerDamage()
    {
        return _playerRandom.Next(_minPlayerDamage, _maxPlayerDamage + 1);
    }

    public void Attack()
    {
        isAttacking = true;

        int currentPlayerDamage = GetRandomPlayerDamage();
        enemy.TakingPlayerDamage(currentPlayerDamage);
        Debug.Log($"Игрок наносит врагу {currentPlayerDamage} ед. урона!");

        _animator.SetBool("isAttacking", true);
        _cameraCursor.enabled = false;
        StartCoroutine(ResetAttack());
    }


    public void EnhancedAttack()
    {
        Debug.Log("Сделана сильная атака");
    }

    public IEnumerator ResetAttack()
    {
        yield return new WaitForSeconds(currentWeapon.attackDelay);
        _cameraCursor.enabled = true;
        isAttacking = false;
    }

    public void SetWeapon(Weapon newWeapon) // метод для обновления текущего оружия (для инвентаря)
    {
        currentWeapon = newWeapon;
    }
}
