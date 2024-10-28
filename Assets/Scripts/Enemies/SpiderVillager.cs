using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System;
using JetBrains.Annotations;

public class SpiderVillager : Enemy
{
    protected override void Start()
    {
        base.Start();
        Attack += PlayAttackAnimation;
        Run += PlayRunAnimation;
        Idle += PlayIdleAnimation;
        Parried += PlayParriedAnimation;
    }

    protected override void PlayerChase()
    {
        base.PlayerChase();
    }

    protected override void PlayerAttack()
    {
        base.PlayerAttack();
    }

    private void PlayAttackAnimation()
    {
        _animator.Play("Attack");
        Debug.Log("Анимация атаки..");
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
