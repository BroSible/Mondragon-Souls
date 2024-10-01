using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

public class Shield_holder : MonoBehaviour
{
    public Shield shield;

    public void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("EnemyTarget") && PlayerLogic._isParrying)
        {
            PlayerLogic._successfulParry = true;
        }
    }
}
