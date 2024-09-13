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
        Debug.Log("попал по врагу");
        if(other.CompareTag("Enemy"))
        {
            Enemy enemy = other.GetComponent<Enemy>();
            
            if (enemy != null)
            {
                enemy.TakingPlayerDamage(weapon.damage);
                Debug.Log($"Enemy take {weapon.damage} damage");
            }
        }
        
    }
}
