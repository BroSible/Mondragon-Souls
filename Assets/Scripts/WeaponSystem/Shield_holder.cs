using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shield_holder : MonoBehaviour
{
    public Shield _shield;
    private Enemy _enemy;
    private PlayerLogic _playerLogic;

    private void Start()
    {
        _playerLogic = GetComponentInParent<PlayerLogic>();
    }

    public void OnTriggerEnter(Collider other)
    {
        _enemy = other.GetComponent<Enemy>();
        if(other.CompareTag("EnemyTarget") && PlayerLogic._isParrying)
        {
            PlayerLogic._successfulParry = true;
            _enemy.ApplyParry();
            _playerLogic.Stamina -= 10f;
        }
    }
}
