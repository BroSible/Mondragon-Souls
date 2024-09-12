using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon", menuName = "Weapon")]
public class Weapon : ScriptableObject
{
    //добавляем сюда различные свойства оружия, впредь до их спрайта для иконок
    public new string name;
    public string type;
    public float attackDelay;
    public string description;
    public float damage;
}
