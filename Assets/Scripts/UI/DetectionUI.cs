using UnityEngine;
using UnityEngine.UI;
using LOS;
using Mirror.Examples.AdditiveLevels;

namespace Twinfiltration
{
    public class DetectionUI : MonoBehaviour
    {
        private const float m_FadeSpeed = 10f;

        [SerializeField] private CanvasGroup m_CanvasGroup;
        [SerializeField] private Image m_DetectionMeter;

        private Transform m_Transform;
        private Transform m_CameraTransform;

        [SerializeField] private LOSVisibilityInfo m_VisibilityInfo;
        private float m_AlphaTarget = 0;
        private float m_DetectionDuration = 2;

        [SerializeField] AudioSource _detectionAudio;

        [SerializeField] AudioSource _gameOverAudio;

        

        private void Awake()
        {
            m_Transform = transform;
            m_CameraTransform = Camera.main.transform;
        }

        private float m_DetectionTimer = 0;
        void Update()
        {
            var deltaTime = Time.deltaTime;

            RotateUI();
            UpdateVisibility();
            if (m_CanvasGroup.alpha == m_AlphaTarget)
            {
                if (m_AlphaTarget == 0)
                {
                    m_DetectionTimer = 0;
                }
                else if (m_AlphaTarget == 1)
                {
                    m_DetectionMeter.fillAmount = m_DetectionTimer / m_DetectionDuration;
                    if (m_DetectionDuration > m_DetectionTimer)
                    {
                        m_DetectionTimer += deltaTime;
                    }
                    else if (!m_WasDetected)
                    {
                        // RUH ROH RAGGY
                        m_WasDetected = true;
                        TriggerGameOver();
                    }
                }

                return;
            }

            var fadeVal = m_CanvasGroup.alpha < m_AlphaTarget ? deltaTime : -deltaTime;
            m_CanvasGroup.alpha += fadeVal * m_FadeSpeed;
        }

        private bool m_WasDetected = false;
        private void TriggerGameOver()
        {
            m_AlphaTarget = 0f;
            // AAAAAAAAAAA DIE DEI DEID DIEDIE DIE
            // trigger gaem ovah
            var p1 = GameObject.FindGameObjectWithTag("Player1").GetComponent<PlayerController>();
            var p2 = GameObject.FindGameObjectWithTag("Player2").GetComponent<PlayerController>();
            var guard = m_VisibilityInfo.VisibleSources[0].GameObject.transform.parent.parent.GetComponent<EnemyController>(); // lol

            for(int i=1; i < m_VisibilityInfo.VisibleSources.Count; i++)
            {
                var newGuard = m_VisibilityInfo.VisibleSources[i].GameObject.transform.parent.parent.GetComponent<EnemyController>();
                newGuard.TriggerGameOver(p1);
            }

            p1.TriggerGameOver(guard);
            p2.TriggerGameOver(guard);
            guard.TriggerGameOver(p1);

            // GAME OVER SCREEN TRIGGERED HERE
            _gameOverAudio.Play();

        }

        private void RotateUI()
        {
            Vector3 toCamera = m_CameraTransform.position - m_Transform.position;
            Vector3 eulerAngles = Quaternion.LookRotation(toCamera, Vector3.up).eulerAngles;

            m_Transform.rotation = Quaternion.Euler(eulerAngles);
        }

        private void UpdateVisibility()
        {
            if (m_WasDetected)
            {
                if (!_detectionAudio.isPlaying)
                {
                    _detectionAudio.Play();
                }
                return;
            }

            bool isVisible = m_VisibilityInfo != null ? m_VisibilityInfo.Visibile : true;
            m_AlphaTarget = isVisible ? 1 : 0;
        }
    }
}
