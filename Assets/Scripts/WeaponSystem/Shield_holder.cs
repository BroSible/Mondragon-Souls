using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

public class Shield_holder : MonoBehaviour
{
    public Shield shield;
    private Enemy enemy;

    public void OnTriggerEnter(Collider other)
    {
        enemy = other.GetComponent<Enemy>();
        if(other.CompareTag("Enemy") && PlayerLogic._isParrying)
        {
            PlayerLogic._successfulParry = true;
            enemy.ApplyParry();
        }
    }
}
