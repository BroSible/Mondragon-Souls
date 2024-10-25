using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System;
using JetBrains.Annotations;

public class SpiderVillager : Enemy
{
    public float distanceToPlayer;

    protected override void Start()
    {
        base.Start();
        Attack += PlayAttackAnimation;
        Run += PlayRunAnimation;
        Idle += PlayIdleAnimation;
        Parried += PlayParriedAnimation;
    }

    protected override void PlayerChasing()
    {
        base.PlayerChasing();

        distanceToPlayer = Vector3.Distance(transform.position, _target.position);

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
        }
    }

    private void PlayAttackAnimation()
    {
        _animator.Play("Attack");
    }

    private void PlayRunAnimation()
    {
        _animator.Play("run");
    }

    private void PlayIdleAnimation()
    {
        _animator.Play("Idle");
    }

    private void PlayParriedAnimation()
    {
        _animator.Play("Parried"); // заменить на анимацию ошеломления когда враг парирован
    }
}
