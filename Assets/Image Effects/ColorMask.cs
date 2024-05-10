using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
[AddComponentMenu("Image Effects/Rendering/Color Mask")]
public class ColorMask : MonoBehaviour
{
    [SerializeField]
    private Shader m_Shader;

    [Range(0, 1f)]
    [SerializeField]
    private float m_Brightness = 1f;

    [Range(0, 1f)]
    [SerializeField]
    private float m_Red = 1f;
    [Range(0, 1f)]
    [SerializeField]
    private float m_Green = 1f;
    [Range(0, 1f)]
    [SerializeField]
    private float m_Blue = 1f;

    private Material m_Material;

    private void OnEnable()
    {
        // Disable the image effect if the m_Shader can't
        // run on the users graphics card
        if (!m_Shader || !m_Shader.isSupported)
            enabled = false;
    }

    private Material material
    {
        get
        {
            if (null == m_Material)
            {
                m_Material = new Material(m_Shader);
                m_Material.hideFlags = HideFlags.HideAndDontSave;
            }

            return m_Material;
        }
    }

    private void OnDisable()
    {
        if (m_Material)
            DestroyImmediate(m_Material);
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        material.SetFloat("_R", m_Red);
        material.SetFloat("_G", m_Green);
        material.SetFloat("_B", m_Blue);
        material.SetFloat("_Brightness", m_Brightness);
        Graphics.Blit(source, destination, material);
    }
}