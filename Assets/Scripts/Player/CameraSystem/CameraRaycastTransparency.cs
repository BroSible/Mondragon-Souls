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
                fadeObject.FadeOut(fadeSpeed);
            }
            else
            {
                fadeObject.FadeIn(fadeSpeed);

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

    public FadeObject(Renderer renderer, Material material)
    {
        this.renderer = renderer;
        this.material = material;
        this.originalColor = material.color;
    }

    public void FadeOut(float fadeSpeed)
    {
        SetMaterialFadeMode();

        Color color = material.color;
        color.a = Mathf.Max(0, color.a - fadeSpeed * Time.deltaTime);
        material.color = color;
    }

    public void FadeIn(float fadeSpeed)
    {
        Color color = material.color;
        color.a = Mathf.Min(originalColor.a, color.a + fadeSpeed * Time.deltaTime);
        material.color = color;

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
        // Проверка, поддерживает ли шейдер прозрачность
        if (material.shader.name != "Universal Render Pipeline/Lit")
        {
            Debug.LogWarning("Материал не использует шейдер URP/Lit. Замените шейдер для корректной работы прозрачности.");
            return;
        }
        
        // Установка параметров прозрачности для URP
        material.SetOverrideTag("RenderType", "Transparent");
        material.SetInt("_Surface", 1); // 1 означает Transparent в URP
        material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        material.SetInt("_ZWrite", 0);
        material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
        material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        material.EnableKeyword("_ALPHAPREMULTIPLY_ON");
    }


    private void ResetMaterialMode()
    {
        material.SetOverrideTag("RenderType", "Opaque");
        material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
        material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
        material.SetInt("_ZWrite", 1);
        material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Geometry;
        material.DisableKeyword("_SURFACE_TYPE_TRANSPARENT");
    }
}
