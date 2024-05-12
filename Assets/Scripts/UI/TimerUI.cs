using System.Collections;
using System.Collections.Generic;
using Twinfiltration;
using UnityEngine;
using UnityEngine.UI;

public class TimerUI : MonoBehaviour
{
    [SerializeField] private float m_AngularFrequency = 10f;
    [SerializeField] private float m_DampingRatio = 0.8f;
    [SerializeField] private Scrollbar m_Scrollbar1;
    [SerializeField] private Scrollbar m_Scrollbar2;
    [SerializeField] private Scrollbar m_Scrollbar3;
    [SerializeField] private Scrollbar m_Scrollbar4;

    private float[] m_ScrollStamps = new float[10] { 1, 0.891f, 0.782f, 0.673f, 0.564f, 0.455f, 0.333f, 0.236f, 0.114f, 0 };

    private void Awake()
    {
        m_Scrollbar1.value = 1;
        m_Scrollbar2.value = 1;
        m_Scrollbar3.value = 1;
        m_Scrollbar4.value = 1;
    }

    public float m_TimePassed = 0f;
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

        float scrollBar1 = m_Scrollbar1.value;
        float scrollBar2 = m_Scrollbar2.value;
        float scrollBar3 = m_Scrollbar3.value;
        float scrollBar4 = m_Scrollbar4.value;

        SpringUtils.UpdateDampedSpringMotion(ref scrollBar1, ref m_Digit1Vel, m_ScrollStamps[digit1], m_SpringCoefs);
        SpringUtils.UpdateDampedSpringMotion(ref scrollBar2, ref m_Digit2Vel, m_ScrollStamps[digit2], m_SpringCoefs);
        SpringUtils.UpdateDampedSpringMotion(ref scrollBar3, ref m_Digit3Vel, m_ScrollStamps[digit3], m_SpringCoefs);
        SpringUtils.UpdateDampedSpringMotion(ref scrollBar4, ref m_Digit4Vel, m_ScrollStamps[digit4], m_SpringCoefs);

        m_Scrollbar1.value = scrollBar1;
        m_Scrollbar2.value = scrollBar2;
        m_Scrollbar3.value = scrollBar3;
        m_Scrollbar4.value = scrollBar4;
    }
}
