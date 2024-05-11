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

        [Server]
        protected void Start()
        {
            if (m_RandNumGen == null)
                m_RandNumGen = new System.Random();
        }

        [Server]
        protected override void GetMovementInput()
        {
            m_TargetDir = m_CharTransform.forward;
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
    }
}
