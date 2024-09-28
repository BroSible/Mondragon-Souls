using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon", menuName = "Weapon")]
public class Weapon : ScriptableObject
{
    //добавляем сюда различные свойства оружия, впредь до их спрайта для иконок

    public enum WeaponType
    {
        Sword, // Меч
        Axe, // Топор/Секира
        Spear, // Копьё
        Scythe, // Коса
        Dagger, // Кинжал
        Epee // Шпага
    }

    public enum WeaponCombatRange
    {
        Short,
        Medium,
        High
    }


    [Header("WeaponSpecs")]
    public new string name;
    public WeaponType type;
    public WeaponCombatRange combatRange;
    public float attackDelay;
    public string description;
    public float damage;
    public float criticalDamageСoefficient;
    public bool isTwoHanded;
}
