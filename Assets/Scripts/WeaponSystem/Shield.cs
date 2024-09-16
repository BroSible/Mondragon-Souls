using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Shield", menuName = "Shield")]
public class Shield : ScriptableObject
{
    [Header("ShieldSpecs")]
    public float framePeriod; // промежуток времени, за который можно парировать атаку врага (в миллесекундах)
    public string type;
    public float protectionFactor; // коэффициент защиты
    public float rollbackTime;
    public string description;
    public float resetParry;
}
