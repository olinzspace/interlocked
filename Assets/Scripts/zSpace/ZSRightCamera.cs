using UnityEngine;
using System;
using System.Collections;
using System.Runtime.InteropServices;

public class ZSRightCamera : MonoBehaviour
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
      ZSCore.Eye eye = ZSCore.Eye.Right;

      if (m_core.AreEyesSwapped())
        eye = ZSCore.Eye.Left;

      gameObject.transform.position = m_core.m_currentCamera.transform.position;
      gameObject.transform.rotation = m_core.m_currentCamera.transform.rotation;
      gameObject.camera.projectionMatrix = m_core.GetProjectionMatrix(eye) * m_core.GetViewMatrix(eye);
    }

    GL.IssuePluginEvent((int)ZSCore.GlPluginEventType.SelectRightEye);
  }

  private ZSCore m_core = null;
}
