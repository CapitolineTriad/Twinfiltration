using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Twinfiltration
{
    [RequireComponent(typeof(CapsuleCollider), typeof(Rigidbody))]
    public class EnemyController : CharacterController
    {
        private static System.Random m_RandNumGen;

        public List<Transform> Waypoints;

        [Server]
        protected void Start()
        {
            if (m_RandNumGen == null)
                m_RandNumGen = new System.Random();
        }

        private float m_LastTime;
        [Server]
        private void Update()
        {
            if(m_InteractTimer > 0)
            {
                m_InteractTimer -= Time.deltaTime;
                if (m_InteractTimer <= 0f && m_LastTime > 0f)
                {
                    m_LastTime = m_InteractTimer;
                    EndInteraction();
                }

                m_LastTime = m_InteractTimer;
            }
        }

        [Server]
        protected override void GetMovementInput()
        {

            m_TargetDir = m_MovementBlocked ? Vector3.zero : m_CharTransform.forward;
            SetAnimatorVars(m_TargetDir.magnitude);
        }

        [ClientRpc]
        protected void SetAnimatorVars(float movementSpeed)
        {
            m_Animator.SetBool("IsMoving", movementSpeed > 0);
        }

        [Server]
        private Vector3 GetRandomDestination()
        {
            var moveTargets = GameObject.FindGameObjectsWithTag("PatrolWaypoint");

            return moveTargets[m_RandNumGen.Next(moveTargets.Length)].transform.position;
        }

        [Server]
        protected override void OnCollisionEnter(Collision collision)
        {
            base.OnCollisionEnter(collision);
            if (collision.collider.gameObject.layer == 8)
            {
                m_CharTransform.rotation = Quaternion.LookRotation(-m_CharTransform.forward, Vector3.up);
            }
        }

        private float m_InteractTimer = 0;
        [Server]
        public void GuardInteract()
        {
            var playerController = GameObject.FindGameObjectWithTag("Player2").GetComponent<PlayerController>();
            if (playerController.m_AbilityUses <= 0)
                return;

            m_MovementBlocked = true;
            StopCharacter();
            SetAnimatorVars(m_TargetDir.magnitude);
            Vector3 toGuard = m_CharTransform.position - playerController.m_CharTransform.position;
            Vector3 toPlayer = playerController.m_CharTransform.position - m_CharTransform.position;

            m_CharTransform.rotation = Quaternion.LookRotation(toPlayer, Vector3.up);
            playerController.m_CharTransform.rotation = Quaternion.LookRotation(toGuard, Vector3.up); // does this need to be a ClientRPC call instead? probably
            playerController.GuardInteract(this);

            m_InteractTimer = 1.3f;
        }

        [Server]
        private void EndInteraction()
        {
            m_MovementBlocked = false;
            // make them path to the rest point
        }
        
        [Command(requiresAuthority = false)]
        public void TriggerGameOver(PlayerController retard)
        {
            Vector3 toPlayer = retard.m_CharTransform.position - m_CharTransform.position;

            m_MovementBlocked = true;
            StopCharacter();
            m_CharTransform.rotation = Quaternion.LookRotation(toPlayer, Vector3.up);
            m_Animator.SetBool("NoticedPlayer", true);
            SetNoticedAnim();
        }

        [ClientRpc]
        private void SetNoticedAnim()
        {
            m_Animator.SetBool("NoticedPlayer", true);
        }
    }
}
