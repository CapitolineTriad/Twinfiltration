using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Twinfiltration
{
    [RequireComponent(typeof(CapsuleCollider), typeof(Rigidbody))]
    public class CharacterController : NetworkBehaviour
    {
        private static System.Random m_RandNumGen2;

        [SerializeField] protected ControllerDefinition m_ControllerDefinition;

        [HideInInspector] public Animator m_Animator;
        protected Rigidbody m_RigidBody;
        [HideInInspector] public Transform m_CharTransform;
        protected CapsuleCollider m_CharCollider;

        protected bool m_MovementBlocked = false;
        protected bool m_IsAccelerating;
        protected bool m_IsDecelerating;
        protected bool m_IsRunning;
        protected bool m_IsMoving;
        protected bool m_IsJumping;
        protected bool m_IsGrounded = true;

        [SerializeField] AudioSource _stepSource;
        [SerializeField] AudioClip[] _audioSourceClips;

        #region Callbacks

        protected virtual void Awake()
        {
            if (m_RandNumGen2 == null)
                m_RandNumGen2 = new System.Random();
            m_CharTransform = transform;
            m_RigidBody = gameObject.GetComponent<Rigidbody>();
            m_CharCollider = gameObject.GetComponent<CapsuleCollider>();
            m_Animator = gameObject.GetComponentInChildren<Animator>();
        }

        private void FixedUpdate()
        {
            var deltaTime = Time.fixedDeltaTime;

            GetMovementInput();

            ApplyMovement(deltaTime);
        }

        /*protected virtual void OnCollisionEnter(Collision collision)
        {
            if (collision.collider.gameObject.layer ==  Mathf.Log(m_ControllerDefinition.TerrainLayer, 2))
                m_IsGrounded = true;
        }

        protected virtual void OnCollisionExit(Collision collision)
        {
            if (collision.collider.gameObject.layer == Mathf.Log(m_ControllerDefinition.TerrainLayer, 2))
                m_IsGrounded = false;
        }*/

        #endregion Callbacks


        #region Physics

        protected Vector3 m_TargetDir = Vector3.zero;
        protected virtual void GetMovementInput()
        {
            throw new NotImplementedException();
        }

        private Vector3 m_GoalVelocity = Vector3.zero;


        private void ApplyMovement(float deltaTime)
        {
            if (!m_IsGrounded && !m_ControllerDefinition.AirControl)
                m_TargetDir = Vector3.zero;

            // Calculate dot product of current movement direction and target direction to determine if we are turning the opposite direction.
            Vector3 currDir = m_GoalVelocity.normalized;
            float dotProd = Vector3.Dot(m_TargetDir, currDir);
            float accelFactor = m_ControllerDefinition.AccelerationTurnCurve.Evaluate(dotProd);

            ApplyRotation(accelFactor); // Turn around faster if changing direcitons.

            var movementHeading = m_CharTransform.forward * m_TargetDir.magnitude;
            float accelVal = (m_IsGrounded ? m_ControllerDefinition.GroundAcceleration : m_ControllerDefinition.AirAcceleration) * accelFactor;
            Vector3 targetVelocity = movementHeading * m_ControllerDefinition.MaxSpeed; // can add movement speed changes here

            m_GoalVelocity = Vector3.MoveTowards(m_GoalVelocity, targetVelocity, accelVal * deltaTime); /*add surface movement velocity here*/

            var neededAccel = (m_GoalVelocity - new Vector3(m_RigidBody.velocity.x, 0, m_RigidBody.velocity.z)) / deltaTime;

            m_RigidBody.AddForce(Vector3.Scale(neededAccel * m_RigidBody.mass, m_ControllerDefinition.MovementForceScale));

            if (_stepSource != null && _audioSourceClips != null && _audioSourceClips.Length > 0)
            {
                if (!_stepSource.isPlaying && m_RigidBody.velocity.magnitude > 0.2f)
                {
                    var clip = _audioSourceClips[m_RandNumGen2.Next(_audioSourceClips.Length)];
                    _stepSource.clip = clip; 
                    _stepSource.Play();
                }
            }
        }

        private void ApplyRotation(float turnFactor)
        {
            if (m_TargetDir == Vector3.zero)
                return;

            var desiredRot = Quaternion.LookRotation(m_TargetDir, Vector3.up);
            m_CharTransform.rotation = Quaternion.RotateTowards(m_CharTransform.rotation, desiredRot, m_ControllerDefinition.TurnSpeed * turnFactor);
        }

        public void StopCharacter()
        {
            m_TargetDir = Vector3.zero;
            m_GoalVelocity = Vector3.zero;
            m_RigidBody.velocity = Vector3.zero;
        }

        #endregion Physics
    }
}