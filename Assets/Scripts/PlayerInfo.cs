using System.Collections.Generic;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(LOS.LOSVisibilityInfo))]
public class PlayerInfo : MonoBehaviour
{
    public TextMeshProUGUI m_VisibilityText;

    public List<string> VisibleSources
    {
        get { return m_VisibleSources; }
    }

    private LOS.LOSVisibilityInfo m_VisibilityInfo;
    private List<string> m_VisibleSources = new List<string>();

    private void OnEnable()
    {
        m_VisibilityInfo = GetComponent<LOS.LOSVisibilityInfo>();
    }

    private int m_LastCount = -1;
    private void FixedUpdate()
    {
        m_VisibleSources.Clear();

        foreach (LOS.ILOSSource losSource in m_VisibilityInfo.VisibleSources)
        {
            // Only add LOS ources that aren't attached to the player
            if (losSource.GameObject.tag != "Player")
            {
                var objname = losSource.GameObject.transform.parent.parent.name;
                if (!m_VisibleSources.Contains(objname))
                    m_VisibleSources.Add(objname);
            }
        }

        m_VisibilityText.transform.position = Camera.main.WorldToScreenPoint(transform.position + Vector3.up * 3);
        if (m_VisibleSources.Count != m_LastCount)
        {
            m_LastCount = m_VisibleSources.Count;
            if(m_LastCount > 0)
            {
                m_VisibilityText.text = $"Seen By: {m_VisibleSources[0]}";
                for(int i=1; i<m_VisibleSources.Count; i++)
                {
                    m_VisibilityText.text += $", {m_VisibleSources[i]}";
                }
            }
            else
            {
                m_VisibilityText.text = "Hidden";
            }
        }
    }
}