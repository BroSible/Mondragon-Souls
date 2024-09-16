using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

public class Shield_holder : MonoBehaviour
{
    public Shield shield;

    public void OnTriggerEnter(Collider other)
    {
        Enemy enemy = GetComponent<Enemy>();
    }
}
