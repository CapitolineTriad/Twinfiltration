using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Twinfiltration
{
    public class SlidingDoors : MonoBehaviour
    {
        [SerializeField] public bool m_IsLocked = false;
        [SerializeField] private Transform m_LeftDoor;
        [SerializeField] private Transform m_RightDoor;
        [SerializeField] private float m_SlideDistance = 0.5f;

        private int m_IsInTrigger;
        private Vector3 m_LeftDoorTarget;
        private Vector3 m_RightDoorTarget;
        private void Awake()
        {
            m_IsInTrigger = 0;
            m_LeftDoorTarget = m_LeftDoor.position;
            m_RightDoorTarget = m_RightDoor.position;
        }

        private void OnTriggerEnter(Collider other)
        {
            int colliderLayer = other.gameObject.layer;
            if ((colliderLayer != 3 || m_IsLocked) && colliderLayer != 6)
                return;

            m_IsInTrigger += 1;
            m_LeftDoorTarget = m_LeftDoor.position - new Vector3(m_SlideDistance, 0, 0);
            m_RightDoorTarget = m_RightDoor.position + new Vector3(m_SlideDistance, 0, 0);
        }

        private void OnTriggerExit(Collider other)
        {
            int colliderLayer = other.gameObject.layer;
            if ((colliderLayer != 3 || m_IsLocked) && colliderLayer != 6)
                return;

            m_IsInTrigger -= 1;
            if (m_IsInTrigger <= 0)
            {
                m_LeftDoorTarget = m_LeftDoor.position + new Vector3(m_SlideDistance, 0, 0);
                m_RightDoorTarget = m_RightDoor.position - new Vector3(m_SlideDistance, 0, 0);

            }
        }

        private SpringCoefs m_SpringCoefs;
        private float m_LeftDoorVelocity;
        private float m_RightDoorVelocity;
        void Update()
        {
            if (SpringUtils.Approximately(m_LeftDoor.position.x, m_LeftDoorTarget.x) && SpringUtils.Approximately(m_RightDoor.position.x, m_RightDoorTarget.x) &&
                SpringUtils.Approximately(m_LeftDoorVelocity, 0) && SpringUtils.Approximately(m_RightDoorVelocity, 0)) // don't do expensive stuff if not needed
                return;

            float deltaTime = Time.deltaTime;
            Vector3 newPos = m_LeftDoor.position;
            SpringUtils.UpdateDampedSpringCoef(ref m_SpringCoefs, deltaTime, 10, 0.6f);
            SpringUtils.UpdateDampedSpringMotion(ref newPos.x, ref m_LeftDoorVelocity, m_LeftDoorTarget.x, m_SpringCoefs);
            m_LeftDoor.position = newPos;
            newPos = m_RightDoor.position;
            SpringUtils.UpdateDampedSpringMotion(ref newPos.x, ref m_RightDoorVelocity, m_RightDoorTarget.x, m_SpringCoefs);
            m_RightDoor.position = newPos;
        }
    }
}
