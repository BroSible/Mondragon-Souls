using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shield_holder : MonoBehaviour
{
    public Shield shield;

    private void OnTriggerEnter(Collider other)
    {
        Enemy enemy = GetComponent<Enemy>();

        if (enemy != null && PlayerLogic._isParrying && enemy._playerInAttackRange)
        {
            Debug.Log("Парирован");
        }
    }
}
