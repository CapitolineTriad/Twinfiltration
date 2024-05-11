using System.Diagnostics.Tracing;
using UnityEngine;

namespace LOS
{
    /// <summary>
    /// Disables a gameobjects renderer if the object is outside the line of sight
    /// </summary>
    [AddComponentMenu("Line of Sight/LOS Object Hider")]
    public class LOSObjectHider : MonoBehaviour
    {
        private LOSCuller m_Culler;
        private LOSVisibilityInfo m_VisibilityInfo;

        private Renderer[] m_Renderers;
        private LOSStencilRenderer[] m_StencilRenderers;
        private LOSSource[] m_LOSSources;

        private void OnEnable()
        {
        }

        private void Start()
        {
            m_VisibilityInfo = GetComponent<LOSVisibilityInfo>();

            m_Renderers = GetComponentsInChildren<Renderer>();
            m_StencilRenderers = GetComponentsInChildren<LOSStencilRenderer>();
            m_LOSSources = GetComponentsInChildren<LOSSource>();
        }

        private void UpdateLOSIndicators(float deltaTime)
        {
            if (m_MaskIntensity == m_IntensityTarget)
                return;

            float changeRate = (m_IntensityTarget > m_MaskIntensity ? deltaTime : -deltaTime) * 5;
            m_MaskIntensity = Mathf.Clamp(m_MaskIntensity + changeRate, 0.0f, 1.0f);
            foreach(var source in m_LOSSources)
                source.MaskIntensity = m_MaskIntensity;
        }

        private bool m_LastSeenState = true;
        private float m_MaskIntensity = 0.01f; // so they get hidden on spawn, wouldn't update otherwise
        private float m_IntensityTarget = 0f;
        private void LateUpdate()
        {
            var deltaTime = Time.unscaledDeltaTime;
            if (m_Culler != null && m_Culler.enabled)
            {
                if (m_LastSeenState != m_Culler.Visibile)
                {
                    m_LastSeenState = m_Culler.Visibile;
                    foreach (var renderer in m_Renderers)
                        renderer.enabled = m_LastSeenState;

                    foreach(var renderer in m_StencilRenderers)
                        renderer.enabled = m_LastSeenState;
                    m_IntensityTarget = m_LastSeenState ? 1 : 0;
                }
            }
            else if (m_VisibilityInfo != null && m_VisibilityInfo.isActiveAndEnabled)
            {
                if (m_LastSeenState != m_VisibilityInfo.Visibile)
                {
                    m_LastSeenState = m_VisibilityInfo.Visibile;
                    foreach (var renderer in m_Renderers)
                        renderer.enabled = m_LastSeenState;

                    foreach (var renderer in m_StencilRenderers)
                        renderer.enabled = m_LastSeenState;
                    m_IntensityTarget = m_LastSeenState ? 1 : 0;
                }
            }

            UpdateLOSIndicators(deltaTime);
        }
    }
}