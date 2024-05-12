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
            BackAndForth,
            RestPoint
        }

        private static System.Random m_RandNumGen;

        public Transform[] Waypoints;
        public Transform[] RestPointPath;

        int _currWaypointIndex = 0;
        int _currRestPointPathIndex = 0;
        int _waypointSign = 1;

        bool isAlreadyOnRestPoint = false;

        [SerializeField] PathType pathType;

        PathType activePathType;



        [Server]
        protected void Start()
        {
            activePathType = pathType;
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
            Vector3 curDestination = Waypoints[_currWaypointIndex].position;

            switch (activePathType)
            {
                case PathType.Circular:
                    // Circular path (default)
                    var planePos = new Vector2(transform.position.x, transform.position.z);
                    var curDestinationPlanePos = new Vector2(curDestination.x, curDestination.z);
                    if (Vector3.Distance(planePos, curDestinationPlanePos) < 0.05f)
                    {
                        StopCharacter();
                        _currWaypointIndex++;
                        _currWaypointIndex %= Waypoints.Length;
                        curDestination = Waypoints[_currWaypointIndex].position;
                    }
                    break;
                case PathType.BackAndForth:
                    var planePos2 = new Vector2(transform.position.x, transform.position.z);
                    var curDestinationPlanePos2 = new Vector2(curDestination.x, curDestination.z);
                    if (Vector3.Distance(planePos2, curDestinationPlanePos2) < 0.05f)
                    {
                        StopCharacter();
                        _currWaypointIndex += _waypointSign;
                        if (_currWaypointIndex == 0)
                        {
                            _waypointSign = 1;
                        } 
                        else if (_currWaypointIndex == Waypoints.Length-1)
                        {
                            _waypointSign = -1;
                        }
                        curDestination = Waypoints[_currWaypointIndex].position;
                    }
                    break;
                case PathType.RestPoint:
                    var currMinDist = Mathf.Infinity;
                    if (!isAlreadyOnRestPoint)
                    {
                        var index = 0;
                        for (int i = 0; i < RestPointPath.Length; i++)
                        {
                            var point = RestPointPath[i];
                       
                            var pointPlanePos = point.position;
                            var dis = Vector2.Distance(new Vector2(transform.position.x, transform.position.z), new Vector2(pointPlanePos.x, pointPlanePos.z)) ;
                            if (dis < currMinDist) 
                            {
                                index = i;
                                currMinDist = dis;
                            }
                        }
                        _currRestPointPathIndex = index;
                        curDestination = RestPointPath[_currRestPointPathIndex].position;
                        isAlreadyOnRestPoint = true;
                    } else
                    {
                        var curDesRestpoint = RestPointPath[_currRestPointPathIndex].position;
                        var planePos3 = new Vector2(transform.position.x, transform.position.z);
                        var curDestinationPlanePos33 = new Vector2(curDesRestpoint.x, curDesRestpoint.z);
                        if (Vector2.Distance(planePos3, curDestinationPlanePos33) < 0.05f)
                        {
                            StopCharacter();

                            _currRestPointPathIndex = Mathf.Clamp(_currRestPointPathIndex, _currRestPointPathIndex + 1, RestPointPath.Length);
                            curDestination = RestPointPath[_currRestPointPathIndex].position;
                        }
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
        [Command(requiresAuthority =false)]
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
            activePathType = PathType.RestPoint;
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
