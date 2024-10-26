using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon_holder : MonoBehaviour
{
    //set this scpript only on weapon
    public Weapon weapon;
    private PlayerAttack _playerAttack;

    void Start()
    {
        _playerAttack = GetComponentInParent<PlayerAttack>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            Enemy enemy = other.GetComponent<Enemy>();
            
            if (_playerAttack.isAttacking)
            {
                enemy.TakingPlayerDmg(weapon.damage);
                Debug.Log($"Enemy take {weapon.damage} damage");
            }

            else if (_playerAttack.IsReposting)
            {
                enemy.TakingPlayerDmg(weapon.damage * weapon.criticalDamageСoefficient);
                Debug.Log($"Enemy take {weapon.damage * weapon.criticalDamageСoefficient} repost damage");
            }

            else if (PlayerAttack._isEnhancedAttacking)
            {
                enemy.TakingPlayerDmg(weapon.damage * 2f);
                Debug.Log($"Enemy take {weapon.damage * 2f} Enhanced damage");
            }
        }
        
    }
}
