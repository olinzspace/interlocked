using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

public class ZSCore : MonoBehaviour
{
  #region ENUMERATIONS

  public enum Eye
  {
    Left = 0,
    Right = 1,
    Center = 2,
    NumEyes
  }

  public enum TrackerTargetType
  {
    Unknown = -1,
    Head = 0,
    Primary = 1,
    Secondary = 2,
    NumTypes
  }

  public enum GlPluginEventType
  {
    SelectLeftEye = 0,
    SelectRightEye = 1,
    FrameDone = 2,
    DisableStereo = 3,
    InitializeLRDetectFullscreen = 4,
    InitializeLRDetectWindowed = 5,
    UpdateLRDetect = 6
  }

  public enum StylusLedColor
  {
    Black = 0,
    White = 1,
    Red = 2,
    Green = 3,
    Blue = 4,
    Cyan = 5,
    Magenta = 6,
    Yellow = 7
  }

  #endregion

  #region UNITY_EDITOR

  public GameObject m_currentCamera = null;

  #endregion

  #region UNITY_CALLBACKS

  void Awake()
  {
    // Initialize the left and right stereo cameras.
    InitializeStereoCameras();

    // Initialize the zSpace plugin.
    m_isZSpaceInitialized = zsup_initialize();

    if (!m_isZSpaceInitialized)
    {
      Debug.Log("Could not locate the zSpace SDK.  Failed to initialize zSpace.");
      return;
    }

    // Initialize left/right detect.
    if (Screen.fullScreen)
      GL.IssuePluginEvent((int)ZSCore.GlPluginEventType.InitializeLRDetectFullscreen);
    else
      GL.IssuePluginEvent((int)ZSCore.GlPluginEventType.InitializeLRDetectWindowed);

    // Set the window position and size.
    zsup_setWindowPosition(Screen.x, Screen.y);
    zsup_setWindowSize(Screen.width, Screen.height);

    if (m_currentCamera != null && m_currentCamera.camera != null)
      SetStereoEnabled(true);

    // Start the update coroutine.
    StartCoroutine(UpdateCoroutine());
  }

  void OnDestroy()
  {
    if (m_isZSpaceInitialized)
    {
      GL.IssuePluginEvent((int)ZSCore.GlPluginEventType.DisableStereo);
      GL.InvalidateState();
      zsup_shutdown();
    }
  }

  void OnGUI()
  {
    Event e = Event.current;

    if (e.isKey && e.type == EventType.KeyUp)
    {
      switch (e.keyCode)
      {
        case KeyCode.Escape:
          Application.Quit();
          break;
        default:
          break;
      }
    }
  }

  #endregion

  #region ZSPACE_APIS

  /// <summary>
  /// Set whether or not stereoscopic 3D is enabled.
  /// </summary>
  /// <param name="isStereoEnabled">True to enable stereoscopic 3D.  False otherwise.</param>
  public void SetStereoEnabled(bool isStereoEnabled)
  {
    if (!m_isZSpaceInitialized)
      return;

    if (m_currentCamera != null && m_currentCamera.camera != null)
    {
      m_currentCamera.camera.enabled = !isStereoEnabled;

      if (m_leftCamera != null)
        m_leftCamera.enabled = isStereoEnabled;

      if (m_rightCamera != null)
        m_rightCamera.enabled = isStereoEnabled;

      m_isStereoEnabled = isStereoEnabled;
    }
  }

  /// <summary>
  /// Check whether or not stereoscopic 3D rendering is enabled.
  /// </summary>
  /// <returns>True if stereoscopic 3D is enabled.  False if not.</returns>
  public bool IsStereoEnabled()
  {
    return m_isStereoEnabled;
  }

  /// <summary>
  /// Set whether or not the left and right eyes are swapped.
  /// </summary>
  /// <param name="areEyesSwapped">Whether or not the left and right eyes are swapped.</param>
  public void SetEyesSwapped(bool areEyesSwapped)
  {
    m_areEyesSwapped = areEyesSwapped;
  }

  /// <summary>
  /// Check whether or not the left and right eyes are swapped.
  /// </summary>
  /// <returns>Whether or not the left and right eyes are swapped.</returns>
  public bool AreEyesSwapped()
  {
    return m_areEyesSwapped;
  }

  /// <summary>
  /// Get the offset of the current display.
  /// </summary>
  /// <returns>The display offset (in meters) in Vector3 format.</returns>
  public Vector3 GetDisplayOffset()
  {
    float[] displayOffsetData = new float[3];
    zsup_getDisplayOffset(displayOffsetData);
    return ConvertFloatArrayToVector3(displayOffsetData);
  }

  /// <summary>
  /// Get the virtual (x, y) position of the current display.
  /// </summary>
  /// <returns>The display position (virtual x, y coordinates) in Vector2 format.</returns>
  public Vector2 GetDisplayPosition()
  {
    float[] displayPositionData = new float[2];
    zsup_getDisplayPosition(displayPositionData);
    return ConvertFloatArrayToVector2(displayPositionData);
  }

  /// <summary>
  /// Get the angle of the current display.
  /// </summary>
  /// <returns>The display angle (in degrees) in Vector2 format.</returns>
  public Vector2 GetDisplayAngle()
  {
    float[] displayAngleData = new float[2];
    zsup_getDisplayAngle(displayAngleData);
    return ConvertFloatArrayToVector2(displayAngleData);
  }


  /// <summary>
  /// Get the resolution of the current display.
  /// </summary>
  /// <returns>The display resolution (in pixels) in Vector2 format.</returns>
  public Vector2 GetDisplayResolution()
  {
    float[] displayResolutionData = new float[2];
    zsup_getDisplayResolution(displayResolutionData);
    return ConvertFloatArrayToVector2(displayResolutionData);
  }


  /// <summary>
  /// Get the size of the current display.
  /// </summary>
  /// <returns>The display size (in meters) in Vector2 format.</returns>
  public Vector2 GetDisplaySize()
  {
    float[] displaySizeData = new float[2];
    zsup_getDisplaySize(displaySizeData);
    return ConvertFloatArrayToVector2(displaySizeData);
  }


  /// <summary>
  /// Set the stereo window's position
  /// </summary>
  /// <param name="x">The left-most position in absolute Window's screen coordinates.</param>
  /// <param name="y">The top-most position in absolute Window's screen coordinates.</param>
  public void SetWindowPosition(int x, int y)
  {
    zsup_setWindowPosition(x, y);
  }

  /// <summary>
  /// Set the stereo window's size.
  /// </summary>
  /// <param name="width">Window width in pixels.</param>
  /// <param name="height">Window height in pixels.</param>
  public void SetWindowSize(int width, int height)
  {
    zsup_setWindowSize(width, height);
  }

  /// <summary>
  /// Set the inter-pupillary distance - the physical distance between the user's eyes.
  /// </summary>
  /// <param name="interPupillaryDistance">The inter-pupillary distance (in meters).</param>
  public void SetInterPupillaryDistance(float interPupillaryDistance)
  {
    zsup_setInterPupillaryDistance(interPupillaryDistance);
  }

  /// <summary>
  /// Get the inter-pupillary distance - the physical distance between the user's eyes.
  /// </summary>
  /// <returns>The inter-pupillary distance (in meters).</returns>
  public float GetInterPupillaryDistance()
  {
    return zsup_getInterPupillaryDistance();
  }

  /// <summary>
  /// Set the stereo level.
  /// </summary>
  /// <param name="stereoLevel">
  /// The stereo level from 0.0f to 1.0f.  A stereo level of 1.0f represents
  /// full stereo.  A stereo level of 0.0f represents no stereo.
  /// </param>
  public void SetStereoLevel(float stereoLevel)
  {
    zsup_setStereoLevel(stereoLevel);
  }

  /// <summary>
  /// Get the stereo level.
  /// </summary>
  /// <returns>
  /// The stereo level from 0.0f to 1.0f.  A stereo level of 1.0f represents
  /// full stereo.  A stereo level of 0.0f represents no stereo.
  /// </returns>
  public float GetStereoLevel()
  {
    return zsup_getStereoLevel();
  }

  /// <summary>
  /// Set the world scale.
  /// </summary>
  /// <param name="worldScale">The world scale.</param>
  public void SetWorldScale(float worldScale)
  {
    zsup_setWorldScale(worldScale);
  }

  /// <summary>
  /// Get the world scale.
  /// </summary>
  /// <returns>The world scale.</returns>
  public float GetWorldScale()
  {
    return zsup_getWorldScale();
  }

  /// <summary>
  /// Set the field of view scale.
  /// </summary>
  /// <param name="fieldOfViewScale">The field of view scale.</param>
  public void SetFieldOfViewScale(float fieldOfViewScale)
  {
    zsup_setFieldOfViewScale(fieldOfViewScale);
  }

  /// <summary>
  /// Get the field of view scale.
  /// </summary>
  /// <returns>The field of view scale.</returns>
  public float GetFieldOfViewScale()
  {
    return zsup_getFieldOfViewScale();
  }

  /// <summary>
  /// Set the zero parallax offset.
  /// </summary>
  /// <param name="zeroParallaxOffset">The zero parallax offset.</param>
  public void SetZeroParallaxOffset(float zeroParallaxOffset)
  {
    zsup_setZeroParallaxOffset(zeroParallaxOffset);
  }

  /// <summary>
  /// Get the zero parallax offset.
  /// </summary>
  /// <returns>The zero parallax offset.</returns>
  public float GetZeroParallaxOffset()
  {
    return zsup_getZeroParallaxOffset();
  }

  /// <summary>
  /// Set the near clip distance.
  /// </summary>
  /// <param name="nearClip">The near clip distance (in meters).</param>
  public void SetNearClip(float nearClip)
  {
    zsup_setNearClip(nearClip);

    if (m_leftCamera != null)
      m_leftCamera.nearClipPlane = nearClip;
    if (m_rightCamera != null)
      m_rightCamera.nearClipPlane = nearClip;
  }

  /// <summary>
  /// Get the near clip distance.
  /// </summary>
  /// <returns>The near clip distance (in meters).</returns>
  public float GetNearClip()
  {
    return zsup_getNearClip();
  }

  /// <summary>
  /// Set the far clip distance.
  /// </summary>
  /// <param name="farClip">The far clip distance (in meters).</param>
  public void SetFarClip(float farClip)
  {
    zsup_setFarClip(farClip);

    if (m_leftCamera != null)
      m_leftCamera.farClipPlane = farClip;
    if (m_rightCamera != null)
      m_rightCamera.farClipPlane = farClip;
  }

  /// <summary>
  /// Get the far clip distance.
  /// </summary>
  /// <returns>The far clip distance (in meters).</returns>
  public float GetFarClip()
  {
    return zsup_getFarClip();
  }

  /// <summary>
  /// Get the view matrix for a specified eye.
  /// </summary>
  /// <param name="eye">The eye: left, right, or center.</param>
  /// <returns>The view matrix in Matrix4x4 format.</returns>
  public Matrix4x4 GetViewMatrix(Eye eye)
  {
    return m_viewMatrices[(int)eye];
  }

  /// <summary>
  /// Get the projection matrix for a specified eye.
  /// </summary>
  /// <param name="eye">The eye: left, right, or center.</param>
  /// <returns>The projection matrix in Matrix4x4 format.</returns>
  public Matrix4x4 GetProjectionMatrix(Eye eye)
  {
    return m_projectionMatrices[(int)eye];
  }

  /// <summary>
  /// Get the position of a specified eye.
  /// </summary>
  /// <param name="eye">The eye: left, right, or center.</param>
  /// <returns>The position of the eye in Vector3 format.</returns>
  public Vector3 GetEyePosition(Eye eye)
  {
    float[] positionData = new float[3];
    zsup_getEyePosition((int)eye, positionData);

    return ConvertFloatArrayToVector3(positionData);
  }

  /// <summary>
  /// Get the frustum bounds for a specified eye.
  /// </summary>
  /// <param name="eye">The eye: left, right, or center.</param>
  /// <param name="bounds">The frustum bounds corresponding to a specified eye laid out as follows:\n\n
  /// [left, right, bottom, top, nearClip, farClip]</param>
  public void GetFrustumBounds(Eye eye, float[/*6*/] bounds)
  {
    zsup_getFrustumBounds((int)eye, bounds);
  }

  /// <summary>
  /// Set whether or not head tracking is enabled.
  /// </summary>
  /// <param name="isHeadTrackingEnabled">Flag to specify whether or not head tracking is enabled.</param>
  public void SetHeadTrackingEnabled(bool isHeadTrackingEnabled)
  {
    zsup_setHeadTrackingEnabled(isHeadTrackingEnabled);
  }

  /// <summary>
  /// Check if head tracking is enabled.
  /// </summary>
  /// <returns>True if head tracking is enabled.  False if not.</returns>
  public bool IsHeadTrackingEnabled()
  {
    return zsup_isHeadTrackingEnabled();
  }

  /// <summary>
  /// Set the uniform scale that is to be applied to the head tracked position.
  /// </summary>
  /// <param name="headTrackingScale">The scale applied to head tracking.</param>
  public void SetHeadTrackingScale(float headTrackingScale)
  {
    zsup_setHeadTrackingScale(headTrackingScale);
  }

  /// <summary>
  /// Get the uniform scale that is applied to the head tracked position.
  /// </summary>
  /// <returns>The scale applied to head tracking.</returns>
  public float GetHeadTrackingScale()
  {
    return zsup_getHeadTrackingScale();
  }

  /// <summary>
  /// Set whether or not stylus tracking is enabled.
  /// </summary>
  /// <param name="isStylusTrackingEnabled">Flag to specify whether or not to enabled stylus tracking.</param>
  public void SetStylusTrackingEnabled(bool isStylusTrackingEnabled)
  {
    zsup_setStylusTrackingEnabled(isStylusTrackingEnabled);
  }

  /// <summary>
  /// Check whether or not stylus tracking is enabled.
  /// </summary>
  /// <returns>True if stylus tracking is enabled.  False if not.</returns>
  public bool IsStylusTrackingEnabled()
  {
    return zsup_isStylusTrackingEnabled();
  }

  /// <summary>
  /// Set whether or no mouse emulation is enabled.
  /// </summary>
  /// <param name="isMouseEmulationEnabled">True to enable mouse emulation, false otherwise.</param>
  public void SetMouseEmulationEnabled(bool isMouseEmulationEnabled)
  {
    zsup_setMouseEmulationEnabled(isMouseEmulationEnabled);
  }

  /// <summary>
  /// Check whether or not mouse emulation is enabled.
  /// </summary>
  /// <returns>True if mouse emulation is enabled.  False if not.</returns>
  public bool IsMouseEmulationEnabled()
  {
    return zsup_isMouseEmulationEnabled();
  }

  /// <summary>
  /// Set the distance at which mouse emulation will be enabled.
  /// </summary>
  /// <param name="mouseEmulationDistance">The mouse emulation distance.</param>
  public void SetMouseEmulationDistance(float mouseEmulationDistance)
  {
    zsup_setMouseEmulationDistance(mouseEmulationDistance);
  }

  /// <summary>
  /// Get the distance at which mouse emulation will be enabled.
  /// </summary>
  /// <returns>The mouse emulation distance.</returns>
  public float GetMouseEmulationDistance()
  {
    return zsup_getMouseEmulationDistance();
  }

  /// <summary>
  /// Set whether or not the LED on the stylus is enabled.
  /// </summary>
  /// <param name="isStylusLedEnabled">Whether or not to enable the stylus LED.</param>
  public void SetStylusLedEnabled(bool isStylusLedEnabled)
  {
    zsup_setStylusLedEnabled(isStylusLedEnabled);
  }

  /// <summary>
  /// Check whether or not the LED on the stylus is enabled.
  /// </summary>
  /// <returns>Whether or not the stylus LED is enabled.</returns>
  public bool IsStylusLedEnabled()
  {
    return zsup_isStylusLedEnabled();
  }

  /// <summary>
  /// Set the stylus LED color.
  /// </summary>
  /// <param name="stylusLedColor">The stylus LED color.</param>
  public void SetStylusLedColor(StylusLedColor stylusLedColor)
  {
    zsup_setStylusLedColor(m_stylusLedColors[(int)stylusLedColor]);
  }

  /// <summary>
  /// Get the stylus LED color.
  /// </summary>
  /// <returns>The current color of the stylus LED.</returns>
  public StylusLedColor GetStylusLedColor()
  {
    int[] stylusLedColor = new int[3];
    zsup_getStylusLedColor(stylusLedColor);

    for (int i = 0; i < m_stylusLedColors.Count; ++i)
    {
      int[] color = m_stylusLedColors[i];

      if (stylusLedColor[0] == color[0] && stylusLedColor[1] == color[1] && stylusLedColor[2] == color[2])
        return (StylusLedColor)i;
    }

    return StylusLedColor.Black;
  }

  /// <summary>
  /// Set whether or not stylus vibration is enabled.  This only determines
  /// whether the appropriate command is sent to the hardware if StartStylusVibration()
  /// is called.  If the stylus is already vibrating, StopStylusVibration() should
  /// be called to stop the current vibration.
  /// </summary>
  /// <param name="isStylusVibrationEnabled">Whether or not stylus vibration is enabled.</param>
  public void SetStylusVibrationEnabled(bool isStylusVibrationEnabled)
  {
    zsup_setStylusVibrationEnabled(isStylusVibrationEnabled);
  }

  /// <summary>
  /// Check whether or not stylus vibration is enabled.
  /// </summary>
  /// <returns>True if stylus vibration is enabled.  False if vibration is disabled.</returns>
  public bool IsStylusVibrationEnabled()
  {
    return zsup_isStylusVibrationEnabled();
  }

  /// <summary>
  /// Set the period for how long the stylus should vibrate.  Note, the actual period set will
  /// depend on the resolution of the motor in the stylus.
  /// </summary>
  /// <param name="stylusVibrationOnPeriod">The on period in seconds.</param>
  public void SetStylusVibrationOnPeriod(float stylusVibrationOnPeriod)
  {
    zsup_setStylusVibrationOnPeriod(stylusVibrationOnPeriod);
  }

  /// <summary>
  /// Get the on period of the stylus vibration.
  /// </summary>
  /// <returns>The on period in seconds.</returns>
  public float GetStylusVibrationOnPeriod()
  {
    return zsup_getStylusVibrationOnPeriod();
  }

  /// <summary>
  /// Set the period for how long the stylus should not vibrate.  Note, the actual period set will
  /// depend on the resolution of the motor in the stylus.
  /// </summary>
  /// <param name="stylusVibrationOffPeriod">The off period in seconds.</param>
  public void SetStylusVibrationOffPeriod(float stylusVibrationOffPeriod)
  {
    zsup_setStylusVibrationOffPeriod(stylusVibrationOffPeriod);
  }

  /// <summary>
  /// Get the off period of the stylus vibration.
  /// </summary>
  /// <returns>The off period in seconds.</returns>
  public float GetStylusVibrationOffPeriod()
  {
    return zsup_getStylusVibrationOffPeriod();
  }

  /// <summary>
  /// Set the repeat count of the stylus vibration.  
  /// 
  /// This corresponds to the number of vibration cycles that occur after the initial vibration.  
  /// If the value passed in is non-negative, one period of "on" then "off" will occur, followed by 
  /// [stylusVibrationRepeatCount] additional cycles.  If the value is negative, it will continue
  /// to vibrate indefinitely or until StopStylusVibration() is called.
  /// 
  /// (stylusVibrationRepeatCount =  0:  1 vibration + 0 additional = 1 total)
  /// 
  /// (stylusVibrationRepeatCount =  1:  1 vibration + 1 additional = 2 total)
  /// 
  /// (stylusVibrationRepeatCount = -1:  infinite)
  /// 
  /// </summary>
  /// <param name="stylusVibrationRepeatCount">The number of times the stylus vibration on/off
  /// pattern should repeat after the initial vibration.  A negative value denotes infinite repetition.</param>
  public void SetStylusVibrationRepeatCount(int stylusVibrationRepeatCount)
  {
    zsup_setStylusVibrationRepeatCount(stylusVibrationRepeatCount);
  }

  /// <summary>
  /// Get the repeat count of the stylus vibration.  This corresponds to the number of vibration cycles
  /// that occur after the initial vibration.
  /// </summary>
  /// <returns>The repeat count of the stylus vibration.</returns>
  public int GetStylusVibrationRepeatCount()
  {
    return zsup_getStylusVibrationRepeatCount();
  }

  /// <summary>
  /// Start vibrating the stylus by repeating the specified "on" and "off" cycles.
  /// </summary>
  public void StartStylusVibration()
  {
    zsup_startStylusVibration();
  }

  /// <summary>
  /// Stop vibrating the stylus if it is currently vibrating.  If StartStylusVibration() is
  /// called again, the stylus will start vibrating the full sequence of "on" and "off" cycles.
  /// </summary>
  public void StopStylusVibration()
  {
    zsup_stopStylusVibration();
  }

  /// <summary>
  /// Get the tracker space pose of a specified default TrackerTarget.
  /// </summary>
  /// <param name="trackerTargetType">The type of TrackerTarget.</param>
  /// <returns>The Matrix4x4 pose in tracker space.</returns>
  public Matrix4x4 GetTrackerTargetPose(TrackerTargetType trackerTargetType)
  {
    return m_trackerTargetPoses[(int)trackerTargetType];
  }

  /// <summary>
  /// Get the camera space pose of a specified default TrackerTarget.
  /// </summary>
  /// <param name="trackerTargetType">The type of TrackerTarget.</param>
  /// <returns>The Matrix4x4 pose in camera space.</returns>
  public Matrix4x4 GetTrackerTargetCameraPose(TrackerTargetType trackerTargetType)
  {
    return m_trackerTargetCameraPoses[(int)trackerTargetType];
  }

  /// <summary>
  /// Get the world space pose of a specified default TrackerTarget.
  /// </summary>
  /// <param name="trackerTargetType">The type of TrackerTarget.</param>
  /// <returns>The Matrix4x4 pose in world space.</returns>
  public Matrix4x4 GetTrackerTargetWorldPose(TrackerTargetType trackerTargetType)
  {
    return m_trackerTargetWorldPoses[(int)trackerTargetType];
  }

  /// <summary>
  /// Get the tracker space buffered pose of a specified default TrackerTarget.
  /// </summary>
  /// <param name="trackerTargetType">The type of TrackerTarget.</param>
  /// <param name="lookBackTime"></param>
  /// <returns></returns>
  public Matrix4x4 GetTrackerTargetBufferedPose(TrackerTargetType trackerTargetType, float lookBackTime)
  {
    float[] matrixData = new float[16];
    zsup_getTrackerTargetBufferedPose((int)trackerTargetType, lookBackTime, matrixData);
    return ConvertFloatArrayToMatrix4x4(matrixData);
  }

  /// <summary>
  /// Get the number of buttons associated with a specified TrackerTarget.
  /// </summary>
  /// <param name="trackerTargetType">The type of the TrackerTarget.</param>
  /// <returns>The number of buttons contained by a TrackerTarget.</returns>
  public int GetNumTrackerTargetButtons(TrackerTargetType trackerTargetType)
  {
    return zsup_getNumTrackerTargetButtons((int)trackerTargetType);
  }

  /// <summary>
  /// Check whether or not a specified target button is pressed.
  /// </summary>
  /// <param name="trackerTargetType">The type of TrackerTarget.</param>
  /// <param name="buttonId">The id of the button.</param>
  /// <returns>True if the button is pressed, false otherwise.</returns>
  public bool IsTrackerTargetButtonPressed(TrackerTargetType trackerTargetType, int buttonId)
  {
    return zsup_isTrackerTargetButtonPressed((int)trackerTargetType, buttonId);
  }

  #endregion

  #region PRIVATE_HELPERS

  /// <summary>
  /// Initialize the left and right stereo cameras.
  /// </summary>
  private void InitializeStereoCameras()
  {
    float currentCameraDepth = 0.0f;

    if (m_currentCamera != null && m_currentCamera.camera != null)
      currentCameraDepth = m_currentCamera.camera.depth;

    GameObject leftCameraObject = GameObject.Find("ZSLeftCamera");
    GameObject rightCameraObject = GameObject.Find("ZSRightCamera");
    GameObject finalCameraObject = GameObject.Find("ZSFinalCamera");

    if (leftCameraObject != null)
      m_leftCamera = leftCameraObject.camera;

    if (rightCameraObject != null)
      m_rightCamera = rightCameraObject.camera;

    if (finalCameraObject != null)
      m_finalCamera = finalCameraObject.camera;

    if (m_leftCamera != null)
    {
      m_leftCamera.enabled = false;
      m_leftCamera.depth = currentCameraDepth + 1.0f;
    }

    if (m_rightCamera != null)
    {
      m_rightCamera.enabled = false;
      m_rightCamera.depth = currentCameraDepth + 2.0f;
    }

    if (m_finalCamera != null)
    {
      m_finalCamera.enabled = false;
      m_finalCamera.depth = currentCameraDepth + 3.0f;
    }
  }

  /// <summary>
  /// Update all of the stereo and tracker information.
  /// </summary>
  private void UpdateInternal()
  {
    if (!m_isZSpaceInitialized)
      return;

    zsup_update();
    GL.IssuePluginEvent((int)GlPluginEventType.UpdateLRDetect);

    UpdateStereoInternal();
    UpdateTrackerInternal();
  }

  /// <summary>
  /// Update all of the stereo information.
  /// </summary>
  private void UpdateStereoInternal()
  {
    // Update the screen position if it has changed.
    if (Screen.x != zsup_getWindowX() || Screen.y != zsup_getWindowY())
      zsup_setWindowPosition(Screen.x, Screen.y);

    // Update the window dimensions if they have changed.
    if (Screen.width != zsup_getWindowWidth() || Screen.height != zsup_getWindowHeight())
      zsup_setWindowSize(Screen.width, Screen.height);

    // Get the view and projection matrices.
    float[] matrixData = new float[16];

    for (int i = 0; i < (int)Eye.NumEyes; ++i)
    {
      zsup_getViewMatrix(i, matrixData);
      m_viewMatrices[i] = ConvertFloatArrayToMatrix4x4(matrixData);

      zsup_getProjectionMatrix(i, matrixData);
      m_projectionMatrices[i] = ConvertFloatArrayToMatrix4x4(matrixData);
    }
  }

  /// <summary>
  /// Update all of the tracker information.
  /// </summary>
  private void UpdateTrackerInternal()
  {
    // Get the tracker, camera, and world space target poses.
    float[] matrixData = new float[16];

    Matrix4x4 rightToLeftCoordinateSystem = Matrix4x4.Scale(new Vector4(1.0f, 1.0f, -1.0f));
    Matrix4x4 currentCameraLocalToWorldMatrix = Matrix4x4.identity;

    if (m_currentCamera != null && m_currentCamera.camera != null)
      currentCameraLocalToWorldMatrix = m_currentCamera.transform.localToWorldMatrix;

    // Get the viewport offset.
    float[] viewportOffset = new float[3];
    zsup_getViewportOffset(viewportOffset);
    viewportOffset[2] = -viewportOffset[2];

    float worldScale = zsup_getWorldScale();
    float fieldOfViewScale = zsup_getFieldOfViewScale();

    // Factor in world and field of view scales for the viewport offset.
    for (int i = 0; i < 3; ++i)
      viewportOffset[i] *= (worldScale * fieldOfViewScale);

    for (int i = 0; i < (int)TrackerTargetType.NumTypes; ++i)
    {
      // Tracker space poses.
      zsup_getTrackerTargetPose(i, matrixData);
      m_trackerTargetPoses[i] = ConvertFloatArrayToMatrix4x4(matrixData);

      // Camera space poses.
      zsup_getTrackerTargetCameraPose(i, matrixData);
      m_trackerTargetCameraPoses[i] = rightToLeftCoordinateSystem * ConvertFloatArrayToMatrix4x4(matrixData) * rightToLeftCoordinateSystem;

      // World space poses.
      m_trackerTargetWorldPoses[i] = m_trackerTargetCameraPoses[i];

      // Scale the position based on world and field of view scales.
      m_trackerTargetWorldPoses[i][0, 3] *= worldScale * fieldOfViewScale;
      m_trackerTargetWorldPoses[i][1, 3] *= worldScale * fieldOfViewScale;
      m_trackerTargetWorldPoses[i][2, 3] *= worldScale;

      // Convert the camera space pose to world space.
      m_trackerTargetWorldPoses[i] = currentCameraLocalToWorldMatrix * m_trackerTargetWorldPoses[i];

      // Apply the viewport offset to the world space pose.
      for (int j = 0; j < 3; ++j)
        m_trackerTargetWorldPoses[i][j, 3] -= viewportOffset[j];
    }
  }

  /// <summary>
  /// Convert an array of 16 floats to Unity's Matrix4x4 format.
  /// </summary>
  /// <param name="matrixData">The matrix data stored in a float array.</param>
  /// <returns>The matrix data in Matrix4x4 format.</returns>
  private Matrix4x4 ConvertFloatArrayToMatrix4x4(float[/*16*/] matrixData)
  {
    Matrix4x4 matrix = new Matrix4x4();

    for (int i = 0; i < 16; i++)
      matrix[i] = matrixData[i];

    return matrix;
  }

  /// <summary>
  /// Convert an array of 2 floats to Unity's Vector2 format.
  /// </summary>
  /// <param name="vectorData">The vector data stored in a float array.</param>
  /// <returns>The vector data in Vector2 format.</returns>
  private Vector2 ConvertFloatArrayToVector2(float[/*2*/] vectorData)
  {
    Vector2 vector2 = new Vector2();

    vector2.x = vectorData[0];
    vector2.y = vectorData[1];

    return vector2;
  }

  /// <summary>
  /// Convert an array of 3 floats to Unity's Vector3 format.
  /// </summary>
  /// <param name="vectorData">The vector data stored in a float array.</param>
  /// <returns>The vector data in Vector3 format.</returns>
  private Vector3 ConvertFloatArrayToVector3(float[/*3*/] vectorData)
  {
    Vector3 vector3 = new Vector3();

    vector3.x = vectorData[0];
    vector3.y = vectorData[1];
    vector3.z = vectorData[2];

    return vector3;
  }

  IEnumerator UpdateCoroutine()
  {
    while (true)
    {
      UpdateInternal();

      // Set the final camera to be enabled so that it can reset the draw buffer
      // to the back buffer for the next frame.
      if (IsStereoEnabled() && m_finalCamera != null)
        m_finalCamera.enabled = true;

      yield return new WaitForEndOfFrame();
    }
  }

  #endregion

  #region PRIVATE_MEMBERS

  private bool m_isZSpaceInitialized = false;
  private bool m_isStereoEnabled = false;
  private bool m_areEyesSwapped = false;

  private Matrix4x4[] m_viewMatrices = new Matrix4x4[(int)Eye.NumEyes];
  private Matrix4x4[] m_projectionMatrices = new Matrix4x4[(int)Eye.NumEyes];

  private Matrix4x4[] m_trackerTargetPoses = new Matrix4x4[(int)TrackerTargetType.NumTypes];
  private Matrix4x4[] m_trackerTargetCameraPoses = new Matrix4x4[(int)TrackerTargetType.NumTypes];
  private Matrix4x4[] m_trackerTargetWorldPoses = new Matrix4x4[(int)TrackerTargetType.NumTypes];

  private Camera m_leftCamera = null;
  private Camera m_rightCamera = null;
  private Camera m_finalCamera = null;

  private static int[] m_black    = { 0, 0, 0 };
  private static int[] m_white    = { 1, 1, 1 };
  private static int[] m_red      = { 1, 0, 0 };
  private static int[] m_green    = { 0, 1, 0 };
  private static int[] m_blue     = { 0, 0, 1 };
  private static int[] m_cyan     = { 0, 1, 1 };
  private static int[] m_magenta  = { 1, 0, 1 };
  private static int[] m_yellow   = { 1, 1, 0 };

  private List<int[]> m_stylusLedColors = new List<int[]>() { m_black, m_white, m_red, m_green, m_blue, m_cyan, m_magenta, m_yellow };

  #endregion

  #region ZSPACE_PLUGIN_IMPORT_DECLARATIONS

  [DllImport("ZSUnityPlugin")]
  private static extern bool zsup_initialize();
  [DllImport("ZSUnityPlugin")]
  private static extern void zsup_update();
  [DllImport("ZSUnityPlugin")]
  private static extern void zsup_shutdown();
  [DllImport("ZSUnityPlugin")]
  private static extern void zsup_getDisplayOffset([In] float[/*3*/] displayOffset);
  [DllImport("ZSUnityPlugin")]
  private static extern void zsup_getDisplayPosition([In] float[/*2*/] displayPosition);
  [DllImport("ZSUnityPlugin")]
  private static extern void zsup_getDisplayAngle([In] float[/*2*/] displayAngle);
  [DllImport("ZSUnityPlugin")]
  private static extern void zsup_getDisplayResolution([In] float[/*2*/] displayResolution);
  [DllImport("ZSUnityPlugin")]
  private static extern void zsup_getDisplaySize([In] float[/*2*/] displaySize);
  [DllImport("ZSUnityPlugin")]
  private static extern void zsup_setWindowPosition([In] int x, [In] int y);
  [DllImport("ZSUnityPlugin")]
  private static extern int zsup_getWindowX();
  [DllImport("ZSUnityPlugin")]
  private static extern int zsup_getWindowY();
  [DllImport("ZSUnityPlugin")]
  private static extern void zsup_setWindowSize([In] int width, [In] int height);
  [DllImport("ZSUnityPlugin")]
  private static extern int zsup_getWindowWidth();
  [DllImport("ZSUnityPlugin")]
  private static extern int zsup_getWindowHeight();
  [DllImport("ZSUnityPlugin")]
  private static extern void zsup_setInterPupillaryDistance([In] float interPupillaryDistance);
  [DllImport("ZSUnityPlugin")]
  private static extern float zsup_getInterPupillaryDistance();
  [DllImport("ZSUnityPlugin")]
  private static extern void zsup_setStereoLevel([In] float stereoLevel);
  [DllImport("ZSUnityPlugin")]
  private static extern float zsup_getStereoLevel();
  [DllImport("ZSUnityPlugin")]
  private static extern void zsup_setWorldScale([In] float worldScale);
  [DllImport("ZSUnityPlugin")]
  private static extern float zsup_getWorldScale();
  [DllImport("ZSUnityPlugin")]
  private static extern void zsup_setFieldOfViewScale([In] float fieldOfViewScale);
  [DllImport("ZSUnityPlugin")]
  private static extern float zsup_getFieldOfViewScale();
  [DllImport("ZSUnityPlugin")]
  private static extern void zsup_setZeroParallaxOffset([In] float zeroParallaxOffset);
  [DllImport("ZSUnityPlugin")]
  private static extern float zsup_getZeroParallaxOffset();
  [DllImport("ZSUnityPlugin")]
  private static extern void zsup_setNearClip([In] float nearClip);
  [DllImport("ZSUnityPlugin")]
  private static extern float zsup_getNearClip();
  [DllImport("ZSUnityPlugin")]
  private static extern void zsup_setFarClip([In] float farClip);
  [DllImport("ZSUnityPlugin")]
  private static extern float zsup_getFarClip();
  [DllImport("ZSUnityPlugin")]
  private static extern void zsup_getViewMatrix([In] int eye, [Out] float[/*16*/] viewMatrix);
  [DllImport("ZSUnityPlugin")]
  private static extern void zsup_getProjectionMatrix([In] int eye, [Out] float[/*16*/] projectionMatrix);
  [DllImport("ZSUnityPlugin")]
  private static extern void zsup_getEyePosition([In] int eye, [Out] float[/*3*/] eyePosition);
  [DllImport("ZSUnityPlugin")]
  private static extern void zsup_getFrustumBounds([In] int eye, [Out] float[/*6*/] frustumBounds);
  [DllImport("ZSUnityPlugin")]
  private static extern void zsup_setHeadTrackingEnabled([In] bool isHeadTrackingEnabled);
  [DllImport("ZSUnityPlugin")]
  private static extern bool zsup_isHeadTrackingEnabled();
  [DllImport("ZSUnityPlugin")]
  private static extern void zsup_setHeadTrackingScale([In] float headTrackingScale);
  [DllImport("ZSUnityPlugin")]
  private static extern float zsup_getHeadTrackingScale();
  [DllImport("ZSUnityPlugin")]
  private static extern void zsup_setStylusTrackingEnabled([In] bool isStylusTrackingEnabled);
  [DllImport("ZSUnityPlugin")]
  private static extern bool zsup_isStylusTrackingEnabled();
  [DllImport("ZSUnityPlugin")]
  private static extern void zsup_setMouseEmulationEnabled([In] bool isMouseEmulationEnabled);
  [DllImport("ZSUnityPlugin")]
  private static extern bool zsup_isMouseEmulationEnabled();
  [DllImport("ZSUnityPlugin")]
  private static extern void zsup_setMouseEmulationDistance([In] float mouseEmulationDistance);
  [DllImport("ZSUnityPlugin")]
  private static extern float zsup_getMouseEmulationDistance();
  [DllImport("ZSUnityPlugin")]
  private static extern void zsup_setStylusLedEnabled([In] bool isStylusLedEnabled);
  [DllImport("ZSUnityPlugin")]
  private static extern bool zsup_isStylusLedEnabled();
  [DllImport("ZSUnityPlugin")]
  private static extern void zsup_setStylusLedColor([In] int[/*3*/] ledColor);
  [DllImport("ZSUnityPlugin")]
  private static extern void zsup_getStylusLedColor([Out] int[/*3*/] ledColor);
  [DllImport("ZSUnityPlugin")]
  private static extern void zsup_setStylusVibrationEnabled([In] bool isStylusVibrationEnabled);
  [DllImport("ZSUnityPlugin")]
  private static extern bool zsup_isStylusVibrationEnabled();
  [DllImport("ZSUnityPlugin")]
  private static extern void zsup_setStylusVibrationOnPeriod([In] float stylusVibrationOnPeriod);
  [DllImport("ZSUnityPlugin")]
  private static extern float zsup_getStylusVibrationOnPeriod();
  [DllImport("ZSUnityPlugin")]
  private static extern void zsup_setStylusVibrationOffPeriod([In] float stylusVibrationOffPeriod);
  [DllImport("ZSUnityPlugin")]
  private static extern float zsup_getStylusVibrationOffPeriod();
  [DllImport("ZSUnityPlugin")]
  private static extern void zsup_setStylusVibrationRepeatCount([In] int stylusVibrationRepeatCount);
  [DllImport("ZSUnityPlugin")]
  private static extern int zsup_getStylusVibrationRepeatCount();
  [DllImport("ZSUnityPlugin")]
  private static extern void zsup_startStylusVibration();
  [DllImport("ZSUnityPlugin")]
  private static extern void zsup_stopStylusVibration();
  [DllImport("ZSUnityPlugin")]
  private static extern void zsup_getTrackerTargetPose([In] int targetType, [Out] float[/*16*/] pose);
  [DllImport("ZSUnityPlugin")]
  private static extern void zsup_getTrackerTargetCameraPose([In] int targetType, [Out] float[/*16*/] cameraPose);
  [DllImport("ZSUnityPlugin")]
  private static extern void zsup_getTrackerTargetBufferedPose([In] int targetType, [In] float lookBackTime, [Out] float[/*16*/] bufferedPose);
  [DllImport("ZSUnityPlugin")]
  private static extern int zsup_getNumTrackerTargetButtons([In] int targetType);
  [DllImport("ZSUnityPlugin")]
  private static extern bool zsup_isTrackerTargetButtonPressed([In] int targetType, [In] int buttonId);
  [DllImport("ZSUnityPlugin")]
  private static extern void zsup_getViewportOffset([Out] float[/*3*/] viewportOffset);
  [DllImport("ZSUnityPlugin")]
  private static extern void zsup_getTrackerToCameraSpaceTransform([Out] float[/*16*/] trackerToCameraSpaceTransform);

  #endregion
}