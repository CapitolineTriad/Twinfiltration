using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Twinfiltration
{
    public class PlayerController : CharacterController
    {
        protected override void Awake()
        {
            base.Awake();
        }

        protected override void GetMovementInput()
        {
            float x = Input.GetAxisRaw("Horizontal");
            float z = Input.GetAxisRaw("Vertical");

            m_TargetDir = new Vector3(x, 0, z);
            if (m_TargetDir.magnitude > 1)
                m_TargetDir.Normalize();
        }

        private void OnDrawGizmos()
        {
            if (m_CharTransform is null)
                return;

            Debug.DrawRay(m_CharTransform.position, m_CharTransform.forward, Color.green);
        }
    }
}