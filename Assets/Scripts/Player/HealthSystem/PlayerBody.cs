using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBody : MonoBehaviour
{
    // Ссылки на модели частей тела

    [Header("BodyPartsModels")]
    public GameObject headModel;
    public GameObject torsoModel;
    public GameObject leftArmModel;
    public GameObject rightArmModel;
    public GameObject leftLegModel;
    public GameObject rightLegModel;


    private Dictionary<BodyParts.BodyPart, BodyPartStatus> bodyParts;

    private void Start()
    {
        bodyParts = new Dictionary<BodyParts.BodyPart, BodyPartStatus>
        {
            { BodyParts.BodyPart.Head, new BodyPartStatus(BodyParts.BodyPart.Head) },
            { BodyParts.BodyPart.Torso, new BodyPartStatus(BodyParts.BodyPart.Torso) },
            { BodyParts.BodyPart.LeftArm, new BodyPartStatus(BodyParts.BodyPart.LeftArm) },
            { BodyParts.BodyPart.RightArm, new BodyPartStatus(BodyParts.BodyPart.RightArm) },
            { BodyParts.BodyPart.LeftLeg, new BodyPartStatus(BodyParts.BodyPart.LeftLeg) },
            { BodyParts.BodyPart.RightLeg, new BodyPartStatus(BodyParts.BodyPart.RightLeg) }
        };
    }

    // Метод для отключения конечности (отключает визуальный объект)
    void SeverLimb(BodyParts.BodyPart part)
    {
        switch (part)
        {
            case BodyParts.BodyPart.Head:
                headModel.SetActive(false);
                break;
            case BodyParts.BodyPart.Torso:
                torsoModel.SetActive(false);
                break;
            case BodyParts.BodyPart.LeftArm:
                leftArmModel.SetActive(false);
                break;
            case BodyParts.BodyPart.RightArm:
                rightArmModel.SetActive(false);
                break;
            case BodyParts.BodyPart.LeftLeg:
                leftLegModel.SetActive(false);
                break;
            case BodyParts.BodyPart.RightLeg:
                rightLegModel.SetActive(false);
                break;

        }
    }

    // Вызов метода при повреждении конечности
    public void TakeDamage(BodyParts.BodyPart part, float damage)
    {
        if (bodyParts.ContainsKey(part))
        {
            bodyParts[part].TakeDamage(damage);

            if (bodyParts[part].State == BodyParts.InjuryState.Severed)
            {
                SeverLimb(part); // Отключаем конечность при потере
            }

            
        }
    }
}
