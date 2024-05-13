using System.Collections;
using System.Collections.Generic;
using Twinfiltration;
using UnityEngine;
using UnityEngine.UI;

public class TimerUI : MonoBehaviour
{
    [SerializeField] private float m_AngularFrequency = 10f;
    [SerializeField] private float m_DampingRatio = 0.8f;

    [SerializeField] private Transform m_Minutex10;
    [SerializeField] private Transform m_Minute;
    [SerializeField] private Transform m_Secondx10;
    [SerializeField] private Transform m_Second;

    [SerializeField] private GameObject m_ContentSample;

    private Rect m_ContentRect;
    private float m_ScrollHeightOrigin;
    private void Awake()
    {
        m_ContentRect = new Rect(0, 0, 30, 30); //m_ContentSample.GetComponent<RectTransform>().rect;
        m_ScrollHeightOrigin = m_Second.localPosition.y;
    }

    [HideInInspector] public float m_TimePassed = 0f;
    private float m_Digit1Vel;
    private float m_Digit2Vel;
    private float m_Digit3Vel;
    private float m_Digit4Vel;
    private SpringCoefs m_SpringCoefs;
    private void Update()
    {
        float deltaTime = Time.deltaTime;
        SpringUtils.UpdateDampedSpringCoef(ref m_SpringCoefs, deltaTime, m_AngularFrequency, m_DampingRatio);

        int digit1 = (int)m_TimePassed % 10;
        int digit2 = (int)m_TimePassed / 10 % 6;
        int digit3 = (int)m_TimePassed / 60 % 10;
        int digit4 = (int)m_TimePassed / 60 / 10 % 10;

        Vector3 m_SecondPos = m_Second.localPosition;
        Vector3 m_Secondx10Pos = m_Secondx10.localPosition;
        Vector3 m_MinutePos = m_Minute.localPosition;
        Vector3 m_Minutex10Pos = m_Minutex10.localPosition;

        SpringUtils.UpdateDampedSpringMotion(ref m_SecondPos.y, ref m_Digit1Vel, m_ScrollHeightOrigin + digit1 * m_ContentRect.height, m_SpringCoefs);
        SpringUtils.UpdateDampedSpringMotion(ref m_Secondx10Pos.y, ref m_Digit2Vel, m_ScrollHeightOrigin + digit2 * m_ContentRect.height, m_SpringCoefs);
        SpringUtils.UpdateDampedSpringMotion(ref m_MinutePos.y, ref m_Digit3Vel, m_ScrollHeightOrigin + digit3 * m_ContentRect.height, m_SpringCoefs);
        SpringUtils.UpdateDampedSpringMotion(ref m_Minutex10Pos.y, ref m_Digit4Vel, m_ScrollHeightOrigin + digit4 * m_ContentRect.height, m_SpringCoefs);

        m_Second.localPosition = m_SecondPos;
        m_Secondx10.localPosition = m_Secondx10Pos;
        m_Minute.localPosition = m_MinutePos;
        m_Minutex10.localPosition = m_Minutex10Pos;
    }
}
