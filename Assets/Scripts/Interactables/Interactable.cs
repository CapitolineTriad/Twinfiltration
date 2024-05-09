using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Twinfiltration
{
    [RequireComponent(typeof(SphereCollider))]
    public class Interactable : MonoBehaviour
    {
        [SerializeField] private Transform m_InteractAnchor;
        [SerializeField] private GameObject m_InteractUI;

        private SphereCollider m_TriggerZone;

        private void Awake()
        {
            m_TriggerZone = GetComponent<SphereCollider>();
            m_InteractUI.SetActive(false);
        }

        private void OnTriggerEnter(Collider other)
        {
            Debug.Log("Entered interaction zone.");
            m_InteractUI.SetActive(true);
        }

        private void OnTriggerExit(Collider other)
        {
            Debug.Log("Exited interaction zone");
            m_InteractUI.SetActive(false);
        }
    }
}