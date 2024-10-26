using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackTrigger : MonoBehaviour
{
    private Enemy _enemy;

    [SerializeField] private Collider _attackCollider;

    [SerializeField] private float _attackDelay;

    [SerializeField] public bool _canDamage;

    private void Start()
    {
        _enemy = GetComponentInParent<Enemy>();
        _attackCollider = GetComponent<Collider>();
        _attackCollider.enabled = false;
        _canDamage = true;
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (_enemy.currentEnemyState == Enemy.EnemyState.Attacking)
            {
                Debug.Log("Возможность наносить урон!");

                if (_attackCollider.enabled && _canDamage)
                {
                    Debug.Log("Реализация дамага");
                    DamageRealise();
                    StartCoroutine(MakeDamageDelay());
                }
            }
        }
    }

    public IEnumerator MakeDamageDelay()
    {
        _canDamage = false;
        yield return new WaitForSeconds(_attackDelay);
        _canDamage = true;
    }

    public void DamageRealise()
    {
        Debug.Log($"Противник нанёс {Enemy._enemyDamage} урона игроку");
        PlayerLogic.TakeDamage(Enemy._enemyDamage);
        Debug.Log($"Оставшееся здоровье игрока: {PlayerLogic._totalPlayerHealth}");
    }
}
