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
        [SerializeField] public Vector3 m_CameraRotation;
        
        private Vector3 m_LastTrackedPosition;
        private Vector3 m_LastPosition;
        private Vector3 m_LastRotation;

        private Quaternion m_RotationStart;
        private Vector3 m_PositionStart;

        private Camera m_Camera;
        private Transform m_CameraTransform;
        private SpringCoefs m_SpringCoefs;

        private void Awake()
        {
            m_Camera = GetComponent<Camera>();
            m_CameraTransform = transform;
            m_LastPosition = m_CameraPosition;
            m_LastRotation = m_CameraRotation;
        }

        private void Update()
        {
            float deltaTime = Time.deltaTime;
            
            SpringUtils.UpdateDampedSpringCoef(ref m_SpringCoefs, deltaTime, m_AngularFrequency, m_DampingRatio);

            var currTrackedPos = m_TrackedObject != null ? m_TrackedObject.position : Vector3.zero;
            if (m_LastPosition != m_CameraPosition || m_LastRotation != m_CameraRotation || m_LastTrackedPosition != currTrackedPos)
            {
                m_InterpolateVal = 0;
                m_LastTrackedPosition = currTrackedPos;
                m_LastPosition = m_CameraPosition;
                m_LastRotation = m_CameraRotation;
                m_RotationStart = m_CameraTransform.rotation;
                m_PositionStart = m_CameraTransform.position;
            }
        }

        private float m_InterpolateVal, m_InterpolateVel;
        private void LateUpdate()
        {
            // Rotate the camera.
            SpringUtils.UpdateDampedSpringMotion(ref m_InterpolateVal, ref m_InterpolateVel, 1f, m_SpringCoefs);
            if (m_TrackedObject != null)
            {
                var cameraDir = (m_TrackedObject.position - m_CameraTransform.position).normalized;
                var lookRotation = Quaternion.LookRotation(cameraDir, Vector3.up);

                m_CameraTransform.rotation = Quaternion.SlerpUnclamped(m_RotationStart, lookRotation, m_InterpolateVal);
            }
            else
            {
                m_CameraTransform.rotation = Quaternion.SlerpUnclamped(m_RotationStart, Quaternion.Euler(m_CameraRotation), m_InterpolateVal);
            }

            m_CameraTransform.position = Vector3.LerpUnclamped(m_PositionStart, m_CameraPosition, m_InterpolateVal);
        }
    }
}