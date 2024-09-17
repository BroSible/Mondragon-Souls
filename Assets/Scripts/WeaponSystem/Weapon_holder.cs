using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon_holder : MonoBehaviour
{
    //this script is purely for getting the scriptableObject

    //set this scpript only on weapon
    public Weapon weapon;

    private void OnTriggerEnter(Collider other)
    {

        if(other.CompareTag("Enemy"))
        {
            Enemy enemy = other.GetComponent<Enemy>();
            
            if (enemy != null && PlayerAttack._isAttacking)
            {
                enemy.TakingPlayerDamage(weapon.damage);
                Debug.Log($"Enemy take {weapon.damage} damage");
            }

            else{
                Debug.Log("??");
            }
        }
        
    }
}
