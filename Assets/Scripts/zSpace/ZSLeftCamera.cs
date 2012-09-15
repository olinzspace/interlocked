using UnityEngine;
using System;
using System.Collections;
using System.Runtime.InteropServices;

public class ZSLeftCamera : MonoBehaviour
{
  void Start()
  {
    GameObject coreObject = GameObject.Find("ZSCore");

    if (coreObject != null)
      m_core = coreObject.GetComponent<ZSCore>();
  }

  void OnPreCull()
  {
    if (m_core != null)
    {
      ZSCore.Eye eye = ZSCore.Eye.Left;

      if (m_core.AreEyesSwapped())
        eye = ZSCore.Eye.Right;

      gameObject.transform.position = m_core.m_currentCamera.transform.position;
      gameObject.transform.rotation = m_core.m_currentCamera.transform.rotation;
      gameObject.camera.projectionMatrix = m_core.GetProjectionMatrix(eye) * m_core.GetViewMatrix(eye);
    }

    GL.IssuePluginEvent((int)ZSCore.GlPluginEventType.SelectLeftEye);
  }

  private ZSCore m_core = null;
}
