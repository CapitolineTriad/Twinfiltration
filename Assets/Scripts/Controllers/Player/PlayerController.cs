using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Twinfiltration
{
    [RequireComponent(typeof(CapsuleCollider), typeof(Rigidbody))]
    public class PlayerController : CharacterController
    {
        [SerializeField] private bool IsDisguised;
        [SerializeField] private GameObject m_TrackerPrefab;
        [SerializeField] public int m_AbilityUses = 3;

        private Transform m_Camera;
        private CameraController m_CameraController;
        private TimerUI m_Timer;
        private AbilityUI m_AbilityUI;

        protected override void Awake()
        {
            base.Awake();
            m_Camera = Camera.main.transform;
            m_Timer = GameObject.FindGameObjectWithTag("UITimer").GetComponent<TimerUI>();
            m_AbilityUI = GameObject.FindGameObjectWithTag("UIAbility").GetComponent<AbilityUI>();
            m_CameraController = Camera.main.GetComponent<CameraController>();
        }

        private void Start()
        {
            if (!isLocalPlayer)
                return;

            m_CameraController.m_TrackedObject = m_CharTransform;
            m_AbilityUI.m_MaxFill = m_AbilityUses;
            m_AbilityUI.m_CurrFill = m_AbilityUses;

            m_IsTimerRunning = true;
        }

        public bool m_IsTimerRunning;
        private float m_TimeElapsed;
        private float m_TimerLastFrame = 0;
        private float m_MovementBlockTimer = 0f;
        private void Update()
        {
            if (!isLocalPlayer)
                return;

            float deltaTime = Time.deltaTime;
            if (m_IsTimerRunning)
            {
                m_TimeElapsed += deltaTime;
                m_Timer.m_TimePassed = m_TimeElapsed;
            }

            if (m_MovementBlockTimer > 0f)
            {
                m_MovementBlockTimer -= deltaTime;
                if(m_MovementBlockTimer <= 0f && m_TimerLastFrame > 0f)
                {
                    if (isHacking)
                        lastPrompt.gameObject.SetActive(false);
                    m_TimerLastFrame = m_MovementBlockTimer;
                    m_Animator.SetBool("IsPlantingDevice", false);
                    m_Animator.SetBool("IsSaluting", false);
                    m_Animator.SetBool("IsInteracting", false);
                    // need to send an update event for all client animators here, probably
                    m_MovementBlocked = false;
                }
                m_TimerLastFrame = m_MovementBlockTimer;
            }
            
            if (!IsDisguised && m_AbilityUses > 0 && (Input.GetKeyDown(KeyCode.F) || Input.GetKeyDown(KeyCode.Joystick1Button2)))
            {
                Vector3 devicePos = m_CharTransform.position + Vector3.up + m_CharTransform.forward;
                if (Physics.Raycast(devicePos, Vector3.down, out var hitInfo, 2, m_ControllerDefinition.TerrainLayer))
                {
                    devicePos.y = hitInfo.point.y;
                    m_MovementBlocked = true;
                    StopCharacter();
                    m_Animator.SetBool("IsPlantingDevice", true); // need to send an update event for all client animators here, probably
                    m_MovementBlockTimer = 1.1f;
                    Instantiate(m_TrackerPrefab, devicePos, m_CharTransform.rotation);
                    m_AbilityUses -= 1;
                    m_AbilityUI.m_CurrFill = m_AbilityUses;
                }
            }
        }

        private void LateUpdate()
        {
            RotateCamera();
        }

        private float camYaw = 0f;
        private void RotateCamera()
        {
            if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.Joystick1Button3))
            {
                camYaw = camYaw + 90f;
                m_CameraController.m_CameraYaw = Mathf.Abs(camYaw) % 360;
            }
            else if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.Joystick1Button4))
            {
                camYaw = camYaw - 90f;
                m_CameraController.m_CameraYaw = Mathf.Abs(camYaw) % 360;
            }
        }

        public void GuardInteract(EnemyController guard)
        {
            m_MovementBlocked = true;
            StopCharacter();
            m_Animator.SetBool("IsSaluting", true); // need to send an update event for all client animators here, probably
            m_MovementBlockTimer = 1.7f;
            m_AbilityUses -= 1;
            m_AbilityUI.m_CurrFill = m_AbilityUses;
        }

        public void TriggerGameOver(EnemyController guard)
        {
            m_MovementBlocked = true;
            StopCharacter();

            if (IsDisguised)
            {

            }
            else
            {
                Vector3 toGuard = guard.m_CharTransform.position - m_CharTransform.position;
                m_CharTransform.rotation = Quaternion.LookRotation(toGuard, Vector3.up);
            }
        }

        bool isHacking = false;
        InteractPrompt lastPrompt;
        public void TriggerHacking(Transform console, InteractPrompt prompt)
        {
            lastPrompt = prompt;
            isHacking = true;
            Vector3 toConsole = console.position - m_CharTransform.position;
            var eulerRot = Quaternion.LookRotation(toConsole, Vector3.up).eulerAngles;
            var currentRot = m_CharTransform.rotation.eulerAngles;
            m_CharTransform.rotation = Quaternion.Euler(currentRot.x, eulerRot.y, currentRot.z);
            m_MovementBlocked = true;
            StopCharacter();
            m_Animator.SetBool("IsInteracting", true); // need to send an update event for all client animators here, probably
            m_MovementBlockTimer = 6f;
        }

        public void InterruptHacking()
        {
            isHacking = false;
            m_MovementBlockTimer = 0.1f;
        }

        protected override void GetMovementInput()
        {
            if (!isLocalPlayer)
                return;

            if (!m_MovementBlocked)
            {
                var viewYaw = Quaternion.Euler(0, m_Camera.rotation.eulerAngles.y, 0);
                float x = Input.GetAxisRaw("Horizontal");
                float z = Input.GetAxisRaw("Vertical");

                m_TargetDir = viewYaw * new Vector3(x, 0, z);
                if (m_TargetDir.magnitude > 1)
                    m_TargetDir.Normalize();
            }
            else m_TargetDir = Vector3.zero;

            m_Animator.SetBool("IsMoving", m_TargetDir.magnitude > 0);
        }
    }
}