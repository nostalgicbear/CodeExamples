using System.Collections.Generic;
using UnityEngine;

public class AutoPositionData : MonoBehaviour
{
    public AutoPositionComponentSequence[] m_PositionalSequence;
    public List<AutoPositionComponentSequence> m_PositionalSequenceList = new List<AutoPositionComponentSequence>();
    public AutoPositionData m_NextPositionalData;
    public int m_CurrentStep = -1;
    public int m_CurrentSubStep = -1;
    public bool m_Reverse;
    public bool m_IgnoreAutomaticAdjusting;
    public bool m_CurrentStepIgnoreAutomaticAdjusting;
    public bool m_LockControls;

    public void Start()
    {
        if (m_PositionalSequence != null)
        {
            m_PositionalSequenceList.AddRange(m_PositionalSequence);
        }
    }

    public void InitiateAutoPositioning()
    {
        m_CurrentStep = 0;
        m_CurrentSubStep = -1;
        if (m_Reverse)
        {
            m_CurrentStep = m_PositionalSequenceList.Count - 1;
        }
        if (m_PositionalSequenceList.Count > 0)
        {
            ContinueAutoPositioning();
        }
    }

    public void ContinueAutoPositioning()
    {
        m_CurrentStepIgnoreAutomaticAdjusting = false;
        m_CurrentSubStep++;
        if (m_CurrentSubStep >= 0 && m_CurrentSubStep < m_PositionalSequenceList[m_CurrentStep].m_PositionalData.Count)
        {
            m_CurrentStepIgnoreAutomaticAdjusting = m_PositionalSequenceList[m_CurrentStep].m_PositionalData[m_CurrentSubStep].m_IgnoreAutomaticAdjusting;
            m_PositionalSequenceList[m_CurrentStep].m_PositionalData[m_CurrentSubStep].m_Parent = this;
            m_PositionalSequenceList[m_CurrentStep].m_PositionalData[m_CurrentSubStep].m_Component.SendMessage("ApplyAutoPositionComponentData", m_PositionalSequenceList[m_CurrentStep].m_PositionalData[m_CurrentSubStep]);
        }
        else
        {
            m_CurrentSubStep = -1;
            if (m_Reverse)
            {
                m_CurrentStep--;
            }
            else
            {
                m_CurrentStep++;
            }
            if (m_CurrentStep >= 0 && m_CurrentStep < m_PositionalSequenceList.Count)
            {
                ContinueAutoPositioning();
            }
            else
            {
                m_CurrentStep = -1;
                if (m_NextPositionalData)
                {
                    m_NextPositionalData.InitiateAutoPositioning();
                }
            }
        }
    }

    public void StopAutoPositioning()
    {
        m_NextPositionalData = null;
        for (int i = 0; i < m_PositionalSequenceList.Count; i++)
        {
            for (int j = 0; j < m_PositionalSequenceList[i].m_PositionalData.Count; j++)
            {
                m_PositionalSequenceList[i].m_PositionalData[j].m_Component.SendMessage("StopAutoPositioning");
            }
        }
    }

    public AutoPositionComponentData GetDataForComponent(GameObject _Object)
    {
        AutoPositionComponentData result = new AutoPositionComponentData();
        for (int i = 0; i < m_PositionalSequenceList.Count; i++)
        {
            for (int j = 0; j < m_PositionalSequenceList[i].m_PositionalData.Count; j++)
            {
                if (m_PositionalSequenceList[i].m_PositionalData[j].m_Component == _Object)
                {
                    result = m_PositionalSequenceList[i].m_PositionalData[j];
                }
            }
        }
        return result;
    }
}