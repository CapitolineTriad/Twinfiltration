using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Twinfiltration
{
    [RequireComponent(typeof(CapsuleCollider), typeof(Rigidbody))]
    public class PlayerController : CharacterController
    {
        [SerializeField] private bool IsDisguised;
        [SerializeField] private GameObject m_TrackerPrefab;
        private Transform m_Camera;

        protected override void Awake()
        {
            m_Camera = Camera.main.transform;

            base.Awake();
        }

        private void Start()
        {
            if (isLocalPlayer)
                Camera.main.GetComponent<CameraController>().m_TrackedObject = m_CharTransform;
        }

        private float m_TimerLastFrame = 0;
        private float m_MovementBlockTimer = 0f;
        private void Update()
        {
            if (!isLocalPlayer)
                return;

            float deltaTime = Time.deltaTime;
            if (m_MovementBlockTimer > 0f)
            {
                m_MovementBlockTimer -= deltaTime;
                if(m_MovementBlockTimer <= 0f && m_TimerLastFrame > 0f)
                {
                    m_TimerLastFrame = m_MovementBlockTimer;
                    m_Animator.SetBool("IsPlantingDevice", false);
                    m_MovementBlocked = false;
                }
                m_TimerLastFrame = m_MovementBlockTimer;
            }
            
            if (!IsDisguised && Input.GetKeyDown(KeyCode.F))
            {
                Vector3 devicePos = m_CharTransform.position + Vector3.up + m_CharTransform.forward;
                if (Physics.Raycast(devicePos, Vector3.down, out var hitInfo, 2, m_ControllerDefinition.TerrainLayer))
                {
                    devicePos.y = hitInfo.point.y;
                    m_MovementBlocked = true;
                    StopCharacter();
                    m_Animator.SetBool("IsPlantingDevice", true);
                    m_MovementBlockTimer = 1.1f;
                    Instantiate(m_TrackerPrefab, devicePos, m_CharTransform.rotation);
                }
            }
        }

        protected override void GetMovementInput()
        {
            if (!isLocalPlayer)
                return;

            var viewYaw = Quaternion.Euler(0, m_Camera.rotation.eulerAngles.y, 0);
            float x = Input.GetAxisRaw("Horizontal");
            float z = Input.GetAxisRaw("Vertical");

            m_TargetDir = viewYaw * new Vector3(x, 0, z);
            if (m_TargetDir.magnitude > 1)
                m_TargetDir.Normalize();

            m_Animator.SetBool("IsMoving", m_TargetDir.magnitude > 0);
        }
    }
}