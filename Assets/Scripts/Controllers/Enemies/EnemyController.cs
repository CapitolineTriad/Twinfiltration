using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Twinfiltration
{
    [RequireComponent(typeof(CapsuleCollider), typeof(Rigidbody))]
    public class EnemyController : CharacterController
    {
        public enum PathType
        {
            Circular,
            BackAndForth
        }

        private static System.Random m_RandNumGen;

        public Transform[] Waypoints;
        int currWaypointIndex = 0;

        int waypointSign = 1;

        [SerializeField] PathType pathType;



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
            Vector3 curDestination = Waypoints[currWaypointIndex].position;

            switch (pathType)
            {
                case PathType.Circular:
                    // Circular path (default)
                    var planePos = new Vector2(transform.position.x, transform.position.z);
                    var curDestinationPlanePos = new Vector2(curDestination.x, curDestination.z);
                    if (Vector3.Distance(planePos, curDestinationPlanePos) < 0.05f)
                    {
                        StopCharacter();
                        currWaypointIndex++;
                        currWaypointIndex %= Waypoints.Length;
                        curDestination = Waypoints[currWaypointIndex].position;
                    }
                    break;
                case PathType.BackAndForth:
                    var planePos2 = new Vector2(transform.position.x, transform.position.z);
                    var curDestinationPlanePos2 = new Vector2(curDestination.x, curDestination.z);
                    if (Vector3.Distance(planePos2, curDestinationPlanePos2) < 0.05f)
                    {
                        StopCharacter();
                        currWaypointIndex += waypointSign;
                        if (currWaypointIndex == 0)
                        {
                            waypointSign = 1;
                        } 
                        else if (currWaypointIndex == Waypoints.Length-1)
                        {
                            waypointSign = -1;
                        }
                        curDestination = Waypoints[currWaypointIndex].position;
                    }
                    break;
            }
            

            m_TargetDir = (curDestination - transform.position);
            m_TargetDir = new Vector3(m_TargetDir.x, 0, m_TargetDir.z).normalized;
            Debug.DrawLine(transform.position, transform.position + m_TargetDir, Color.magenta);
            Debug.DrawRay(curDestination, Vector3.up, Color.magenta);
            SetAnimatorVars(m_TargetDir.magnitude);
        }

        [ClientRpc]
        protected void SetAnimatorVars(float movementSpeed)
        {
            m_Animator.SetBool("IsMoving", movementSpeed > 0);
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
