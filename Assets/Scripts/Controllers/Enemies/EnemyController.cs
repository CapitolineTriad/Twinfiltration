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
            RestPoint,
            StandStill
        }

        private static System.Random m_RandNumGen;

        public Transform[] Waypoints;
        public Transform[] RestPointPath;

        int _currWaypointIndex = 0;
        int _currRestPointPathIndex = 0;
        int _waypointSign = 1;

        bool isAlreadyOnRestPointPath = false;

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
            if (activePathType == PathType.StandStill)
            {
                return;
            }
            if (m_MovementBlocked)
            {
                return;
            }

            Vector3 curDestination = Vector3.zero;
            if (activePathType == PathType.RestPoint)
            {
                curDestination = RestPointPath[_currRestPointPathIndex].position;
            } 
            else if (activePathType != PathType.StandStill)
            {
                curDestination = Waypoints[_currWaypointIndex].position;
            }


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
                    Debug.Log("RestPoint");
                    var currMinDist = Mathf.Infinity;
                    if (!isAlreadyOnRestPointPath)
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
                        isAlreadyOnRestPointPath = true;
                    } 
                    else
                    {
                        var curDesRestpoint = RestPointPath[_currRestPointPathIndex].position;
                        var currentRestPointPathPosition = new Vector2(transform.position.x, transform.position.z);
                        var currentDestionationPlanePos = new Vector2(curDesRestpoint.x, curDesRestpoint.z);
                        if (Vector3.Distance(currentRestPointPathPosition, currentDestionationPlanePos) < 0.1f)
                        {
                            StopCharacter();
                            if (_currRestPointPathIndex == RestPointPath.Length - 1)
                            {
                                activePathType = PathType.StandStill;
                            }
                            _currRestPointPathIndex++;
                            _currRestPointPathIndex = Mathf.Min(_currRestPointPathIndex, RestPointPath.Length-1);
                            curDestination = RestPointPath[_currRestPointPathIndex].position;

                        }
                    }
                    break;
            }

            var speedFactor = activePathType == PathType.RestPoint ? 3f : 1f;

            m_TargetDir = (curDestination - transform.position);
            m_TargetDir = new Vector3(m_TargetDir.x, 0, m_TargetDir.z).normalized * speedFactor;
            Debug.DrawLine(transform.position, transform.position + m_TargetDir, Color.magenta);
            Debug.DrawRay(curDestination, Vector3.up, Color.magenta);
            SetAnimatorVars(m_TargetDir.magnitude);
        }

        [ClientRpc]
        protected void SetAnimatorVars(float movementSpeed)
        {
            m_Animator.SetBool("IsMoving", movementSpeed > 0);
        }

        private float m_InteractTimer = 0;
        [Command(requiresAuthority =false)]
        public void GuardInteract(PlayerController playerController)
        {
            m_MovementBlocked = true;
            StopCharacter();
            SetAnimatorVars(m_TargetDir.magnitude);
            Vector3 toPlayer = playerController.m_CharTransform.position - m_CharTransform.position;
            m_CharTransform.rotation = Quaternion.LookRotation(toPlayer, Vector3.up);
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
