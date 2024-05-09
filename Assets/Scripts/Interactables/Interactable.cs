using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Twinfiltration
{
    [RequireComponent(typeof(SphereCollider))]
    public class Interactable : MonoBehaviour
    {
        private const float m_FadeSpeed = 10;

        [SerializeField] private Transform m_InteractAnchor;
        [SerializeField] private GameObject m_InteractUI;

        private SphereCollider m_TriggerZone;
        private CanvasGroup m_CanvasGroup;
        private float m_AlphaTarget = 0f;

        private void Awake()
        {
            m_TriggerZone = GetComponent<SphereCollider>();
            m_CanvasGroup = m_InteractUI.GetComponent<CanvasGroup>();
            m_InteractUI.SetActive(false);
            m_CanvasGroup.alpha = 0f;
        }

        private void Update()
        {
            if (!m_InteractUI.activeInHierarchy)
                return;

            if (m_CanvasGroup.alpha == m_AlphaTarget)
            {
                if (m_AlphaTarget <= 0f)
                    m_InteractUI.SetActive(false);

                return;
            }

            var deltaTime = Time.deltaTime;
            var fadeVal = m_CanvasGroup.alpha < m_AlphaTarget ? deltaTime : -deltaTime;
            m_CanvasGroup.alpha += fadeVal * m_FadeSpeed;
        }

        private void OnTriggerEnter(Collider other)
        {
            Debug.Log("Entered interaction zone.");
            m_InteractUI.SetActive(true);
            m_AlphaTarget = 1f;
        }

        private void OnTriggerExit(Collider other)
        {
            Debug.Log("Exited interaction zone");
            m_AlphaTarget = 0f;
        }
    }
}