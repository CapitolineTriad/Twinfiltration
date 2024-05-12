using LOS;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Twinfiltration
{
    [RequireComponent(typeof(SphereCollider))]
    public class InteractPrompt : MonoBehaviour
    {
        private const float m_FadeSpeed = 7;

        [SerializeField] private bool m_Player1Only;
        [SerializeField] private bool m_Player2Only;
        [SerializeField] private GameObject m_InteractUI;
        [SerializeField] private TextMeshProUGUI m_InteractText;
        [SerializeField][TextArea(1, 1)] private string m_InteractPrompt = "";
        [SerializeField] private UnityEvent m_InteractAction;

        private LOSVisibilityInfo m_VisibilityInfo;
        private Transform m_CameraTransform;
        private Transform m_InteractTransform;
        private CanvasGroup m_CanvasGroup;
        private float m_AlphaTarget = 0f;

        private void Awake()
        {
            m_VisibilityInfo = transform.parent.GetComponent<LOSVisibilityInfo>();
            m_InteractTransform = m_InteractUI.transform;
            m_CameraTransform = Camera.main.transform;
            m_CanvasGroup = m_InteractUI.GetComponent<CanvasGroup>();
            m_InteractUI.SetActive(false);
            m_CanvasGroup.alpha = 0f;

            if (m_InteractText != null && m_InteractPrompt != "")
                m_InteractText.text = m_InteractPrompt;
        }

        private void Update()
        {
            UpdateVisibility();
            if (!m_InteractUI.activeInHierarchy)
                return;

            RotateUI();
            if (m_VisibilityInfo != null && !m_VisibilityInfo.Visibile && m_AlphaTarget > 0) // hide prompt when object becomes invisible
            {
                m_AlphaTarget = 0f;
            }

            if (m_CanvasGroup.alpha == m_AlphaTarget)
            {
                if (m_AlphaTarget >= 1f)
                    HandleInput();

                if (m_AlphaTarget <= 0f)
                    m_InteractUI.SetActive(false);

                return;
            }

            var deltaTime = Time.deltaTime;
            var fadeVal = m_CanvasGroup.alpha < m_AlphaTarget ? deltaTime : -deltaTime;
            m_CanvasGroup.alpha += fadeVal * m_FadeSpeed;
        }

        private void HandleInput()
        {
            if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Joystick1Button1))
                m_InteractAction.Invoke();
        }

        private void RotateUI()
        {
            Vector3 toCamera = m_CameraTransform.position - m_InteractTransform.position;
            Vector3 eulerAngles = Quaternion.LookRotation(toCamera, Vector3.up).eulerAngles;

            m_InteractTransform.rotation = Quaternion.Euler(eulerAngles);
        }

        private bool m_IsInTrigger = false;
        private void UpdateVisibility()
        {
            bool isVisible = m_VisibilityInfo != null ? m_VisibilityInfo.Visibile : true;
            if(m_IsInTrigger && isVisible)
            {
                m_InteractUI.SetActive(true);
                m_AlphaTarget = 1f;
            }
            else
            {
                m_AlphaTarget = 0f;
            }
        }

        public void TriggerHacking()
        {
            var p1 = GameObject.FindGameObjectWithTag("Player1").GetComponent<PlayerController>();
            p1.TriggerHacking(transform.parent, this);
        }

        public void TriggerGuardInteract()
        {
            var p2 = GameObject.FindGameObjectWithTag("Player2").GetComponent<PlayerController>();
            p2.GuardInteract(transform.parent.GetComponent<EnemyController>());
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.layer != 3)
                return;

            if (m_Player2Only && other.tag != "Player2")
                return;
            else if (m_Player1Only && other.tag != "Player1")
                return;

            Debug.Log("Entered interaction zone.");
            m_IsInTrigger = true;
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.gameObject.layer != 3)
                return;

            Debug.Log("Exited interaction zone");
            m_IsInTrigger = false;
        }
    }
}