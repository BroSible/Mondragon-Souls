using System.Collections;
using System.Collections.Generic;
using UnityEditor.MPE;
using UnityEngine;

public class AttackTrigger : MonoBehaviour
{
    private Enemy _enemy;
    [SerializeField] private Collider _attackCollider;
    private Animator _animator; 
    [SerializeField] private float _damageDelay;
    [SerializeField] private bool _canDamage = true; // Изначально установлено в true, чтобы сразу можно было нанести урон
    [SerializeField] private bool _inAttackTrigger = false;
    private Coroutine _damageCoroutine = null;
    [SerializeField] private string _attackAnimationName; // Имя анимации атаки

    private void Start()
    {
        _enemy = GetComponentInParent<Enemy>();
        _attackCollider = GetComponent<Collider>();

        _attackCollider.enabled = false; // Изначально коллайдер отключен
        _animator = GetComponentInParent<Animator>();
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player") && !_canDamage)
        {
            Debug.Log("Игрок в зоне атаки");

            _canDamage = true;
            _inAttackTrigger = true;

            if (_canDamage && _damageCoroutine == null)
            {
                _damageCoroutine = StartCoroutine(MakeDamageDelay());
            }
        }
        else
        {
            _inAttackTrigger = false;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        _inAttackTrigger = false;
        _canDamage = false; // альтернативная логика

        if (_damageCoroutine != null)
        {
            _damageCoroutine = null;
            _canDamage = true;
        }
    }

    public void DamageRealise()
    {
        Debug.Log($"Противник нанёс {Enemy._enemyDamage} урона игроку");
        PlayerLogic.TakeDamage(Enemy._enemyDamage);
    }

    public IEnumerator MakeDamageDelay()
    {
        _canDamage = false; // Запрещаем нанесение урона до завершения задержки
        yield return new WaitForSeconds(_damageDelay);

        if (_inAttackTrigger)
        {
            DamageRealise();
            // StartCoroutine(EnableColliderAtEnd());
        }
        else
        {
            Debug.Log("Триггер inAttackTrigger не сработал");
        }

        _canDamage = true; // Разрешаем следующую атаку после задержки
        _damageCoroutine = null; // Очищаем ссылку на корутину
    }

    private IEnumerator EnableColliderAtEnd()
    {
        AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(0);
        float animationLength = stateInfo.length;
        yield return new WaitForSeconds(animationLength);
        _attackCollider.enabled = true;
        DamageRealise();
        yield return new WaitForSeconds(0.1f); // Короткая задержка, чтобы игрок успел попасть под атаку
        _attackCollider.enabled = false;
    }
}
