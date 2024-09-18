using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ArmorHolder : MonoBehaviour
{
    public Armor armor;
    public PlayerLogic playerLogic;

    private void Start()
    {
        playerLogic = GetComponentInParent<PlayerLogic>();
    
        GetArmorStats();
    }

    private void Update()   
    {
        GetArmorStats();
    }

    private void GetArmorStats()
    {
        playerLogic.headHealthPoints += armor.headProtectionFactor;
        playerLogic.leftArmHealthPoints += armor.armProtectionFactor;
        playerLogic.rightArmHealthPoints += armor.armProtectionFactor;
        playerLogic.leftLegHealthPoints += armor.legProtectionFactor;
        playerLogic.rightLegHealthPoints += armor.legProtectionFactor;
        playerLogic.bodyHealthPoints += armor.bodyProtectionFactor;
    }
}
