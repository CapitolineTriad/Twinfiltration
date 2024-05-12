using TMPro;
using Twinfiltration;
using UnityEngine;
using UnityEngine.UI;

public class AbilityUI : MonoBehaviour
{
    [SerializeField] private Image m_Image;
    [SerializeField] private TextMeshProUGUI m_Text;
    [SerializeField] private float m_AngularFrequency = 10f;
    [SerializeField] private float m_DampingRatio = 0.8f;

    [SerializeField] public float m_MaxFill = 3;
    [SerializeField] public float m_CurrFill = 3;

    private System.Random m_Random = new System.Random();
    private Transform m_UITransform;
    private Vector3 m_OriginalPosition;

    private void Awake()
    {
        m_UITransform = transform;
        m_LastFill = m_CurrFill;
        m_OriginalPosition = m_UITransform.position;
        m_Text.text = $"Uses: {m_CurrFill}";
    }

    public void BumpUI()
    {
        m_XVel = m_Random.Next(-100, 100);
        m_YVel = m_Random.Next(-100, 100);
    }

    private float m_FillVelocity;
    public void UpdateFill()
    {
        float fillPerc = m_CurrFill / m_MaxFill;
        float fillAmount = m_Image.fillAmount;
        SpringUtils.UpdateDampedSpringMotion(ref fillAmount, ref m_FillVelocity, fillPerc, m_SpringCoefs);
        m_Image.fillAmount = fillAmount;
    }

    private float m_XVel;
    private float m_YVel;
    private float m_LastFill;
    private SpringCoefs m_SpringCoefs;
    private void Update()
    {
        var deltaTime = Time.deltaTime;
        SpringUtils.UpdateDampedSpringCoef(ref m_SpringCoefs, deltaTime, m_AngularFrequency, m_DampingRatio);

        UpdateFill();
        if (m_LastFill != m_CurrFill)
        {
            BumpUI();
            m_Text.text = $"Uses: {m_CurrFill}";
        }
        m_LastFill = m_CurrFill;

        Vector3 newPos = m_UITransform.position;
        SpringUtils.UpdateDampedSpringMotion(ref newPos.x, ref m_XVel, m_OriginalPosition.x, m_SpringCoefs);
        SpringUtils.UpdateDampedSpringMotion(ref newPos.y, ref m_YVel, m_OriginalPosition.y, m_SpringCoefs);
        m_UITransform.position = newPos;
    }
}
