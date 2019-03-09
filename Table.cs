using System.Collections.Generic;
using UnityEngine;

public abstract class Table : MonoBehaviour
{
    [Header("Generic Table")]
    public Transform m_Patient;
    public Transform m_PatientLocation;
    public bool m_AutoPosition;

    [Header("Generic Physics")]
    public DeviceCollisionComponent[] m_CollisionComponents;
    public bool m_IsColliding;
    public LayerMask m_PhysicsOnLayers;

    [Header("Generic Auto Positioning")]
    public AutoPositionDataParser m_AutoPositioningDataParser;
    public List<AutoPositionData> m_AutoPositioningData;
    public AutoPositionData m_CurrentAutoPositioning;
    public int m_AutoPositioningNumber;

    public void StartColliding()
    { //Gets called from DeviceCollisionComponent when a collision occurs between two objects
      // print("Scanner is colliding");
        m_IsColliding = true;
    }

    public abstract void RevertUpdate();

    public int StartAutoPositioning()
    {
        m_AutoPositioningNumber = -1;
        for (int i = 0; i < m_AutoPositioningData.Count; i++)
        {
            if (m_AutoPositioningData[i].name == "AutoPositionData_" + ControlsConversion.Instance.m_AutoPositionInput)
            {
                m_AutoPositioningNumber = i;
            }
        }
        if (m_AutoPositioningNumber == -1)
        {
            return -1;
        }
        m_CurrentAutoPositioning = m_AutoPositioningData[m_AutoPositioningNumber];
        m_CurrentAutoPositioning.InitiateAutoPositioning();
        return 1;
    }

    public void StopAutoPositioning()
    {
        if (m_CurrentAutoPositioning != null)
        {
            m_CurrentAutoPositioning.StopAutoPositioning();
            m_CurrentAutoPositioning = null;
        }
    }

    public abstract void Reset();

    public void ResetPhysics()
    {
        m_CollisionComponents = base.transform.GetComponentsInChildren<DeviceCollisionComponent>();
        for (int i = 0; i < m_CollisionComponents.Length; i++)
        {
            m_CollisionComponents[i].SetMessageGod(gameObject);
            m_CollisionComponents[i].m_CollisionWithLayers = m_PhysicsOnLayers;
        }
        Collider[] componentsInChildren = base.transform.GetComponentsInChildren<Collider>();
        for (int j = 0; j < componentsInChildren.Length; j++)
        {
            for (int k = 0; k < componentsInChildren.Length; k++)
            {
                Physics.IgnoreCollision(componentsInChildren[j], componentsInChildren[k]);
            }
        }
    }
}