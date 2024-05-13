﻿using System.Collections.Generic;
using UnityEngine;

namespace LOS
{
    [AddComponentMenu("Line of Sight/LOS Visibility Info")]
    public class LOSVisibilityInfo : MonoBehaviour
    {
        [Tooltip("Selects which layers block raycasts used for visibility calculations")]
        [SerializeField]
        private LayerMask m_RaycastLayerMask = -1;

        [Tooltip("Will be registered as invisible when not seen by a LOS camera with this tag. Empty means it's revealed when any LOS camera sees it.")]
        [SerializeField]
        public string m_SeenByTag = string.Empty;

        [Tooltip("The mesh renderer that should determine the actor's visibility.")]
        [SerializeField]
        private Renderer m_MeshRenderer;

        private List<ILOSSource> m_VisibleSources = new List<ILOSSource>();

        public List<ILOSSource> VisibleSources
        {
            get { return m_VisibleSources; }
        }

        public LayerMask RaycastLayerMask
        {
            get { return m_RaycastLayerMask; }
            set { m_RaycastLayerMask = value; }
        }

        public bool Visibile
        {
            get { return m_VisibleSources.Count > 0; }
        }


        public delegate void OnLineOfSightEnterHandler(GameObject sender, ILOSSource losSource);

        public delegate void OnLineOfSightStayHandler(GameObject sender, ILOSSource losSource);

        public delegate void OnLineOfSightExitHandler(GameObject sender, ILOSSource losSource);


        public event OnLineOfSightEnterHandler OnLineOfSightEnter;

        public event OnLineOfSightStayHandler OnLineOfSightStay;

        public event OnLineOfSightExitHandler OnLineOfSightExit;

        private void OnEnable()
        {
            enabled &= Util.Verify(m_MeshRenderer != null, "No renderer attached to this GameObject! LOS Culler component must be added to a GameObject containing a MeshRenderer or Skinned Mesh Renderer!");
        }

        private void Update()
        {
            UpdateVisibleSources();
        }

        /// <summary>
        /// Checks to see if object is inside the view frustum of any of the LOS cameras.
        /// Ideally should be called in OnWillRenderObject, but it's to late to disable renderer..
        /// Early outs when visible to one camera.
        /// </summary>
        private void UpdateVisibleSources()
        {
            Bounds meshBounds = m_MeshRenderer.bounds;

            // Get list of sources.
            List<LOSSource> losSources = LOSManager.Instance.LOSSources;

            for (int i = 0; i < losSources.Count; ++i)
            {
                LOSSource losSource = losSources[i];

                bool isVisible = LOSHelper.CheckBoundsVisibility(losSource, meshBounds, m_RaycastLayerMask.value, m_SeenByTag);

                UpdateList(losSource, isVisible);
            }
        }

        /// <summary>
        /// Updates the list with visible sources and trigger events if needed
        /// </summary>
        private void UpdateList(ILOSSource losSource, bool isVisible)
        {
            if (isVisible)
            {
                // LOS Source is already in list.
                if (m_VisibleSources.Contains(losSource))
                {
                    InvokeOnLineOfSightStay(losSource);
                }
                // LOS Source is added to list.
                else
                {
                    m_VisibleSources.Add(losSource);
                    InvokeOnLineOfSightEnter(losSource);
                }
            }
            else
            {
                // Source is removed from list.
                if (m_VisibleSources.Contains(losSource))
                {
                    m_VisibleSources.Remove(losSource);
                    InvokeOnLineOfSightEXit(losSource);
                }
            }
        }

        private void InvokeOnLineOfSightEnter(ILOSSource losSource)
        {
            if (OnLineOfSightEnter != null)
            {
                OnLineOfSightEnter(gameObject, losSource);
            }
        }

        private void InvokeOnLineOfSightStay(ILOSSource losSource)
        {
            if (OnLineOfSightStay != null)
            {
                OnLineOfSightStay(gameObject, losSource);
            }
        }

        private void InvokeOnLineOfSightEXit(ILOSSource losSource)
        {
            if (OnLineOfSightExit != null)
            {
                OnLineOfSightExit(gameObject, losSource);
            }
        }
    }
}