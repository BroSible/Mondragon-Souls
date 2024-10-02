using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shield_holder : MonoBehaviour
{
    public Shield shield;
    private Enemy enemy;

    public void OnTriggerEnter(Collider other)
    {
        enemy = other.GetComponentInParent<Enemy>();
        if(other.CompareTag("EnemyTarget") && PlayerLogic._isParrying)
        {
            PlayerLogic._successfulParry = true;
            enemy.ApplyParry();
        }
    }
}
