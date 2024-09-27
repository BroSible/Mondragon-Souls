using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon_holder : MonoBehaviour
{
    //set this scpript only on weapon
    public Weapon weapon;

    private void OnTriggerEnter(Collider other)
    {
        other = GetComponentInParent<Collider>();
        if(other.CompareTag("Enemy"))
        {
            Enemy enemy = other.GetComponent<Enemy>();
            
            if (enemy != null && PlayerAttack._isAttacking)
            {
                enemy.TakingPlayerDamage(weapon.damage);
                Debug.Log($"Enemy take {weapon.damage} damage");
            }

            else if(PlayerAttack._isReposting)
            {
                enemy.TakingPlayerDamage(weapon.damage * weapon.criticalDamageСoefficient);
                Debug.Log($"Enemy take {weapon.damage * weapon.criticalDamageСoefficient} damage");
            }
        }
        
    }
}
