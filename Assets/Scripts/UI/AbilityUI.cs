using Twinfiltration;
using UnityEngine;
using UnityEngine.UI;

public class AbilityUI : MonoBehaviour
{
    [SerializeField] private Image m_Image;
    [SerializeField] private float m_AngularFrequency = 10f;
    [SerializeField] private float m_DampingRatio = 0.8f;

    [SerializeField] public float m_MaxFill = 3;
    [SerializeField] public float m_CurrFill = 3;

    private Transform m_UITransform;


    private void Awake()
    {
        m_UITransform = transform;
    }

    public void BumpUI(float bumpStrength)
    {
        m_XVel += bumpStrength;
        m_YVel += bumpStrength;
        m_ZVel += bumpStrength;
    }

    private float m_FillVelocity;
    public void UpdateFill()
    {
        float fillPerc = m_CurrFill / m_MaxFill;
        m_Image.fillAmount = fillPerc;
    }

    private float m_XVel;
    private float m_YVel;
    private float m_ZVel;
    private SpringCoefs m_SpringCoefs;
    private void Update()
    {
        var deltaTime = Time.deltaTime;
        SpringUtils.UpdateDampedSpringCoef(ref m_SpringCoefs, deltaTime, m_AngularFrequency, m_DampingRatio);

        Vector3 newPos = m_UITransform.position;
        SpringUtils.UpdateDampedSpringMotion(ref newPos.x, ref m_XVel, newPos.x, m_SpringCoefs);
        SpringUtils.UpdateDampedSpringMotion(ref newPos.y, ref m_YVel, newPos.y, m_SpringCoefs);
        SpringUtils.UpdateDampedSpringMotion(ref newPos.z, ref m_ZVel, newPos.z, m_SpringCoefs);
        m_UITransform.position = newPos;
    }
}
