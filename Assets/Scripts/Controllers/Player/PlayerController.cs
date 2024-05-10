using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Twinfiltration
{
    [RequireComponent(typeof(CapsuleCollider), typeof(Rigidbody))]
    public class PlayerController : CharacterController
    {
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

        protected override void GetMovementInput()
        {
            var viewYaw = Quaternion.Euler(0, m_Camera.rotation.eulerAngles.y, 0);
            float x = Input.GetAxisRaw("Horizontal");
            float z = Input.GetAxisRaw("Vertical");

            m_TargetDir = viewYaw * new Vector3(x, 0, z);
            if (m_TargetDir.magnitude > 1)
                m_TargetDir.Normalize();

            m_Animator.SetBool("IsMoving", m_TargetDir.magnitude > 0);
        }

        private void OnDrawGizmos()
        {
            if (m_CharTransform is null)
                return;

            Debug.DrawRay(m_CharTransform.position, m_CharTransform.forward, Color.green);
        }
    }
}