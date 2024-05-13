using Mirror;
using Org.BouncyCastle.Asn1.Mozilla;
using Org.BouncyCastle.Security;
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
        [SerializeField] public int m_AbilityUses = 6;

        private Transform m_Camera;
        private CameraController m_CameraController;
        private TimerUI m_Timer;
        private AbilityUI m_AbilityUI;
        [SerializeField] GameObject _gameOverUI;
        [SerializeField] AudioSource _gameMusic;
        [SerializeField] AudioClip[] _saluteAudioClips;

        [SerializeField] AudioListener _audioListener;

        private static System.Random m_RandNumGen;

        protected override void Awake()
        {
            if (m_RandNumGen == null)
                m_RandNumGen = new System.Random();

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

            if (IsDisguised)
                m_CharTransform.position = new Vector3(15.68f, 2f, 0f);

            _stepSource.volume = 0.1f;
            _audioListener.enabled = true;
            Cursor.lockState = CursorLockMode.Locked;
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
                    {
                        lastPrompt.gameObject.SetActive(false);
                        if (lastPromptTag == "CONSOLE1")
                        {
                            UnlockDoorServer("DOORTAG1");
                        }
                        else if (lastPromptTag == "CONSOLE2")
                        {
                            UnlockDoorServer("DOORTAG2");
                        }
                    }
                    m_TimerLastFrame = m_MovementBlockTimer;
                    SetAnimBoolServer("IsPlantingDevice", false);
                    SetAnimBoolServer("IsSaluting", false);
                    SetAnimBoolServer("IsInteracting", false);
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
                    SetAnimBoolServer("IsPlantingDevice", true); // need to send an update event for all client animators here, probably
                    m_MovementBlockTimer = 1.1f;
                    SpawnDeviceServer(devicePos, m_CharTransform.rotation);
                    m_AbilityUses -= 1;
                    m_AbilityUI.m_CurrFill = m_AbilityUses;
                }
            }
        }

        [Command(requiresAuthority = false)]
        private void UnlockDoorServer(string doorTag)
        {
            UnlockDoorClient(doorTag);
        }

        [ClientRpc]
        private void UnlockDoorClient(string doorTag)
        {
            GameObject.FindGameObjectWithTag(doorTag).GetComponent<SlidingDoors>().m_IsLocked = false;
        }

        [Command(requiresAuthority = false)]
        private void SpawnDeviceServer(Vector3 pos, Quaternion rot)
        {
            SpawnDeviceClient(pos, rot);
        }

        [ClientRpc]
        private void SpawnDeviceClient(Vector3 pos, Quaternion rot)
        {
            Instantiate(m_TrackerPrefab, pos, rot);
        }

        private void LateUpdate()
        {
            float deltaTime = Time.deltaTime;

            RotateCamera(deltaTime);
        }

        const float rotationAmount = 45f;
        private float camYaw = 0f;
        private void RotateCamera(float deltaTime)
        {
            if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.Joystick1Button6))
            {
                camYaw = (camYaw + rotationAmount) % 360;
                m_CameraController.m_CameraYaw = camYaw;
            }
            else if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.Joystick1Button5))
            {
                camYaw = (camYaw <= 0 ? 360f - rotationAmount : camYaw - rotationAmount) % 360;
                m_CameraController.m_CameraYaw = Mathf.Abs(camYaw) % 360;
            }
        }

        public void SetCameraRotation(float camRot)
        {
            camYaw = camRot;
            m_CameraController.m_CameraYaw = camYaw;
        }

        public void GuardInteract(EnemyController guard)
        {
            if (m_AbilityUses <= 0)
                return;

            Vector3 toGuard = guard.m_CharTransform.position - m_CharTransform.position;
            m_CharTransform.rotation = Quaternion.LookRotation(toGuard, Vector3.up);

            m_MovementBlocked = true;
            StopCharacter();
            SetAnimBoolServer("IsSaluting", true); // need to send an update event for all client animators here, probably
            m_MovementBlockTimer = 3.2f;
            m_AbilityUses -= 1;
            m_AbilityUI.m_CurrFill = m_AbilityUses;
            guard.GuardInteract(this); // server command

            PlayGuardAudioServer();
        }

        public void TriggerGameOver(EnemyController guard)
        {
            if (_gameMusic != null)
            {
                _gameMusic.Stop();
            }
            m_MovementBlocked = true;
            StopCharacter();

            if (!IsDisguised && guard != null)
            {
                Vector3 toGuard = guard.m_CharTransform.position - m_CharTransform.position;
                m_CharTransform.rotation = Quaternion.LookRotation(toGuard, Vector3.up);
            }
        }

        public void TriggerWin()
        {
            if (!isLocalPlayer)
                return;

            m_MovementBlocked = true;
            StopCharacter();
        }

        public void TriggerGameOverScreen()
        {
            _gameOverUI.SetActive(true);
        }

        bool isHacking = false;
        string lastPromptTag;
        InteractPrompt lastPrompt;
        [SerializeField] AudioSource _guardSaluteAudio;
        [SerializeField] AudioSource _hackingAudioSource;
        public void TriggerHacking(Transform console, InteractPrompt prompt, string consoleTag)
        {
            if (isHacking)
            {
                InterruptHacking();
                return;
            }

            lastPromptTag = consoleTag;
            lastPrompt = prompt;
            prompt.m_InteractText.text = "Stop Hack";
            isHacking = true;
            PlayHackAudioServer(true);
            Vector3 toConsole = console.position - m_CharTransform.position;
            var eulerRot = Quaternion.LookRotation(toConsole, Vector3.up).eulerAngles;
            var currentRot = m_CharTransform.rotation.eulerAngles;
            m_CharTransform.rotation = Quaternion.Euler(currentRot.x, eulerRot.y, currentRot.z);
            m_MovementBlocked = true;
            StopCharacter();
            SetAnimBoolServer("IsInteracting", true); // need to send an update event for all client animators here, probably
            m_MovementBlockTimer = 6f;
        }

        public void InterruptHacking()
        {
            PlayHackAudioServer(false);
            lastPrompt.m_InteractText.text = "Hack";
            isHacking = false;
            m_MovementBlockTimer = 0.1f;
        }

        [Command(requiresAuthority = false)]
        private void PlayHackAudioServer(bool play)
        {
            PlayHackAudioClient(play);
        }

        [ClientRpc]
        private void PlayHackAudioClient(bool play)
        {
            if (play)
            {
                _hackingAudioSource.Play();
            }
            else
            {
                _hackingAudioSource.Stop();
            }
        }

        [Command(requiresAuthority = false)]
        private void PlayGuardAudioServer()
        {
            PlayGuardAudioClient();
        }

        [ClientRpc]
        private void PlayGuardAudioClient()
        {
            var clip = _saluteAudioClips[m_RandNumGen.Next(_saluteAudioClips.Length)];
            _guardSaluteAudio?.PlayOneShot(clip);
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

            SetAnimBoolServer("IsMoving", m_TargetDir.magnitude > 0);
        }

        [Command(requiresAuthority = false)]
        private void SetAnimBoolServer(string name, bool value)
        {
            SetAnimBoolClients(name, value);
        }

        [ClientRpc]
        private void SetAnimBoolClients(string name, bool value)
        {
            m_Animator.SetBool(name, value);
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (IsDisguised || collision.gameObject.layer != 6)
                return;

            EnemyController enemyController = collision.gameObject.GetComponent<EnemyController>();
            if (!enemyController)
                return;

            GetComponentInChildren<DetectionUI>().TriggerGameOver(); // wonderful
        }
    }
}