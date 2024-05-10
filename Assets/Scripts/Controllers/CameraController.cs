using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.TerrainTools;
using UnityEngine;

namespace Twinfiltration
{
    /*[CustomEditor(typeof(CameraController))]
    public class Stuff : Editor
    {
        public override void OnInspectorGUI()
        {
            CameraController cameraController = target as CameraController;
            if(GUILayout.Button("Left"))
            {
                cameraController.m_CameraPosition = new Vector3(-100, 15, 15);
                cameraController.m_CameraRotation = new Vector3(35, 90, 0);
            }
            if (GUILayout.Button("Right"))
            {
                cameraController.m_CameraPosition = new Vector3(25, 15, 15);
                cameraController.m_CameraRotation = new Vector3(35, 270, 0);
            }
            if (GUILayout.Button("Top"))
            {
                cameraController.m_CameraPosition = new Vector3(-40, 15, 80);
                cameraController.m_CameraRotation = new Vector3(35, 180, 0);
            }
            if (GUILayout.Button("Bottom"))
            {
                cameraController.m_CameraPosition = new Vector3(-40, 15, -50);
                cameraController.m_CameraRotation = new Vector3(35, 0, 0);
            }
        }
    }*/

    public class CameraController : MonoBehaviour
    {
        [SerializeField] private float m_AngularFrequency;
        [SerializeField] private float m_DampingRatio;

        [SerializeField] public Transform m_TrackedObject;
        [SerializeField] public Vector3 m_CameraPosition;
        [SerializeField] public float m_CameraPitch;
        [SerializeField] public float m_CameraYaw;
        

        private Camera m_Camera;
        private Transform m_CameraTransform;
        private SpringCoefs m_SpringCoefs;

        private void Awake()
        {
            m_Camera = GetComponent<Camera>();
            m_CameraTransform = transform;
        }

        private float m_LastPitch, m_LastYaw;
        private Quaternion m_RotationStart;
        private void Update()
        {
            float deltaTime = Time.deltaTime;
            SpringUtils.UpdateDampedSpringCoef(ref m_SpringCoefs, deltaTime, m_AngularFrequency, m_DampingRatio);

            bool rotationChanged = m_LastPitch != m_CameraPitch || m_LastYaw != m_CameraYaw;
            if (rotationChanged)
            {
                m_RotationStart = m_CameraTransform.rotation;
                m_RotInterpolateVal = 0;
                m_LastPitch = m_CameraPitch;
                m_LastYaw = m_CameraYaw;
            }
        }

        private float m_RotInterpolateVal, m_RotInterpolateVel;
        private float m_PosXInterpolateVel, m_PosYInterpolateVel, m_PosZInterpolateVel;
        private void LateUpdate()
        {
            SpringUtils.UpdateDampedSpringMotion(ref m_RotInterpolateVal, ref m_RotInterpolateVel, 1f, m_SpringCoefs);
            m_CameraTransform.rotation = Quaternion.SlerpUnclamped(m_RotationStart, Quaternion.Euler(new Vector3(m_CameraPitch, m_CameraYaw, 0)), m_RotInterpolateVal);

            Vector3 targetPos = m_TrackedObject.position + Quaternion.Euler(0, m_CameraYaw, 0) * m_CameraPosition;
            Vector3 interpolatePos = m_CameraTransform.position;
            SpringUtils.UpdateDampedSpringMotion(ref interpolatePos.x, ref m_PosXInterpolateVel, targetPos.x, m_SpringCoefs);
            SpringUtils.UpdateDampedSpringMotion(ref interpolatePos.y, ref m_PosYInterpolateVel, targetPos.y, m_SpringCoefs);
            SpringUtils.UpdateDampedSpringMotion(ref interpolatePos.z, ref m_PosZInterpolateVel, targetPos.z, m_SpringCoefs);

            m_CameraTransform.position = interpolatePos;
        }
    }
}