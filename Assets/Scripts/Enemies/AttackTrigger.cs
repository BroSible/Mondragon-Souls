using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackTrigger : MonoBehaviour
{
    private Enemy _enemy;
    [SerializeField] private Collider _attackCollider;
    [SerializeField] private float _attackDelay;
    [SerializeField] private bool _canDamage;
    [SerializeField] private bool _blockAttack;

    private void Start()
    {
        _enemy = GetComponentInParent<Enemy>();
        _attackCollider = GetComponent<Collider>();
        _attackCollider.enabled = false;
        _blockAttack = false;
    }

    private void Update()
    {
        if (!_enemy._playerInAttackRange)
        {
            _canDamage = true;
        }
        else
        {
            _canDamage = false;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (_attackCollider.enabled && _canDamage && !_blockAttack)
            {
                DamageRealise();
                StartCoroutine(MakeDamageDelay());
            }
        }
    }

    public IEnumerator MakeDamageDelay()
    {
        _blockAttack = true;
        _canDamage = false;
        yield return new WaitForSeconds(_attackDelay);
        _blockAttack = false;
        _canDamage = true;
    }

    public void DamageRealise()
    {
        Debug.Log($"Противник нанёс {Enemy._enemyDamage} урона игроку");
        PlayerLogic.TakeDamage(Enemy._enemyDamage);
        Debug.Log($"Оставшееся здоровье игрока: {PlayerLogic._totalPlayerHealth}");   
    }
}
