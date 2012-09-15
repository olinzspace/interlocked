using UnityEngine;
using System;
using System.Collections;
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

  #endregion

  #region UNITY_EDITOR

  public GameObject m_currentCamera = null;
  public float m_worldScale = 1.0f;
  public float m_fieldOfViewScale = 1.0f;
  public float m_stereoLevel = 1.0f;

  #endregion

  #region UNITY_CALLBACKS

  void Awake()
  {
    // Initialize the left and right stereo cameras.
    InitializeStereoCameras();

    // Initialize the zSpace plugin.
    m_isZSpaceInitialized = zSpaceInitialize();

    if (!m_isZSpaceInitialized)
    {
      Debug.Log("Failed to initialize zSpace.");
      return;
    }

    // Set the window position and size.
    zSpaceSetWindowPosition(Screen.x, Screen.y);
    zSpaceSetWindowSize(Screen.width, Screen.height);

    // Set the initial world and field of view scales.
    zSpaceSetWorldScale(m_worldScale);
    zSpaceSetFieldOfViewScale(m_fieldOfViewScale);

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
      zSpaceShutdown();
    }
  }

  void OnGUI()
  {
    Event e = Event.current;

    if (e.isKey && e.type == EventType.KeyUp)
    {
      switch (e.keyCode)
      {
        case KeyCode.M:
          SetEyesSwapped(!AreEyesSwapped());
          break;
        case KeyCode.E:
          SetMouseEmulationEnabled(!IsMouseEmulationEnabled());
          break;
        case KeyCode.O:
          SetStereoEnabled(!IsStereoEnabled());
          break;
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
    zSpaceGetDisplayOffset(displayOffsetData);
    return ConvertFloatArrayToVector3(displayOffsetData);
  }

  /// <summary>
  /// Get the virtual (x, y) position of the current display.
  /// </summary>
  /// <returns>The display position (virtual x, y coordinates) in Vector2 format.</returns>
  public Vector2 GetDisplayPosition()
  {
    float[] displayPositionData = new float[2];
    zSpaceGetDisplayPosition(displayPositionData);
    return ConvertFloatArrayToVector2(displayPositionData);
  }

  /// <summary>
  /// Get the angle of the current display.
  /// </summary>
  /// <returns>The display angle (in degrees) in Vector2 format.</returns>
  public Vector2 GetDisplayAngle()
  {
    float[] displayAngleData = new float[2];
    zSpaceGetDisplayAngle(displayAngleData);
    return ConvertFloatArrayToVector2(displayAngleData);
  }


  /// <summary>
  /// Get the resolution of the current display.
  /// </summary>
  /// <returns>The display resolution (in pixels) in Vector2 format.</returns>
  public Vector2 GetDisplayResolution()
  {
    float[] displayResolutionData = new float[2];
    zSpaceGetDisplayResolution(displayResolutionData);
    return ConvertFloatArrayToVector2(displayResolutionData);
  }


  /// <summary>
  /// Get the size of the current display.
  /// </summary>
  /// <returns>The display size (in meters) in Vector2 format.</returns>
  public Vector2 GetDisplaySize()
  {
    float[] displaySizeData = new float[2];
    zSpaceGetDisplaySize(displaySizeData);
    return ConvertFloatArrayToVector2(displaySizeData);
  }


  /// <summary>
  /// Set the stereo window's position
  /// </summary>
  /// <param name="x">The left-most position in absolute Window's screen coordinates.</param>
  /// <param name="y">The top-most position in absolute Window's screen coordinates.</param>
  public void SetWindowPosition(int x, int y)
  {
    zSpaceSetWindowPosition(x, y);
  }

  /// <summary>
  /// Set the stereo window's size.
  /// </summary>
  /// <param name="width">Window width in pixels.</param>
  /// <param name="height">Window height in pixels.</param>
  public void SetWindowSize(int width, int height)
  {
    zSpaceSetWindowSize(width, height);
  }

  /// <summary>
  /// Set the inter-pupillary distance - the physical distance between the user's eyes.
  /// </summary>
  /// <param name="interPupillaryDistance">The inter-pupillary distance (in meters).</param>
  public void SetInterPupillaryDistance(float interPupillaryDistance)
  {
    zSpaceSetInterPupillaryDistance(interPupillaryDistance);
  }

  /// <summary>
  /// Get the inter-pupillary distance - the physical distance between the user's eyes.
  /// </summary>
  /// <returns>The inter-pupillary distance (in meters).</returns>
  public float GetInterPupillaryDistance()
  {
    return zSpaceGetInterPupillaryDistance();
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
    m_stereoLevel = stereoLevel;
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
    return m_stereoLevel;
  }

  /// <summary>
  /// Set the world scale.
  /// </summary>
  /// <param name="worldScale">The world scale.</param>
  public void SetWorldScale(float worldScale)
  {
    m_worldScale = worldScale;
  }

  /// <summary>
  /// Get the world scale.
  /// </summary>
  /// <returns>The world scale.</returns>
  public float GetWorldScale()
  {
    return m_worldScale;
  }

  /// <summary>
  /// Set the field of view scale.
  /// </summary>
  /// <param name="fieldOfViewScale">The field of view scale.</param>
  public void SetFieldOfViewScale(float fieldOfViewScale)
  {
    m_fieldOfViewScale = fieldOfViewScale;
  }

  /// <summary>
  /// Get the field of view scale.
  /// </summary>
  /// <returns>The field of view scale.</returns>
  public float GetFieldOfViewScale()
  {
    return m_fieldOfViewScale;
  }

  /// <summary>
  /// Set the zero parallax offset.
  /// </summary>
  /// <param name="zeroParallaxOffset">The zero parallax offset.</param>
  public void SetZeroParallaxOffset(float zeroParallaxOffset)
  {
    zSpaceSetZeroParallaxOffset(zeroParallaxOffset);
  }

  /// <summary>
  /// Get the zero parallax offset.
  /// </summary>
  /// <returns>The zero parallax offset.</returns>
  public float GetZeroParallaxOffset()
  {
    return zSpaceGetZeroParallaxOffset();
  }

  /// <summary>
  /// Set the near clip distance.
  /// </summary>
  /// <param name="nearClip">The near clip distance (in meters).</param>
  public void SetNearClip(float nearClip)
  {
    zSpaceSetNearClip(nearClip);
  }

  /// <summary>
  /// Get the near clip distance.
  /// </summary>
  /// <returns>The near clip distance (in meters).</returns>
  public float GetNearClip()
  {
    return zSpaceGetNearClip();
  }

  /// <summary>
  /// Set the far clip distance.
  /// </summary>
  /// <param name="farClip">The far clip distance (in meters).</param>
  public void SetFarClip(float farClip)
  {
    zSpaceSetFarClip(farClip);
  }

  /// <summary>
  /// Get the far clip distance.
  /// </summary>
  /// <returns>The far clip distance (in meters).</returns>
  public float GetFarClip()
  {
    return zSpaceGetFarClip();
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
    zSpaceGetEyePosition((int)eye, positionData);

    return ConvertFloatArrayToVector3(positionData);
  }

  /// <summary>
  /// Get the frustum bounds for a specified eye.
  /// </summary>
  /// <param name="eye">The eye: left, right, or center.</param>
  /// <param name="bounds">The bounds of the frustum.</param>
  public void GetFrustumBounds(Eye eye, float[/*6*/] bounds)
  {
    zSpaceGetFrustumBounds((int)eye, bounds);
  }

  /// <summary>
  /// Set whether or not head tracking is enabled.
  /// </summary>
  /// <param name="isHeadTrackingEnabled">Flag to specify whether or not head tracking is enabled.</param>
  public void SetHeadTrackingEnabled(bool isHeadTrackingEnabled)
  {
    zSpaceSetHeadTrackingEnabled(isHeadTrackingEnabled);
  }

  /// <summary>
  /// Check if head tracking is enabled.
  /// </summary>
  /// <returns>True if head tracking is enabled.  False if not.</returns>
  public bool IsHeadTrackingEnabled()
  {
    return zSpaceIsHeadTrackingEnabled();
  }

  /// <summary>
  /// Set whether or not stylus tracking is enabled.
  /// </summary>
  /// <param name="isStylusTrackingEnabled">Flag to specify whether or not to enabled stylus tracking.</param>
  public void SetStylusTrackingEnabled(bool isStylusTrackingEnabled)
  {
    zSpaceSetStylusTrackingEnabled(isStylusTrackingEnabled);
  }

  /// <summary>
  /// Check whether or not stylus tracking is enabled.
  /// </summary>
  /// <returns>True if stylus tracking is enabled.  False if not.</returns>
  public bool IsStylusTrackingEnabled()
  {
    return zSpaceIsStylusTrackingEnabled();
  }

  /// <summary>
  /// Set whether or no mouse emulation is enabled.
  /// </summary>
  /// <param name="isMouseEmulationEnabled">True to enable mouse emulation, false otherwise.</param>
  public void SetMouseEmulationEnabled(bool isMouseEmulationEnabled)
  {
    zSpaceSetMouseEmulationEnabled(isMouseEmulationEnabled);
  }

  /// <summary>
  /// Check whether or not mouse emulation is enabled.
  /// </summary>
  /// <returns>True if mouse emulation is enabled.  False if not.</returns>
  public bool IsMouseEmulationEnabled()
  {
    return zSpaceIsMouseEmulationEnabled();
  }

  /// <summary>
  /// Set the distance at which mouse emulation will be enabled.
  /// </summary>
  /// <param name="mouseEmulationDistance">The mouse emulation distance.</param>
  public void SetMouseEmulationDistance(float mouseEmulationDistance)
  {
    zSpaceSetMouseEmulationDistance(mouseEmulationDistance);
  }

  /// <summary>
  /// Get the distance at which mouse emulation will be enabled.
  /// </summary>
  /// <returns>The mouse emulation distance.</returns>
  public float GetMouseEmulationDistance()
  {
    return zSpaceGetMouseEmulationDistance();
  }

  /// <summary>
  /// Get the tracker space pose of a specified default TrackerTarget.
  /// </summary>
  /// <param name="trackerTargetType">The type of TrackerTarget.</param>
  /// <returns>The Matrix4x4 pose in tracker space.</returns>
  public Matrix4x4 GetTargetPose(TrackerTargetType trackerTargetType)
  {
    return m_targetPoses[(int)trackerTargetType];
  }

  /// <summary>
  /// Get the camera space pose of a specified default TrackerTarget.
  /// </summary>
  /// <param name="trackerTargetType">The type of TrackerTarget.</param>
  /// <returns>The Matrix4x4 pose in camera space.</returns>
  public Matrix4x4 GetTargetCameraPose(TrackerTargetType trackerTargetType)
  {
    return m_targetCameraPoses[(int)trackerTargetType];
  }

  /// <summary>
  /// Get the world space pose of a specified default TrackerTarget.
  /// </summary>
  /// <param name="trackerTargetType">The type of TrackerTarget.</param>
  /// <returns>The Matrix4x4 pose in world space.</returns>
  public Matrix4x4 GetTargetWorldPose(TrackerTargetType trackerTargetType)
  {
    return m_targetWorldPoses[(int)trackerTargetType];
  }

  /// <summary>
  /// Get the tracker space buffered pose of a specified default TrackerTarget.
  /// </summary>
  /// <param name="trackerTargetType">The type of TrackerTarget.</param>
  /// <param name="lookBackTime"></param>
  /// <returns></returns>
  public Matrix4x4 GetTargetBufferedPose(TrackerTargetType trackerTargetType, float lookBackTime)
  {
    float[] matrixData = new float[16];
    zSpaceGetTargetBufferedPose((int)trackerTargetType, lookBackTime, matrixData);
    return ConvertFloatArrayToMatrix4x4(matrixData);
  }


  /// <summary>
  /// Set whether or not the stylus GameObject has tracking enabled.
  /// </summary>
  /// <param name="isStylusGameObjectTrackingEnabled">Flag to enable/disable tracking.</param>
  public void SetStylusGameObjectTrackingEnabled(bool isStylusGameObjectTrackingEnabled)
  {
    m_isStylusGameObjectTrackingEnabled = isStylusGameObjectTrackingEnabled;
  }

  /// <summary>
  /// Check whether or not the stylus GameObject has tracking enabled.
  /// </summary>
  /// <returns>Whether or not the stylus GameObject has tracking enabled.</returns>
  public bool IsStylusGameObjectTrackingEnabled()
  {
    return m_isStylusGameObjectTrackingEnabled;
  }

  /// <summary>
  /// Get the number of buttons associated with a specified TrackerTarget.
  /// </summary>
  /// <param name="trackerTargetType">The type of the TrackerTarget.</param>
  /// <returns>The number of buttons contained by a TrackerTarget.</returns>
  public int GetNumTargetButtons(TrackerTargetType trackerTargetType)
  {
    return zSpaceGetNumTargetButtons((int)trackerTargetType);
  }

  /// <summary>
  /// Check whether or not a specified target button is pressed.
  /// </summary>
  /// <param name="trackerTargetType">The type of TrackerTarget.</param>
  /// <param name="buttonId">The id of the button.</param>
  /// <returns>True if the button is pressed, false otherwise.</returns>
  public bool IsTargetButtonPressed(TrackerTargetType trackerTargetType, int buttonId)
  {
    return zSpaceIsTargetButtonPressed((int)trackerTargetType, buttonId);
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

    zSpaceUpdate();
    GL.IssuePluginEvent((int)GlPluginEventType.UpdateLRDetect);

    UpdateStereoInternal();
    UpdateTrackerInternal();
  }

  /// <summary>
  /// Update all of the stereo information.
  /// </summary>
  private void UpdateStereoInternal()
  {
    // Update the world scale if it has changed.
    m_worldScale = Mathf.Max(m_worldScale, 1.0e-3f);
    if (m_worldScale != zSpaceGetWorldScale())
      zSpaceSetWorldScale(m_worldScale);

    // Update the field of view scale if it has changed.
    m_fieldOfViewScale = Mathf.Max(m_fieldOfViewScale, 1.0e-3f);
    if (m_fieldOfViewScale != zSpaceGetFieldOfViewScale())
      zSpaceSetFieldOfViewScale(m_fieldOfViewScale);

    // Update the stereo level if it has changed.
    m_stereoLevel = Mathf.Max(m_stereoLevel, 1.0e-3f);
    if (m_stereoLevel != zSpaceGetStereoLevel())
      zSpaceSetStereoLevel(m_stereoLevel);

    // Update the screen position if it has changed.
    if (Screen.x != zSpaceGetWindowX() || Screen.y != zSpaceGetWindowY())
      zSpaceSetWindowPosition(Screen.x, Screen.y);

    // Update the window dimensions if they have changed.
    if (Screen.width != zSpaceGetWindowWidth() || Screen.height != zSpaceGetWindowHeight())
      zSpaceSetWindowSize(Screen.width, Screen.height);

    // Get the view and projection matrices.
    float[] matrixData = new float[16];

    for (int i = 0; i < (int)Eye.NumEyes; ++i)
    {
      zSpaceGetViewMatrix(i, matrixData);
      m_viewMatrices[i] = ConvertFloatArrayToMatrix4x4(matrixData);

      zSpaceGetProjectionMatrix(i, matrixData);
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
    zSpaceGetViewportOffset(viewportOffset);
    viewportOffset[2] = -viewportOffset[2];

    // Factor in world and field of view scales for the viewport offset.
    for (int i = 0; i < 3; ++i)
      viewportOffset[i] *= (m_worldScale * m_fieldOfViewScale);

    for (int i = 0; i < (int)TrackerTargetType.NumTypes; ++i)
    {
      // Tracker space poses.
      zSpaceGetTargetPose(i, matrixData);
      m_targetPoses[i] = ConvertFloatArrayToMatrix4x4(matrixData);

      // Camera space poses.
      zSpaceGetTargetCameraPose(i, matrixData);
      m_targetCameraPoses[i] = ConvertFloatArrayToMatrix4x4(matrixData);

      // World space poses.
      m_targetWorldPoses[i] = m_targetCameraPoses[i];

      // Scale the position based on world and field of view scales.
      m_targetWorldPoses[i][0, 3] *= m_worldScale * m_fieldOfViewScale;
      m_targetWorldPoses[i][1, 3] *= m_worldScale * m_fieldOfViewScale;
      m_targetWorldPoses[i][2, 3] *= m_worldScale;

      // Convert the camera space pose to world space.
      m_targetWorldPoses[i] = currentCameraLocalToWorldMatrix * rightToLeftCoordinateSystem * m_targetWorldPoses[i];

      // Apply the viewport offset to the world space pose.
      for (int j = 0; j < 3; ++j)
        m_targetWorldPoses[i][j, 3] -= viewportOffset[j];
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
    // Initialize left/right detect.
    if (Screen.fullScreen)
      GL.IssuePluginEvent((int)ZSCore.GlPluginEventType.InitializeLRDetectFullscreen);
    else
      GL.IssuePluginEvent((int)ZSCore.GlPluginEventType.InitializeLRDetectWindowed);

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
  private bool m_isStylusGameObjectTrackingEnabled = true;
  private bool m_isStereoEnabled = false;
  private bool m_areEyesSwapped = false;

  private Matrix4x4[] m_viewMatrices = new Matrix4x4[(int)Eye.NumEyes];
  private Matrix4x4[] m_projectionMatrices = new Matrix4x4[(int)Eye.NumEyes];

  private Matrix4x4[] m_targetPoses = new Matrix4x4[(int)TrackerTargetType.NumTypes];
  private Matrix4x4[] m_targetCameraPoses = new Matrix4x4[(int)TrackerTargetType.NumTypes];
  private Matrix4x4[] m_targetWorldPoses = new Matrix4x4[(int)TrackerTargetType.NumTypes];

  private Camera m_leftCamera = null;
  private Camera m_rightCamera = null;
  private Camera m_finalCamera = null;

  #endregion

  #region ZSPACE_PLUGIN_IMPORT_DECLARATIONS

  [DllImport("ZSUnityPlugin")]
  private static extern bool zSpaceInitialize();
  [DllImport("ZSUnityPlugin")]
  private static extern void zSpaceUpdate();
  [DllImport("ZSUnityPlugin")]
  private static extern void zSpaceShutdown();
  [DllImport("ZSUnityPlugin")]
  private static extern void zSpaceGetDisplayOffset([In] float[/*3*/] displayOffset);
  [DllImport("ZSUnityPlugin")]
  private static extern void zSpaceGetDisplayPosition([In] float[/*2*/] displayPosition);
  [DllImport("ZSUnityPlugin")]
  private static extern void zSpaceGetDisplayAngle([In] float[/*2*/] displayAngle);
  [DllImport("ZSUnityPlugin")]
  private static extern void zSpaceGetDisplayResolution([In] float[/*2*/] displayResolution);
  [DllImport("ZSUnityPlugin")]
  private static extern void zSpaceGetDisplaySize([In] float[/*2*/] displaySize);
  [DllImport("ZSUnityPlugin")]
  private static extern void zSpaceSetWindowPosition([In] int x, [In] int y);
  [DllImport("ZSUnityPlugin")]
  private static extern int zSpaceGetWindowX();
  [DllImport("ZSUnityPlugin")]
  private static extern int zSpaceGetWindowY();
  [DllImport("ZSUnityPlugin")]
  private static extern void zSpaceSetWindowSize([In] int width, [In] int height);
  [DllImport("ZSUnityPlugin")]
  private static extern int zSpaceGetWindowWidth();
  [DllImport("ZSUnityPlugin")]
  private static extern int zSpaceGetWindowHeight();
  [DllImport("ZSUnityPlugin")]
  private static extern void zSpaceSetInterPupillaryDistance([In] float interPupillaryDistance);
  [DllImport("ZSUnityPlugin")]
  private static extern float zSpaceGetInterPupillaryDistance();
  [DllImport("ZSUnityPlugin")]
  private static extern void zSpaceSetStereoLevel([In] float stereoLevel);
  [DllImport("ZSUnityPlugin")]
  private static extern float zSpaceGetStereoLevel();
  [DllImport("ZSUnityPlugin")]
  private static extern void zSpaceSetWorldScale([In] float worldScale);
  [DllImport("ZSUnityPlugin")]
  private static extern float zSpaceGetWorldScale();
  [DllImport("ZSUnityPlugin")]
  private static extern void zSpaceSetFieldOfViewScale([In] float fieldOfViewScale);
  [DllImport("ZSUnityPlugin")]
  private static extern float zSpaceGetFieldOfViewScale();
  [DllImport("ZSUnityPlugin")]
  private static extern void zSpaceSetZeroParallaxOffset([In] float zeroParallaxOffset);
  [DllImport("ZSUnityPlugin")]
  private static extern float zSpaceGetZeroParallaxOffset();
  [DllImport("ZSUnityPlugin")]
  private static extern void zSpaceSetNearClip([In] float nearClip);
  [DllImport("ZSUnityPlugin")]
  private static extern float zSpaceGetNearClip();
  [DllImport("ZSUnityPlugin")]
  private static extern void zSpaceSetFarClip([In] float farClip);
  [DllImport("ZSUnityPlugin")]
  private static extern float zSpaceGetFarClip();
  [DllImport("ZSUnityPlugin")]
  private static extern void zSpaceGetViewMatrix([In] int eye, [Out] float[/*16*/] viewMatrix);
  [DllImport("ZSUnityPlugin")]
  private static extern void zSpaceGetProjectionMatrix([In] int eye, [Out] float[/*16*/] projectionMatrix);
  [DllImport("ZSUnityPlugin")]
  private static extern void zSpaceGetEyePosition([In] int eye, [Out] float[/*3*/] eyePosition);
  [DllImport("ZSUnityPlugin")]
  private static extern void zSpaceGetFrustumBounds([In] int eye, [Out] float[/*6*/] frustumBounds);
  [DllImport("ZSUnityPlugin")]
  private static extern void zSpaceSetHeadTrackingEnabled([In] bool isHeadTrackingEnabled);
  [DllImport("ZSUnityPlugin")]
  private static extern bool zSpaceIsHeadTrackingEnabled();
  [DllImport("ZSUnityPlugin")]
  private static extern void zSpaceSetStylusTrackingEnabled([In] bool isStylusTrackingEnabled);
  [DllImport("ZSUnityPlugin")]
  private static extern bool zSpaceIsStylusTrackingEnabled();
  [DllImport("ZSUnityPlugin")]
  private static extern void zSpaceSetMouseEmulationEnabled([In] bool isMouseEmulationEnabled);
  [DllImport("ZSUnityPlugin")]
  private static extern bool zSpaceIsMouseEmulationEnabled();
  [DllImport("ZSUnityPlugin")]
  private static extern void zSpaceSetMouseEmulationDistance([In] float mouseEmulationDistance);
  [DllImport("ZSUnityPlugin")]
  private static extern float zSpaceGetMouseEmulationDistance();
  [DllImport("ZSUnityPlugin")]
  private static extern void zSpaceGetTargetPose([In] int targetType, [Out] float[/*16*/] pose);
  [DllImport("ZSUnityPlugin")]
  private static extern void zSpaceGetTargetCameraPose([In] int targetType, [Out] float[/*16*/] cameraPose);
  [DllImport("ZSUnityPlugin")]
  private static extern void zSpaceGetTargetBufferedPose([In] int targetType, [In] float lookBackTime, [Out] float[/*16*/] bufferedPose);
  [DllImport("ZSUnityPlugin")]
  private static extern int zSpaceGetNumTargetButtons([In] int targetType);
  [DllImport("ZSUnityPlugin")]
  private static extern bool zSpaceIsTargetButtonPressed([In] int targetType, [In] int buttonId);
  [DllImport("ZSUnityPlugin")]
  private static extern void zSpaceGetViewportOffset([Out] float[/*3*/] viewportOffset);
  [DllImport("ZSUnityPlugin")]
  private static extern void zSpaceGetTrackerToCameraSpaceTransform([Out] float[/*16*/] trackerToCameraSpaceTransform);

  #endregion
}