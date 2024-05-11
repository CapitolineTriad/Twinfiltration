using System.Collections.Generic;
using UnityEngine;

namespace LOS
{
    [AddComponentMenu("Line of Sight/LOS Culler")]
    public class LOSCuller : MonoBehaviour
    {
        [Tooltip("Selects which layers block raycasts used for visibility calculations")]
        [SerializeField]
        private LayerMask m_RaycastLayerMask = -1;

        [Tooltip("Renderer will be culled when not seen by a LOS camera with this tag. Empty means it's revealed when any LOS camera sees it.")]
        [SerializeField]
        private string m_SeenByTag = string.Empty;


        private bool m_IsVisible = true;


        public LayerMask RaycastLayerMask
        {
            get { return m_RaycastLayerMask; }
            set { m_RaycastLayerMask = value; }
        }

        public bool Visibile
        {
            get { return m_IsVisible; }
        }

        private void OnEnable()
        {
            enabled &= Util.Verify(GetComponent<Renderer>() != null, "No renderer attached to this GameObject! LOS Culler component must be added to a GameObject containing a MeshRenderer or Skinned Mesh Renderer!");
        }

        private void Update()
        {
            m_IsVisible = CustomCull(gameObject.GetComponent<Renderer>().bounds, m_RaycastLayerMask.value, m_SeenByTag);
        }

        /// <summary>
        /// Checks to see if object is inside the view frustum of any of the LOS cameras.
        /// Ideally should be called in OnWillRenderObject, but it's to late to disable renderer..
        /// Early outs when visible to one camera.
        /// </summary>
        private static bool CustomCull(Bounds meshBounds, int layerMask, string seenByTag = "")
        {
            // Get list of sources.
            List<LOSSource> losSources = LOSManager.Instance.LOSSources;

            for (int i = 0; i < losSources.Count; ++i)
            {
                LOSSource losSource = losSources[i];
                if (LOSHelper.CheckBoundsVisibility(losSource, meshBounds, layerMask, seenByTag))
                {
                    return true;
                }
            }

            return false;
        }
    }
}