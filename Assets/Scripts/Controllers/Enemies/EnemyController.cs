using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Twinfiltration
{
    [RequireComponent(typeof(CapsuleCollider), typeof(Rigidbody))]
    public class EnemyController : CharacterController
    {
        private static System.Random m_RandNumGen;

        protected override void Awake()
        {
            if (m_RandNumGen == null)
                m_RandNumGen = new System.Random();

            base.Awake();
        }

        protected override void GetMovementInput()
        {
            throw new System.NotImplementedException();
        }

        private Vector3 GetRandomDestination()
        {
            var moveTargets = GameObject.FindGameObjectsWithTag("PatrolWaypoint");

            return moveTargets[m_RandNumGen.Next(moveTargets.Length)].transform.position;
        }
    }
}
