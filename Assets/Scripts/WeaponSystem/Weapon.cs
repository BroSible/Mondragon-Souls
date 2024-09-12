using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon", menuName = "Weapon")]
public class Weapon : ScriptableObject
{
    //добавляем сюда различные свойства оружия, впредь до их спрайта для иконок

    [Header("WeaponSpecs")]
    public new string name;
    public string type;
    public float attackDelay;
    public string description;
    public float damage;

    [Header("Visual")]
    public GameObject modelMesh;
    public Vector3 displayScale = new Vector3(1f, 1f, 1f); // Добавлено поле для масштаба отображения
}
