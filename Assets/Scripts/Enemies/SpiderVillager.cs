using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System;

public class SpiderVillager : Enemy
{
    private Vector3 _randomOffset;
    private float _timeSinceLastDirectionChange = 0f;
    private float _directionChangeInterval = 1f;

    protected override void Start()
    {
        base.Start();
    }

    protected override void SearchPatrolPoint()
    {
        NavMeshHit hit;
        // NavMeshQueryFilter filter = new NavMeshQueryFilter();

        Vector3 randomPoint = transform.position + UnityEngine.Random.insideUnitSphere * _patrolPointRange;

        if (NavMesh.SamplePosition(randomPoint, out hit, _patrolPointRange, NavMesh.AllAreas))
        {
            _currentPatrolPoint = hit.position;
            _isPatrolPointSet = true;

            _randomOffset = new Vector3(UnityEngine.Random.Range(-2f, 2f), UnityEngine.Random.Range(-2f, 2f));
        }
    }

    protected override void Patrolling()
    {
        if (!_isPatrolPointSet)
        {
            SearchPatrolPoint();
        }
        else
        {
            // Периодически изменяем направление движения
            if (_timeSinceLastDirectionChange > _directionChangeInterval)
            {
                // Меняем скорость на случайную для создания эффекта "рывков"
                _agent.speed = UnityEngine.Random.Range(_patrolSpeed * 0.5f, _patrolSpeed * 1.5f);
                _timeSinceLastDirectionChange = 0f;
            }

            // Добавляем случайное отклонение от цели
            _agent.SetDestination(_currentPatrolPoint + _randomOffset);

            _timeSinceLastDirectionChange += Time.deltaTime;
        }

        Vector3 distanceToPatrolPoint = transform.position - _currentPatrolPoint;

        if (distanceToPatrolPoint.magnitude < 1f)
        {
            _isPatrolPointSet = false;
        }
    }

    protected override void PlayerChasing()
    {
        base.PlayerChasing();
        float distanceToPlayer = Vector3.Distance(transform.position, _target.position);

        if (distanceToPlayer > _attackRange)
        {
            _agent.isStopped = false;
            _agent.SetDestination(_target.position);
        }
        else
        {
            currentEnemyState = EnemyState.inAttack;
        }

        // Переход в патрулирование, если игрок вне зоны преследования
        if (!_playerInChaseRange)
        {
            currentEnemyState = EnemyState.inPatrolling;
            Debug.Log("Игрок вышел за пределы радиуса преследования. Враг возвращается к патрулированию.");
        }
    }
}
