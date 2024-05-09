using UnityEngine;

namespace Twinfiltration
{
    [CreateAssetMenu(fileName = "Data", menuName = "Scriptable Objects/Character Controller Definition")]
    public class ControllerDefinition : ScriptableObject
    {
        [Header("Control")]
        public bool AirControl = false;

        [Header("Orientation")]
        public float TurnSpeed = 5;

        [Header("Layers")]
        public LayerMask CharacterLayer;
        public LayerMask TerrainLayer;

        [Header("Locomotion")]
        public float MaxSpeed = 5;
        public float AirAcceleration = 10;
        public float GroundAcceleration = 20;
        public AnimationCurve AccelerationTurnCurve = AnimationCurve.Linear(-1, 1, 1, 1);

        public Vector3 MovementForceScale = Vector3.one;
    }
}