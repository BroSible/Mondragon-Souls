using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shield_holder : MonoBehaviour
{
    public Shield _shield;
    public Enemy _enemy { get; private set; } // Враг, с которым произошло столкновение
    private PlayerLogic _playerLogic;
    private Coroutine _resetEnemyCoroutine;
    public float enemyDuration = 1.0f; // Время, на которое сохраняем врага после триггера

    private void Start()
    {
        _playerLogic = GetComponentInParent<PlayerLogic>();
    }

    private void OnTriggerEnter(Collider other)
    {
        // Проверяем, имеет ли коллайдер нужный тег
        if (other.CompareTag("EnemyTarget") && PlayerLogic._isParrying)
        {
            // Пытаемся найти компонент Enemy на корневом объекте (враге)
            Enemy detectedEnemy = other.GetComponentInParent<Enemy>();
            if (detectedEnemy != null)
            {
                // Сохраняем ссылку на врага и запускаем логику парирования
                _enemy = detectedEnemy;
                PlayerLogic._successfulParry = true;
                _enemy.ApplyParry();
                _playerLogic.Stamina -= 10f;

                // Перезапускаем корутину для сброса enemy
                if (_resetEnemyCoroutine != null)
                {
                    StopCoroutine(ResetEnemyAfterDuration());
                }
                _resetEnemyCoroutine = StartCoroutine(ResetEnemyAfterDuration());
            }
        }
    }

    private IEnumerator ResetEnemyAfterDuration()
    {
        yield return new WaitForSeconds(enemyDuration);
        _enemy = null; // Сбрасываем ссылку на врага
    }
}
