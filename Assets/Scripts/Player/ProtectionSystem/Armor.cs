using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "New Armor", menuName = "Armor")]
public class Armor : ScriptableObject
{
    public enum ArmorType 
    {
        Heavy,
        Light
    }

    public ArmorType armorType;
    public string armorInfo;
    
    public float headProtectionFactor;
    public float armProtectionFactor;
    public float bodyProtectionFactor;
    public float legProtectionFactor;
}
