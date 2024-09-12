using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BodyPartStatus : MonoBehaviour
{
    public BodyParts.BodyPart Part { get; private set; }
    public BodyParts.InjuryState State { get; private set; }
    public float Health { get; private set; } // Здоровье этой части тела

    public BodyPartStatus(BodyParts.BodyPart part)
    {
        Part = part;
        State = BodyParts.InjuryState.Healthy;
        Health = 100f; // Начальное здоровье
    }

    // Метод для получения урона
    public void TakeDamage(float damage)
    {
        if (State != BodyParts.InjuryState.Severed) // Если конечность не потеряна
        {
            Health -= damage;
            if (Health <= 0)
            {
                State = BodyParts.InjuryState.Severed; // Потеря конечности
            }
            else
            {
                State = BodyParts.InjuryState.Damaged; // Урон конечности
            }
        }
    }
}
