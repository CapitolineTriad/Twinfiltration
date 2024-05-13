using UnityEngine;

namespace Twinfiltration
{
    public class CameraTrigger : MonoBehaviour
    {
        [SerializeField] private float m_DesiredYaw = 0;

        private bool m_PassedThrough = false;
        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.layer != 3)
                return;

            PlayerController collider = other.GetComponent<PlayerController>();
            if (collider.isLocalPlayer)
            {
                float newYaw = (!m_PassedThrough ? m_DesiredYaw : 360f - m_DesiredYaw) % 360f;
                collider.SetCameraRotation(newYaw);
                m_PassedThrough = !m_PassedThrough;
            }
        }
    }
}
