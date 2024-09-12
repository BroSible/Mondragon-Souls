using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageDealer : MonoBehaviour
{
    public BodyParts.BodyPart TargetPart; // Какая часть тела будет повреждена
    public float DamageAmount;  // Урон, который будет нанесён

    private void OnCollisionEnter(Collision collision)
    {
        PlayerBody playerBody = collision.gameObject.GetComponent<PlayerBody>();
        
        if (playerBody != null)
        {
            playerBody.TakeDamage(TargetPart, DamageAmount);
        }
    }
}
