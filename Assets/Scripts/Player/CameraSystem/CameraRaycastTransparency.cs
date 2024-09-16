using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CameraRaycastTransparency : MonoBehaviour
{
    public Transform player;
    public LayerMask obstacleLayer;
    public float fadeSpeed = 2f;
    public Color rayColor = Color.red;

    private List<FadeObject> fadeObjects = new List<FadeObject>(); // Список всех объектов, которые скрываем

    void Update()
    {
        if (player != null)
        {
            HandleRaycast();
            UpdateFadeObjects();
        }
    }

    void HandleRaycast()
    {
        ResetVisibility();

        Vector3 direction = player.position - Camera.main.transform.position;

        float distanceToPlayer = Vector3.Distance(Camera.main.transform.position, player.position);
        Debug.DrawLine(Camera.main.transform.position, player.position, rayColor);

        RaycastHit[] hits;

        hits = Physics.RaycastAll(Camera.main.transform.position, direction, distanceToPlayer, obstacleLayer);

        foreach (RaycastHit hit in hits)
        {
            Renderer hitRenderer = hit.collider.GetComponent<Renderer>();

            if (hitRenderer != null)
            {
                FadeObject fadeObject = fadeObjects.Find(f => f.renderer == hitRenderer);

                if (fadeObject == null)
                {
                    Material newMaterial = new Material(hitRenderer.material);
                    newMaterial.shader = hitRenderer.material.shader;
                    hitRenderer.material = newMaterial;

                    fadeObject = new FadeObject(hitRenderer, newMaterial);
                    fadeObjects.Add(fadeObject);
                }
                fadeObject.isFadingOut = true;
            }
        }
    }

    void ResetVisibility()
    {
        foreach (FadeObject fadeObject in fadeObjects)
        {
            fadeObject.isFadingOut = false;
        }
    }

    void UpdateFadeObjects()
    {
        for (int i = fadeObjects.Count - 1; i >= 0; i--)
        {
            FadeObject fadeObject = fadeObjects[i];

            if (fadeObject.isFadingOut)
            {
                // Плавно уменьшаем прозрачность
                fadeObject.FadeOut(fadeSpeed);
            }
            else
            {
                // Плавно возвращаем прозрачность
                fadeObject.FadeIn(fadeSpeed);

                // Если объект полностью восстановил прозрачность, удаляем его из списка
                if (fadeObject.IsFullyVisible())
                {
                    fadeObject.ResetMaterial();
                    fadeObjects.RemoveAt(i);
                }
            }
        }
    }
}

public class FadeObject
{
    public Renderer renderer;
    public Material material;
    public bool isFadingOut;
    private Color originalColor;
    private float originalMode;

    public FadeObject(Renderer renderer, Material material)
    {
        this.renderer = renderer;
        this.material = material;
        this.originalColor = material.color;
        this.originalMode = material.GetFloat("_Mode");
    }

    public void FadeOut(float fadeSpeed)
    {
        // Устанавливаем режим рендеринга в Fade для плавной прозрачности
        if (material.GetFloat("_Mode") != 2f) // 2 соответствует режиму Fade
        {
            SetMaterialFadeMode();
        }

        Color color = material.color;
        color.a = Mathf.Max(0, color.a - fadeSpeed * Time.deltaTime);
        material.color = color;
    }

    public void FadeIn(float fadeSpeed)
    {
        Color color = material.color;
        color.a = Mathf.Min(originalColor.a, color.a + fadeSpeed * Time.deltaTime);
        material.color = color;

        // Если прозрачность полностью восстановлена, возвращаем исходный режим рендеринга
        if (IsFullyVisible())
        {
            ResetMaterialMode();
        }
    }

    public bool IsFullyVisible()
    {
        return Mathf.Approximately(material.color.a, originalColor.a);
    }

    public void ResetMaterial()
    {
        material.color = originalColor;
        ResetMaterialMode();
    }

    private void SetMaterialFadeMode()
    {
        material.SetFloat("_Mode", 2f);
        material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        material.SetInt("_ZWrite", 0);
        material.DisableKeyword("_ALPHATEST_ON");
        material.EnableKeyword("_ALPHABLEND_ON");
        material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
    }

    private void ResetMaterialMode()
    {
        material.SetFloat("_Mode", originalMode);
        material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
        material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
        material.SetInt("_ZWrite", 1);
        material.DisableKeyword("_ALPHATEST_ON");
        material.DisableKeyword("_ALPHABLEND_ON");
        material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        material.renderQueue = -1;
    }
}
